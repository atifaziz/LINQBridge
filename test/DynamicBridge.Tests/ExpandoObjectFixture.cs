#region License, Terms and Author(s)
//
// DynamicBridge tests
//
// Differential fixture: compiled into DynamicBridge.Tests (net30) and, by link, into
// Dynamic.Tests (net40). Every assertion must hold identically on both.
//
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;

namespace DynamicBridge.Tests
{
    [TestFixture]
    public class ExpandoObjectFixture
    {
        [Test]
        public void SetAndGetMember()
        {
            dynamic expando = new ExpandoObject();
            expando.First = "alan";
            Assert.AreEqual("alan", expando.First);
        }

        [Test]
        public void OverwriteMember()
        {
            dynamic expando = new ExpandoObject();
            expando.Value = 1;
            expando.Value = 2;
            Assert.AreEqual(2, expando.Value);
        }

        [Test]
        public void MissingMemberThrows()
        {
            dynamic expando = new ExpandoObject();
            Assert.Throws<RuntimeBinderException>(delegate { var ignored = expando.Missing; });
        }

        [Test]
        public void BehavesAsADictionary()
        {
            var expando = new ExpandoObject();
            dynamic subject = expando;
            subject.One = 1;
            subject.Two = 2;

            var dictionary = (IDictionary<string, object>)expando;
            Assert.AreEqual(2, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey("One"));
            Assert.AreEqual(1, dictionary["One"]);
        }

        [Test]
        public void DictionaryWritesAreVisibleDynamically()
        {
            var expando = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)expando;
            dictionary["Added"] = "yes";

            dynamic subject = expando;
            Assert.AreEqual("yes", subject.Added);
        }

        [Test]
        public void RemoveThroughTheDictionary()
        {
            var expando = new ExpandoObject();
            dynamic subject = expando;
            subject.Gone = 1;

            var dictionary = (IDictionary<string, object>)expando;
            Assert.IsTrue(dictionary.Remove("Gone"));
            Assert.AreEqual(0, dictionary.Count);
            Assert.Throws<RuntimeBinderException>(delegate { var ignored = subject.Gone; });
        }

        [Test]
        public void EnumeratesInInsertionOrder()
        {
            var expando = new ExpandoObject();
            dynamic subject = expando;
            subject.A = 1;
            subject.B = 2;
            subject.C = 3;

            var keys = new List<string>();
            foreach (var pair in (IEnumerable<KeyValuePair<string, object>>)expando)
                keys.Add(pair.Key);

            Assert.AreEqual(3, keys.Count);
            Assert.AreEqual("A", keys[0]);
            Assert.AreEqual("B", keys[1]);
            Assert.AreEqual("C", keys[2]);
        }

        [Test]
        public void RaisesPropertyChanged()
        {
            var expando = new ExpandoObject();
            var changed = new List<string>();
            ((INotifyPropertyChanged)expando).PropertyChanged +=
                delegate(object sender, PropertyChangedEventArgs e) { changed.Add(e.PropertyName); };

            dynamic subject = expando;
            subject.Watched = 1;

            Assert.AreEqual(1, changed.Count);
            Assert.AreEqual("Watched", changed[0]);
        }

        [Test]
        public void HoldsNull()
        {
            var expando = new ExpandoObject();
            dynamic subject = expando;
            subject.Nothing = null;

            Assert.IsNull(subject.Nothing);
            Assert.IsTrue(((IDictionary<string, object>)expando).ContainsKey("Nothing"));
        }

        [Test]
        public void HoldsADelegateThatCanBeInvoked()
        {
            dynamic expando = new ExpandoObject();
            expando.Twice = new Func<int, int>(value => value * 2);
            Assert.AreEqual(8, expando.Twice(4));
        }

        [Test]
        public void MemberNamesAreCaseSensitive()
        {
            dynamic expando = new ExpandoObject();
            expando.Value = 1;
            Assert.Throws<RuntimeBinderException>(delegate { var ignored = expando.value; });
        }
    }
}
