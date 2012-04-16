#region License, Terms and Author(s)
//
// LINQBridge
// Copyright (c) 2007 Atif Aziz, Joseph Albahari. All rights reserved.
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    #endregion

    /// <summary>
    /// Helps to extract method name, parameters, test condition and expectation.
    /// </summary>

    [ Serializable ]
    internal sealed class TestCaseName
    {
        private static readonly IList<string> zeroArguments = new string[0];

        /// <param name="name">
        /// String like <c>"Distinct_ComparerArg_NonDistinctValues_ReturnsOnlyDistinctValues"</c>.</param>

        public TestCaseName(string name)
        {
            var parts = new Queue<string>(name.Split(new[] {'_'}, 4, StringSplitOptions.RemoveEmptyEntries));

            if (parts.Count < 2)
                throw new ArgumentException(null, "name");

            MethodName = parts.Dequeue();

            Arguments = parts.Count < 3 
                      ? zeroArguments 
                      : new ReadOnlyCollection<string>(
                            parts.Dequeue()
                                 .Split(new[] {"Arg"}, StringSplitOptions.RemoveEmptyEntries)
                                 .TakeWhile(s => s.Length > 0)
                                 .ToArray());

            StateUnderTest = parts.Count > 1 
                           ? CamelCase.FormatSentence(parts.Dequeue()) 
                           : string.Empty;

            ExpectedBehavior = parts.Dequeue();

            var throwsWord = "Throws";
            if (ExpectedBehavior.StartsWith(throwsWord, StringComparison.InvariantCultureIgnoreCase)
                && ExpectedBehavior.EndsWith("Exception", StringComparison.InvariantCultureIgnoreCase))
            {
                ExpectedBehavior = ExpectedBehavior.Substring(0, throwsWord.Length) 
                                 + " " + ExpectedBehavior.Substring(throwsWord.Length);
            }
            else
            {
                ExpectedBehavior = CamelCase.FormatSentence(ExpectedBehavior);
            }
        }

        public string MethodName { get; private set; }
        public IList<string> Arguments { get; private set; }
        public string StateUnderTest { get; private set; }
        public string ExpectedBehavior { get; private set; }
    }
}
