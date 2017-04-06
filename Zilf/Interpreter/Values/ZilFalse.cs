/* Copyright 2010-2017 Jesse McGrew
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Zilf.Language;

namespace Zilf.Interpreter.Values
{
    [BuiltinType(StdAtom.FALSE, PrimType.LIST)]
    class ZilFalse : ZilObject, IStructure
    {
        readonly ZilList wrappedList;

        [ChtypeMethod]
        public ZilFalse(ZilList value)
        {
            Contract.Requires(value != null);
            this.wrappedList = value;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(wrappedList != null);
        }

        public override string ToString()
        {
            if (Recursion.TryLock(this))
            {
                try
                {
                    return "#FALSE " + wrappedList;
                }
                finally
                {
                    Recursion.Unlock(this);
                }
            }

            return "#FALSE...";
        }

        public override StdAtom StdTypeAtom => StdAtom.FALSE;

        public override PrimType PrimType => PrimType.LIST;

        public override ZilObject GetPrimitive(Context ctx) => wrappedList;

        public override bool IsTrue => false;

        public override bool Equals(object obj)
        {
            return obj is ZilFalse other && other.wrappedList.Equals(this.wrappedList);
        }

        public override int GetHashCode() => wrappedList.GetHashCode();

        #region IStructure Members

        public ZilObject GetFirst() => wrappedList.First;

        public IStructure GetRest(int skip) => ((IStructure)wrappedList).GetRest(skip);

        public IStructure GetBack(int skip) => throw new NotSupportedException();

        public IStructure GetTop() => throw new NotSupportedException();

        public void Grow(int end, int beginning, ZilObject defaultValue) =>
            throw new NotSupportedException();

        public bool IsEmpty => wrappedList.IsEmpty;

        public ZilObject this[int index]
        {
            get => ((IStructure)wrappedList)[index];
            set => ((IStructure)value)[index] = value;
        }

        public int GetLength() => wrappedList.GetLength();

        public int? GetLength(int limit) => wrappedList.GetLength(limit);

        #endregion

        public IEnumerator<ZilObject> GetEnumerator() => wrappedList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => wrappedList.GetEnumerator();
    }
}