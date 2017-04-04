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
using Zilf.Interpreter.Values;

namespace Zilf.Interpreter
{
    /// <summary>
    /// Represents a set of LVAL bindings, e.g. those belonging to a PROG or FUNCTION invocation,
    /// with optional dynamic inheritance from a parent environment.
    /// </summary>
    class LocalEnvironment : IDisposable
    {
        readonly Context ctx;
        readonly Dictionary<ZilAtom, Binding> bindings = new Dictionary<ZilAtom, Binding>();

        /// <summary>
        /// Creates a new environment with no bindings.
        /// </summary>
        public LocalEnvironment(Context ctx)
            : this(ctx, null)
        {
        }

        /// <summary>
        /// Creates a new environment, optionally inheriting bindings from a parent environment.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="parent">The parent environment, or <b>null</b> to not inherit any bindings.</param>
        /// <remarks>Changes made to bindings in the parent environment will be visible in the new environment,
        /// unless overridden by bindings created in the new environment with <see cref="Rebind"/>.</remarks>
        public LocalEnvironment(Context ctx, LocalEnvironment parent)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));

            this.ctx = ctx;
            this.Parent = parent;
        }

        /// <summary>
        /// Pops the environment from the context.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (this == ctx.LocalEnvironment)
            {
                ctx.PopEnvironment();
            }
            else
            {
                throw new InvalidOperationException("LocalEnvironment being disposed must be at the top of the stack");
            }
        }

        /// <summary>
        /// Gets the parent environment, or <b>null</b> if the environment was created without inheritance.
        /// </summary>
        public LocalEnvironment Parent { get; }

        Binding MaybeGetBinding(ZilAtom atom)
        {
            Contract.Requires(atom != null);

            if (bindings.ContainsKey(atom))
                return bindings[atom];

            if (Parent != null)
                return Parent.MaybeGetBinding(atom);

            return null;
        }

        Binding GetOrCreateBinding(ZilAtom atom)
        {
            Contract.Requires(atom != null);
            Contract.Ensures(Contract.Result<Binding>() != null);

            var result = MaybeGetBinding(atom);

            if (result == null)
            {
                result = new Binding(null);
                bindings.Add(atom, result);
            }

            return result;
        }

        /// <summary>
        /// Indicates whether an atom is bound in this or any parent environment.
        /// </summary>
        /// <param name="atom">The atom.</param>
        /// <returns><b>true</b> if the atom is bound in this environment or any parent environment,
        /// or <b>false</b> if the atom is unbound.</returns>
        public bool IsLocalBound(ZilAtom atom)
        {
            return MaybeGetBinding(atom) != null;
        }

        /// <summary>
        /// Gets the local value of an atom visible in this environment, or null.
        /// </summary>
        /// <param name="atom">The atom.</param>
        /// <returns>The value assigned to the atom in this environment, or the nearest parent environment
        /// in which it was bound, or <b>null</b> if the atom is unbound or unassigned.</returns>
        public ZilObject GetLocalVal(ZilAtom atom)
        {
            return MaybeGetBinding(atom)?.Value;
        }

        /// <summary>
        /// Sets the local value of an atom visible in this environment.
        /// </summary>
        /// <param name="atom">The atom.</param>
        /// <param name="value">The new value, or <b>null</b> to unassign the value.</param>
        /// <remarks>If the atom is bound, the value will be assigned using that binding, which may
        /// exist in a parent environment. If the atom is unbound, a new binding will be created
        /// in this environment.</remarks>
        /// <exception cref="Zilf.Language.DeclCheckError"><paramref name="value"/> does not
        /// match the existing DECL for <paramref name="atom"/>.</exception>
        public void SetLocalVal(ZilAtom atom, ZilObject value)
        {
            var binding = GetOrCreateBinding(atom);
            ctx.MaybeCheckDecl(value, binding.Decl, "LVAL of {0}", atom);
            binding.Value = value;
        }

        /// <summary>
        /// Creates a binding for an atom in this environment, or changes the assigned value if
        /// it's already bound.
        /// </summary>
        /// <param name="atom">The atom.</param>
        /// <param name="value">The new value, or <b>null</b> to unassign the value.</param>
        /// <param name="decl">The new DECL, or <b>null</b> to leave it unchanged.</param>
        /// <remarks>
        /// <para>If the atom is bound in a parent environment, this will create a new binding
        /// that shadows the inherited one; the parent's binding will not be changed.
        /// If the atom is bound in this environment, that binding will be changed, and the
        /// previously assigned value will be overwritten.</para>
        /// <para>This method does not check <paramref name="value"/> against any DECL.</para>
        /// </remarks>
        public void Rebind(ZilAtom atom, ZilObject value = null, ZilObject decl = null)
        {
            if (bindings.TryGetValue(atom, out var binding))
            {
                binding.Value = value;
            }
            else
            {
                binding = new Binding(value);
                bindings.Add(atom, binding);
            }

            if (decl != null)
                binding.Decl = decl;
        }
    }
}
