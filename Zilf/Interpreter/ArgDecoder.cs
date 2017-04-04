﻿/* Copyright 2010-2017 Jesse McGrew
 * 
 * This file is part of ZILF.
 * 
 * ZILF is free software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * ZILF is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with ZILF.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Zilf.Common;
using Zilf.Diagnostics;
using Zilf.Interpreter.Values;
using Zilf.Language;

namespace Zilf.Interpreter
{
    abstract class ArgumentDecodingError : InterpreterError
    {
        [Obsolete("Use a constructor that takes a Diagnostic.")]
        protected ArgumentDecodingError(string message)
            : base(message) { }

        protected ArgumentDecodingError(Diagnostic diagnostic)
            : base(diagnostic) { }

        protected ArgumentDecodingError(SerializationInfo si, StreamingContext sc)
            : base(si, sc)
        {
        }
    }

    abstract class CallSite
    {
        protected CallSite(string name)
        {
            this.Name = name;
        }

        protected string Name { get; }
        public abstract string ChildName { get; }

        public sealed override string ToString()
        {
            return Name;
        }

        public string DescribeArgument(int childIndex)
        {
            return $"{Name}: {ChildName} {childIndex + 1}";
        }
    }

    sealed class FunctionCallSite : CallSite
    {
        public FunctionCallSite(string name)
            : base(name)
        {
        }

        public override string ChildName => "arg";
    }

    sealed class StructuredArgumentCallSite : CallSite
    {
        public StructuredArgumentCallSite(CallSite parent, int argIndex)
            : base(parent.DescribeArgument(argIndex))
        {
        }

        public override string ChildName => "element";
    }

    sealed class ArgumentCountError : ArgumentDecodingError
    {
        ArgumentCountError(Diagnostic diagnostic)
            : base(diagnostic)
        {
        }

        ArgumentCountError(SerializationInfo si, StreamingContext sc)
            : base(si, sc)
        {
        }

        public static ArgumentCountError WrongCount(CallSite site, int lowerBound, int? upperBound, bool morePrefix = false)
        {
            const int PlainMessageCode = InterpreterMessages._0_Requires_1_21s;
            const int MessageCodeWithMore = InterpreterMessages._0_Requires_1_Additional_21s;

            var range = new ArgCountRange(lowerBound, upperBound);
            ArgCountHelpers.FormatArgCount(range, out var cs);

            var diag = DiagnosticFactory<InterpreterMessages>.Instance.GetDiagnostic(
                DiagnosticContext.Current.SourceLine,
                morePrefix ? MessageCodeWithMore : PlainMessageCode,
                new object[] { site.ToString(), cs, site.ChildName },
                null);

            return new ArgumentCountError(diag);
        }

        public static ArgumentCountError TooMany(CallSite site, int firstUnexpectedIndex, int? suspiciousTypeIndex)
        {
            Diagnostic info;
            var sourceLine = DiagnosticContext.Current.SourceLine;

            if (suspiciousTypeIndex != null)
            {
                info = DiagnosticFactory<InterpreterMessages>.Instance.GetDiagnostic(
                    sourceLine,
                    InterpreterMessages.Check_Types_Of_Earlier_0s_Eg_0_1,
                    new object[] { site.ChildName, suspiciousTypeIndex });
            }
            else
            {
                info = null;
            }

            var diag = DiagnosticFactory<InterpreterMessages>.Instance.GetDiagnostic(
                sourceLine,
                InterpreterMessages._0_Too_Many_1s_Starting_At_1_2,
                new object[] { site.ToString(), site.ChildName, firstUnexpectedIndex },
                null,
                info != null ? new[] { info } : null);

            return new ArgumentCountError(diag);
        }
    }

    sealed class ArgumentTypeError : ArgumentDecodingError
    {
        public ArgumentTypeError(CallSite site, int index, string constraintDesc)
            : base(MakeDiagnostic(
                null,
                InterpreterMessages._0_Expected_1,
                new object[] { site.DescribeArgument(index), constraintDesc }))
        {
        }

        ArgumentTypeError(SerializationInfo si, StreamingContext sc)
            : base(si, sc)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    sealed class DeclAttribute : Attribute
    {
        public DeclAttribute(string pattern)
        {
            this.Pattern = pattern;
        }

        public string Pattern { get; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    sealed class RequiredAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Struct)]
    sealed class ZilStructuredParamAttribute : Attribute
    {
        public ZilStructuredParamAttribute(StdAtom typeAtom)
        {
            this.TypeAtom = typeAtom;
        }

        public StdAtom TypeAtom { get; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    sealed class ZilOptionalAttribute : Attribute
    {
        public object Default { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    sealed class EitherAttribute : Attribute
    {
        public EitherAttribute(params Type[] types)
        {
            this.Types = types;
        }

        public Type[] Types { get; }
        public string DefaultParamDesc { get; set; }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    sealed class ZilSequenceParamAttribute : Attribute
    {
    }

    [AttributeUsage(
        AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Interface |
        AttributeTargets.Parameter | AttributeTargets.Field)]
    sealed class ParamDescAttribute : Attribute
    {
        public ParamDescAttribute(string name)
        {
            this.Description = name;
        }

        public string Description { get; }
    }

    partial class ArgDecoder
    {
        /// <summary>
        /// A function that decodes some number of <see cref="ZilObject"/> arguments into objects,
        /// calling <see cref="DecodingStepCallbacks.Ready"/> for each object produced, and then
        /// returns the index of the next argument.
        /// </summary>
        /// <param name="arguments">An array of arguments to be processed.</param>
        /// <param name="index">The index within <paramref name="arguments"/> of the first argument
        /// to be processed.</param>
        /// <param name="cb">A <see cref="DecodingStepCallbacks"/> structure containing context
        /// for the step. The <see cref="DecodingStepCallbacks.Ready"/> delegate will be called
        /// for each object produced.</param>
        /// <returns>The index of the first argument, greater than or equal to <paramref name="index"/>,
        /// that was not processed. This may point past the end of <paramref name="arguments"/>
        /// if the step consumed all input arguments, or it may be equal to <paramref name="index"/>
        /// if the step consumed none.</returns>
        delegate int DecodingStep(ZilObject[] arguments, int index, DecodingStepCallbacks cb);

        delegate void ErrorCallback(int? index = null);

        struct DecodingStepCallbacks
        {
            public Context Context;
            public CallSite Site;
            public Action<object> Ready;
            public ErrorCallback Error;
            public Action Missing;
        }

        struct DecodingStepInfo
        {
            public DecodingStep Step;
            public Constraint Constraint;
            public string Description;

            /// <summary>
            /// The minimum number of arguments this step will consume.
            /// </summary>
            public int LowerBound;
            /// <summary>
            /// The maximum number of arguments this step will consume.
            /// </summary>
            public int? UpperBound;
        }

        readonly Context ctx;
        DecodingStepInfo[] StepInfos { get; }
        int LowerBound { get; }
        int? UpperBound { get; }

        public string Description { get; }

        ArgDecoder(Context ctx, ParameterInfo[] parameters)
        {
            this.ctx = ctx;

            StepInfos = new DecodingStepInfo[parameters.Length - 1];
            LowerBound = 0;
            UpperBound = 0;

            var sb = new StringBuilder();

            // skip first arg (Context)
            for (int i = 1; i < parameters.Length; i++)
            {
                var stepInfo = PrepareOne(parameters[i]);
                StepInfos[i - 1] = stepInfo;

                LowerBound += stepInfo.LowerBound;

                if (stepInfo.UpperBound == null)
                {
                    UpperBound = null;
                }
                else
                {
                    UpperBound += stepInfo.UpperBound;
                }

                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }

                sb.Append(stepInfo.Description);
            }

            this.Description = sb.ToString();
        }

        DecodingStepInfo PrepareOne(ParameterInfo pi)
        {
            var zilOptAttr = pi.GetCustomAttribute<ZilOptionalAttribute>();

            bool isOptional;
            object defaultValue;

            if (zilOptAttr != null)
            {
                if (pi.IsOptional)
                    throw new InvalidOperationException($"Expected {nameof(ZilOptionalAttribute)} or {nameof(pi.IsOptional)}, not both");

                isOptional = true;
                defaultValue = zilOptAttr.Default;
            }
            else
            {
                isOptional = pi.IsOptional;
                defaultValue = pi.HasDefaultValue ? pi.DefaultValue : null;
            }

            var result = PrepareOne(
                pi.ParameterType,
                Hyphenate(pi.Name),
                pi.GetCustomAttributes(false),
                isOptional,
                defaultValue);

            result.Description = OverrideParamDesc(pi, isOptional) ?? result.Description;

            return result;
        }

        DecodingStepInfo PrepareOne(FieldInfo fi)
        {
            var zilOptAttr = fi.GetCustomAttribute<ZilOptionalAttribute>();

            bool isOptional;
            object defaultValue;

            if (zilOptAttr != null)
            {
                isOptional = true;
                defaultValue = zilOptAttr.Default;
            }
            else
            {
                isOptional = false;
                defaultValue = null;
            }

            var result = PrepareOne(
                fi.FieldType,
                Hyphenate(fi.Name),
                fi.GetCustomAttributes(false),
                isOptional,
                defaultValue);

            result.Description = OverrideParamDesc(fi, isOptional) ?? result.Description;

            return result;
        }

        static string OverrideParamDesc(FieldInfo fi, bool isOptional)
        {
            return MaybeWithBrackets(OverrideParamDesc(fi), isOptional);
        }

        static string OverrideParamDesc(FieldInfo fi)
        {
            var attr = fi.GetCustomAttribute<ParamDescAttribute>();

            if (attr != null)
                return attr.Description;

            var decl = fi.GetCustomAttribute<DeclAttribute>();

            if (decl != null)
            {
                var result = OverrideParamDesc(decl);
                if (result != null)
                    return result;
            }

            return OverrideParamDesc(fi.FieldType);
        }

        static string OverrideParamDesc(ParameterInfo pi, bool isOptional)
        {
            return MaybeWithBrackets(OverrideParamDesc(pi), isOptional);
        }

        static string OverrideParamDesc(ParameterInfo pi)
        {
            var attr = pi.GetCustomAttribute<ParamDescAttribute>();

            if (attr != null)
                return attr.Description;

            var decl = pi.GetCustomAttribute<DeclAttribute>();

            if (decl != null)
            {
                var result = OverrideParamDesc(decl);
                if (result != null)
                    return result;
            }

            return OverrideParamDesc(pi.ParameterType);
        }

        static string OverrideParamDesc(Type t)
        {
            var typeAttr = t.GetCustomAttribute<ParamDescAttribute>();

            if (typeAttr != null)
            {
                return typeAttr.Description;
            }

            if (t.IsValueType && !t.IsPrimitive)
            {
                var fields = GetStructFieldsInOrder(t);
                if (fields.Length == 2)
                {
                    if (fields[0].GetCustomAttribute<DeclAttribute>()?.Pattern == "'QUOTE")
                    {
                        var decl = fields[1].GetCustomAttribute<DeclAttribute>();
                        if (decl != null)
                        {
                            var result = OverrideParamDesc(decl, "'");
                            if (result != null)
                            {
                                return result;
                            }
                        }

                        var second = OverrideParamDesc(fields[1]);
                        if (second != null)
                        {
                            return "'" + second;
                        }
                    }
                }
            }

            return null;
        }

        static Regex orDeclRegex = new Regex(@"^<OR (?:'([^ >]+)\s*)+>$");

        static string OverrideParamDesc(DeclAttribute decl, string prefix = "")
        {
            if (decl.Pattern.StartsWith("'", StringComparison.Ordinal))
            {
                return decl.Pattern.Substring(1);
            }

            var match = orDeclRegex.Match(decl.Pattern);
            if (match.Success)
            {
                return WithCurlies(
                    string.Join(
                        " | ",
                        from Capture c in match.Groups[1].Captures
                        select prefix + c.Value));
            }

            return null;
        }

        static string WithEllipsis(string s)
        {
            return s + " ...";
        }

        static string WithBrackets(string s)
        {
            return "[" + s + "]";
        }

        static string MaybeWithBrackets(string s, bool isOptional)
        {
            return s != null && isOptional ? WithBrackets(s) : s;
        }

        static string WithCurlies(string s)
        {
            return "{" + s + "}";
        }

        static string WithParens(string s)
        {
            return "(" + s + ")";
        }

        static string WithAngles(string s)
        {
            return "<" + s + ">";
        }

        static string Hyphenate(string s)
        {
            var sb = new StringBuilder(s.Length);
            sb.Append(char.ToLowerInvariant(s[0]));

            for (int i = 1; i < s.Length; i++)
            {
                var c = s[i];
                if (char.IsUpper(c))
                {
                    sb.Append('-');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        DecodingStepInfo PrepareOne(Type paramType, string name, object[] customAttributes,
            bool isOptional, object defaultValueWhenOptional)
        {
            DecodingStepInfo result;
            object defaultValue = null;
            ZilStructuredParamAttribute zilStructAttr;
            ZilSequenceParamAttribute sequenceAttr;
            EitherAttribute eitherAttr;

            var isRequired = customAttributes.OfType<RequiredAttribute>().Any();
            if (isRequired && isOptional)
            {
                throw new InvalidOperationException("A parameter can't be both optional and required");
            }

            if ((eitherAttr = customAttributes.OfType<EitherAttribute>().SingleOrDefault()) != null &&
                !paramType.IsArray)
            {
                result = PrepareOneEither(paramType, eitherAttr.DefaultParamDesc ?? name, eitherAttr.Types);
            }
            else if (eitherAttr != null && paramType.IsArray)
            {
                var elemType = paramType.GetElementType();
                var innerStepInfo = PrepareOneEither(elemType, eitherAttr.DefaultParamDesc ?? name, eitherAttr.Types);
                result = PrepareOneArrayFromInnerStep(elemType, innerStepInfo, isRequired, out defaultValue);
            }
            else if (paramType.IsValueType &&
                (zilStructAttr = paramType.GetCustomAttribute<ZilStructuredParamAttribute>()) != null)
            {
                result = PrepareOneStructured(paramType);
            }
            else if (paramType.IsArray &&
                (zilStructAttr = paramType.GetElementType().GetCustomAttribute<ZilStructuredParamAttribute>()) != null)
            {
                var elemType = paramType.GetElementType();
                var innerStepInfo = PrepareOneStructured(elemType);
                result = PrepareOneArrayFromInnerStep(elemType, innerStepInfo, isRequired, out defaultValue);
            }
            else if (paramType.IsValueType &&
                (sequenceAttr = paramType.GetCustomAttribute<ZilSequenceParamAttribute>()) != null)
            {
                result = PrepareOneSequence(paramType);
            }
            else if (paramType.IsArray &&
                (sequenceAttr = paramType.GetElementType().GetCustomAttribute<ZilSequenceParamAttribute>()) != null)
            {
                var elemType = paramType.GetElementType();
                var innerStepInfo = PrepareOneSequence(elemType);
                result = PrepareOneArrayFromInnerStep(elemType, innerStepInfo, isRequired, out defaultValue);
            }
            else if (paramType == typeof(ZilObject))
            {
                // decode to ZilObject as-is
                result = new DecodingStepInfo
                {
                    Constraint = Constraint.AnyObject,
                    Description = name,
                    Step = (a, i, c) =>
                    {
                        if (i >= a.Length)
                        {
                            c.Missing();
                        }

                        c.Ready(a[i]);
                        return i + 1;
                    },
                    LowerBound = 1,
                    UpperBound = 1
                };
            }
            else if (IsZilObjectType(paramType))
            {
                var zoType = paramType;
                Constraint constraint;

                if (paramType == typeof(IStructure))
                {
                    constraint = Constraint.Structured;
                }
                else if (paramType != typeof(ZilObject))
                {
                    var builtinAttr = zoType.GetCustomAttribute<BuiltinTypeAttribute>();
                    if (builtinAttr == null)
                        throw new InvalidOperationException($"Type {paramType} is missing a BuiltinTypeAttribute");
                    constraint = Constraint.OfType(builtinAttr.Name);
                }
                else
                {
                    constraint = Constraint.AnyObject;
                }

                result = new DecodingStepInfo
                {
                    Constraint = constraint,
                    Description = name,
                    Step = (a, i, c) =>
                    {
                        if (i >= a.Length)
                        {
                            c.Missing();
                        }

                        if (!zoType.IsInstanceOfType(a[i]))
                        {
                            c.Error();
                        }

                        c.Ready(a[i]);
                        return i + 1;
                    },
                    LowerBound = 1,
                    UpperBound = 1
                };
            }
            else if (paramType == typeof(IApplicable))
            {
                result = new DecodingStepInfo
                {
                    Constraint = Constraint.Applicable,
                    Description = name,
                    Step = (a, i, c) =>
                    {
                        if (i >= a.Length)
                        {
                            c.Missing();
                        }

                        var ap = a[i].AsApplicable(c.Context);

                        if (ap == null)
                        {
                            c.Error();
                        }

                        c.Ready(ap);
                        return i + 1;
                    },
                    LowerBound = 1,
                    UpperBound = 1
                };
            }
            else if (paramType == typeof(int) || paramType == typeof(int?))
            {
                result = PrepareOneNullableConversion<ZilFix, int>(StdAtom.FIX, fix => fix.Value,
                    paramType, name, out defaultValue);
            }
            else if (paramType == typeof(string))
            {
                result = PrepareOneConversion<ZilString, string>(StdAtom.STRING, str => str.Text,
                    name, out defaultValue);
            }
            else if (paramType == typeof(char) || paramType == typeof(char?))
            {
                result = PrepareOneNullableConversion<ZilChar, char>(StdAtom.CHARACTER, ch => ch.Char,
                    paramType, name, out defaultValue);
            }
            else if (paramType == typeof(bool) || paramType == typeof(bool?))
            {
                result = PrepareOneNullableConversion<ZilObject, bool>(null, zo => zo.IsTrue,
                    paramType, name, out defaultValue);
            }
            else if (paramType.IsArray && IsZilObjectType(paramType.GetElementType()))
            {
                // decode as an array containing all remaining args
                var eltype = paramType.GetElementType();
                defaultValue = Array.CreateInstance(eltype, 0);

                Constraint constraint;
                if (eltype == typeof(IApplicable))
                {
                    constraint = Constraint.Applicable;
                }
                else if (eltype == typeof(IStructure))
                {
                    constraint = Constraint.Structured;
                }
                else if (eltype != typeof(ZilObject))
                {
                    var builtinAttr = eltype.GetCustomAttribute<BuiltinTypeAttribute>();
                    if (builtinAttr == null)
                        throw new InvalidOperationException($"Type {eltype} is missing a BuiltinTypeAttribute");
                    constraint = Constraint.OfType(builtinAttr.Name);
                }
                else
                {
                    constraint = Constraint.AnyObject;
                }

                result = new DecodingStepInfo
                {
                    Constraint = constraint,
                    Description = WithEllipsis(name),
                    Step = (a, i, c) =>
                    {
                        for (int j = i; j < a.Length; j++)
                        {
                            if (!eltype.IsInstanceOfType(a[j]))
                            {
                                c.Error();
                            }
                        }

                        var array = Array.CreateInstance(eltype, a.Length - i);

                        if (isRequired && array.Length == 0)
                        {
                            c.Missing();
                        }

                        Array.Copy(a, i, array, 0, array.Length);
                        c.Ready(array);
                        return a.Length;
                    },
                    LowerBound = 0,
                    UpperBound = null
                };

                if (isRequired)
                    result.LowerBound = 1;
            }
            else if (paramType == typeof(IApplicable[]))
            {
                // decode as an array containing all remaining args
                defaultValue = new IApplicable[0];

                result = new DecodingStepInfo
                {
                    Constraint = Constraint.Applicable,
                    Description = WithEllipsis(name),
                    Step = (a, i, c) =>
                    {
                        if (i >= a.Length)
                        {
                            c.Missing();
                        }

                        var array = new IApplicable[a.Length - i];

                        if (isRequired && array.Length == 0)
                        {
                            c.Error();
                        }

                        for (int j = i; j < a.Length; j++)
                        {
                            var ap = a[j].AsApplicable(c.Context);

                            if (ap == null)
                            {
                                c.Error();
                            }

                            array[j - i] = ap;
                        }

                        c.Ready(array);
                        return a.Length;
                    },
                    LowerBound = 0,
                    UpperBound = null
                };

                if (isRequired)
                    result.LowerBound = 1;
            }
            else if (paramType == typeof(int[]))
            {
                result = PrepareOneArrayConversion<ZilFix, int>(StdAtom.FIX, fix => fix.Value,
                    isRequired, name, out defaultValue);
            }
            else if (paramType == typeof(string[]))
            {
                result = PrepareOneArrayConversion<ZilString, string>(StdAtom.STRING, str => str.Text,
                    isRequired, name, out defaultValue);
            }
            else if (paramType == typeof(LocalEnvironment))
            {
                // decode as an optional ZilEnvironment, defaulting to the current local environment
                if (isOptional)
                    throw new InvalidOperationException($"{nameof(LocalEnvironment)} parameter is implicitly optional already");

                result = new DecodingStepInfo
                {
                    Constraint = Constraint.OfType(StdAtom.ENVIRONMENT),
                    Description = "[environment]",
                    Step = (a, i, c) =>
                    {
                        if (i < a.Length && a[i] is ZilEnvironment zenv)
                        {
                            c.Ready(zenv.LocalEnvironment);
                            return i + 1;
                        }

                        c.Ready(c.Context.LocalEnvironment);
                        return i;
                    },
                    LowerBound = 0,
                    UpperBound = 1
                };
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(paramType));
            }

            // modifiers
            var declAttr = customAttributes.OfType<DeclAttribute>().FirstOrDefault();
            if (declAttr != null)
            {
                var prevStep = result.Step;
                var decl = Program.Parse(ctx, declAttr.Pattern).Single();

                result.Constraint = result.Constraint.And(ctx, Constraint.FromDecl(ctx, decl));

                // for array (varargs) parameters, the decl is checked against a LIST containing all the args
                if (paramType.IsArray)
                {
                    result.Step = (a, i, c) =>
                    {
                        var prevConsumed = prevStep(a, i, c);
                        var list = new ZilList(a.Skip(i).Take(prevConsumed));

                        if (!Decl.Check(c.Context, list, decl))
                        {
                            c.Error();
                        }

                        return prevConsumed;
                    };
                }
                else
                {
                    result.Step = (a, i, c) =>
                    {
                        if (i >= a.Length)
                        {
                            c.Missing();
                        }

                        if (!Decl.Check(c.Context, a[i], decl))
                        {
                            c.Error();
                        }

                        return prevStep(a, i, c);
                    };
                }
            }

            if (isOptional)
            {
                result.LowerBound = 0;

                var prevStep = result.Step;
                var constraint = result.Constraint;

                defaultValueWhenOptional = defaultValueWhenOptional ?? defaultValue;

                result.Description = WithBrackets(result.Description);

                result.Step = (a, i, c) =>
                {
                    if (i < a.Length && constraint.Allows(c.Context, a[i]))
                    {
                        return prevStep(a, i, c);
                    }
                    c.Ready(defaultValueWhenOptional);
                    return i;
                };
            }

            return result;
        }

        DecodingStepInfo PrepareOneArrayFromInnerStep(
            Type elemType, DecodingStepInfo innerStepInfo, bool isRequired,
            out object defaultValue)
        {
            var result = new DecodingStepInfo
            {
                Constraint = innerStepInfo.Constraint,
                Description = WithEllipsis(innerStepInfo.Description),
                Step = (a, i, c) =>
                {
                    if (isRequired && a.Length <= i)
                    {
                        c.Missing();
                    }

                    /* Unlike other array decoder types, this one has to handle
                     * the possibility that the inner step will consume multiple
                     * arguments. */

                    var output = new System.Collections.ArrayList(a.Length - i);
                    var outerReady = c.Ready;

                    c.Ready = obj => output.Add(obj);

                    while (i < a.Length)
                    {
                        var next = innerStepInfo.Step(a, i, c);
                        Contract.Assert(next >= i);

                        if (next == i)
                        {
                            c.Error(i);
                        }

                        i = next;
                    }

                    outerReady(output.ToArray(elemType));
                    return i;
                },
                LowerBound = 0,
                UpperBound = null
            };

            if (isRequired)
                result.LowerBound = 1;

            defaultValue = Array.CreateInstance(elemType, 0);
            return result;
        }

        DecodingStepInfo PrepareOneConversion<TZil, TValue>(
            StdAtom? typeAtom, Func<TZil, TValue> convert, string name, out object defaultValue)
            where TZil : ZilObject
        {
            var constraint = (typeAtom != null) ? Constraint.OfType(typeAtom.Value) : Constraint.AnyObject;

            defaultValue = default(TValue);

            return new DecodingStepInfo
            {
                Constraint = constraint,
                Description = name,
                Step = (a, i, c) =>
                {
                    if (i >= a.Length)
                    {
                        c.Missing();
                    }

                    if (!(a[i] is TZil zo))
                    {
                        c.Error();
                    }
                    else
                    {
                        c.Ready(convert(zo));
                    }
                    return i + 1;
                },
                LowerBound = 1,
                UpperBound = 1
            };
        }

        DecodingStepInfo PrepareOneNullableConversion<TZil, TValue>(
            StdAtom? typeAtom, Func<TZil, TValue> convert, Type paramType,
            string name, out object defaultValue)
            where TZil : ZilObject
            where TValue : struct
        {
            var constraint = (typeAtom != null) ? Constraint.OfType(typeAtom.Value) : Constraint.AnyObject;

            if (paramType == typeof(TValue))
            {
                defaultValue = default(TValue);
            }
            else if (paramType == typeof(TValue?))
            {
                defaultValue = null;
            }
            else
            {
                throw new ArgumentException(
                    $"Expected {typeof(TValue)} or {typeof(TValue?)} but got {paramType}", nameof(paramType));
            }

            return new DecodingStepInfo
            {
                Constraint = constraint,
                Description = name,
                Step = (a, i, c) =>
                {
                    if (i >= a.Length)
                    {
                        c.Missing();
                    }

                    if (!(a[i] is TZil zo))
                    {
                        c.Error();
                    }
                    else
                    {
                        c.Ready(convert(zo));
                    }
                    return i + 1;
                },
                LowerBound = 1,
                UpperBound = 1
            };
        }

        DecodingStepInfo PrepareOneArrayConversion<TZil, TValue>(
            StdAtom? typeAtom, Func<TZil, TValue> convert, bool isRequired,
            string name, out object defaultValue)
            where TZil : ZilObject
        {
            var constraint = (typeAtom != null) ? Constraint.OfType(typeAtom.Value) : Constraint.AnyObject;

            defaultValue = new int[0];

            var result = new DecodingStepInfo
            {
                Constraint = constraint,
                Description = WithEllipsis(name),
                Step = (a, i, c) =>
                {
                    var array = new TValue[a.Length - i];

                    if (isRequired && array.Length == 0)
                    {
                        c.Missing();
                    }

                    for (int j = 0; j < array.Length; j++)
                    {
                        if (!(a[i + j] is TZil zo))
                        {
                            c.Error();
                        }
                        else
                        {
                            array[j] = convert(zo);
                        }
                    }
                    c.Ready(array);
                    return a.Length;
                },
                LowerBound = 0,
                UpperBound = null
            };

            if (isRequired)
                result.LowerBound = 1;

            return result;
        }

        // TODO: cache the result
        DecodingStepInfo PrepareOneStructured(Type structType)
        {
            Contract.Requires(structType != null);
            Contract.Requires(structType.IsValueType);
            Contract.Requires(structType.IsLayoutSequential);

            var typeAtom = structType.GetCustomAttribute<ZilStructuredParamAttribute>().TypeAtom;

            var stepInfos = PrepareStepsFromStruct(structType, out var fields, out var lowerBound, out var upperBound);

            var descPieces = stepInfos.Select(i => i.Description);
            string description;
            switch (typeAtom)
            {
                case StdAtom.ADECL:
                    description = string.Join(":", descPieces);
                    break;

                case StdAtom.FORM:
                    description = WithAngles(string.Join(" ", descPieces));
                    break;

                default:
                    description = WithParens(string.Join(" ", descPieces));
                    break;
            }

            var result = new DecodingStepInfo
            {
                LowerBound = 1,
                UpperBound = 1,

                Constraint = Constraint.OfType(typeAtom),
                Description = description,
                Step = (a, i, c) =>
                {
                    if (i >= a.Length)
                    {
                        c.Missing();
                    }

                    if (a[i].StdTypeAtom != typeAtom)
                    {
                        c.Error();
                    }

                    var input = (IStructure)a[i];
                    var innerSite = new StructuredArgumentCallSite(c.Site, i);

                    if ((lowerBound >= 1 && input.GetLength(lowerBound - 1) < lowerBound) ||
                        (upperBound != null && !(input.GetLength(upperBound.Value) <= upperBound)))
                    {
                        throw ArgumentCountError.WrongCount(innerSite, lowerBound, upperBound);
                    }

                    var inputLength = input.GetLength();
                    var output = Activator.CreateInstance(structType);

                    var elements = new ZilObject[inputLength];
                    for (int j = 0; j < elements.Length; j++)
                        elements[j] = input[j];

                    var elemIndex = 0;
                    var stepIndex = 0;
                    var remainingLowerBound = lowerBound;
                    var remainingUpperBound = upperBound;

                    var outerReady = c.Ready;
                    c.Site = innerSite;
                    c.Error = j =>
                    {
                        throw new ArgumentTypeError(c.Site, j ?? elemIndex, stepInfos[stepIndex].Constraint.ToString());
                    };
                    c.Missing = () =>
                    {
                        throw ArgumentCountError.WrongCount(c.Site, remainingLowerBound, remainingUpperBound, true);
                    };
                    c.Ready = obj => fields[stepIndex].SetValue(output, obj);

                    for (; stepIndex < stepInfos.Length; stepIndex++)
                    {
                        var step = stepInfos[stepIndex].Step;
                        var next = step(elements, elemIndex, c);
                        Contract.Assert(next >= elemIndex);
                        elemIndex = next;
                        remainingLowerBound -= stepInfos[stepIndex].LowerBound;
                        remainingUpperBound -= stepInfos[stepIndex].UpperBound;
                    }

                    if (elemIndex < inputLength)
                    {
                        // TODO: clarify error message (argument count might be fine but types are wrong)  -- need an example!
                        throw ArgumentCountError.WrongCount(c.Site, lowerBound, upperBound);
                    }

                    outerReady(output);
                    return i + 1;
                }
            };

            return result;
        }

        DecodingStepInfo[] PrepareStepsFromStruct(Type structType, out FieldInfo[] fields, out int lowerBound, out int? upperBound)
        {
            Contract.Requires(structType != null);
            Contract.Requires(structType.IsValueType);
            Contract.Requires(structType.IsLayoutSequential);
            Contract.Ensures(Contract.ValueAtReturn(out fields) != null);
            Contract.Ensures(Contract.Result<DecodingStepInfo[]>() != null);
            Contract.Ensures(Contract.ValueAtReturn(out fields).Length == Contract.Result<DecodingStepInfo[]>().Length);
            Contract.Ensures(Contract.ValueAtReturn(out lowerBound) >= 0);
            Contract.Ensures(!(Contract.ValueAtReturn(out upperBound) < 0));

            fields = GetStructFieldsInOrder(structType);
            var stepInfos = new DecodingStepInfo[fields.Length];
            lowerBound = 0;
            upperBound = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                var stepInfo = PrepareOne(fields[i]);
                stepInfos[i] = stepInfo;

                lowerBound += stepInfo.LowerBound;

                if (stepInfo.UpperBound == null)
                {
                    upperBound = null;
                }
                else
                {
                    upperBound += stepInfo.UpperBound;
                }
            }

            return stepInfos;
        }

        static FieldInfo[] GetStructFieldsInOrder(Type structType)
        {
            return structType.GetFields()
                .OrderBy(f => Marshal.OffsetOf(structType, f.Name).ToInt64())
                .ToArray();
        }

        // TODO: cache the result?
        DecodingStepInfo PrepareOneEither(Type paramType, string name, Type[] inputTypes)
        {
            Contract.Requires(paramType != null);
            Contract.Requires(inputTypes != null);
            Contract.Requires(inputTypes.Length > 0);
            Contract.Requires(Contract.ForAll(inputTypes, t => paramType.IsAssignableFrom(t)));

            var choices = new DecodingStep[inputTypes.Length];
            var choiceConstraints = new Constraint[inputTypes.Length];
            int? lowerBound = null;
            int? upperBound = 0;
            var constraint = Constraint.Forbidden;

            var names = new HashSet<string>();

            var noAttributes = new object[0];
            for (int i = 0; i < inputTypes.Length; i++)
            {
                var stepInfo = PrepareOne(inputTypes[i], name, noAttributes, false, null);
                names.Add(OverrideParamDesc(inputTypes[i]) ?? stepInfo.Description);
                constraint = constraint.Or(ctx, stepInfo.Constraint);
                choices[i] = stepInfo.Step;
                choiceConstraints[i] = stepInfo.Constraint;

                if (lowerBound == null || stepInfo.LowerBound < lowerBound)
                {
                    lowerBound = stepInfo.LowerBound;
                }

                if (stepInfo.UpperBound == null || stepInfo.UpperBound > upperBound)
                {
                    upperBound = stepInfo.UpperBound;
                }
            }

            string description;
            switch (names.Count)
            {
                case 0:
                    description = name;
                    break;

                case 1:
                    description = names.Single();
                    break;

                default:
                    description = WithCurlies(string.Join(" | ", names));
                    break;
            }

            var result = new DecodingStepInfo
            {
                LowerBound = (int)lowerBound,
                UpperBound = upperBound,

                Constraint = constraint,
                Description = description,

                Step = (a, i, c) =>
                {
                    var outerError = c.Error;
                    ArgumentDecodingError exception = null;
                    int choiceIndex = 0;

                    c.Error = j =>
                    {
                        throw new ArgumentTypeError(c.Site, j ?? i, choiceConstraints[choiceIndex].ToString());
                    };

                    for (; choiceIndex < choices.Length; choiceIndex++)
                    {
                        if (i < a.Length && !choiceConstraints[choiceIndex].Allows(c.Context, a[i]))
                        {
                            // doesn't pass constraint, don't try
                            continue;
                        }

                        var step = choices[choiceIndex];
                        try
                        {
                            return step(a, i, c);
                        }
                        catch (ArgumentDecodingError ex)
                        {
                            exception = ex;
                            continue;
                        }
                    }

                    if (exception == null)
                    {
                        // none of the choices were promising enough to try
                        outerError();
                    }
                    else
                    {
                        ExceptionDispatchInfo.Capture(exception).Throw();
                    }

                    // shouldn't get here
                    throw new UnreachableCodeException();
                }
            };

            return result;
        }

        // TODO: cache the result?
        DecodingStepInfo PrepareOneSequence(Type seqType)
        {
            Contract.Requires(seqType != null);
            Contract.Requires(seqType.IsValueType);
            Contract.Requires(seqType.IsLayoutSequential);

            var stepInfos = PrepareStepsFromStruct(seqType, out var fields, out var lowerBound, out var upperBound);

            var description = string.Join(" ", stepInfos.Select(i => i.Description));

            var result = new DecodingStepInfo
            {
                LowerBound = lowerBound,
                UpperBound = upperBound,

                Constraint = stepInfos[0].Constraint,
                Description = description,
                Step = (a, i, c) =>
                {
                    var remainingArgs = a.Length - i;
                    if (remainingArgs < lowerBound)
                    {
                        throw ArgumentCountError.WrongCount(c.Site, lowerBound - remainingArgs, upperBound - remainingArgs, true);
                    }

                    var output = Activator.CreateInstance(seqType);

                    var stepIndex = 0;

                    var outerReady = c.Ready;
                    c.Ready = obj => fields[stepIndex].SetValue(output, obj);
                    c.Error = j =>
                    {
                        throw new ArgumentTypeError(c.Site, j ?? i, stepInfos[stepIndex].Constraint.ToString());
                    };

                    for (; stepIndex < stepInfos.Length; stepIndex++)
                    {
                        var step = stepInfos[stepIndex].Step;
                        var next = step(a, i, c);
                        Contract.Assert(next >= i);
                        i = next;
                    }

                    outerReady(output);
                    return i;
                }
            };

            return result;
        }

        /// <summary>
        /// Returns true if the type is a simple subclass or interface of <see cref="ZilObject"/>,
        /// such that an argument value needs no conversion other than a cast.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>true if the type is a simple subclass, otherwise false.</returns>
        /// <remarks>
        /// <para>For this purpose, a type is "simple" if we can convert an argument value to the type
        /// simply by casting it. This includes <see cref="ZilObject"/> and its derived classes,
        /// as well as <see cref="IStructure"/> because all structured types implement it.</para>
        /// <para>It does <b>not</b> include <see cref="IApplicable"/>, because any ZIL type can be
        /// made applicable via <c>APPLYTYPE</c> even if its C# type does not implement the interface.
        /// Arguments must be converted to <see cref="IApplicable"/> with
        /// <see cref="ApplicableExtensions.AsApplicable(ZilObject, Context)"/> instead.</para>
        /// </remarks>
        static bool IsZilObjectType(Type t)
        {
            return typeof(ZilObject).IsAssignableFrom(t) || t == typeof(IStructure);
        }

        public static ArgDecoder FromMethodInfo(MethodInfo methodInfo, Context ctx)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));

            if (!typeof(ZilObject).IsAssignableFrom(methodInfo.ReturnType))
                throw new ArgumentException("Method return type is not assignable to ZilObject");

            var parameters = methodInfo.GetParameters();

            if (parameters.Length < 1 || parameters[0].ParameterType != typeof(Context))
                throw new ArgumentException("First parameter type must be Context");

            return new ArgDecoder(ctx, parameters);
        }

        public static SubrDelegate WrapMethodAsSubrDelegate(MethodInfo methodInfo, Context ctx,
            out string paramDescription)
        {
            Contract.Requires(methodInfo.IsStatic);
            Contract.Ensures(Contract.Result<SubrDelegate>() != null);

            return WrapMethodAsSubrDelegate(methodInfo, ctx, out paramDescription, null);
        }

        static SubrDelegate WrapMethodAsSubrDelegate(MethodInfo methodInfo, Context ctx,
            out string paramDescription,
            Dictionary<MethodInfo, SubrDelegate> alreadyDone)
        {
            Contract.Requires(methodInfo.IsStatic);
            Contract.Ensures(Contract.Result<SubrDelegate>() != null);

            var parameters = methodInfo.GetParameters();

            if (parameters.Length == 3 &&
                parameters[0].ParameterType == typeof(string) &&
                parameters[1].ParameterType == typeof(Context) &&
                parameters[2].ParameterType == typeof(ZilObject[]))
            {
                paramDescription = "args ...";
                return (SubrDelegate)Delegate.CreateDelegate(
                    typeof(SubrDelegate), methodInfo);
            }

            var decoder = ArgDecoder.FromMethodInfo(methodInfo, ctx);
            paramDescription = decoder.Description;

            SubrDelegate del = (name, c, args) =>
            {
                try
                {
                    return (ZilObject)methodInfo.Invoke(null, decoder.Decode(name, c, args));
                }
                catch (TargetInvocationException ex)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                    throw new UnreachableCodeException();
                }
            };

            // handle MdlZilRedirectAttribute
            var redirectAttr = methodInfo.GetCustomAttribute<Subrs.MdlZilRedirectAttribute>();

            if (redirectAttr != null)
            {
                var targetMethodInfo = redirectAttr.Type.GetMethod(redirectAttr.Target, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                if (targetMethodInfo == null)
                    throw new InvalidOperationException("Can't find redirect target " + redirectAttr.Target);

                alreadyDone = alreadyDone ?? new Dictionary<MethodInfo, SubrDelegate>();
                alreadyDone.Add(methodInfo, del);

                if (alreadyDone.TryGetValue(targetMethodInfo, out var targetDel) == false)
                {
                    targetDel = WrapMethodAsSubrDelegate(targetMethodInfo, ctx, out _, alreadyDone);
                    if (!alreadyDone.ContainsKey(targetMethodInfo))
                        alreadyDone.Add(targetMethodInfo, targetDel);
                }

                var prevDel = del;
                var topLevelOnly = redirectAttr.TopLevelOnly;

                del = (name, c, args) =>
                {
                    if ((c.CurrentFile.Flags & FileFlags.MdlZil) != 0 &&
                        (!topLevelOnly || c.AtTopLevel))
                    {
                        return targetDel(name, c, args);
                    }

                    return prevDel(name, c, args);
                };
            }

            return del;
        }

        public object[] Decode(string name, Context ctx, ZilObject[] args)
        {
            var site = new FunctionCallSite(name);

            if (args.Length < LowerBound || args.Length > UpperBound)
            {
                throw ArgumentCountError.WrongCount(site, LowerBound, UpperBound);
            }

            var result = new List<object>(1 + args.Length) { ctx };

            var argIndex = 0;
            Constraint savedConstraint = null;
            var remainingLowerBound = LowerBound;
            var remainingUpperBound = UpperBound;
            int? lastUnderachievingStepArgIndex = null;

            var callbacks = new DecodingStepCallbacks
            {
                Context = ctx,
                Site = site,

                Missing = () =>
                {
                    throw ArgumentCountError.WrongCount(site, remainingLowerBound, remainingUpperBound, true);
                },
                Ready = o => result.Add(o)
            };

            for (var stepIndex = 0; stepIndex < StepInfos.Length; stepIndex++)
            {
                var constraint = StepInfos[stepIndex].Constraint;
                if (savedConstraint != null)
                {
                    constraint = constraint.Or(ctx, savedConstraint);
                    savedConstraint = null;
                }
                callbacks.Error = i => { throw new ArgumentTypeError(
                    site, i ?? argIndex, constraint.ToString()); };

                var step = StepInfos[stepIndex].Step;
                var next = step(args, argIndex, callbacks);
                Contract.Assert(next >= argIndex);

                var stepLowerBound = StepInfos[stepIndex].LowerBound;
                var stepUpperBound = StepInfos[stepIndex].UpperBound;

                remainingLowerBound -= stepLowerBound;
                remainingUpperBound -= stepUpperBound;

                if ((stepUpperBound == null && next < args.Length) ||
                    (stepUpperBound != null && next < argIndex + (int)stepUpperBound))
                {
                    // this step could theoretically have consumed more arguments
                    lastUnderachievingStepArgIndex = argIndex;
                }

                if (next == argIndex)
                {
                    // optional step didn't match anything. propagate its constraint forward so the
                    // next step can produce a better error message if it fails.
                    savedConstraint = constraint;
                }
                else
                {
                    argIndex = next;
                }
            }

            if (argIndex < args.Length)
            {
                /* We've already checked args.Length against the range of possible argument counts,
                 * so if there are still arguments left unmatched, that must mean there were
                 * optional arguments that we skipped, as well as extra arguments at the end that
                 * we have no step to match. */

                if (savedConstraint != null)
                {
                    /* At least one of the optional arguments we skipped was at the end, e.g.:
                     *
                     *     expected   <FOO fix [atom]>
                     *     actual     <FOO fix string>
                     *
                     * We decoded the default value for the atom, and now we're left with a string
                     * we don't know what to do with. We can assume the user meant to pass an
                     * atom, so we treat this as a type error, using the saved constraint to
                     * indicate the correct type.
                     */

                    throw new ArgumentTypeError(site, argIndex, savedConstraint.ToString());
                }

                /* The last optional argument we skipped was earlier in the list, e.g.:
                    *
                    *     expected   <FOO fix [atom] string>
                    *     actual     <FOO fix string string>
                    *
                    * Or it might have been a sequence inside Either:
                    *
                    *     expected   <FOO {atom | string oblist}>
                    *     actual     <FOO atom oblist>
                    *
                    * The user might have passed the wrong type for an earlier argument. We can
                    * guess which one, if we noticed a step that didn't reach its full potential.
                    * (Heartbreaking.) But we can't be sure, so we can't call this a type error.
                    */

                throw ArgumentCountError.TooMany(site, argIndex + 1, lastUnderachievingStepArgIndex + 1);
            }

            return result.ToArray();
        }
    }
}
