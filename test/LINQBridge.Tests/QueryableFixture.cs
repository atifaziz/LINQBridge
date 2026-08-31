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
    using System.Linq;
    using System.Linq.Expressions;
    using NUnit.Framework;

    #endregion

    /// <summary>
    /// Exercises the <see cref="Queryable"/> operators, and with them the
    /// expression-tree-to-delegate pipeline underneath: EnumerableQuery,
    /// EnumerableRewriter (which rewrites Queryable calls into Enumerable
    /// calls) and EnumerableExecutor.
    /// </summary>
    /// <remarks>
    /// Compiled into both LINQBridge.Tests and LINQ.Tests, so it must stay
    /// within the Framework 3.5 API surface.
    /// </remarks>

    [TestFixture]
    public sealed class QueryableFixture
    {
        // ReSharper disable InconsistentNaming

        private static IQueryable<int> Numbers
        {
            get { return new[] { 5, 1, 4, 2, 3 }.AsQueryable(); }
        }

        private static IQueryable<string> Words
        {
            get { return new[] { "banana", "apple", "cherry" }.AsQueryable(); }
        }

        #region AsQueryable

        [Test]
        public void AsQueryable_Sequence_ProducesQueryableOverSameElements()
        {
            var q = Numbers;
            Assert.That(q.ElementType, Is.EqualTo(typeof(int)));
            Assert.That(q.Provider, Is.Not.Null);
            Assert.That(q.Expression, Is.Not.Null);
            CollectionAssert.AreEqual(new[] { 5, 1, 4, 2, 3 }, q.ToArray());
        }

        [Test]
        public void AsQueryable_AlreadyQueryable_ReturnsSameInstance()
        {
            var q = Numbers;
            Assert.That(q.AsQueryable(), Is.SameAs(q));
        }

        [Test]
        public void AsQueryable_NonGenericSequence_ProducesQueryable()
        {
            IEnumerable source = new[] { 1, 2, 3 };
            var q = source.AsQueryable();
            Assert.That(q.ElementType, Is.EqualTo(typeof(int)));
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, q.Cast<int>().ToArray());
        }

        [Test]
        public void AsQueryable_NullSource_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ((IEnumerable<int>) null).AsQueryable());
        }

        [Test]
        public void Queryable_EnumeratesThroughNonGenericEnumerable()
        {
            var list = new List<int>();
            foreach (var n in (IEnumerable) Numbers)
                list.Add((int) n);
            CollectionAssert.AreEqual(new[] { 5, 1, 4, 2, 3 }, list);
        }

        #endregion

        #region Restriction, projection and partitioning

        [Test]
        public void Where_Predicate_FiltersElements()
        {
            CollectionAssert.AreEqual(new[] { 5, 4, 3 }, Numbers.Where(n => n > 2).ToArray());
        }

        [Test]
        public void Where_IndexedPredicate_PassesIndex()
        {
            CollectionAssert.AreEqual(new[] { 5, 4, 3 },
                                      Numbers.Where((n, i) => i % 2 == 0).ToArray());
        }

        [Test]
        public void Where_NullPredicate_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => Numbers.Where((Expression<Func<int, bool>>) null));
        }

        [Test]
        public void Where_NullSource_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => ((IQueryable<int>) null).Where(n => n > 1));
        }

        [Test]
        public void Select_Projection_MapsElements()
        {
            CollectionAssert.AreEqual(new[] { 10, 2, 8, 4, 6 }, Numbers.Select(n => n * 2).ToArray());
        }

        [Test]
        public void Select_IndexedProjection_PassesIndex()
        {
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 },
                                      Numbers.Select((n, i) => i).ToArray());
        }

        [Test]
        public void SelectMany_Flattens()
        {
            var q = new[] { "ab", "cd" }.AsQueryable();
            CollectionAssert.AreEqual(new[] { 'a', 'b', 'c', 'd' },
                                      q.SelectMany(s => s.ToCharArray()).ToArray());
        }

        [Test]
        public void SelectMany_WithResultSelector_PairsSourceAndElement()
        {
            var q = new[] { "ab" }.AsQueryable();
            CollectionAssert.AreEqual(new[] { "ab:a", "ab:b" },
                                      q.SelectMany(s => s.ToCharArray(), (s, c) => s + ":" + c).ToArray());
        }

        [Test]
        public void SelectMany_IndexedSelector_PassesIndex()
        {
            var q = new[] { "a", "b" }.AsQueryable();
            CollectionAssert.AreEqual(new[] { "0a", "1b" },
                                      q.SelectMany((s, i) => new[] { i.ToString() + s }).ToArray());
        }

        [Test]
        public void Take_TakesLeadingElements()
        {
            CollectionAssert.AreEqual(new[] { 5, 1 }, Numbers.Take(2).ToArray());
        }

        [Test]
        public void Skip_SkipsLeadingElements()
        {
            CollectionAssert.AreEqual(new[] { 2, 3 }, Numbers.Skip(3).ToArray());
        }

        [Test]
        public void TakeWhile_StopsAtFirstFailure()
        {
            CollectionAssert.AreEqual(new[] { 5 }, Numbers.TakeWhile(n => n > 2).ToArray());
        }

        [Test]
        public void TakeWhile_IndexedPredicate_StopsAtFirstFailure()
        {
            CollectionAssert.AreEqual(new[] { 5, 1 }, Numbers.TakeWhile((n, i) => i < 2).ToArray());
        }

        [Test]
        public void SkipWhile_SkipsUntilFirstFailure()
        {
            CollectionAssert.AreEqual(new[] { 1, 4, 2, 3 }, Numbers.SkipWhile(n => n > 2).ToArray());
        }

        [Test]
        public void SkipWhile_IndexedPredicate_SkipsUntilFirstFailure()
        {
            CollectionAssert.AreEqual(new[] { 4, 2, 3 }, Numbers.SkipWhile((n, i) => i < 2).ToArray());
        }

        #endregion

        #region Ordering

        [Test]
        public void OrderBy_SortsAscending()
        {
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, Numbers.OrderBy(n => n).ToArray());
        }

        [Test]
        public void OrderByDescending_SortsDescending()
        {
            CollectionAssert.AreEqual(new[] { 5, 4, 3, 2, 1 },
                                      Numbers.OrderByDescending(n => n).ToArray());
        }

        [Test]
        public void OrderBy_WithComparer_UsesComparer()
        {
            CollectionAssert.AreEqual(new[] { 5, 4, 3, 2, 1 },
                                      Numbers.OrderBy(n => n, new ReverseComparer()).ToArray());
        }

        [Test]
        public void ThenBy_BreaksTiesAscending()
        {
            var q = new[] { "bb", "aa", "ab", "ba" }.AsQueryable();
            CollectionAssert.AreEqual(new[] { "aa", "ab", "ba", "bb" },
                                      q.OrderBy(s => s[0]).ThenBy(s => s[1]).ToArray());
        }

        [Test]
        public void ThenByDescending_BreaksTiesDescending()
        {
            var q = new[] { "aa", "ab" }.AsQueryable();
            CollectionAssert.AreEqual(new[] { "ab", "aa" },
                                      q.OrderBy(s => s[0]).ThenByDescending(s => s[1]).ToArray());
        }

        [Test]
        public void OrderBy_ReturnsOrderedQueryable()
        {
            Assert.That(Numbers.OrderBy(n => n), Is.InstanceOf<IOrderedQueryable<int>>());
        }

        [Test]
        public void Reverse_ReversesSequence()
        {
            CollectionAssert.AreEqual(new[] { 3, 2, 4, 1, 5 }, Numbers.Reverse().ToArray());
        }

        #endregion

        #region Grouping and joining

        [Test]
        public void GroupBy_KeySelector_GroupsByKey()
        {
            var groups = Numbers.GroupBy(n => n % 2).ToArray();
            Assert.That(groups.Length, Is.EqualTo(2));
            Assert.That(groups[0].Key, Is.EqualTo(1));
            CollectionAssert.AreEqual(new[] { 5, 1, 3 }, groups[0].ToArray());
            CollectionAssert.AreEqual(new[] { 4, 2 }, groups[1].ToArray());
        }

        [Test]
        public void GroupBy_ElementSelector_ProjectsElements()
        {
            var groups = Numbers.GroupBy(n => n % 2, n => n * 10).ToArray();
            CollectionAssert.AreEqual(new[] { 50, 10, 30 }, groups[0].ToArray());
        }

        [Test]
        public void GroupBy_ResultSelector_ProjectsGroups()
        {
            var sums = Numbers.GroupBy(n => n % 2, (key, values) => values.Sum()).ToArray();
            CollectionAssert.AreEqual(new[] { 9, 6 }, sums);
        }

        [Test]
        public void GroupBy_ElementAndResultSelector_ProjectsBoth()
        {
            var sums = Numbers.GroupBy(n => n % 2, n => n * 2, (key, values) => values.Sum()).ToArray();
            CollectionAssert.AreEqual(new[] { 18, 12 }, sums);
        }

        [Test]
        public void Join_MatchesOnKey()
        {
            var outer = new[] { 1, 2, 3 }.AsQueryable();
            var inner = new[] { 2, 3, 4 };
            var joined = outer.Join(inner, o => o, i => i, (o, i) => o * 10 + i).ToArray();
            CollectionAssert.AreEqual(new[] { 22, 33 }, joined);
        }

        [Test]
        public void GroupJoin_GroupsInnerByOuterKey()
        {
            var outer = new[] { 1, 2 }.AsQueryable();
            var inner = new[] { 1, 1, 2 };
            var joined = outer.GroupJoin(inner, o => o, i => i, (o, g) => o + ":" + g.Count()).ToArray();
            CollectionAssert.AreEqual(new[] { "1:2", "2:1" }, joined);
        }

        #endregion

        #region Set operators

        [Test]
        public void Distinct_RemovesDuplicates()
        {
            var q = new[] { 1, 2, 2, 3, 1 }.AsQueryable();
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, q.Distinct().ToArray());
        }

        [Test]
        public void Union_MergesWithoutDuplicates()
        {
            var q = new[] { 1, 2 }.AsQueryable();
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, q.Union(new[] { 2, 3 }).ToArray());
        }

        [Test]
        public void Intersect_KeepsCommonElements()
        {
            var q = new[] { 1, 2, 3 }.AsQueryable();
            CollectionAssert.AreEqual(new[] { 2, 3 }, q.Intersect(new[] { 2, 3, 4 }).ToArray());
        }

        [Test]
        public void Except_RemovesElementsOfSecond()
        {
            var q = new[] { 1, 2, 3 }.AsQueryable();
            CollectionAssert.AreEqual(new[] { 1 }, q.Except(new[] { 2, 3 }).ToArray());
        }

        [Test]
        public void Concat_AppendsSecond()
        {
            var q = new[] { 1, 2 }.AsQueryable();
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, q.Concat(new[] { 3 }).ToArray());
        }

        [Test]
        public void SequenceEqual_SameElements_IsTrue()
        {
            Assert.That(Numbers.SequenceEqual(new[] { 5, 1, 4, 2, 3 }), Is.True);
        }

        [Test]
        public void SequenceEqual_DifferentElements_IsFalse()
        {
            Assert.That(Numbers.SequenceEqual(new[] { 1, 2 }), Is.False);
        }

        #endregion

        #region Conversion and element operators

        [Test]
        public void Cast_ConvertsElements()
        {
            IQueryable source = new object[] { 1, 2 }.AsQueryable();
            CollectionAssert.AreEqual(new[] { 1, 2 }, source.Cast<int>().ToArray());
        }

        [Test]
        public void Cast_IncompatibleElement_ThrowsInvalidCastException()
        {
            IQueryable source = new object[] { 1, "s" }.AsQueryable();
            Assert.Throws<InvalidCastException>(() => source.Cast<int>().ToArray());
        }

        [Test]
        public void OfType_KeepsOnlyMatchingElements()
        {
            IQueryable source = new object[] { 1, "s", 2 }.AsQueryable();
            CollectionAssert.AreEqual(new[] { 1, 2 }, source.OfType<int>().ToArray());
        }

        [Test]
        public void First_ReturnsFirstElement()
        {
            Assert.That(Numbers.First(), Is.EqualTo(5));
        }

        [Test]
        public void First_WithPredicate_ReturnsFirstMatch()
        {
            Assert.That(Numbers.First(n => n < 3), Is.EqualTo(1));
        }

        [Test]
        public void First_EmptySequence_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => Empty().First());
        }

        [Test]
        public void FirstOrDefault_EmptySequence_ReturnsDefault()
        {
            Assert.That(Empty().FirstOrDefault(), Is.EqualTo(0));
        }

        [Test]
        public void FirstOrDefault_NoMatch_ReturnsDefault()
        {
            Assert.That(Numbers.FirstOrDefault(n => n > 100), Is.EqualTo(0));
        }

        [Test]
        public void Last_ReturnsLastElement()
        {
            Assert.That(Numbers.Last(), Is.EqualTo(3));
        }

        [Test]
        public void Last_WithPredicate_ReturnsLastMatch()
        {
            Assert.That(Numbers.Last(n => n > 3), Is.EqualTo(4));
        }

        [Test]
        public void LastOrDefault_EmptySequence_ReturnsDefault()
        {
            Assert.That(Empty().LastOrDefault(), Is.EqualTo(0));
        }

        [Test]
        public void Single_SingleElement_ReturnsIt()
        {
            Assert.That(new[] { 42 }.AsQueryable().Single(), Is.EqualTo(42));
        }

        [Test]
        public void Single_MoreThanOneElement_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => Numbers.Single());
        }

        [Test]
        public void Single_WithPredicate_ReturnsOnlyMatch()
        {
            Assert.That(Numbers.Single(n => n == 4), Is.EqualTo(4));
        }

        [Test]
        public void SingleOrDefault_EmptySequence_ReturnsDefault()
        {
            Assert.That(Empty().SingleOrDefault(), Is.EqualTo(0));
        }

        [Test]
        public void ElementAt_ReturnsElementAtIndex()
        {
            Assert.That(Numbers.ElementAt(2), Is.EqualTo(4));
        }

        [Test]
        public void ElementAt_OutOfRange_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Numbers.ElementAt(99));
        }

        [Test]
        public void ElementAtOrDefault_OutOfRange_ReturnsDefault()
        {
            Assert.That(Numbers.ElementAtOrDefault(99), Is.EqualTo(0));
        }

        [Test]
        public void DefaultIfEmpty_EmptySequence_YieldsSingleDefault()
        {
            CollectionAssert.AreEqual(new[] { 0 }, Empty().DefaultIfEmpty().ToArray());
        }

        [Test]
        public void DefaultIfEmpty_WithExplicitDefault_YieldsThatValue()
        {
            CollectionAssert.AreEqual(new[] { -1 }, Empty().DefaultIfEmpty(-1).ToArray());
        }

        [Test]
        public void DefaultIfEmpty_NonEmptySequence_YieldsSource()
        {
            CollectionAssert.AreEqual(new[] { 5, 1, 4, 2, 3 }, Numbers.DefaultIfEmpty().ToArray());
        }

        #endregion

        #region Quantifiers and aggregates

        [Test]
        public void Any_NonEmptySequence_IsTrue()
        {
            Assert.That(Numbers.Any(), Is.True);
        }

        [Test]
        public void Any_EmptySequence_IsFalse()
        {
            Assert.That(Empty().Any(), Is.False);
        }

        [Test]
        public void Any_WithPredicate_ReportsMatch()
        {
            Assert.That(Numbers.Any(n => n > 4), Is.True);
            Assert.That(Numbers.Any(n => n > 100), Is.False);
        }

        [Test]
        public void All_AllMatch_IsTrue()
        {
            Assert.That(Numbers.All(n => n > 0), Is.True);
            Assert.That(Numbers.All(n => n > 1), Is.False);
        }

        [Test]
        public void Contains_PresentElement_IsTrue()
        {
            Assert.That(Numbers.Contains(4), Is.True);
            Assert.That(Numbers.Contains(99), Is.False);
        }

        [Test]
        public void Contains_WithComparer_UsesComparer()
        {
            Assert.That(Words.Contains("APPLE", StringComparer.OrdinalIgnoreCase), Is.True);
        }

        [Test]
        public void Count_ReturnsElementCount()
        {
            Assert.That(Numbers.Count(), Is.EqualTo(5));
        }

        [Test]
        public void Count_WithPredicate_CountsMatches()
        {
            Assert.That(Numbers.Count(n => n > 2), Is.EqualTo(3));
        }

        [Test]
        public void LongCount_ReturnsElementCount()
        {
            Assert.That(Numbers.LongCount(), Is.EqualTo(5L));
        }

        [Test]
        public void LongCount_WithPredicate_CountsMatches()
        {
            Assert.That(Numbers.LongCount(n => n > 2), Is.EqualTo(3L));
        }

        [Test]
        public void Sum_AddsElements()
        {
            Assert.That(Numbers.Sum(), Is.EqualTo(15));
        }

        [Test]
        public void Sum_WithSelector_AddsProjectedElements()
        {
            Assert.That(Numbers.Sum(n => n * 2), Is.EqualTo(30));
        }

        [Test]
        public void Sum_Doubles_AddsElements()
        {
            var q = new[] { 1.5, 2.5 }.AsQueryable();
            Assert.That(q.Sum(), Is.EqualTo(4.0));
        }

        [Test]
        public void Sum_Nullables_IgnoresNulls()
        {
            var q = new int?[] { 1, null, 2 }.AsQueryable();
            Assert.That(q.Sum(), Is.EqualTo(3));
        }

        [Test]
        public void Min_ReturnsSmallest()
        {
            Assert.That(Numbers.Min(), Is.EqualTo(1));
        }

        [Test]
        public void Min_WithSelector_ReturnsSmallestProjection()
        {
            Assert.That(Numbers.Min(n => -n), Is.EqualTo(-5));
        }

        [Test]
        public void Max_ReturnsLargest()
        {
            Assert.That(Numbers.Max(), Is.EqualTo(5));
        }

        [Test]
        public void Max_WithSelector_ReturnsLargestProjection()
        {
            Assert.That(Numbers.Max(n => -n), Is.EqualTo(-1));
        }

        [Test]
        public void Average_ReturnsMean()
        {
            Assert.That(Numbers.Average(), Is.EqualTo(3.0));
        }

        [Test]
        public void Average_WithSelector_ReturnsMeanOfProjection()
        {
            Assert.That(Numbers.Average(n => n * 2), Is.EqualTo(6.0));
        }

        [Test]
        public void Average_EmptySequence_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => Empty().Average());
        }

        [Test]
        public void Aggregate_WithoutSeed_FoldsElements()
        {
            Assert.That(Numbers.Aggregate((a, b) => a + b), Is.EqualTo(15));
        }

        [Test]
        public void Aggregate_WithSeed_FoldsFromSeed()
        {
            Assert.That(Numbers.Aggregate(100, (a, b) => a + b), Is.EqualTo(115));
        }

        [Test]
        public void Aggregate_WithSeedAndResultSelector_ProjectsResult()
        {
            Assert.That(Numbers.Aggregate(0, (a, b) => a + b, total => total * 2), Is.EqualTo(30));
        }

        #endregion

        #region Composition and deferred execution

        [Test]
        public void Query_ComposedOperators_EvaluateInOrder()
        {
            var result = Numbers.Where(n => n > 1)
                                .OrderBy(n => n)
                                .Select(n => n * n)
                                .Take(2)
                                .ToArray();
            CollectionAssert.AreEqual(new[] { 4, 9 }, result);
        }

        [Test]
        public void Query_BuiltFromQuerySyntax_Evaluates()
        {
            var result = from n in Numbers
                         where n % 2 == 1
                         orderby n descending
                         select n * 10;
            CollectionAssert.AreEqual(new[] { 50, 30, 10 }, result.ToArray());
        }

        [Test]
        public void Query_IsNotEvaluatedUntilEnumerated()
        {
            var flag = new Flag();
            var query = Numbers.Where(n => flag.Set() && n > 0);
            Assert.That(flag.Value, Is.False);
            query.ToArray();
            Assert.That(flag.Value, Is.True);
        }

        [Test]
        public void Query_Expression_GrowsWithEachOperator()
        {
            var one = Numbers.Where(n => n > 1);
            var two = one.Select(n => n);
            Assert.That(one.Expression, Is.Not.SameAs(two.Expression));
            Assert.That(two.Expression.NodeType, Is.EqualTo(ExpressionType.Call));
        }

        [Test]
        public void Query_ElementTypeReflectsProjection()
        {
            Assert.That(Numbers.Select(n => n.ToString()).ElementType, Is.EqualTo(typeof(string)));
        }

        [Test]
        public void Provider_Execute_RunsRewrittenExpression()
        {
            var source = Numbers;
            var call = Expression.Call(typeof(Queryable), "Count", new[] { typeof(int) },
                                       source.Expression);
            Assert.That(source.Provider.Execute<int>(call), Is.EqualTo(5));
        }

        [Test]
        public void Provider_CreateQuery_ProducesEquivalentQuery()
        {
            var source = Numbers;
            var created = source.Provider.CreateQuery<int>(source.Expression);
            CollectionAssert.AreEqual(source.ToArray(), created.ToArray());
        }

        [Test]
        public void ToList_MaterialisesQuery()
        {
            var list = Numbers.Where(n => n > 3).ToList();
            Assert.That(list.Count, Is.EqualTo(2));
        }

        [Test]
        public void ToDictionary_MaterialisesQuery()
        {
            var map = Numbers.ToDictionary(n => n, n => n * 2);
            Assert.That(map[4], Is.EqualTo(8));
        }

        #endregion

        #region Helpers

        private static IQueryable<int> Empty()
        {
            return new int[0].AsQueryable();
        }

        private sealed class Flag
        {
            public bool Value;
            public bool Set() { Value = true; return true; }
        }

        private sealed class ReverseComparer : IComparer<int>
        {
            public int Compare(int x, int y) { return y.CompareTo(x); }
        }

        #endregion
    }
}
