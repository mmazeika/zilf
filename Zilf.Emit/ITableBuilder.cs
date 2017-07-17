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

using System.Diagnostics.Contracts;
using JetBrains.Annotations;

namespace Zilf.Emit
{
    [ContractClass(typeof(ITableBuilderContracts))]
    public interface ITableBuilder : IConstantOperand
    {
        void AddByte(byte value);
        void AddByte([NotNull] IOperand value);
        void AddShort(short value);
        void AddShort([NotNull] IOperand value);
    }

    [ContractClassFor(typeof(ITableBuilder))]
    abstract class ITableBuilderContracts : ITableBuilder
    {
        public abstract IConstantOperand Add(IConstantOperand other);

        public void AddByte(byte value)
        {
            throw new System.NotImplementedException();
        }

        public void AddByte(IOperand value)
        {
            Contract.Requires(value != null);
        }

        public void AddShort(short value)
        {
            throw new System.NotImplementedException();
        }

        public void AddShort(IOperand value)
        {
            Contract.Requires(value != null);
        }
    }
}