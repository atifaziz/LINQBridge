#region License, Terms and Author(s)
//
// LINQBridge
// Copyright (c) 2007 Atif Aziz, Joseph Albahari. All rights reserved.
//
//  Author(s):
//
//      Dominik Hug, http://www.dominikhug.ch
//      Atif Aziz, http://www.raboof.com
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

namespace LinqBridge.Tests
{
    #region Imports

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;
    using NUnit.Framework.SyntaxHelpers;

    #endregion

    internal sealed class Reader<T> : IEnumerable<T>, IEnumerator<T>
    {
        public event EventHandler Disposed;
        public event EventHandler Enumerated;

        private IEnumerable<T> source;
        private IEnumerator<T> cursor;

        public Reader(IEnumerable<T> values)
        {
            Debug.Assert(values != null);
            source = values;
        }

        private IEnumerator<T> Enumerator
        {
            get
            {
                if (cursor == null)
                    GetEnumerator();
                return this;
            }
        }

        public object EOF
        {
            get { return Enumerator.MoveNext(); }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (source == null) throw new Exception("A LINQ Operator called GetEnumerator() twice.");
            cursor = source.GetEnumerator();
            source = null;

            var handler = Enumerated;
            if (handler != null)
                handler(this, EventArgs.Empty);

            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T Read()
        {
            if (!Enumerator.MoveNext())
                throw new InvalidOperationException("No more elements in the source sequence.");
            return Enumerator.Current;
        }

        void IDisposable.Dispose()
        {
            source = null;
            var e = cursor;
            cursor = null;

            if (e != null)
            {
                e.Dispose();

                var handler = Disposed;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        private IEnumerator<T> GetSourceEnumerator()
        {
            if (source != null && cursor == null)
                throw new InvalidOperationException(/* GetEnumerator not called yet */);
            if (source == null && cursor == null) 
                throw new ObjectDisposedException(GetType().FullName);

            return cursor;
        }

        bool IEnumerator.MoveNext()
        {
            return GetSourceEnumerator().MoveNext();
        }

        void IEnumerator.Reset()
        {
            GetSourceEnumerator().Reset();
        }

        T IEnumerator<T>.Current
        {
            get { return GetSourceEnumerator().Current; }
        }

        object IEnumerator.Current
        {
            get { return ((IEnumerator<T>) this).Current; }
        }
    }

    internal static class ReaderTestExtensions
    {
        public static void AssertEnded<T>(this Reader<T> reader)
        {
            Debug.Assert(reader != null);

            Assert.That(reader.EOF, Is.False, "Too many elements in source.");
        }

        public static Reader<T> AssertNext<T>(this Reader<T> reader, Constraint constraint)
        {
            Debug.Assert(reader != null);
            Debug.Assert(constraint != null);

            Assert.That(reader.Read(), constraint);
            return reader;
        }
    }
}