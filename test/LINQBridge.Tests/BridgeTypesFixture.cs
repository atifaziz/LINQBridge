#region License, Terms and Author(s)
//
// LINQBridge
// Copyright (c) 2007 Atif Aziz, Joseph Albahari. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it
// under the terms of the New BSD License, a copy of which should have
// been delivered along with this distribution.
//
#endregion

namespace LinqBridge.Tests
{
    #region Imports

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using NUnit.Framework;

    #endregion

    /// <summary>
    /// Exercises the parts of LINQBridge that have no Framework 3.5
    /// counterpart to compare against: the bridge's own helper types, the
    /// pieces backported from later frameworks, and the DataAnnotations
    /// subset.
    /// </summary>
    /// <remarks>
    /// Deliberately NOT linked into LINQ.Tests: these types either do not
    /// exist in System.Core 3.5 (OrderedEnumerable, Key, KeyComparer,
    /// Net20Interlocked), arrived in Framework 4.0
    /// (ReadOnlyCollectionBuilder), or live in a separate assembly
    /// (DataAnnotations).
    /// </remarks>

    [TestFixture]
    public sealed class BridgeTypesFixture
    {
        // ReSharper disable InconsistentNaming

        #region Net20Interlocked

        [Test]
        public void CompareExchange_NullLocationMatchingNullComparand_Exchanges()
        {
            // The lazy resource loaders throughout LINQBridge rely on exactly
            // this call shape, so a null location1 must not throw.
            string location = null;
            var original = Net20Interlocked.CompareExchange(ref location, "value", null);
            Assert.That(original, Is.Null);
            Assert.That(location, Is.EqualTo("value"));
        }

        [Test]
        public void CompareExchange_ReturnsOriginalValueNotNewValue()
        {
            var location = "before";
            var original = Net20Interlocked.CompareExchange(ref location, "after", "before");
            Assert.That(original, Is.EqualTo("before"));
            Assert.That(location, Is.EqualTo("after"));
        }

        [Test]
        public void CompareExchange_ComparandDoesNotMatch_LeavesLocationAlone()
        {
            var location = "before";
            var original = Net20Interlocked.CompareExchange(ref location, "after", "other");
            Assert.That(original, Is.EqualTo("before"));
            Assert.That(location, Is.EqualTo("before"));
        }

        [Test]
        public void CompareExchange_ComparesByReferenceNotEquality()
        {
            var location = new string('a', 3);
            var equalButDistinct = new string('a', 3);
            Assert.That(ReferenceEquals(location, equalButDistinct), Is.False);

            Net20Interlocked.CompareExchange(ref location, "replaced", equalButDistinct);
            Assert.That(location, Is.EqualTo("aaa"));
        }

        #endregion

        #region Resource loading

        [Test]
        public void ExpressionErrors_CarryAMessageFromResources()
        {
            // Regression guard: the expression string resources used to be
            // unreachable, which turned every argument validation failure into
            // a NullReferenceException or MissingManifestResourceException.
            var e = Assert.Throws<ArgumentException>(
                () => System.Linq.Expressions.Expression.Constant(null, typeof(int)));
            Assert.That(string.IsNullOrEmpty(e.Message), Is.False);
            Assert.That(e.Message, Is.Not.StringContaining("MissingManifestResource"));
        }

        [Test]
        public void EnumerableErrors_CarryAMessageFromResources()
        {
            var e = Assert.Throws<InvalidOperationException>(() => new int[0].First());
            Assert.That(string.IsNullOrEmpty(e.Message), Is.False);
        }

        #endregion

        #region StrongBox

        [Test]
        public void StrongBox_HoldsAndExposesValue()
        {
            var box = new StrongBox<int>(5);
            Assert.That(box.Value, Is.EqualTo(5));
            box.Value = 6;
            Assert.That(box.Value, Is.EqualTo(6));
        }

        [Test]
        public void StrongBox_ImplementsIStrongBox()
        {
            IStrongBox box = new StrongBox<int>(7);
            Assert.That(box.Value, Is.EqualTo(7));
            box.Value = 8;
            Assert.That(((StrongBox<int>) box).Value, Is.EqualTo(8));
        }

        [Test]
        public void StrongBox_DefaultValue_IsHeldAsIs()
        {
            // NOTE: unlike System.Core's StrongBox<T>, LINQBridge's has no
            // parameterless constructor, so the default is passed explicitly.
            Assert.That(new StrongBox<int>(default(int)).Value, Is.EqualTo(0));
        }

        #endregion

        #region ReadOnlyCollectionBuilder

        [Test]
        public void Builder_Add_GrowsCollection()
        {
            var builder = new ReadOnlyCollectionBuilder<int> { 1, 2 };
            Assert.That(builder.Count, Is.EqualTo(2));
            Assert.That(builder[0], Is.EqualTo(1));
        }

        [Test]
        public void Builder_FromSequence_CopiesElements()
        {
            var builder = new ReadOnlyCollectionBuilder<int>(new[] { 1, 2, 3 });
            Assert.That(builder.Count, Is.EqualTo(3));
        }

        [Test]
        public void Builder_WithCapacity_StartsEmptyWithThatCapacity()
        {
            var builder = new ReadOnlyCollectionBuilder<int>(8);
            Assert.That(builder.Count, Is.EqualTo(0));
            Assert.That(builder.Capacity, Is.EqualTo(8));
        }

        [Test]
        public void Builder_Insert_PlacesElementAtIndex()
        {
            var builder = new ReadOnlyCollectionBuilder<int> { 1, 3 };
            builder.Insert(1, 2);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, builder.ToArray());
        }

        [Test]
        public void Builder_RemoveAndRemoveAt_DropElements()
        {
            var builder = new ReadOnlyCollectionBuilder<int> { 1, 2, 3 };
            Assert.That(builder.Remove(2), Is.True);
            Assert.That(builder.Remove(9), Is.False);
            builder.RemoveAt(0);
            CollectionAssert.AreEqual(new[] { 3 }, builder.ToArray());
        }

        [Test]
        public void Builder_IndexOfAndContains_LocateElements()
        {
            var builder = new ReadOnlyCollectionBuilder<int> { 1, 2, 3 };
            Assert.That(builder.IndexOf(2), Is.EqualTo(1));
            Assert.That(builder.IndexOf(9), Is.EqualTo(-1));
            Assert.That(builder.Contains(3), Is.True);
        }

        [Test]
        public void Builder_Reverse_ReversesInPlace()
        {
            var builder = new ReadOnlyCollectionBuilder<int> { 1, 2, 3 };
            builder.Reverse();
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, builder.ToArray());
        }

        [Test]
        public void Builder_ReverseRange_ReversesThatRangeOnly()
        {
            var builder = new ReadOnlyCollectionBuilder<int> { 1, 2, 3, 4 };
            builder.Reverse(1, 2);
            CollectionAssert.AreEqual(new[] { 1, 3, 2, 4 }, builder.ToArray());
        }

        [Test]
        public void Builder_Clear_EmptiesCollection()
        {
            var builder = new ReadOnlyCollectionBuilder<int> { 1, 2 };
            builder.Clear();
            Assert.That(builder.Count, Is.EqualTo(0));
        }

        [Test]
        public void Builder_CopyTo_CopiesElements()
        {
            var builder = new ReadOnlyCollectionBuilder<int> { 1, 2 };
            var array = new int[3];
            builder.CopyTo(array, 1);
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, array);
        }

        [Test]
        public void Builder_ToReadOnlyCollection_SnapshotsElements()
        {
            var builder = new ReadOnlyCollectionBuilder<int> { 1, 2 };
            var collection = builder.ToReadOnlyCollection();
            CollectionAssert.AreEqual(new[] { 1, 2 }, collection);
        }

        [Test]
        public void Builder_Indexer_OutOfRange_Throws()
        {
            // ContractUtils.Requires raises a plain ArgumentException here, where
            // Framework 4.0's builder raises ArgumentOutOfRangeException; asserting
            // the base type keeps this true either way.
            var builder = new ReadOnlyCollectionBuilder<int>();
            Assert.Throws<ArgumentException>(() => { var unused = builder[0]; });
        }

        [Test]
        public void Builder_Enumerates()
        {
            var builder = new ReadOnlyCollectionBuilder<int> { 1, 2 };
            var seen = new List<int>();
            foreach (var n in builder)
                seen.Add(n);
            CollectionAssert.AreEqual(new[] { 1, 2 }, seen);
        }

        [Test]
        public void Builder_AsNonGenericList_Works()
        {
            IList list = new ReadOnlyCollectionBuilder<int> { 1 };
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(1));
            Assert.That(list.Contains(1), Is.True);
        }

        #endregion

        #region Key and KeyComparer

        [Test]
        public void Key_WrapsValue()
        {
            Assert.That(new Key<int>(3).Value, Is.EqualTo(3));
            Assert.That(new Key<string>(null).Value, Is.Null);
        }

        [Test]
        public void KeyComparer_DelegatesEqualityToInnerComparer()
        {
            var comparer = new KeyComparer<string>(StringComparer.OrdinalIgnoreCase);
            Assert.That(comparer.Equals(new Key<string>("a"), new Key<string>("A")), Is.True);
            Assert.That(comparer.Equals(new Key<string>("a"), new Key<string>("b")), Is.False);
        }

        [Test]
        public void KeyComparer_DelegatesHashCodeToInnerComparer()
        {
            var comparer = new KeyComparer<string>(StringComparer.OrdinalIgnoreCase);
            Assert.That(comparer.GetHashCode(new Key<string>("a")),
                        Is.EqualTo(comparer.GetHashCode(new Key<string>("A"))));
        }

        [Test]
        public void KeyComparer_HandlesNullValues()
        {
            var comparer = new KeyComparer<string>(EqualityComparer<string>.Default);
            Assert.That(comparer.Equals(new Key<string>(null), new Key<string>(null)), Is.True);
            Assert.That(comparer.Equals(new Key<string>(null), new Key<string>("a")), Is.False);
        }

        [Test]
        public void KeyComparer_SupportsHashSetOfNullableKeys()
        {
            var set = new HashSet<Key<string>>(new KeyComparer<string>(EqualityComparer<string>.Default));
            Assert.That(set.Add(new Key<string>(null)), Is.True);
            Assert.That(set.Add(new Key<string>(null)), Is.False);
            Assert.That(set.Add(new Key<string>("a")), Is.True);
        }

        #endregion

        #region DelegatingComparer

        [Test]
        public void DelegatingComparer_UsesSuppliedDelegate()
        {
            var comparer = new DelegatingComparer<int>((x, y) => y.CompareTo(x));
            Assert.That(comparer.Compare(1, 2), Is.GreaterThan(0));
            Assert.That(comparer.Compare(2, 1), Is.LessThan(0));
            Assert.That(comparer.Compare(1, 1), Is.EqualTo(0));
        }

        [Test]
        public void DelegatingComparer_SortsWithSuppliedOrder()
        {
            var items = new[] { 1, 3, 2 };
            Array.Sort(items, new DelegatingComparer<int>((x, y) => y.CompareTo(x)));
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, items);
        }

        [Test]
        public void DelegatingComparer_NullDelegate_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DelegatingComparer<int>(null));
        }

        #endregion

        #region OrderedEnumerable

        [Test]
        public void OrderedEnumerable_SortsAscending()
        {
            var ordered = new OrderedEnumerable<int, int>(new[] { 3, 1, 2 }, n => n, null, false);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, ordered.ToArray());
        }

        [Test]
        public void OrderedEnumerable_SortsDescending()
        {
            var ordered = new OrderedEnumerable<int, int>(new[] { 3, 1, 2 }, n => n, null, true);
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, ordered.ToArray());
        }

        [Test]
        public void OrderedEnumerable_WithComparer_UsesComparer()
        {
            var ordered = new OrderedEnumerable<int, int>(
                new[] { 1, 3, 2 }, n => n, new DelegatingComparer<int>((x, y) => y.CompareTo(x)), false);
            CollectionAssert.AreEqual(new[] { 3, 2, 1 }, ordered.ToArray());
        }

        [Test]
        public void OrderedEnumerable_IsStable()
        {
            var source = new[] { "bb", "aa", "ab", "ba" };
            var ordered = new OrderedEnumerable<string, char>(source, s => s[0], null, false);
            CollectionAssert.AreEqual(new[] { "aa", "ab", "bb", "ba" }, ordered.ToArray());
        }

        [Test]
        public void OrderedEnumerable_CreateOrderedEnumerable_AppliesSecondaryKey()
        {
            var source = new[] { "bb", "aa", "ab", "ba" };
            var ordered = (IOrderedEnumerable<string>)
                new OrderedEnumerable<string, char>(source, s => s[0], null, false);
            var thenBy = ordered.CreateOrderedEnumerable(s => s[1], null, false);
            CollectionAssert.AreEqual(new[] { "aa", "ab", "ba", "bb" }, thenBy.ToArray());
        }

        [Test]
        public void OrderedEnumerable_EmptySource_YieldsNothing()
        {
            var ordered = new OrderedEnumerable<int, int>(new int[0], n => n, null, false);
            Assert.That(ordered.ToArray().Length, Is.EqualTo(0));
        }

        [Test]
        public void OrderedEnumerable_IsDeferred()
        {
            var source = new List<int> { 3, 1 };
            var ordered = new OrderedEnumerable<int, int>(source, n => n, null, false);
            source.Add(2);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, ordered.ToArray());
        }

        [Test]
        public void OrderedEnumerable_NullSource_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new OrderedEnumerable<int, int>(null, n => n, null, false));
        }

        [Test]
        public void OrderedEnumerable_NullKeySelector_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new OrderedEnumerable<int, int>(new int[0], null, null, false));
        }

        #endregion

        #region Lookup

        [Test]
        public void ToLookup_GroupsBySelectedKey()
        {
            var lookup = new[] { 1, 2, 3, 4 }.ToLookup(n => n % 2);
            Assert.That(lookup.Count, Is.EqualTo(2));
            CollectionAssert.AreEqual(new[] { 1, 3 }, lookup[1].ToArray());
            CollectionAssert.AreEqual(new[] { 2, 4 }, lookup[0].ToArray());
        }

        [Test]
        public void Lookup_UnknownKey_YieldsEmptySequence()
        {
            var lookup = new[] { 1 }.ToLookup(n => n);
            Assert.That(lookup.Contains(9), Is.False);
            Assert.That(lookup[9].Count(), Is.EqualTo(0));
        }

        [Test]
        public void Lookup_EnumeratesGroupings()
        {
            var lookup = new[] { 1, 2 }.ToLookup(n => n % 2);
            var keys = lookup.Select(g => g.Key).ToArray();
            CollectionAssert.AreEquivalent(new[] { 0, 1 }, keys);
        }

        [Test]
        public void ToLookup_WithElementSelector_ProjectsElements()
        {
            var lookup = new[] { 1, 2 }.ToLookup(n => n % 2, n => n * 10);
            CollectionAssert.AreEqual(new[] { 10 }, lookup[1].ToArray());
        }

        #endregion

        #region DataAnnotations

        [Test]
        public void RequiredAttribute_NullValue_IsInvalid()
        {
            Assert.That(new RequiredAttribute().IsValid(null), Is.False);
        }

        [Test]
        public void RequiredAttribute_EmptyString_IsInvalid()
        {
            Assert.That(new RequiredAttribute().IsValid(string.Empty), Is.False);
        }

        [Test]
        public void RequiredAttribute_NonEmptyValue_IsValid()
        {
            Assert.That(new RequiredAttribute().IsValid("x"), Is.True);
            Assert.That(new RequiredAttribute().IsValid(0), Is.True);
        }

        [Test]
        public void Validate_InvalidValue_ThrowsValidationException()
        {
            var attribute = new RequiredAttribute();
            var e = Assert.Throws<ValidationException>(() => attribute.Validate(null, "Name"));
            Assert.That(e.ValidationAttribute, Is.SameAs(attribute));
            Assert.That(e.Value, Is.Null);
        }

        [Test]
        public void Validate_ValidValue_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new RequiredAttribute().Validate("x", "Name"));
        }

        [Test]
        public void FormatErrorMessage_IncludesMemberName()
        {
            var message = new RequiredAttribute().FormatErrorMessage("Name");
            Assert.That(string.IsNullOrEmpty(message), Is.False);
            Assert.That(message, Is.StringContaining("Name"));
        }

        [Test]
        public void ErrorMessage_WhenSet_IsUsedByFormatErrorMessage()
        {
            var attribute = new RequiredAttribute { ErrorMessage = "{0} is mandatory" };
            Assert.That(attribute.FormatErrorMessage("Name"), Is.EqualTo("Name is mandatory"));
        }

        [Test]
        public void ValidationException_Constructors_SetMessageAndInnerException()
        {
            Assert.That(new ValidationException("m").Message, Is.EqualTo("m"));

            var inner = new InvalidOperationException("inner");
            var e = new ValidationException("m", inner);
            Assert.That(e.InnerException, Is.SameAs(inner));
        }

        #endregion
    }
}
