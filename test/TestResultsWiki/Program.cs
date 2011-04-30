#region License, Terms and Author(s)
//
// LINQBridge
// Copyright (c) 2007-9 Atif Aziz, Joseph Albahari. All rights reserved.
//
//  Author(s):
//
//      Dominik Hug, http://www.dominikhug.ch
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the New BSD License, a copy of which should have 
// been delivered along with this distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT 
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
#endregion

namespace TestResultsWiki
{
    #region Imports

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml;

    #endregion

    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                Run(args);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Trace.TraceError(e.ToString());
                return -1;
            }
        }

        private static void Run(string[] args) 
        {
            if (args.Length == 0)
                throw new ApplicationException("Missing NUnit XML output file name argument.");

            const string testCaseXPath = "//test-suite[@name='EnumerableFixture']/results/test-case";

            var reports = (
                    from arg in args 
                    let parts = arg.Split(new[]{'='}, 2) 
                    let path = parts.Last()
                    select new {
                        Title = parts.Length > 1 && !string.IsNullOrEmpty(parts[0])
                                ? parts[0] 
                                : Path.GetFileNameWithoutExtension(path),
                        TestCases = LoadXmlDocument(path).SelectNodes(testCaseXPath).Cast<XmlElement>().ToArray()
                    }
                ).ToArray();

            var baseHeaders = new[]
            {
                "Method under test", 
                "Test condition", 
                "Expected result"
            };

            var headers = reports.Select(r => "*" + r.Title + "*").Concat(baseHeaders).ToArray();
            var testByName = reports.SelectMany(r => r.TestCases)
                                    .GroupBy(test => test.GetAttribute("name").Split('.').Last())
                                    .ToArray();

            if (reports.Length > 1)
            {
                var inconsistencies = testByName.Where(g => g.Count() != reports.Length).ToArray();
                if (inconsistencies.Length > 0)
                {
                    foreach (var test in inconsistencies)
                        Console.Error.WriteLine("Missing results for {0}.", test.Key);
                    throw new ApplicationException("Tests results are inconsistent.");
                }
            }

            const string msdnUrlFormat = @"http://msdn.microsoft.com/en-us/library/system.linq.enumerable.{0}.aspx";

            var suite = from nodes in testByName
                        where nodes.Key.Contains("_")
                        let description = new TestCaseName(nodes.Key)
                        let results = (
                            from node in nodes
                            let success = "true".Equals(node.GetAttribute("success"), StringComparison.OrdinalIgnoreCase)
                            select new
                            {
                                Success = success,
                                Executed = "true".Equals(node.GetAttribute("executed"), StringComparison.OrdinalIgnoreCase),
                                Message = success ? null : node.SelectSingleNode("*/message").InnerText
                            })
                            .ToArray()
                        select new { Description = description, Results = results };

            //
            // Create the table of results, which is an array (rows) of 
            // string array (columns).
            //

            var table = //...
                Enumerable.Repeat(headers, 1) // prepend headers to rows...
                .Concat(
                    from test in suite
                    let info = test.Description
                    select (
                       test.Results
                       .Select(r => r.Executed ? (r.Success ? "PASS" : "*FAIL*") : "-")
                       .Concat(new[] {
                          string.Format(@"[{0} {1}]{2}",
                             /* 0 */ string.Format(msdnUrlFormat, info.MethodName.ToLowerInvariant()),
                             /* 1 */ info.MethodName,
                             /* 2 */ info.Arguments.Any() 
                                     ? "(" + string.Join(", ", info.Arguments.ToArray()) + ")" 
                                     : string.Empty),
                          info.StateUnderTest,
                          info.ExpectedBehavior 
                       }))
                       .ToArray()
                ).ToArray();

            //
            // Calculate the fixed width for each column based on maximum
            // width across corresponding cells.
            //

            var widths = headers.Select((h, i) => table.Max(cols => cols[i].Length))
                                .ToArray();

            //
            // Write out the table in Wiki format where each cell is padded 
            // to the columns width.
            //

            foreach (var row in table)
            {
                Console.WriteLine("|| {0} ||",
                    string.Join(" || ", 
                        row.Select((col, i) => col.PadRight(widths[i])).ToArray()));
            }
        }

        private static XmlDocument LoadXmlDocument(string path)
        {
            var document = new XmlDocument();
            document.Load(path);
            return document;
        }
    }
}
