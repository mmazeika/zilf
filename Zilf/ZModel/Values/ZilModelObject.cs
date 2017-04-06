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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Zilf.Interpreter;
using Zilf.Interpreter.Values;
using Zilf.Language;
using Zilf.Diagnostics;

namespace Zilf.ZModel.Values
{
    [BuiltinType(StdAtom.OBJECT, PrimType.LIST)]
    class ZilModelObject : ZilObject
    {
        readonly ZilAtom name;
        readonly ZilList[] props;
        readonly bool isRoom;

        public ZilModelObject(ZilAtom name, ZilList[] props, bool isRoom)
        {
            this.name = name;
            this.props = props;
            this.isRoom = isRoom;
        }

        [ChtypeMethod]
        public static ZilModelObject FromList(Context ctx, ZilListBase list)
        {
            Contract.Requires(ctx != null);

            if (list.IsEmpty)
                throw new InterpreterError(
                    InterpreterMessages._0_Must_Have_1_Element1s, 
                    "list coerced to OBJECT",
                    new CountableString("at least 1", false));

            if (!(list.First is ZilAtom atom))
                throw new InterpreterError(InterpreterMessages.Element_0_Of_1_Must_Be_2, 1, "list coerced to OBJECT", "an atom");

            if (!list.Rest.All(zo => zo is ZilList))
                throw new InterpreterError(
                    InterpreterMessages._0_In_1_Must_Be_2,
                    "elements after first",
                    "list coerced to OBJECT",
                    "lists");

            // TODO: set isRoom
            return new ZilModelObject(atom, list.Rest.Cast<ZilList>().ToArray(), false);
        }

        public ZilAtom Name
        {
            get { return name; }
        }

        public ZilList[] Properties
        {
            get { return props; }
        }

        public bool IsRoom
        {
            get { return isRoom; }
        }

        public override string ToString()
        {
            return ToString(zo => zo.ToString());
        }

        protected override string ToStringContextImpl(Context ctx, bool friendly)
        {
            return ToString(zo => zo.ToStringContext(ctx, friendly));
        }

        string ToString(Func<ZilObject, string> convert)
        {
            Contract.Requires(convert != null);
            Contract.Ensures(Contract.Result<string>() != null);

            var sb = new StringBuilder("#OBJECT (");
            sb.Append(convert(name));

            foreach (ZilList p in props)
            {
                sb.Append(' ');
                sb.Append(convert(p));
            }

            sb.Append(')');
            return sb.ToString();
        }

        public override StdAtom StdTypeAtom => StdAtom.OBJECT;

        public override PrimType PrimType => PrimType.LIST;

        public override ZilObject GetPrimitive(Context ctx)
        {
            var result = new List<ZilObject>(1 + props.Length);
            result.Add(name);
            result.AddRange(props);
            return new ZilList(result);
        }
    }
}