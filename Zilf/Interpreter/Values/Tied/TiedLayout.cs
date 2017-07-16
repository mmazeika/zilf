﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Zilf.Interpreter.Values.Tied
{
    sealed class TiedLayout
    {
        internal static readonly Dictionary<Type, TiedLayout> Layouts = new Dictionary<Type, TiedLayout>();

        [NotNull]
        public static TiedLayout Create<T>(params Expression<Func<T, ZilObject>>[] elements)
            where T : ZilObject, IStructure
        {
            var properties = from e in elements
                             let m = (MemberExpression)e.Body
                             let pi = (PropertyInfo)m.Member
                             select pi;

            return new TiedLayout(properties.ToArray());
        }

        public TiedLayout(PropertyInfo[] properties, [CanBeNull] PropertyInfo catchAll = null)
        {
            PropertyInfos = properties;
            CatchAllPropertyInfo = catchAll;
        }

        public IReadOnlyList<PropertyInfo> PropertyInfos { get; }
        public PropertyInfo CatchAllPropertyInfo { get; }
        public int MinLength => PropertyInfos.Count;

        [NotNull]
        public TiedLayout WithCatchAll<T>([NotNull] Expression<Func<T, IStructure>> catchAll)
            where T : ZilObject, IStructure
        {
            if (CatchAllPropertyInfo != null)
                throw new InvalidOperationException($"{nameof(CatchAllPropertyInfo)} already set");

            var m = (MemberExpression)catchAll.Body;
            var pi = (PropertyInfo)m.Member;

            return new TiedLayout((PropertyInfo[])PropertyInfos, pi);
        }
    }
}
