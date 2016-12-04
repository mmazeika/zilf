﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using TestHelper;
using ZilfErrorMessages;

namespace ZilfErrorMessages.Test
{
    [TestClass]
    public class MessageConstantTests : DiagnosticVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public async Task MessageConstantAnalyzer_NotTriggered()
        {
            var test = @"";

            await VerifyCSharpDiagnosticAsync(test);
        }

        //Diagnostic triggered and checked for
        [TestMethod]
        public async Task MessageConstantAnalyzer_DuplicateMessageCode_Triggered()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Zilf.Diagnostics;
using Zilf.Language;

namespace Zilf.Diagnostics {
    [AttributeUsage(AttributeTargets.Class)]
    class MessageSetAttribute : Attribute {
        public MessageSetAttribute(string s) {}
    }

    [AttributeUsage(AttributeTargets.Field)]
    class MessageAttribute : Attribute {
        public MessageAttribute(string s) {}
    }

    [MessageSet(""foo"")]
    class InterpreterMessages {
        [Message(""foo"")]
        public const int Foo = 1;
        [Message(""bar"")]
        public const int Bar = 1;
        [Message(""bar"")]
        public const int Baz = 2;
        [Message(""quux"")]
        public const int Quux = 2;
    }
}
";
            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = "ZILF0002",
                    Message = string.Format("The code '{0}' is used more than once in message set '{1}'", "1", "InterpreterMessages"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 27, 26) }
                },
                new DiagnosticResult
                {
                    Id = "ZILF0003",
                    Message = string.Format("This format string is used more than once in message set '{0}'", "InterpreterMessages"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 28, 18), new DiagnosticResultLocation("Test0.cs", 26, 18) }
                },
                new DiagnosticResult
                {
                    Id = "ZILF0002",
                    Message = string.Format("The code '{0}' is used more than once in message set '{1}'", "2", "InterpreterMessages"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 31, 26) }
                },
            };

            await VerifyCSharpDiagnosticAsync(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MessageConstantAnalyzer();
        }
    }
}