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
using Zilf.Interpreter.Values;
using Zilf.Language;
using JetBrains.Annotations;
using System.Diagnostics;

namespace Zilf.Interpreter
{
    abstract class Frame : IDisposable
    {
        [NotNull]
        public Context Context { get; }
        public Frame Parent { get; }
        [NotNull]
        public ISourceLine SourceLine { get; }

        public abstract string Description { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [ContractInvariantMethod]
        [Conditional("CONTRACTS_FULL")]
        void ObjectInvariant()
        {
            Contract.Invariant(Context != null);
            Contract.Invariant(SourceLine != null);
        }

        protected Frame([NotNull] Context ctx, [NotNull] ZilForm callingForm)
        {
            Contract.Requires(ctx != null);
            Contract.Requires(callingForm != null);

            Context = ctx;
            Parent = ctx.TopFrame;
            SourceLine = callingForm.SourceLine;
        }

        protected Frame([NotNull] Context ctx, [NotNull] ISourceLine sourceLine)
        {
            Contract.Requires(ctx != null);
            Contract.Requires(sourceLine != null);

            Context = ctx;
            Parent = ctx.TopFrame;
            SourceLine = sourceLine;
        }

        /// <exception cref="InvalidOperationException">This <see cref="Frame"/> was not on top of the stack.</exception>
        public void Dispose()
        {
            if (this == Context.TopFrame)
            {
                Context.PopFrame();
            }
            else
            {
                throw new InvalidOperationException($"{nameof(Frame)} being disposed must be at the top of the stack");
            }
        }
    }
}
