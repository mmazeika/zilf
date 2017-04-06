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
using System.Diagnostics.Contracts;
using Zilf.Interpreter;
using Zilf.Interpreter.Values;
using Zilf.Diagnostics;
using System.Runtime.Serialization;

namespace Zilf.Language
{
    /// <summary>
    /// Allows non-structured types to be checked against structure DECLs.
    /// </summary>
    [ContractClass(typeof(IProvideStructureForDeclCheckContract))]
    interface IProvideStructureForDeclCheck
    {
        IStructure GetStructureForDeclCheck(Context ctx);
    }

    [ContractClassFor(typeof(IProvideStructureForDeclCheck))]
    abstract class IProvideStructureForDeclCheckContract : IProvideStructureForDeclCheck
    {
        public IStructure GetStructureForDeclCheck(Context ctx)
        {
            Contract.Requires(ctx != null);
            Contract.Ensures(Contract.Result<IStructure>() != null);
            return default(IStructure);
        }
    }

    /// <summary>
    /// Raised when a user-defined DECL check fails.
    /// </summary>
    class DeclCheckError : InterpreterError
    {
        const int DiagnosticCode = InterpreterMessages.Expected_0_To_Match_DECL_1_But_Got_2;

        /// <summary>
        /// Gets the value that didn't pass the DECL.
        /// </summary>
        public ZilObject Value { get; private set; }
        /// <summary>
        /// Gets the DECL that prevented the value from being used.
        /// </summary>
        public ZilObject Pattern { get; private set; }
        /// <summary>
        /// Gets a string describing what the value was going to be used for.
        /// </summary>
        public string Usage { get; private set; }

        public DeclCheckError(Context ctx, ZilObject value, ZilObject pattern, string usage)
            : base(DiagnosticCode, usage, pattern.ToStringContext(ctx, false), value.ToStringContext(ctx, false))
        {
            this.Value = value;
            this.Pattern = pattern;
            this.Usage = usage;
        }

        public DeclCheckError(IProvideSourceLine src, Context ctx, ZilObject value, ZilObject pattern,
            string usage)
            : base(src, DiagnosticCode, usage, pattern.ToStringContext(ctx, false), value.ToStringContext(ctx, false))
        {
            this.Value = value;
            this.Pattern = pattern;
            this.Usage = usage;
        }

        public DeclCheckError(Context ctx, ZilObject value, ZilObject pattern,
            string usageFormat, object arg0)
            : this(ctx, value, pattern, string.Format(usageFormat, arg0))
        {
        }

        public DeclCheckError(IProvideSourceLine src, Context ctx, ZilObject value, ZilObject pattern,
            string usageFormat, object arg0)
            : this(src, ctx, value, pattern, string.Format(usageFormat, arg0))
        {
        }

        protected DeclCheckError(SerializationInfo si, StreamingContext sc)
            : base(si, sc)
        {
        }
    }

    class Decl
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public static bool Check(Context ctx, ZilObject value, ZilObject pattern)
        {
            Contract.Requires(ctx != null);
            Contract.Requires(value != null);
            Contract.Requires(pattern != null);

            ZilAtom atom;
            bool segment = false;

            switch (pattern.StdTypeAtom)
            {
                case StdAtom.ATOM:
                    atom = (ZilAtom)pattern;
                    switch (atom.StdAtom)
                    {
                        case StdAtom.ANY:
                            return true;

                        case StdAtom.APPLICABLE:
                            return value.IsApplicable(ctx);

                        case StdAtom.STRUCTURED:
                            return (value is IStructure);

                        case StdAtom.TUPLE:
                            // special case
                            return (value.StdTypeAtom == StdAtom.LIST);

                        default:
                            // arbitrary atoms can be type names...
                            if (ctx.IsRegisteredType(atom))
                            {
                                var typeAtom = value.GetTypeAtom(ctx);

                                if (typeAtom == atom)
                                    return true;

                                // special cases: a raw TABLE value can substitute for a TABLE-based type, or VECTOR
                                if (typeAtom.StdAtom == StdAtom.TABLE &&
                                    (atom.StdAtom == StdAtom.VECTOR || ctx.GetTypePrim(atom) == PrimType.TABLE))
                                    return true;

                                return false;
                            }

                            // ...or aliases
                            var aliased = ctx.GetProp(atom, ctx.GetStdAtom(StdAtom.DECL));

                            // TODO: better check for circular aliases
                            if (aliased != null && aliased != atom)
                                return Check(ctx, value, aliased);

                            throw new InterpreterError(
                                InterpreterMessages.Unrecognized_0_1,
                                "atom in DECL pattern",
                                atom);
                    }

                case StdAtom.SEGMENT:
                    pattern = ((ZilSegment)pattern).Form;
                    segment = true;
                    goto case StdAtom.FORM;

                case StdAtom.FORM:
                    var form = (ZilForm)pattern;
                    var first = form.First;

                    // special forms
                    atom = first as ZilAtom;
                    if (atom != null)
                    {
                        switch (atom.StdAtom)
                        {
                            case StdAtom.OR:
                                foreach (var subpattern in form.Rest)
                                    if (Check(ctx, value, subpattern))
                                        return true;
                                return false;

                            case StdAtom.QUOTE:
                                return form.Rest.First.Equals(value);

                            case StdAtom.PRIMTYPE:
                                return value.PrimType == ctx.GetTypePrim((ZilAtom)form.Rest.First);
                        }
                    }

                    // structure form: first pattern element is a DECL matched against the whole structure
                    // (usually a type atom), remaining elements are matched against the structure elements
                    if (!Check(ctx, value, first))
                        return false;

                    if (value is IStructure valueAsStructure)
                    {
                        // yay
                    }
                    else if (value is IProvideStructureForDeclCheck structProvider)
                    {
                        valueAsStructure = structProvider.GetStructureForDeclCheck(ctx);
                    }
                    else
                    {
                        return false;
                    }

                    return CheckElements(ctx, valueAsStructure, (ZilForm)pattern, segment);

                default:
                    throw new InterpreterError(
                        InterpreterMessages.Unrecognized_0_1,
                        "value in DECL pattern",
                        pattern.ToStringContext(ctx, false));
            }
        }

        static bool CheckElements(Context ctx, IStructure structure, ZilForm pattern, bool segment)
        {
            foreach (var subpattern in pattern.Rest)
            {
                if (subpattern is ZilVector vector)
                {
                    var len = vector.GetLength();
                    if (len > 0 && vector[0] is ZilAtom atom)
                    {
                        int i;

                        switch (atom.StdAtom)
                        {
                            case StdAtom.REST:
                                i = 1;
                                while (!structure.IsEmpty)
                                {
                                    if (!Check(ctx, structure.GetFirst(), vector[i]))
                                        return false;

                                    i++;
                                    if (i >= len)
                                        i = 1;

                                    structure = structure.GetRest(1);
                                }

                                // !<FOO [REST A B C]> must repeat A B C a whole number of times
                                // (ZILF extension)
                                if (segment && i != 1)
                                {
                                    return false;
                                }

                                return true;

                            case StdAtom.OPT:
                            case StdAtom.OPTIONAL:
                                // greedily match OPT elements until the structure ends or a match fails
                                for (i = 1; i < len; i++)
                                {
                                    if (structure.IsEmpty || !Check(ctx, structure.GetFirst(), vector[i]))
                                        break;

                                    structure = structure.GetRest(1);
                                }

                                // move on to the next subpattern, if any
                                continue;
                        }
                    }
                    else if (len > 0 && vector[0] is ZilFix fix)
                    {
                        var count = fix.Value;

                        for (int i = 0; i < count; i++)
                        {
                            for (int j = 1; j < vector.GetLength(); j++)
                            {
                                if (structure.IsEmpty)
                                    return false;

                                if (!Check(ctx, structure.GetFirst(), vector[j]))
                                    return false;

                                structure = structure.GetRest(1);
                            }
                        }

                        // move on to the next subpattern, if any
                        continue;
                    }

                    throw new InterpreterError(
                        InterpreterMessages.Unrecognized_0_1,
                        "vector in DECL pattern",
                        vector.ToStringContext(ctx, false));
                }

                if (structure.IsEmpty)
                    return false;

                if (!Check(ctx, structure.GetFirst(), subpattern))
                    return false;

                structure = structure.GetRest(1);
            }

            if (segment && !structure.IsEmpty)
            {
                return false;
            }

            return true;
        }
    }
}
