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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.Contracts;

namespace IntegrationTests
{
    [TestClass]
    public class TellTests
    {
        static RoutineAssertionHelper AssertRoutine(string argSpec, string body)
        {
            Contract.Requires(argSpec != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(body));

            return new RoutineAssertionHelper(argSpec, body);
        }

        [TestMethod]
        public void Tell_Macro_Should_Be_Used_If_Defined()
        {
            AssertRoutine("", "<TELL 21>")
                .WithGlobal("<DEFMAC TELL ('X) <FORM PRINTN <* .X 2>>>")
                .Outputs("42");
            
        }

        [TestMethod]
        public void Tell_Builtin_Should_Support_Basic_Operations()
        {
            AssertRoutine("", "<TELL \"AB\" C 67 CR N 123 CRLF D ,OBJ>")
                .WithGlobal("<OBJECT OBJ (DESC \"obj\")>")
                .Outputs("ABC\n123\nobj");
        }

        [TestMethod]
        public void Tell_Builtin_Should_Support_New_Tokens()
        {
            AssertRoutine("", "<TELL DBL 21 CRLF WUTEVA \"hello\" GLOB WUTEVA 45 CR>")
                .WithGlobal(
                    "<TELL-TOKENS " +
                    "  (CR CRLF)        <CRLF>" +
                    "  DBL *            <PRINT-DBL .X>" +
                    "  WUTEVA *:STRING  <PRINTI .X>" +
                    "  WUTEVA *:FIX     <PRINTN .X>" +
                    "  GLOB             <PRINTN ,GLOB>>")
                .WithGlobal("<ROUTINE PRINT-DBL (X) <PRINTN <* 2 .X>>>")
                .WithGlobal("<GLOBAL GLOB 123>")
                .Outputs("42\nhello12345\n");
        }

        [TestMethod]
        public void Tell_Builtin_Should_Reject_Complex_Outputs()
        {
            AssertRoutine("", "<>")
                .WithGlobal("<TELL-TOKENS DBL * <PRINTN <* 2 .X>>>")
                .DoesNotCompile();
        }

        [TestMethod]
        public void Tell_Builtin_Should_Reject_Mismatched_Captures()
        {
            AssertRoutine("", "<>")
                .WithGlobal("<TELL-TOKENS DBL * <PRINT-DBL>>")
                .DoesNotCompile();

            AssertRoutine("", "<>")
                .WithGlobal("<TELL-TOKENS DBL * <PRINT-DBL .X .Y>>")
                .DoesNotCompile();
        }

        [TestMethod]
        public void Tell_Builtin_Should_Translate_Strings()
        {
            AssertRoutine("", "<TELL \"foo|bar|\nbaz\nquux\">")
                .Outputs("foo\nbar\nbaz quux");
        }

        [TestMethod]
        public void Tell_Builtin_Should_Support_Characters()
        {
            AssertRoutine("", @"<TELL !\A !\B !\C>")
                .Outputs("ABC");
        }

        [TestMethod]
        public void CRLF_CHARACTER_Should_Affect_String_Translation()
        {
            AssertRoutine("", "<TELL \"foo^bar\">")
                .WithGlobal("<SETG CRLF-CHARACTER !\\^>")
                .Outputs("foo\nbar");
        }

        [TestMethod]
        public void Two_Spaces_After_Period_Should_Collapse_By_Default()
        {
            AssertRoutine("", "<TELL \"Hi.  Hi.   Hi.|  Hi!  Hi?  \" CR>")
                .Outputs("Hi. Hi.  Hi.\n Hi!  Hi?  \n");
        }

        [TestMethod]
        public void Two_Spaces_After_Period_Should_Not_Collapse_With_PRESERVE_SPACES()
        {
            AssertRoutine("", "<TELL \"Hi.  Hi.   Hi.|  Hi!  Hi?  \" CR>")
                .WithGlobal("<SETG PRESERVE-SPACES? T>")
                .Outputs("Hi.  Hi.   Hi.\n  Hi!  Hi?  \n");
        }

        [TestMethod]
        public void CHRSET_Should_Affect_Text_Decoding()
        {
            /*     1         2         3 
             * 67890123456789012345678901
             * zyxwvutsrqponmlkjihgfedcba
             * 
             *   z=6   i=23  l=20
             * 1 00110 10111 10100
             */
            AssertRoutine("", "<PRINTB ,MYTEXT>")
                .WithGlobal("<CHRSET 0 \"zyxwvutsrqponmlkjihgfedcba\">")
                .WithGlobal("<CONSTANT MYTEXT <TABLE #2 1001101011110100>>")
                .InV5()
                .Outputs("zil");
        }

        [TestMethod]
        public void CHRSET_Should_Affect_Text_Encoding()
        {
            AssertRoutine("",
                "<PRINT ,MYTEXT> <CRLF> " +
                "<PRINTN <- <GET <* 4 ,MYTEXT> 0> ,ENCODED-TEXT>>")
                .WithGlobal("<CHRSET 0 \"zyxwvutsrqponmlkjihgfedcba\">")
                .WithGlobal("<CONSTANT MYTEXT \"zil\">")
                .WithGlobal("<CONSTANT ENCODED-TEXT #2 1001101011110100>")
                .InV5()
                .Outputs("zil\n0");
        }

        [TestMethod]
        public void LANGUAGE_Should_Affect_Text_Encoding()
        {
            AssertRoutine("",
                "<TELL \"%>M%obeltr%agerf%u%se%<\">")
                .WithGlobal("<LANGUAGE GERMAN>")
                .InV5()
                .Outputs("»Möbelträgerfüße«");
        }

        [TestMethod]
        public void LANGUAGE_Should_Affect_Vocabulary_Encoding()
        {
            AssertRoutine("", @"<PRINTB ,W?\%A\%S>")
                .WithGlobal("<LANGUAGE GERMAN>")
                .WithGlobal(@"<OBJECT FOO (SYNONYM \%A\%S)>")
                .InV5()
                .Outputs("äß");
        }
    }
}
