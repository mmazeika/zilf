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
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zilf.Interpreter;
using Zilf.Interpreter.Values;
using Zilf.Language;
using System.Linq;

namespace ZilfTests.Interpreter
{
    [TestClass]
    public class ArgSpecTests
    {
        [TestMethod]
        public void ActivationAtom_Becomes_NAME_Argument_In_ZilListBody()
        {
            var ctx = new Context();

            var spec = new ArgSpec(ZilAtom.Parse("FOO", ctx), ZilAtom.Parse("ACT", ctx), new ZilObject[0]);

            CollectionAssert.AreEqual(
                new ZilObject[]
                {
                    new ZilString("NAME"),
                    ZilAtom.Parse("ACT", ctx),
                },
                spec.AsZilListBody().ToArray());
        }

        [TestMethod]
        public void ARGS_Is_Included_In_ZilListBody()
        {
            var ctx = new Context();

            var spec = new ArgSpec(ZilAtom.Parse("FOO", ctx), null, new ZilObject[] { new ZilString("ARGS"), ZilAtom.Parse("A", ctx) });

            CollectionAssert.AreEqual(
                new ZilObject[]
                {
                    new ZilString("ARGS"),
                    ZilAtom.Parse("A", ctx),
                },
                spec.AsZilListBody().ToArray());
        }

        [TestMethod]
        public void TUPLE_Is_Included_In_ZilListBody()
        {
            var ctx = new Context();

            var spec = new ArgSpec(ZilAtom.Parse("FOO", ctx), null, new ZilObject[] { new ZilString("TUPLE"), ZilAtom.Parse("A", ctx) });

            CollectionAssert.AreEqual(
                new ZilObject[]
                {
                    new ZilString("TUPLE"),
                    ZilAtom.Parse("A", ctx),
                },
                spec.AsZilListBody().ToArray());
        }

        [TestMethod]
        public void ARGS_And_TUPLE_Can_Be_ADECLs()
        {
            var ctx = new Context();

            var spec1 = new ArgSpec(ZilAtom.Parse("FOO", ctx), null, new ZilObject[]
            {
                new ZilString("ARGS"),
                new ZilAdecl(
                    ZilAtom.Parse("A", ctx),
                    ctx.GetStdAtom(StdAtom.LIST)),
            });

            var spec2 = new ArgSpec(ZilAtom.Parse("FOO", ctx), null, new ZilObject[]
            {
                new ZilString("TUPLE"),
                new ZilAdecl(
                    ZilAtom.Parse("A", ctx),
                    ctx.GetStdAtom(StdAtom.LIST)),
            });
        }

        [TestMethod]
        public void AsZilListBody_Returns_ADECLs_For_Arguments_With_DECLs()
        {
            var ctx = new Context();

            var spec = new ArgSpec(ZilAtom.Parse("FOO", ctx), null, new ZilObject[]
            {
                new ZilAdecl(
                    ZilAtom.Parse("A1", ctx),
                    ctx.GetStdAtom(StdAtom.FIX)),
                new ZilAdecl(
                    new ZilForm(new ZilObject[] {
                        ctx.GetStdAtom(StdAtom.QUOTE),
                        ZilAtom.Parse("A2", ctx),
                    }),
                    ctx.GetStdAtom(StdAtom.FORM)),
                new ZilString("TUPLE"),
                new ZilAdecl(
                    ZilAtom.Parse("A3", ctx),
                    ctx.GetStdAtom(StdAtom.LIST)),
            });

            CollectionAssert.AreEqual(
                new ZilObject[] {
                    new ZilAdecl(
                        ZilAtom.Parse("A1", ctx),
                        ctx.GetStdAtom(StdAtom.FIX)),
                    new ZilAdecl(
                        new ZilForm(new ZilObject[] {
                            ctx.GetStdAtom(StdAtom.QUOTE),
                            ZilAtom.Parse("A2", ctx),
                        }),
                        ctx.GetStdAtom(StdAtom.FORM)),
                    new ZilString("TUPLE"),
                    new ZilAdecl(
                        ZilAtom.Parse("A3", ctx),
                        ctx.GetStdAtom(StdAtom.LIST)),
                },
                spec.AsZilListBody().ToArray());
        }


        [TestMethod]
        public void VALUE_Is_Included_In_ZilListBody()
        {
            var ctx = new Context();

            var spec = new ArgSpec(ZilAtom.Parse("FOO", ctx), null, new ZilObject[]
            {
                new ZilString("AUX"),
                ZilAtom.Parse("X", ctx),
                new ZilString("NAME"),
                ZilAtom.Parse("N", ctx),
                new ZilString("VALUE"),
                new ZilForm(new ZilObject[]
                {
                    ctx.GetStdAtom(StdAtom.OR),
                    ctx.GetStdAtom(StdAtom.FIX),
                    ctx.GetStdAtom(StdAtom.FALSE),
                }),
            });

            CollectionAssert.AreEqual(
                new ZilObject[]
                {
                    new ZilString("AUX"),
                    ZilAtom.Parse("X", ctx),
                    new ZilString("NAME"),
                    ZilAtom.Parse("N", ctx),
                    new ZilString("VALUE"),
                    new ZilForm(new ZilObject[]
                    {
                        ctx.GetStdAtom(StdAtom.OR),
                        ctx.GetStdAtom(StdAtom.FIX),
                        ctx.GetStdAtom(StdAtom.FALSE),
                    }),
                },
                spec.AsZilListBody().ToArray());
        }
    }
}
