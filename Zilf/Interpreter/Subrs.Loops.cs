﻿/* Copyright 2010, 2015 Jesse McGrew
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
using System.Runtime.InteropServices;
using Zilf.Interpreter.Values;
using Zilf.Language;

namespace Zilf.Interpreter
{
    static partial class Subrs
    {
        public static class BindingParams
        {
            [ZilStructuredParam(StdAtom.LIST)]
            public struct BindingList
            {
                public Binding[] Bindings;
            }

            [ZilSequenceParam]
            public struct Binding
            {
                [Either(typeof(BindingName), typeof(BindingWithInitializer))]
                public object Content;

                public ZilAtom Atom
                {
                    get
                    {
                        if (Content is BindingName)
                            return ((BindingName)Content).Atom;

                        return ((BindingWithInitializer)Content).Name.Atom;
                    }
                }

                public ZilObject Initializer
                {
                    get
                    {
                        if (Content is BindingWithInitializer)
                            return ((BindingWithInitializer)Content).Initializer;

                        return null;
                    }
                }
            }

            [ZilSequenceParam]
            public struct BindingName
            {
                [Either(typeof(ZilAtom), typeof(BindingAdecl))]
                public object Content;

                public ZilAtom Atom
                {
                    get
                    {
                        var atom = Content as ZilAtom;
                        if (atom != null)
                            return atom;

                        return ((BindingAdecl)Content).Atom;
                    }
                }
            }

            [ZilStructuredParam(StdAtom.ADECL)]
            public struct BindingAdecl
            {
                public ZilAtom Atom;
                public ZilObject Decl;
            }

            [ZilStructuredParam(StdAtom.LIST)]
            public struct BindingWithInitializer
            {
                public BindingName Name;
                public ZilObject Initializer;
            }
        }

        [FSubr]
        public static ZilObject PROG(Context ctx,
            [Optional] ZilAtom activationAtom,
            BindingParams.BindingList bindings,
            [Required] ZilObject[] body)
        {
            SubrContracts(ctx);

            return PerformProg(ctx, activationAtom, bindings, body, "PROG", false, true);
        }

        [FSubr]
        public static ZilObject REPEAT(Context ctx,
            [Optional] ZilAtom activationAtom,
            BindingParams.BindingList bindings,
            [Required] ZilObject[] body)
        {
            SubrContracts(ctx);

            return PerformProg(ctx, activationAtom, bindings, body, "REPEAT", true, true);
        }

        [FSubr]
        public static ZilObject BIND(Context ctx,
            [Optional] ZilAtom activationAtom,
            BindingParams.BindingList bindings,
            [Required] ZilObject[] body)
        {
            SubrContracts(ctx);

            return PerformProg(ctx, activationAtom, bindings, body, "BIND", false, false);
        }

        private static ZilObject PerformProg(Context ctx, ZilAtom activationAtom,
            BindingParams.BindingList bindings, ZilObject[] body, string name, bool repeat, bool catchy)
        {
            SubrContracts(ctx);
            Contract.Requires(name != null);

            var activation = new ZilActivation(ctx.GetStdAtom(StdAtom.PROG));

            // bind atoms
            Queue<ZilAtom> boundAtoms = new Queue<ZilAtom>();

            try
            {
                if (activationAtom != null)
                {
                    ctx.PushLocalVal(activationAtom, activation);
                    boundAtoms.Enqueue(activationAtom);
                }

                foreach (var b in bindings.Bindings)
                {
                    var atom = b.Atom;
                    var value = b.Initializer?.Eval(ctx);

                    ctx.PushLocalVal(atom, value);
                    boundAtoms.Enqueue(atom);
                }

                if (catchy)
                    ctx.PushEnclosingProgActivation(activation);

                // evaluate body
                ZilObject result = null;
                bool again;
                do
                {
                    again = false;
                    foreach (var expr in body)
                    {
                        try
                        {
                            result = expr.Eval(ctx);
                        }
                        catch (ReturnException ex) when (ex.Activation == activation)
                        {
                            return ex.Value;
                        }
                        catch (AgainException ex) when (ex.Activation == activation)
                        {
                            again = true;
                        }
                    }
                } while (repeat || again);

                Contract.Assert(result != null);

                return result;
            }
            finally
            {
                while (boundAtoms.Count > 0)
                    ctx.PopLocalVal(boundAtoms.Dequeue());

                if (activationAtom != null)
                    ctx.PopLocalVal(activationAtom);

                if (catchy)
                    ctx.PopEnclosingProgActivation();
            }
        }

        [Subr]
        public static ZilObject RETURN(Context ctx, ZilObject value = null, ZilActivation activation = null)
        {
            SubrContracts(ctx);

            if (value == null) {
                value = ctx.TRUE;
            }

            if (activation == null)
            {
                activation = ctx.GetEnclosingProgActivation();
                if (activation == null)
                    throw new InterpreterError("RETURN: no enclosing PROG/REPEAT");
            }

            throw new ReturnException(activation, value);
        }

        [Subr]
        public static ZilObject AGAIN(Context ctx, ZilActivation activation = null)
        {
            SubrContracts(ctx);

            if (activation == null)
            {
                activation = ctx.GetEnclosingProgActivation();
                if (activation == null)
                    throw new InterpreterError("AGAIN: no enclosing PROG/REPEAT");
            }

            throw new AgainException(activation);
        }
    }
}
