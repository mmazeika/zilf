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

namespace Zilf.ZModel
{
    /// <summary>
    /// Specifies the order of links in the object tree.
    /// </summary>
    /// <remarks>
    /// These values are named with regard to how the game will traverse the objects (following FIRST? and NEXT?).
    /// The compiler processes them in the opposite order when inserting them into the tree.
    /// </remarks>
    enum TreeOrdering
    {
        /// <summary>
        /// Reverse definition order, except for the first defined child of each parent, which remains the first child linked.
        /// </summary>
        Default,
        /// <summary>
        /// Reverse definition order.
        /// </summary>
        ReverseDefined
    }
}