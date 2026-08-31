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
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;

namespace DynamicBridge.Tests
{
    [TestFixture]
    public class DynamicObjectFixture
    {
        [Test]
        public void TrySetMemberThenTryGetMember()
        {
            var bag = new Bag();
            dynamic subject = bag;

            subject.Colour = "red";
            Assert.AreEqual(1, bag.SetMemberCalls);
            Assert.AreEqual("red", subject.Colour);
            Assert.AreEqual(1, bag.GetMemberCalls);
        }

        [Test]
        public void TryGetMemberReturningFalseFallsBackToTheClrType()
        {
            dynamic subject = new Bag();
            Assert.AreEqual("clr-property", subject.Fixed);
        }

        [Test]
        public void TryGetMemberReturningFalseForAnUnknownMemberThrows()
        {
            dynamic subject = new Bag();
            Assert.Throws<RuntimeBinderException>(delegate { var ignored = subject.NotThere; });
        }

        [Test]
        public void TryInvokeMember()
        {
            var bag = new Bag();
            dynamic subject = bag;

            Assert.AreEqual("Anything:2", subject.Anything(1, 2));
            Assert.AreEqual(1, bag.InvokeMemberCalls);
        }

        [Test]
        public void TryInvokeMemberReturningFalseFallsBackToTheClrType()
        {
            dynamic subject = new PassThrough();
            Assert.AreEqual("real1", subject.RealMethod(1));
        }

        [Test]
        public void TryDeleteMemberIsReachedThroughTheDictionary()
        {
            var bag = new Bag();
            dynamic subject = bag;
            subject.Temp = 1;
            Assert.AreEqual(1, bag.Count);
        }

        [Test]
        public void TryGetIndexAndTrySetIndex()
        {
            var bag = new Bag();
            dynamic subject = bag;

            Assert.AreEqual("index:1", subject[0]);
            Assert.AreEqual("index:2", subject[0, 1]);

            subject[9] = "value";
            Assert.AreEqual(1, bag.Count);
        }

        [Test]
        public void TryConvert()
        {
            dynamic subject = new Bag();
            string text = subject;
            Assert.AreEqual("converted", text);
        }

        [Test]
        public void TryConvertReturningFalseThrows()
        {
            dynamic subject = new Bag();
            Assert.Throws<RuntimeBinderException>(delegate { int ignored = subject; });
        }

        [Test]
        public void TryBinaryOperation()
        {
            dynamic subject = new Bag();
            Assert.AreEqual("binary:Add", subject + 1);
            Assert.AreEqual("binary:Multiply", subject * 2);
        }

        [Test]
        public void TryUnaryOperation()
        {
            dynamic subject = new Bag();
            Assert.AreEqual("unary:Negate", -subject);
        }

        [Test]
        public void TryInvoke()
        {
            dynamic subject = new Bag();
            Assert.AreEqual("invoked:2", subject(1, 2));
        }

        [Test]
        public void GetDynamicMemberNames()
        {
            var bag = new Bag();
            dynamic subject = bag;
            subject.One = 1;
            subject.Two = 2;

            var names = new List<string>(bag.GetDynamicMemberNames());
            Assert.AreEqual(2, names.Count);
            Assert.IsTrue(names.Contains("One"));
            Assert.IsTrue(names.Contains("Two"));
        }

        [Test]
        public void BinderNameIsPassedThrough()
        {
            var bag = new Bag();
            dynamic subject = bag;
            subject.SomeSpecificName = 1;
            Assert.AreEqual(1, subject.SomeSpecificName);
        }

        [Test]
        public void StaticallyTypedAccessDoesNotUseTheDynamicProtocol()
        {
            var bag = new Bag();
            Assert.AreEqual("clr-property", bag.Fixed);
            Assert.AreEqual(0, bag.GetMemberCalls);
        }
    }
}
