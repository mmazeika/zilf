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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Zilf.Diagnostics;
using Zilf.Emit;
using Zilf.Interpreter;
using Zilf.Interpreter.Values;
using Zilf.Language;
using Zilf.ZModel;
using Zilf.ZModel.Values;
using Zilf.ZModel.Vocab;

namespace Zilf.Compiler
{
    partial class Compilation
    {
        IFlagBuilder GetFlag(ZilAtom flag)
        {
            if (flag == null)
                return null;

            ZilAtom originalFlag;
            if (Context.ZEnvironment.TryGetBitSynonym(flag, out originalFlag))
            {
                flag = originalFlag;
            }

            DefineFlag(flag);
            return Flags[flag];
        }

        void DefineProperty(ZilAtom prop)
        {
            Contract.Requires(prop != null);

            if (!Properties.ContainsKey(prop))
            {
                // create property builder
                var pb = Game.DefineProperty(prop.ToString());
                Properties.Add(prop, pb);

                // create constant
                string propConstName = "P?" + prop;
                var propAtom = ZilAtom.Parse(propConstName, Context);
                Constants.Add(propAtom, pb);
            }
        }

        void DefineFlag(ZilAtom flag)
        {
            Contract.Requires(flag != null);

            if (!Flags.ContainsKey(flag))
            {
                // create flag builder
                var fb = Game.DefineFlag(flag.ToString());
                Flags.Add(flag, fb);
                UniqueFlags++;

                // create constant
                Constants.Add(flag, fb);
            }
        }

        void DefineFlagAlias(ZilAtom alias, ZilAtom original)
        {
            Contract.Requires(alias != null);
            Contract.Requires(original != null);
            Contract.Ensures(Constants.ContainsKey(alias));

            if (!Flags.ContainsKey(alias))
            {
                var fb = Flags[original];
                Constants.Add(alias, fb);
            }
        }

        /// <summary>
        /// Contains unique atoms used as special values in <see cref="PreBuildObject( ZilModelObject)"/>.
        /// </summary>
        /// <remarks>
        /// Since DESC and IN (or LOC) can be used as property names when the property definition
        /// matches the direction pattern (most commonly seen with IN), these atoms are used to separately
        /// track whether the names have been used as properties and/or pseudo-properties.
        /// </remarks>
        static class PseudoPropertyAtoms
        {
            public static readonly ZilAtom Desc = new ZilAtom("?DESC?", null, StdAtom.None);
            public static readonly ZilAtom Location = new ZilAtom("?IN/LOC?", null, StdAtom.None);
        }

        void PreBuildObject(ZilModelObject model)
        {
            Contract.Requires(model != null);

            var globalsByName = Context.ZEnvironment.Globals.ToDictionary(g => g.Name);
            var propertiesSoFar = new HashSet<ZilAtom>();

            var preBuilders = new ComplexPropDef.ElementPreBuilders
            {
                CreateVocabWord = (atom, partOfSpeech, src) =>
                {
                    IWord word;

                    switch (partOfSpeech.StdAtom)
                    {
                        case StdAtom.ADJ:
                        case StdAtom.ADJECTIVE:
                            word = Context.ZEnvironment.GetVocabAdjective(atom, src);
                            break;

                        case StdAtom.NOUN:
                        case StdAtom.OBJECT:
                            word = Context.ZEnvironment.GetVocabNoun(atom, src);
                            break;

                        case StdAtom.BUZZ:
                            word = Context.ZEnvironment.GetVocabBuzzword(atom, src);
                            break;

                        case StdAtom.PREP:
                            word = Context.ZEnvironment.GetVocabPreposition(atom, src);
                            break;

                        case StdAtom.DIR:
                            word = Context.ZEnvironment.GetVocabDirection(atom, src);
                            break;

                        case StdAtom.VERB:
                            word = Context.ZEnvironment.GetVocabVerb(atom, src);
                            break;

                        default:
                            Context.HandleError(new CompilerError(model, CompilerMessages.Unrecognized_0_1, "part of speech", partOfSpeech));
                            break;
                    }
                },

                ReserveGlobal = atom =>
                {
                    ZilGlobal g;
                    if (globalsByName.TryGetValue(atom, out g))
                        g.StorageType = GlobalStorageType.Hard;
                }
            };

            // for detecting implicitly defined directions
            var directionPattern = Context.GetProp(
                Context.GetStdAtom(StdAtom.DIRECTIONS), Context.GetStdAtom(StdAtom.PROPSPEC)) as ComplexPropDef;

            // create property builders for all properties on this object as needed,
            // and set up P?FOO constants for them. also create vocabulary words for 
            // SYNONYM and ADJECTIVE property values, and constants for FLAGS values.
            foreach (ZilList prop in model.Properties)
            {
                using (DiagnosticContext.Push(prop.SourceLine))
                {
                    // the first element must be an atom identifying the property
                    var atom = prop.First as ZilAtom;
                    if (atom == null)
                    {
                        Context.HandleError(new CompilerError(model, CompilerMessages.Property_Specification_Must_Start_With_An_Atom));
                        continue;
                    }

                    ZilAtom uniquePropertyName;

                    // exclude phony built-in properties
                    /* we also detect directions here, which are tricky for a few reasons:
                     * - they can be implicitly defined by a property spec that looks sufficiently direction-like
                     * - (IN ROOMS) is not a direction, even if IN is explicitly defined as a direction -- but (IN "string") is!
                     * - (FOO BAR) is not enough to implicitly define FOO as a direction, even if (DIR R:ROOM)
                     *   is a pattern for directions
                     */
                    bool phony;
                    bool? isSynonym = null;
                    Synonym synonym = null;
                    var definedDirection = Context.ZEnvironment.Directions.Contains(atom);

                    if (prop.Rest != null && prop.Rest.Rest != null &&
                        (!prop.Rest.Rest.IsEmpty ||
                         (definedDirection && !(prop.Rest.First is ZilAtom))) &&
                        (definedDirection ||
                         (directionPattern != null && directionPattern.Matches(Context, prop))))
                    {
                        // it's a direction
                        phony = false;

                        // could be a new implicitly defined direction
                        if (!Context.ZEnvironment.Directions.Contains(atom))
                        {
                            synonym = Context.ZEnvironment.Synonyms.FirstOrDefault(s => s.SynonymWord.Atom == atom);

                            if (synonym == null)
                            {
                                isSynonym = false;
                                Context.ZEnvironment.Directions.Add(atom);
                                Context.ZEnvironment.GetVocabDirection(atom, prop.SourceLine);
                                if (directionPattern != null)
                                    Context.SetPropDef(atom, directionPattern);
                                uniquePropertyName = atom;
                            }
                            else
                            {
                                isSynonym = true;
                                uniquePropertyName = synonym.OriginalWord.Atom;
                            }
                        }
                        else
                        {
                            uniquePropertyName = atom;
                        }
                    }
                    else
                    {
                        switch (atom.StdAtom)
                        {
                            case StdAtom.DESC:
                                phony = true;
                                uniquePropertyName = PseudoPropertyAtoms.Desc;
                                break;
                            case StdAtom.IN:
                                // (IN FOO) is a location, but (IN "foo") is a property
                                if (prop.Rest.First is ZilAtom)
                                    goto case StdAtom.LOC;
                                goto default;
                            case StdAtom.LOC:
                                phony = true;
                                uniquePropertyName = PseudoPropertyAtoms.Location;
                                break;
                            case StdAtom.FLAGS:
                                phony = true;
                                // multiple FLAGS definitions are OK
                                uniquePropertyName = null;
                                break;
                            default:
                                phony = false;
                                uniquePropertyName = atom;
                                break;
                        }
                    }

                    if (uniquePropertyName != null)
                    {
                        if (propertiesSoFar.Contains(uniquePropertyName))
                        {
                            Context.HandleError(new CompilerError(
                                prop,
                                CompilerMessages.Duplicate_0_Definition_1,
                                phony ? "pseudo-property" : "property",
                                atom.ToStringContext(Context, false)));
                        }
                        else
                        {
                            propertiesSoFar.Add(uniquePropertyName);
                        }
                    }

                    if (!phony && !Properties.ContainsKey(atom))
                    {
                        if (isSynonym == null)
                        {
                            synonym = Context.ZEnvironment.Synonyms.FirstOrDefault(s => s.SynonymWord.Atom == atom);
                            isSynonym = (synonym != null);
                        }

                        if ((bool)isSynonym)
                        {
                            IPropertyBuilder origPb;
                            var origAtom = synonym.OriginalWord.Atom;
                            if (Properties.TryGetValue(origAtom, out origPb) == false)
                            {
                                DefineProperty(origAtom);
                                origPb = Properties[origAtom];
                            }
                            Properties.Add(atom, origPb);

                            var pAtom = ZilAtom.Parse("P?" + atom, Context);
                            Constants.Add(pAtom, origPb);

                            var origSpec = Context.GetProp(origAtom, Context.GetStdAtom(StdAtom.PROPSPEC));
                            Context.PutProp(atom, Context.GetStdAtom(StdAtom.PROPSPEC), origSpec);
                        }
                        else
                        {
                            DefineProperty(atom);
                        }
                    }

                    // check for a PROPSPEC
                    var propspec = Context.GetProp(atom, Context.GetStdAtom(StdAtom.PROPSPEC));
                    if (propspec != null)
                    {
                        var complexDef = propspec as ComplexPropDef;
                        if (complexDef != null)
                        {
                            // PROPDEF pattern
                            if (complexDef.Matches(Context, prop))
                            {
                                complexDef.PreBuildProperty(Context, prop, preBuilders);
                            }
                        }
                        else
                        {
                            // name of a custom property builder function
                            var form = new ZilForm(new ZilObject[] { propspec, prop }) { SourceLine = prop.SourceLine };
                            var specOutput = form.Eval(Context);
                            ZilList propBody;
                            if (specOutput.StdTypeAtom != StdAtom.LIST ||
                                (propBody = ((ZilList)specOutput).Rest) == null || propBody.IsEmpty)
                            {
                                Context.HandleError(new CompilerError(model, CompilerMessages.PROPSPEC_For_Property_0_Returned_A_Bad_Value_1, atom, specOutput));
                                continue;
                            }

                            // replace the property body with the propspec's output
                            prop.Rest = propBody;
                        }
                    }
                    else
                    {
                        switch (atom.StdAtom)
                        {
                            case StdAtom.SYNONYM:
                                foreach (ZilObject obj in prop.Rest)
                                {
                                    atom = obj as ZilAtom;
                                    if (atom == null)
                                        continue;

                                    try
                                    {
                                        var word = Context.ZEnvironment.GetVocabNoun(atom, prop.SourceLine);
                                    }
                                    catch (ZilError ex)
                                    {
                                        Context.HandleError(ex);
                                    }
                                }
                                break;

                            case StdAtom.ADJECTIVE:
                                foreach (ZilObject obj in prop.Rest)
                                {
                                    atom = obj as ZilAtom;
                                    if (atom == null)
                                        continue;

                                    try
                                    {
                                        var word = Context.ZEnvironment.GetVocabAdjective(atom, prop.SourceLine);
                                    }
                                    catch (ZilError ex)
                                    {
                                        Context.HandleError(ex);
                                    }
                                }
                                break;

                            case StdAtom.PSEUDO:
                                foreach (ZilObject obj in prop.Rest)
                                {
                                    var str = obj as ZilString;
                                    if (str == null)
                                        continue;

                                    try
                                    {
                                        var word = Context.ZEnvironment.GetVocabNoun(ZilAtom.Parse(str.Text, Context), prop.SourceLine);
                                    }
                                    catch (ZilError ex)
                                    {
                                        Context.HandleError(ex);
                                    }
                                }
                                break;

                            case StdAtom.FLAGS:
                                foreach (ZilObject obj in prop.Rest)
                                {
                                    atom = obj as ZilAtom;
                                    if (atom == null)
                                        continue;

                                    try
                                    {
                                        ZilAtom original;
                                        if (Context.ZEnvironment.TryGetBitSynonym(atom, out original))
                                        {
                                            DefineFlag(original);
                                        }
                                        else
                                        {
                                            DefineFlag(atom);
                                        }
                                    }
                                    catch (ZilError ex)
                                    {
                                        Context.HandleError(ex);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        void BuildObject(ZilModelObject model, IObjectBuilder ob)
        {
            Contract.Requires(model != null);
            Contract.Requires(ob != null);

            var elementConverters = new ComplexPropDef.ElementConverters
            {
                CompileConstant = CompileConstant,

                GetAdjectiveValue = (atom, src) =>
                {
                    var word = Context.ZEnvironment.GetVocabAdjective(atom, src);
                    if (Context.ZEnvironment.ZVersion == 3)
                    {
                        return Constants[ZilAtom.Parse("A?" + word.Atom, Context)];
                    }
                    return Vocabulary[word];
                },

                GetGlobalNumber = atom => Globals[atom],

                GetVocabWord = (atom, partOfSpeech, src) =>
                {
                    IWord word;

                    switch (partOfSpeech.StdAtom)
                    {
                        case StdAtom.ADJ:
                        case StdAtom.ADJECTIVE:
                            word = Context.ZEnvironment.GetVocabAdjective(atom, src);
                            break;

                        case StdAtom.NOUN:
                        case StdAtom.OBJECT:
                            word = Context.ZEnvironment.GetVocabNoun(atom, src);
                            break;

                        case StdAtom.BUZZ:
                            word = Context.ZEnvironment.GetVocabBuzzword(atom, src);
                            break;

                        case StdAtom.PREP:
                            word = Context.ZEnvironment.GetVocabPreposition(atom, src);
                            break;

                        case StdAtom.DIR:
                            word = Context.ZEnvironment.GetVocabDirection(atom, src);
                            break;

                        case StdAtom.VERB:
                            word = Context.ZEnvironment.GetVocabVerb(atom, src);
                            break;

                        default:
                            Context.HandleError(new CompilerError(model, CompilerMessages.Unrecognized_0_1, "part of speech", partOfSpeech));
                            return Game.Zero;
                    }

                    return Vocabulary[word];
                }
            };

            foreach (ZilList prop in model.Properties)
            {
                IPropertyBuilder pb;
                ITableBuilder tb;
                int length = 0;

                bool noSpecialCases = false;

                // the first element must be an atom identifying the property
                var propName = prop.First as ZilAtom;
                ZilList propBody = prop.Rest;
                if (propName == null)
                {
                    Context.HandleError(new CompilerError(model, CompilerMessages.Property_Specification_Must_Start_With_An_Atom));
                    continue;
                }

                // check for IN/LOC, which can take precedence over PROPSPEC
                ZilObject value = propBody.First;
                if (propName.StdAtom == StdAtom.LOC ||
                    (propName.StdAtom == StdAtom.IN && ((IStructure)propBody).GetLength(1) == 1) && value is ZilAtom)
                {
                    var valueAtom = value as ZilAtom;
                    if (valueAtom == null)
                    {
                        Context.HandleError(new CompilerError(model, CompilerMessages.Value_For_0_Property_Must_Be_1, propName, "an atom"));
                        continue;
                    }
                    IObjectBuilder parent;
                    if (Objects.TryGetValue(valueAtom, out parent) == false)
                    {
                        Context.HandleError(new CompilerError(
                            model,
                            CompilerMessages.No_Such_Object_0,
                            valueAtom.ToString()));
                        continue;
                    }
                    ob.Parent = parent;
                    ob.Sibling = parent.Child;
                    parent.Child = ob;
                    continue;
                }

                // check for a PUTPROP giving a PROPDEF pattern or hand-coded property builder
                var propspec = Context.GetProp(propName, Context.GetStdAtom(StdAtom.PROPSPEC));
                if (propspec != null)
                {
                    var complexDef = propspec as ComplexPropDef;
                    if (complexDef != null)
                    {
                        // PROPDEF pattern
                        if (complexDef.Matches(Context, prop))
                        {
                            tb = ob.AddComplexProperty(Properties[propName]);
                            complexDef.BuildProperty(Context, prop, tb, elementConverters);
                            continue;
                        }
                    }
                    else
                    {
                        // name of a custom property builder function
                        // PreBuildObject already called the function and replaced the property body
                        noSpecialCases = true;
                    }
                }

                // built-in property builder, so at least one value has to follow the atom (except for FLAGS)
                if (value == null)
                {
                    if (propName.StdAtom != StdAtom.FLAGS)
                        Context.HandleError(new CompilerError(model, CompilerMessages.Property_Has_No_Value_0, propName.ToString()));
                    continue;
                }

                // check for special cases
                bool handled = false;
                if (!noSpecialCases)
                {
                    switch (propName.StdAtom)
                    {
                        case StdAtom.DESC:
                            handled = true;
                            if (value.StdTypeAtom != StdAtom.STRING)
                            {
                                Context.HandleError(new CompilerError(model, CompilerMessages.Value_For_0_Property_Must_Be_1, propName, "a STRING"));
                                continue;
                            }
                            ob.DescriptiveName = value.ToStringContext(Context, true);
                            continue;

                        case StdAtom.FLAGS:
                            handled = true;
                            foreach (ZilObject obj in propBody)
                            {
                                var atom = obj as ZilAtom;
                                if (atom == null)
                                {
                                    Context.HandleError(new CompilerError(model, CompilerMessages.Values_For_0_Property_Must_Be_1, propName, "atoms"));
                                    break;
                                }

                                ZilAtom original;
                                if (Context.ZEnvironment.TryGetBitSynonym(atom, out original))
                                    atom = original;

                                IFlagBuilder fb = Flags[atom];
                                ob.AddFlag(fb);
                            }
                            continue;

                        case StdAtom.SYNONYM:
                            handled = true;
                            tb = ob.AddComplexProperty(Properties[propName]);
                            foreach (ZilObject obj in propBody)
                            {
                                var atom = obj as ZilAtom;
                                if (atom == null)
                                {
                                    Context.HandleError(new CompilerError(model, CompilerMessages.Values_For_0_Property_Must_Be_1, propName, "atoms"));
                                    break;
                                }

                                var word = Context.ZEnvironment.GetVocabNoun(atom, prop.SourceLine);
                                IWordBuilder wb = Vocabulary[word];
                                tb.AddShort(wb);
                                length += 2;
                            }
                            break;

                        case StdAtom.ADJECTIVE:
                            handled = true;
                            tb = ob.AddComplexProperty(Properties[propName]);
                            foreach (ZilObject obj in propBody)
                            {
                                var atom = obj as ZilAtom;
                                if (atom == null)
                                {
                                    Context.HandleError(new CompilerError(model, CompilerMessages.Values_For_0_Property_Must_Be_1, propName, "atoms"));
                                    break;
                                }

                                var word = Context.ZEnvironment.GetVocabAdjective(atom, prop.SourceLine);
                                IWordBuilder wb = Vocabulary[word];
                                if (Context.ZEnvironment.ZVersion == 3)
                                {
                                    tb.AddByte(Constants[ZilAtom.Parse("A?" + word.Atom, Context)]);
                                    length++;
                                }
                                else
                                {
                                    tb.AddShort(wb);
                                    length += 2;
                                }
                            }
                            break;

                        case StdAtom.PSEUDO:
                            handled = true;
                            tb = ob.AddComplexProperty(Properties[propName]);
                            foreach (ZilObject obj in propBody)
                            {
                                var str = obj as ZilString;

                                if (str != null)
                                {
                                    var word = Context.ZEnvironment.GetVocabNoun(ZilAtom.Parse(str.Text, Context), prop.SourceLine);
                                    IWordBuilder wb = Vocabulary[word];
                                    tb.AddShort(wb);
                                }
                                else
                                {
                                    tb.AddShort(CompileConstant(obj));
                                }
                                length += 2;
                            }
                            break;

                        case StdAtom.GLOBAL:
                            if (Context.ZEnvironment.ZVersion == 3)
                            {
                                handled = true;
                                tb = ob.AddComplexProperty(Properties[propName]);
                                foreach (ZilObject obj in propBody)
                                {
                                    var atom = obj as ZilAtom;
                                    if (atom == null)
                                    {
                                        Context.HandleError(new CompilerError(model, CompilerMessages.Values_For_0_Property_Must_Be_1, propName, "atoms"));
                                        break;
                                    }

                                    IObjectBuilder ob2;
                                    if (Objects.TryGetValue(atom, out ob2) == false)
                                    {
                                        Context.HandleError(new CompilerError(model, CompilerMessages.No_Such_Object_0, atom));
                                        break;
                                    }

                                    tb.AddByte(ob2);
                                    length++;
                                }
                            }
                            break;
                    }
                }

                if (!handled)
                {
                    // nothing special, just one or more words
                    pb = Properties[propName];
                    Contract.Assume(pb != null);
                    if (propBody.Rest.IsEmpty)
                    {
                        var word = CompileConstant(value);
                        if (word == null)
                        {
                            Context.HandleError(new CompilerError(
                                prop,
                                CompilerMessages.Nonconstant_Initializer_For_0_1_2,
                                "property",
                                propName,
                                value));
                            word = Game.Zero;
                        }
                        ob.AddWordProperty(pb, word);
                        length = 2;
                    }
                    else
                    {
                        tb = ob.AddComplexProperty(pb);
                        foreach (ZilObject obj in propBody)
                        {
                            var word = CompileConstant(obj);
                            if (word == null)
                            {
                                Context.HandleError(new CompilerError(
                                    prop,
                                    CompilerMessages.Nonconstant_Initializer_For_0_1_2,
                                    "property",
                                    propName,
                                    obj));
                                word = Game.Zero;
                            }
                            tb.AddShort(word);
                            length += 2;
                        }
                    }
                }

                // check property length
                if (length > Game.MaxPropertyLength)
                    Context.HandleError(new CompilerError(
                        prop,
                        CompilerMessages.Property_0_Is_Too_Long_Max_1_Byte1s,
                        propName.ToStringContext(Context, true),
                        Game.MaxPropertyLength));
            }

            //XXX debug line refs for objects
            if (WantDebugInfo)
                Game.DebugFile.MarkObject(ob, new DebugLineRef(), new DebugLineRef());
        }
    }
}
