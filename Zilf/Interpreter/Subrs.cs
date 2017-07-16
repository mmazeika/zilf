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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Zilf.Interpreter.Values;
using JetBrains.Annotations;

namespace Zilf.Interpreter
{
    delegate ZilResult SubrDelegate(string name, Context ctx, ZilObject[] args);

    static partial class Subrs
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        [MeansImplicitUse]
        public class SubrAttribute : Attribute
        {
            public SubrAttribute()
            {
            }

            public SubrAttribute(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public sealed class FSubrAttribute : SubrAttribute
        {
            public FSubrAttribute()
            {
            }

            public FSubrAttribute(string name)
                : base(name)
            {
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        public sealed class MdlZilRedirectAttribute : Attribute
        {
            public MdlZilRedirectAttribute([NotNull] Type type, [NotNull] string target)
            {
                Contract.Requires(type != null);
                Contract.Requires(!string.IsNullOrWhiteSpace(target));

                Type = type;
                Target = target;
            }

            [NotNull]
            public Type Type { get; }

            [NotNull]
            public string Target { get; }

            public bool TopLevelOnly { get; set; }
        }

#pragma warning disable ContracsReSharperInterop_NotNullForContract // Element with not-null contract does not have a corresponding [NotNull] attribute.
        // ReSharper disable UnusedParameter.Local
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "ctx")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "args")]
        [ContractAbbreviator]
        [Conditional("CONTRACTS_FULL")]
        static void SubrContracts([NotNull] Context ctx, [ItemNotNull] [NotNull] ZilObject[] args)
        {
            Contract.Requires(ctx != null);
            Contract.Requires(args != null);
            //Contract.Requires(args.Length == 0 || Contract.ForAll(args, a => a != null));
            Contract.Ensures(Contract.Result<ZilObject>() != null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "ctx")]
        [ContractAbbreviator]
        [Conditional("CONTRACTS_FULL")]
        static void SubrContracts([NotNull] Context ctx)
        {
            Contract.Requires(ctx != null);
            Contract.Ensures(Contract.Result<ZilObject>() != null);
        }
        // ReSharper restore UnusedParameter.Local
#pragma warning restore ContracsReSharperInterop_NotNullForContract // Element with not-null contract does not have a corresponding [NotNull] attribute.
    }
}
