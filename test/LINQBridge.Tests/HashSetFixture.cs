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
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    #endregion

    /// <summary>
    /// Exercises <see cref="HashSet{T}"/>, the Framework 3.5 collection
    /// LINQBridge backports for CLR 2.0.
    /// </summary>
    /// <remarks>
    /// Compiled into both LINQBridge.Tests and LINQ.Tests, so it must stay
    /// within the Framework 3.5 API surface.
    /// </remarks>

    [TestFixture]
    public sealed class HashSetFixture
    {
        // ReSharper disable InconsistentNaming

        #region Construction

        [Test]
        public void Ctor_Default_IsEmpty()
        {
            Assert.That(new HashSet<int>().Count, Is.EqualTo(0));
        }

        [Test]
        public void Ctor_FromSequence_DeduplicatesElements()
        {
            var set = new HashSet<int>(new[] { 1, 2, 2, 3 });
            Assert.That(set.Count, Is.EqualTo(3));
        }

        [Test]
        public void Ctor_WithComparer_UsesComparer()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            set.Add("a");
            Assert.That(set.Add("A"), Is.False);
            Assert.That(set.Count, Is.EqualTo(1));
            Assert.That(set.Comparer, Is.SameAs(StringComparer.OrdinalIgnoreCase));
        }

        [Test]
        public void Ctor_FromSequenceWithComparer_DeduplicatesUsingComparer()
        {
            var set = new HashSet<string>(new[] { "a", "A", "b" }, StringComparer.OrdinalIgnoreCase);
            Assert.That(set.Count, Is.EqualTo(2));
        }

        [Test]
        public void Ctor_NullSequence_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new HashSet<int>((IEnumerable<int>) null));
        }

        [Test]
        public void Comparer_Default_IsDefaultEqualityComparer()
        {
            Assert.That(new HashSet<int>().Comparer, Is.EqualTo(EqualityComparer<int>.Default));
        }

        #endregion

        #region Add, Remove, Contains

        [Test]
        public void Add_NewElement_ReturnsTrueAndGrows()
        {
            var set = new HashSet<int>();
            Assert.That(set.Add(1), Is.True);
            Assert.That(set.Count, Is.EqualTo(1));
        }

        [Test]
        public void Add_DuplicateElement_ReturnsFalseAndDoesNotGrow()
        {
            var set = new HashSet<int> { 1 };
            Assert.That(set.Add(1), Is.False);
            Assert.That(set.Count, Is.EqualTo(1));
        }

        [Test]
        public void Add_Null_IsSupportedForReferenceTypes()
        {
            var set = new HashSet<string>();
            Assert.That(set.Add(null), Is.True);
            Assert.That(set.Contains(null), Is.True);
            Assert.That(set.Add(null), Is.False);
        }

        [Test]
        public void Contains_PresentElement_IsTrue()
        {
            var set = new HashSet<int>(new[] { 1, 2 });
            Assert.That(set.Contains(2), Is.True);
            Assert.That(set.Contains(3), Is.False);
        }

        [Test]
        public void Remove_PresentElement_ReturnsTrueAndShrinks()
        {
            var set = new HashSet<int>(new[] { 1, 2 });
            Assert.That(set.Remove(1), Is.True);
            Assert.That(set.Count, Is.EqualTo(1));
            Assert.That(set.Contains(1), Is.False);
        }

        [Test]
        public void Remove_AbsentElement_ReturnsFalse()
        {
            Assert.That(new HashSet<int>(new[] { 1 }).Remove(9), Is.False);
        }

        [Test]
        public void Remove_ThenAdd_ReusesFreedSlot()
        {
            var set = new HashSet<int>(new[] { 1, 2, 3 });
            set.Remove(2);
            set.Add(4);
            Assert.That(set.Count, Is.EqualTo(3));
            CollectionAssert.AreEquivalent(new[] { 1, 3, 4 }, set.ToArray());
        }

        [Test]
        public void Clear_RemovesEverything()
        {
            var set = new HashSet<int>(new[] { 1, 2, 3 });
            set.Clear();
            Assert.That(set.Count, Is.EqualTo(0));
            Assert.That(set.Contains(1), Is.False);
        }

        [Test]
        public void RemoveWhere_RemovesMatchingElementsAndReturnsCount()
        {
            var set = new HashSet<int>(new[] { 1, 2, 3, 4 });
            Assert.That(set.RemoveWhere(n => n % 2 == 0), Is.EqualTo(2));
            CollectionAssert.AreEquivalent(new[] { 1, 3 }, set.ToArray());
        }

        [Test]
        public void RemoveWhere_NullPredicate_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new HashSet<int>().RemoveWhere(null));
        }

        #endregion

        #region Set operations

        [Test]
        public void UnionWith_AddsMissingElements()
        {
            var set = new HashSet<int>(new[] { 1, 2 });
            set.UnionWith(new[] { 2, 3 });
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, set.ToArray());
        }

        [Test]
        public void IntersectWith_KeepsCommonElements()
        {
            var set = new HashSet<int>(new[] { 1, 2, 3 });
            set.IntersectWith(new[] { 2, 3, 4 });
            CollectionAssert.AreEquivalent(new[] { 2, 3 }, set.ToArray());
        }

        [Test]
        public void IntersectWith_EmptyOther_EmptiesSet()
        {
            var set = new HashSet<int>(new[] { 1, 2 });
            set.IntersectWith(new int[0]);
            Assert.That(set.Count, Is.EqualTo(0));
        }

        [Test]
        public void ExceptWith_RemovesOtherElements()
        {
            var set = new HashSet<int>(new[] { 1, 2, 3 });
            set.ExceptWith(new[] { 2 });
            CollectionAssert.AreEquivalent(new[] { 1, 3 }, set.ToArray());
        }

        [Test]
        public void ExceptWith_Self_EmptiesSet()
        {
            var set = new HashSet<int>(new[] { 1, 2 });
            set.ExceptWith(set);
            Assert.That(set.Count, Is.EqualTo(0));
        }

        [Test]
        public void SymmetricExceptWith_KeepsElementsInExactlyOneSet()
        {
            var set = new HashSet<int>(new[] { 1, 2, 3 });
            set.SymmetricExceptWith(new[] { 3, 4 });
            CollectionAssert.AreEquivalent(new[] { 1, 2, 4 }, set.ToArray());
        }

        [Test]
        public void SetOperations_NullOther_ThrowArgumentNullException()
        {
            var set = new HashSet<int>();
            Assert.Throws<ArgumentNullException>(() => set.UnionWith(null));
            Assert.Throws<ArgumentNullException>(() => set.IntersectWith(null));
            Assert.Throws<ArgumentNullException>(() => set.ExceptWith(null));
            Assert.Throws<ArgumentNullException>(() => set.SymmetricExceptWith(null));
        }

        #endregion

        #region Set predicates

        [Test]
        public void IsSubsetOf_SubsetAndEqualSet_AreTrue()
        {
            var set = new HashSet<int>(new[] { 1, 2 });
            Assert.That(set.IsSubsetOf(new[] { 1, 2, 3 }), Is.True);
            Assert.That(set.IsSubsetOf(new[] { 1, 2 }), Is.True);
            Assert.That(set.IsSubsetOf(new[] { 1 }), Is.False);
        }

        [Test]
        public void IsProperSubsetOf_ExcludesEqualSet()
        {
            var set = new HashSet<int>(new[] { 1, 2 });
            Assert.That(set.IsProperSubsetOf(new[] { 1, 2, 3 }), Is.True);
            Assert.That(set.IsProperSubsetOf(new[] { 1, 2 }), Is.False);
        }

        [Test]
        public void IsSupersetOf_SupersetAndEqualSet_AreTrue()
        {
            var set = new HashSet<int>(new[] { 1, 2, 3 });
            Assert.That(set.IsSupersetOf(new[] { 1, 2 }), Is.True);
            Assert.That(set.IsSupersetOf(new[] { 1, 2, 3 }), Is.True);
            Assert.That(set.IsSupersetOf(new[] { 1, 4 }), Is.False);
        }

        [Test]
        public void IsProperSupersetOf_ExcludesEqualSet()
        {
            var set = new HashSet<int>(new[] { 1, 2, 3 });
            Assert.That(set.IsProperSupersetOf(new[] { 1, 2 }), Is.True);
            Assert.That(set.IsProperSupersetOf(new[] { 1, 2, 3 }), Is.False);
        }

        [Test]
        public void EmptySet_IsSubsetOfEverythingAndProperSubsetOfNonEmpty()
        {
            var empty = new HashSet<int>();
            Assert.That(empty.IsSubsetOf(new int[0]), Is.True);
            Assert.That(empty.IsProperSubsetOf(new[] { 1 }), Is.True);
            Assert.That(empty.IsProperSubsetOf(new int[0]), Is.False);
        }

        [Test]
        public void Overlaps_SharedElement_IsTrue()
        {
            var set = new HashSet<int>(new[] { 1, 2 });
            Assert.That(set.Overlaps(new[] { 2, 3 }), Is.True);
            Assert.That(set.Overlaps(new[] { 3, 4 }), Is.False);
        }

        [Test]
        public void SetEquals_SameElementsInAnyOrder_IsTrue()
        {
            var set = new HashSet<int>(new[] { 1, 2, 3 });
            Assert.That(set.SetEquals(new[] { 3, 2, 1 }), Is.True);
            Assert.That(set.SetEquals(new[] { 1, 2 }), Is.False);
        }

        [Test]
        public void SetEquals_IgnoresDuplicatesInOther()
        {
            Assert.That(new HashSet<int>(new[] { 1, 2 }).SetEquals(new[] { 1, 1, 2 }), Is.True);
        }

        [Test]
        public void SetPredicates_NullOther_ThrowArgumentNullException()
        {
            var set = new HashSet<int>();
            Assert.Throws<ArgumentNullException>(() => set.IsSubsetOf(null));
            Assert.Throws<ArgumentNullException>(() => set.IsSupersetOf(null));
            Assert.Throws<ArgumentNullException>(() => set.Overlaps(null));
            Assert.Throws<ArgumentNullException>(() => set.SetEquals(null));
        }

        #endregion

        #region Copying, enumeration and comparer

        [Test]
        public void CopyTo_Array_CopiesAllElements()
        {
            var set = new HashSet<int>(new[] { 1, 2, 3 });
            var array = new int[3];
            set.CopyTo(array);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, array);
        }

        [Test]
        public void CopyTo_ArrayWithOffset_CopiesFromOffset()
        {
            var set = new HashSet<int>(new[] { 1 });
            var array = new int[2];
            set.CopyTo(array, 1);
            Assert.That(array[0], Is.EqualTo(0));
            Assert.That(array[1], Is.EqualTo(1));
        }

        [Test]
        public void CopyTo_LimitedCount_CopiesOnlyThatMany()
        {
            var set = new HashSet<int>(new[] { 1, 2, 3 });
            var array = new int[3];
            set.CopyTo(array, 0, 2);
            Assert.That(array[2], Is.EqualTo(0));
        }

        [Test]
        public void CopyTo_NullArray_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new HashSet<int>().CopyTo(null));
        }

        [Test]
        public void CopyTo_NegativeIndex_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new HashSet<int>().CopyTo(new int[1], -1));
        }

        [Test]
        public void Enumerator_VisitsEveryElementOnce()
        {
            var set = new HashSet<int>(new[] { 1, 2, 3 });
            var seen = new List<int>();
            foreach (var n in set)
                seen.Add(n);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, seen);
        }

        [Test]
        public void Enumerator_EmptySet_YieldsNothing()
        {
            var count = 0;
            foreach (var unused in new HashSet<int>())
                count++;
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void AsCollection_IsNotReadOnly()
        {
            ICollection<int> set = new HashSet<int>();
            Assert.That(set.IsReadOnly, Is.False);
            set.Add(1);
            Assert.That(set.Count, Is.EqualTo(1));
        }

        [Test]
        public void CreateSetComparer_ComparesByContent()
        {
            var comparer = HashSet<int>.CreateSetComparer();
            var first = new HashSet<int>(new[] { 1, 2 });
            var second = new HashSet<int>(new[] { 2, 1 });
            Assert.That(comparer.Equals(first, second), Is.True);
            Assert.That(comparer.GetHashCode(first), Is.EqualTo(comparer.GetHashCode(second)));
            Assert.That(comparer.Equals(first, new HashSet<int>(new[] { 1 })), Is.False);
        }

        [Test]
        public void TrimExcess_PreservesContent()
        {
            var set = new HashSet<int>(new[] { 1, 2, 3 });
            set.Remove(2);
            set.TrimExcess();
            CollectionAssert.AreEquivalent(new[] { 1, 3 }, set.ToArray());
        }

        [Test]
        public void HashSet_WorksWithLinqOperators()
        {
            var set = new HashSet<int>(new[] { 1, 2, 3, 4 });
            CollectionAssert.AreEqual(new[] { 2, 4 }, set.Where(n => n % 2 == 0).OrderBy(n => n).ToArray());
        }

        [Test]
        public void HashSet_WithCustomComparer_GroupsEquivalentValues()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            set.UnionWith(new[] { "one", "ONE", "two" });
            Assert.That(set.Count, Is.EqualTo(2));
            Assert.That(set.Contains("TWO"), Is.True);
        }

        [Test]
        public void HashSet_ManyElements_StoresAndRetrievesAll()
        {
            var set = new HashSet<int>();
            for (var i = 0; i < 1000; i++)
                set.Add(i);
            Assert.That(set.Count, Is.EqualTo(1000));
            for (var i = 0; i < 1000; i++)
                Assert.That(set.Contains(i), Is.True);
        }

        #endregion
    }
}
