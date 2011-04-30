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

    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    #endregion

    internal static class CamelCase
    {
        /// <summary>
        /// Parses and yields words from a string of camel-cased words.
        /// For example, <c>"HelloWorld"</c> is parse 
        /// into the sequence { <c>"Hello"</c>, <c>"World"</c> }.
        /// </summary>
        
        public static IEnumerable<string> GetWords(string camelCase)
        {
            if (string.IsNullOrEmpty(camelCase))
                yield break;

            var sb = new StringBuilder();
            
            for (var i = 0; i < camelCase.Length; i++)
            {
                var ch = camelCase[i];
                if (char.IsUpper(ch) && sb.Length > 0)
                {
                    yield return sb.ToString();
                    sb.Length = 0;
                }
                sb.Append(ch);
            }

            if (sb.Length > 0)
                yield return sb.ToString();
        }

        /// <summary>
        /// Creates a sentence out of a camel-cased string of words. 
        /// For example, <c>"TheQuickBrownFox"</c> is converted to 
        /// <c>"The quick brown fox"</c>.
        /// </summary>
        
        public static string FormatSentence(string camelCase)
        {
            if (string.IsNullOrEmpty(camelCase))
                return string.Empty;

            var words = GetWords(camelCase)
                        .Select((word, i) => i > 0 ? word.DecapWord() : word);

            return string.Join(" ", words.ToArray());
        }
    }
}
