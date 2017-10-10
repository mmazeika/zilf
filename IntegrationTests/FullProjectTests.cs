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

extern alias JBA;
using DiffLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text.RegularExpressions;
using JBA::JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationTests
{
    [TestClass]
    public class FullProjectTests
    {
        const string ProjectsDirName = "FullTestProjects";
        const string LibraryDirName = "Library";
        const int PerTestTimeoutMilliseconds = 60000;

        static string projectsDir, libraryDir;

        /// <exception cref="IOException">Can't locate projects and library directories</exception>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            projectsDir = libraryDir = null;

            // find a directory containing ProjectsDirName and LibraryDirName
            var dir = Directory.GetCurrentDirectory();

            do
            {
                if (Directory.Exists(Path.Combine(dir, ProjectsDirName)) &&
                    Directory.Exists(Path.Combine(dir, LibraryDirName)))
                {
                    projectsDir = Path.Combine(dir, ProjectsDirName);
                    libraryDir = Path.Combine(dir, LibraryDirName);
                    break;
                }

                dir = Directory.GetParent(dir).FullName;
            } while (dir != Path.GetPathRoot(dir));

            if (projectsDir == null)
                throw new IOException("Can't locate projects and library directories");
        }

        [LinqTunnel]
        [NotNull]
        [UsedImplicitly]
        static IEnumerable<string[]> GetProjects()
        {
            Contract.Ensures(Contract.Result<IEnumerable<string[]>>() != null);
            return from dir in Directory.EnumerateDirectories(projectsDir, "*", SearchOption.AllDirectories)
                   let baseName = Path.GetFileName(dir)
                   let mainZilFile = Path.Combine(dir, baseName + ".zil")
                   where File.Exists(mainZilFile)
                   select new[] { baseName, dir, mainZilFile };
        }

        /// <exception cref="AssertInconclusiveException">Always thrown.</exception>
        [DataTestMethod]
        [DynamicData("GetProjects", DynamicDataSourceType.Method)]
        [Timeout(PerTestTimeoutMilliseconds)]
        public void TestProjects([NotNull] string baseName, [NotNull] string dir, [NotNull] string mainZilFile)
        {
            Contract.Requires(dir != null);
            Contract.Requires(baseName != null);
            Contract.Requires(mainZilFile != null);
            Console.WriteLine("Testing {0}", dir);

            var outputFile = Path.Combine(dir, baseName + ".output.txt");
            var inputFile = Path.Combine(dir, baseName + ".input.txt");

            bool testExecution = File.Exists(outputFile) && File.Exists(inputFile);

            var helper = new FileBasedZlrHelper(mainZilFile,
                new[] { dir, libraryDir }, inputFile)
            {
                WantStatusLine = true
            };

            Assert.IsTrue(helper.Compile(), "Failed to compile");
            Assert.IsTrue(helper.Assemble(), "Failed to assemble");

            if (testExecution)
            {
                var actualOutput = helper.Execute();

                var massagedActual = MassageText(actualOutput);
                var massagedExpected = MassageText(File.ReadAllText(outputFile));
                if (massagedActual != massagedExpected)
                {
                    string[] expectedLines = SplitLines(massagedExpected);
                    string[] actualLines = SplitLines(massagedActual);

                    var diff = Diff.CalculateSections(expectedLines, actualLines);
                    int e = 0, a = 0;
                    foreach (var change in diff)
                    {
                        if (!change.IsMatch)
                        {
                            Console.WriteLine("=== At line {0}, {1} ===", e + 1, a + 1);

                            for (int k = e; k < e + change.LengthInCollection1; k++)
                            {
                                Console.WriteLine("-{0}", expectedLines[k]);
                            }

                            for (int m = a; m < a + change.LengthInCollection2; m++)
                            {
                                Contract.Assume(m >= 0); // prevent spurious "Array access might be below lower bound"
                                Console.WriteLine("+{0}", actualLines[m]);
                            }

                            Console.WriteLine();
                        }

                        e += change.LengthInCollection1;
                        a += change.LengthInCollection2;
                    }

                    Assert.Fail("Expected output not found (diff written to console)");
                }
            }
            else
            {
                Assert.Inconclusive("Expected input and/or output files missing.");
            }
        }

        [NotNull]
        static readonly Regex SerialNumberRegex = new Regex(@"(?<=Serial number )\d{6}", RegexOptions.IgnoreCase);

        [NotNull]
        static readonly Regex ZilfVersionRegex = new Regex(@"ZILF \d+\.\d+ lib \S+");

        [NotNull]
        static string MassageText([NotNull] string text)
        {
            Contract.Requires(text != null);
            Contract.Ensures(Contract.Result<string>() != null);

            text = SerialNumberRegex.Replace(text, "######");
            text = ZilfVersionRegex.Replace(text, "ZILF #.# lib ##");
            return text;
        }

        [NotNull]
        static string[] SplitLines([NotNull] string text)
        {
            Contract.Requires(text != null);
            Contract.Ensures(Contract.Result<string[]>() != null);

            var lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].EndsWith("\r"))
                    lines[i] = lines[i].Substring(0, lines[i].Length - 1);
            }

            return lines;
        }
    }
}
