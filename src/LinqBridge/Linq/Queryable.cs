// Decompiled with JetBrains decompiler
// Type: System.Linq.Queryable
// Assembly: System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: CF2EEE1F-04F2-4122-84EA-132765256A1E
// Assembly location: C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.5\System.Core.dll
// XML documentation location: C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Core.xml

using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq
{
    /// <summary>Provides a set of <see langword="static" /> (<see langword="Shared" /> in Visual Basic) methods for querying data structures that implement <see cref="T:System.Linq.IQueryable`1" />.</summary>
    public static class Queryable
    {
        /// <summary>Converts a generic <see cref="T:System.Collections.Generic.IEnumerable`1" /> to a generic <see cref="T:System.Linq.IQueryable`1" />.</summary>
        /// <param name="source">A sequence to convert.</param>
        /// <typeparam name="TElement">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that represents the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static IQueryable<TElement> AsQueryable<TElement>(
          this IEnumerable<TElement> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source is IQueryable<TElement> ? (IQueryable<TElement>)source : (IQueryable<TElement>)new EnumerableQuery<TElement>(source);
        }

        /// <summary>Converts an <see cref="T:System.Collections.IEnumerable" /> to an <see cref="T:System.Linq.IQueryable" />.</summary>
        /// <param name="source">A sequence to convert.</param>
        /// <returns>An <see cref="T:System.Linq.IQueryable" /> that represents the input sequence.</returns>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="source" /> does not implement <see cref="T:System.Collections.Generic.IEnumerable`1" /> for some <paramref name="T" />.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static IQueryable AsQueryable(this IEnumerable source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (source is IQueryable)
                return (IQueryable)source;
            return EnumerableQuery.Create((TypeHelper.FindGenericType(typeof(IEnumerable<>), source.GetType()) ?? throw new ArgumentException(nameof(source))).GetGenericArguments()[0], source);
        }

        /// <summary>Filters a sequence of values based on a predicate.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains elements from the input sequence that satisfy the condition specified by <paramref name="predicate" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> Where<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Filters a sequence of values based on a predicate. Each element's index is used in the logic of the predicate function.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to filter.</param>
        /// <param name="predicate">A function to test each element for a condition; the second parameter of the function represents the index of the element in the source sequence.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains elements from the input sequence that satisfy the condition specified by <paramref name="predicate" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> Where<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, int, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Filters the elements of an <see cref="T:System.Linq.IQueryable" /> based on a specified type.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable" /> whose elements to filter.</param>
        /// <typeparam name="TResult">The type to filter the elements of the sequence on.</typeparam>
        /// <returns>A collection that contains the elements from <paramref name="source" /> that have type <paramref name="TResult" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static IQueryable<TResult> OfType<TResult>(this IQueryable source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return (IQueryable<TResult>)source.Provider.CreateQuery((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), source.Expression));
        }

        /// <summary>Converts the elements of an <see cref="T:System.Linq.IQueryable" /> to the specified type.</summary>
        /// <param name="source">The <see cref="T:System.Linq.IQueryable" /> that contains the elements to be converted.</param>
        /// <typeparam name="TResult">The type to convert the elements of <paramref name="source" /> to.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains each element of the source sequence converted to the specified type.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidCastException">An element in the sequence cannot be cast to type <paramref name="TResult" />.</exception>
        public static IQueryable<TResult> Cast<TResult>(this IQueryable source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return (IQueryable<TResult>)source.Provider.CreateQuery((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), source.Expression));
        }

        /// <summary>Projects each element of a sequence into a new form.</summary>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by the function represented by <paramref name="selector" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> whose elements are the result of invoking a projection function on each element of <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static IQueryable<TResult> Select<TSource, TResult>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TResult>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.CreateQuery<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Projects each element of a sequence into a new form by incorporating the element's index.</summary>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by the function represented by <paramref name="selector" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> whose elements are the result of invoking a projection function on each element of <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static IQueryable<TResult> Select<TSource, TResult>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, int, TResult>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.CreateQuery<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IEnumerable`1" /> and combines the resulting sequences into one sequence.</summary>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the sequence returned by the function represented by <paramref name="selector" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> whose elements are the result of invoking a one-to-many projection function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static IQueryable<TResult> SelectMany<TSource, TResult>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, IEnumerable<TResult>>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.CreateQuery<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IEnumerable`1" /> and combines the resulting sequences into one sequence. The index of each source element is used in the projected form of that element.</summary>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A projection function to apply to each element; the second parameter of this function represents the index of the source element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the sequence returned by the function represented by <paramref name="selector" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> whose elements are the result of invoking a one-to-many projection function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static IQueryable<TResult> SelectMany<TSource, TResult>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, int, IEnumerable<TResult>>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.CreateQuery<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IEnumerable`1" /> that incorporates the index of the source element that produced it. A result selector function is invoked on each element of each intermediate sequence, and the resulting values are combined into a single, one-dimensional sequence and returned.</summary>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="collectionSelector">A projection function to apply to each element of the input sequence; the second parameter of this function represents the index of the source element.</param>
        /// <param name="resultSelector">A projection function to apply to each element of each intermediate sequence.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TCollection">The type of the intermediate elements collected by the function represented by <paramref name="collectionSelector" />.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> whose elements are the result of invoking the one-to-many projection function <paramref name="collectionSelector" /> on each element of <paramref name="source" /> and then mapping each of those sequence elements and their corresponding <paramref name="source" /> element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is <see langword="null" />.</exception>
        public static IQueryable<TResult> SelectMany<TSource, TCollection, TResult>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, int, IEnumerable<TCollection>>> collectionSelector,
          Expression<Func<TSource, TCollection, TResult>> resultSelector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (collectionSelector == null)
                throw Error.ArgumentNull(nameof(collectionSelector));
            if (resultSelector == null)
                throw Error.ArgumentNull(nameof(resultSelector));
            return source.Provider.CreateQuery<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TCollection), typeof(TResult)), source.Expression, (Expression)Expression.Quote((Expression)collectionSelector), (Expression)Expression.Quote((Expression)resultSelector)));
        }

        /// <summary>Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IEnumerable`1" /> and invokes a result selector function on each element therein. The resulting values from each intermediate sequence are combined into a single, one-dimensional sequence and returned.</summary>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="collectionSelector">A projection function to apply to each element of the input sequence.</param>
        /// <param name="resultSelector">A projection function to apply to each element of each intermediate sequence.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TCollection">The type of the intermediate elements collected by the function represented by <paramref name="collectionSelector" />.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> whose elements are the result of invoking the one-to-many projection function <paramref name="collectionSelector" /> on each element of <paramref name="source" /> and then mapping each of those sequence elements and their corresponding <paramref name="source" /> element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is <see langword="null" />.</exception>
        public static IQueryable<TResult> SelectMany<TSource, TCollection, TResult>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, IEnumerable<TCollection>>> collectionSelector,
          Expression<Func<TSource, TCollection, TResult>> resultSelector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (collectionSelector == null)
                throw Error.ArgumentNull(nameof(collectionSelector));
            if (resultSelector == null)
                throw Error.ArgumentNull(nameof(resultSelector));
            return source.Provider.CreateQuery<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TCollection), typeof(TResult)), source.Expression, (Expression)Expression.Quote((Expression)collectionSelector), (Expression)Expression.Quote((Expression)resultSelector)));
        }

        private static Expression GetSourceExpression<TSource>(IEnumerable<TSource> source) => source is IQueryable<TSource> queryable ? queryable.Expression : (Expression)Expression.Constant((object)source, typeof(IEnumerable<TSource>));

        /// <summary>Correlates the elements of two sequences based on matching keys. The default equality comparer is used to compare keys.</summary>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that has elements of type <paramref name="TResult" /> obtained by performing an inner join on two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="outer" /> or <paramref name="inner" /> or <paramref name="outerKeySelector" /> or <paramref name="innerKeySelector" /> or <paramref name="resultSelector" /> is <see langword="null" />.</exception>
        public static IQueryable<TResult> Join<TOuter, TInner, TKey, TResult>(
          this IQueryable<TOuter> outer,
          IEnumerable<TInner> inner,
          Expression<Func<TOuter, TKey>> outerKeySelector,
          Expression<Func<TInner, TKey>> innerKeySelector,
          Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            if (outer == null)
                throw Error.ArgumentNull(nameof(outer));
            if (inner == null)
                throw Error.ArgumentNull(nameof(inner));
            if (outerKeySelector == null)
                throw Error.ArgumentNull(nameof(outerKeySelector));
            if (innerKeySelector == null)
                throw Error.ArgumentNull(nameof(innerKeySelector));
            if (resultSelector == null)
                throw Error.ArgumentNull(nameof(resultSelector));
            return outer.Provider.CreateQuery<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TOuter), typeof(TInner), typeof(TKey), typeof(TResult)), outer.Expression, Queryable.GetSourceExpression<TInner>(inner), (Expression)Expression.Quote((Expression)outerKeySelector), (Expression)Expression.Quote((Expression)innerKeySelector), (Expression)Expression.Quote((Expression)resultSelector)));
        }

        /// <summary>Correlates the elements of two sequences based on matching keys. A specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> is used to compare keys.</summary>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to hash and compare keys.</param>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that has elements of type <paramref name="TResult" /> obtained by performing an inner join on two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="outer" /> or <paramref name="inner" /> or <paramref name="outerKeySelector" /> or <paramref name="innerKeySelector" /> or <paramref name="resultSelector" /> is <see langword="null" />.</exception>
        public static IQueryable<TResult> Join<TOuter, TInner, TKey, TResult>(
          this IQueryable<TOuter> outer,
          IEnumerable<TInner> inner,
          Expression<Func<TOuter, TKey>> outerKeySelector,
          Expression<Func<TInner, TKey>> innerKeySelector,
          Expression<Func<TOuter, TInner, TResult>> resultSelector,
          IEqualityComparer<TKey> comparer)
        {
            if (outer == null)
                throw Error.ArgumentNull(nameof(outer));
            if (inner == null)
                throw Error.ArgumentNull(nameof(inner));
            if (outerKeySelector == null)
                throw Error.ArgumentNull(nameof(outerKeySelector));
            if (innerKeySelector == null)
                throw Error.ArgumentNull(nameof(innerKeySelector));
            if (resultSelector == null)
                throw Error.ArgumentNull(nameof(resultSelector));
            return outer.Provider.CreateQuery<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TOuter), typeof(TInner), typeof(TKey), typeof(TResult)), outer.Expression, Queryable.GetSourceExpression<TInner>(inner), (Expression)Expression.Quote((Expression)outerKeySelector), (Expression)Expression.Quote((Expression)innerKeySelector), (Expression)Expression.Quote((Expression)resultSelector), (Expression)Expression.Constant((object)comparer, typeof(IEqualityComparer<TKey>))));
        }

        /// <summary>Correlates the elements of two sequences based on key equality and groups the results. The default equality comparer is used to compare keys.</summary>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains elements of type <paramref name="TResult" /> obtained by performing a grouped join on two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="outer" /> or <paramref name="inner" /> or <paramref name="outerKeySelector" /> or <paramref name="innerKeySelector" /> or <paramref name="resultSelector" /> is <see langword="null" />.</exception>
        public static IQueryable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
          this IQueryable<TOuter> outer,
          IEnumerable<TInner> inner,
          Expression<Func<TOuter, TKey>> outerKeySelector,
          Expression<Func<TInner, TKey>> innerKeySelector,
          Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector)
        {
            if (outer == null)
                throw Error.ArgumentNull(nameof(outer));
            if (inner == null)
                throw Error.ArgumentNull(nameof(inner));
            if (outerKeySelector == null)
                throw Error.ArgumentNull(nameof(outerKeySelector));
            if (innerKeySelector == null)
                throw Error.ArgumentNull(nameof(innerKeySelector));
            if (resultSelector == null)
                throw Error.ArgumentNull(nameof(resultSelector));
            return outer.Provider.CreateQuery<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TOuter), typeof(TInner), typeof(TKey), typeof(TResult)), outer.Expression, Queryable.GetSourceExpression<TInner>(inner), (Expression)Expression.Quote((Expression)outerKeySelector), (Expression)Expression.Quote((Expression)innerKeySelector), (Expression)Expression.Quote((Expression)resultSelector)));
        }

        /// <summary>Correlates the elements of two sequences based on key equality and groups the results. A specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> is used to compare keys.</summary>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
        /// <param name="comparer">A comparer to hash and compare keys.</param>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains elements of type <paramref name="TResult" /> obtained by performing a grouped join on two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="outer" /> or <paramref name="inner" /> or <paramref name="outerKeySelector" /> or <paramref name="innerKeySelector" /> or <paramref name="resultSelector" /> is <see langword="null" />.</exception>
        public static IQueryable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
          this IQueryable<TOuter> outer,
          IEnumerable<TInner> inner,
          Expression<Func<TOuter, TKey>> outerKeySelector,
          Expression<Func<TInner, TKey>> innerKeySelector,
          Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector,
          IEqualityComparer<TKey> comparer)
        {
            if (outer == null)
                throw Error.ArgumentNull(nameof(outer));
            if (inner == null)
                throw Error.ArgumentNull(nameof(inner));
            if (outerKeySelector == null)
                throw Error.ArgumentNull(nameof(outerKeySelector));
            if (innerKeySelector == null)
                throw Error.ArgumentNull(nameof(innerKeySelector));
            if (resultSelector == null)
                throw Error.ArgumentNull(nameof(resultSelector));
            return outer.Provider.CreateQuery<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TOuter), typeof(TInner), typeof(TKey), typeof(TResult)), outer.Expression, Queryable.GetSourceExpression<TInner>(inner), (Expression)Expression.Quote((Expression)outerKeySelector), (Expression)Expression.Quote((Expression)innerKeySelector), (Expression)Expression.Quote((Expression)resultSelector), (Expression)Expression.Constant((object)comparer, typeof(IEqualityComparer<TKey>))));
        }

        /// <summary>Sorts the elements of a sequence in ascending order according to a key.</summary>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by <paramref name="keySelector" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1" /> whose elements are sorted according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> is <see langword="null" />.</exception>
        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, (Expression)Expression.Quote((Expression)keySelector)));
        }

        /// <summary>Sorts the elements of a sequence in ascending order by using a specified comparer.</summary>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by <paramref name="keySelector" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1" /> whose elements are sorted according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="comparer" /> is <see langword="null" />.</exception>
        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector,
          IComparer<TKey> comparer)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, (Expression)Expression.Quote((Expression)keySelector), (Expression)Expression.Constant((object)comparer, typeof(IComparer<TKey>))));
        }

        /// <summary>Sorts the elements of a sequence in descending order according to a key.</summary>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by <paramref name="keySelector" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1" /> whose elements are sorted in descending order according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> is <see langword="null" />.</exception>
        public static IOrderedQueryable<TSource> OrderByDescending<TSource, TKey>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, (Expression)Expression.Quote((Expression)keySelector)));
        }

        /// <summary>Sorts the elements of a sequence in descending order by using a specified comparer.</summary>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by <paramref name="keySelector" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1" /> whose elements are sorted in descending order according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="comparer" /> is <see langword="null" />.</exception>
        public static IOrderedQueryable<TSource> OrderByDescending<TSource, TKey>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector,
          IComparer<TKey> comparer)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, (Expression)Expression.Quote((Expression)keySelector), (Expression)Expression.Constant((object)comparer, typeof(IComparer<TKey>))));
        }

        /// <summary>Performs a subsequent ordering of the elements in a sequence in ascending order according to a key.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IOrderedQueryable`1" /> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented by <paramref name="keySelector" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1" /> whose elements are sorted according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> is <see langword="null" />.</exception>
        public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(
          this IOrderedQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, (Expression)Expression.Quote((Expression)keySelector)));
        }

        /// <summary>Performs a subsequent ordering of the elements in a sequence in ascending order by using a specified comparer.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IOrderedQueryable`1" /> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented by <paramref name="keySelector" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1" /> whose elements are sorted according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="comparer" /> is <see langword="null" />.</exception>
        public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(
          this IOrderedQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector,
          IComparer<TKey> comparer)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, (Expression)Expression.Quote((Expression)keySelector), (Expression)Expression.Constant((object)comparer, typeof(IComparer<TKey>))));
        }

        /// <summary>Performs a subsequent ordering of the elements in a sequence in descending order, according to a key.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IOrderedQueryable`1" /> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented by <paramref name="keySelector" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1" /> whose elements are sorted in descending order according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> is <see langword="null" />.</exception>
        public static IOrderedQueryable<TSource> ThenByDescending<TSource, TKey>(
          this IOrderedQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, (Expression)Expression.Quote((Expression)keySelector)));
        }

        /// <summary>Performs a subsequent ordering of the elements in a sequence in descending order by using a specified comparer.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IOrderedQueryable`1" /> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key that is returned by the <paramref name="keySelector" /> function.</typeparam>
        /// <returns>A collection whose elements are sorted in descending order according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="comparer" /> is <see langword="null" />.</exception>
        public static IOrderedQueryable<TSource> ThenByDescending<TSource, TKey>(
          this IOrderedQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector,
          IComparer<TKey> comparer)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, (Expression)Expression.Quote((Expression)keySelector), (Expression)Expression.Constant((object)comparer, typeof(IComparer<TKey>))));
        }

        /// <summary>Returns a specified number of contiguous elements from the start of a sequence.</summary>
        /// <param name="source">The sequence to return elements from.</param>
        /// <param name="count">The number of elements to return.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains the specified number of elements from the start of <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> Take<TSource>(
          this IQueryable<TSource> source,
          int count)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Constant((object)count)));
        }

        /// <summary>Returns elements from a sequence as long as a specified condition is true.</summary>
        /// <param name="source">The sequence to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains elements from the input sequence occurring before the element at which the test specified by <paramref name="predicate" /> no longer passes.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> TakeWhile<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Returns elements from a sequence as long as a specified condition is true. The element's index is used in the logic of the predicate function.</summary>
        /// <param name="source">The sequence to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition; the second parameter of the function represents the index of the element in the source sequence.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains elements from the input sequence occurring before the element at which the test specified by <paramref name="predicate" /> no longer passes.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> TakeWhile<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, int, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Bypasses a specified number of elements in a sequence and then returns the remaining elements.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return elements from.</param>
        /// <param name="count">The number of elements to skip before returning the remaining elements.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains elements that occur after the specified index in the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> Skip<TSource>(
          this IQueryable<TSource> source,
          int count)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Constant((object)count)));
        }

        /// <summary>Bypasses elements in a sequence as long as a specified condition is true and then returns the remaining elements.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains elements from <paramref name="source" /> starting at the first element in the linear series that does not pass the test specified by <paramref name="predicate" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> SkipWhile<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Bypasses elements in a sequence as long as a specified condition is true and then returns the remaining elements. The element's index is used in the logic of the predicate function.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition; the second parameter of this function represents the index of the source element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains elements from <paramref name="source" /> starting at the first element in the linear series that does not pass the test specified by <paramref name="predicate" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> SkipWhile<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, int, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in <paramref name="keySelector" />.</typeparam>
        /// <returns>An IQueryable&lt;IGrouping&lt;TKey, TSource&gt;&gt; in C# or IQueryable(Of IGrouping(Of TKey, TSource)) in Visual Basic where each <see cref="T:System.Linq.IGrouping`2" /> object contains a sequence of objects and a key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> is <see langword="null" />.</exception>
        public static IQueryable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            return source.Provider.CreateQuery<IGrouping<TKey, TSource>>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, (Expression)Expression.Quote((Expression)keySelector)));
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and projects the elements for each group by using a specified function.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to map each source element to an element in an <see cref="T:System.Linq.IGrouping`2" />.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in <paramref name="keySelector" />.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each <see cref="T:System.Linq.IGrouping`2" />.</typeparam>
        /// <returns>An IQueryable&lt;IGrouping&lt;TKey, TElement&gt;&gt; in C# or IQueryable(Of IGrouping(Of TKey, TElement)) in Visual Basic where each <see cref="T:System.Linq.IGrouping`2" /> contains a sequence of objects of type <paramref name="TElement" /> and a key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> is <see langword="null" />.</exception>
        public static IQueryable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector,
          Expression<Func<TSource, TElement>> elementSelector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            if (elementSelector == null)
                throw Error.ArgumentNull(nameof(elementSelector));
            return source.Provider.CreateQuery<IGrouping<TKey, TElement>>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement)), source.Expression, (Expression)Expression.Quote((Expression)keySelector), (Expression)Expression.Quote((Expression)elementSelector)));
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and compares the keys by using a specified comparer.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in <paramref name="keySelector" />.</typeparam>
        /// <returns>An IQueryable&lt;IGrouping&lt;TKey, TSource&gt;&gt; in C# or IQueryable(Of IGrouping(Of TKey, TSource)) in Visual Basic where each <see cref="T:System.Linq.IGrouping`2" /> contains a sequence of objects and a key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="comparer" /> is <see langword="null" />.</exception>
        public static IQueryable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector,
          IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            return source.Provider.CreateQuery<IGrouping<TKey, TSource>>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, (Expression)Expression.Quote((Expression)keySelector), (Expression)Expression.Constant((object)comparer, typeof(IEqualityComparer<TKey>))));
        }

        /// <summary>Groups the elements of a sequence and projects the elements for each group by using a specified function. Key values are compared by using a specified comparer.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to map each source element to an element in an <see cref="T:System.Linq.IGrouping`2" />.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in <paramref name="keySelector" />.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each <see cref="T:System.Linq.IGrouping`2" />.</typeparam>
        /// <returns>An IQueryable&lt;IGrouping&lt;TKey, TElement&gt;&gt; in C# or IQueryable(Of IGrouping(Of TKey, TElement)) in Visual Basic where each <see cref="T:System.Linq.IGrouping`2" /> contains a sequence of objects of type <paramref name="TElement" /> and a key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> or <paramref name="comparer" /> is <see langword="null" />.</exception>
        public static IQueryable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector,
          Expression<Func<TSource, TElement>> elementSelector,
          IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            if (elementSelector == null)
                throw Error.ArgumentNull(nameof(elementSelector));
            return source.Provider.CreateQuery<IGrouping<TKey, TElement>>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement)), source.Expression, (Expression)Expression.Quote((Expression)keySelector), (Expression)Expression.Quote((Expression)elementSelector), (Expression)Expression.Constant((object)comparer, typeof(IEqualityComparer<TKey>))));
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. The elements of each group are projected by using a specified function.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to map each source element to an element in an <see cref="T:System.Linq.IGrouping`2" />.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in <paramref name="keySelector" />.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each <see cref="T:System.Linq.IGrouping`2" />.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector" />.</typeparam>
        /// <returns>An T:System.Linq.IQueryable`1 that has a type argument of <paramref name="TResult" /> and where each element represents a projection over a group and its key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> or <paramref name="resultSelector" /> is <see langword="null" />.</exception>
        public static IQueryable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector,
          Expression<Func<TSource, TElement>> elementSelector,
          Expression<Func<TKey, IEnumerable<TElement>, TResult>> resultSelector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            if (elementSelector == null)
                throw Error.ArgumentNull(nameof(elementSelector));
            if (resultSelector == null)
                throw Error.ArgumentNull(nameof(resultSelector));
            return source.Provider.CreateQuery<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement), typeof(TResult)), source.Expression, (Expression)Expression.Quote((Expression)keySelector), (Expression)Expression.Quote((Expression)elementSelector), (Expression)Expression.Quote((Expression)resultSelector)));
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in <paramref name="keySelector" />.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector" />.</typeparam>
        /// <returns>An T:System.Linq.IQueryable`1 that has a type argument of <paramref name="TResult" /> and where each element represents a projection over a group and its key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="resultSelector" /> is <see langword="null" />.</exception>
        public static IQueryable<TResult> GroupBy<TSource, TKey, TResult>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector,
          Expression<Func<TKey, IEnumerable<TSource>, TResult>> resultSelector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            if (resultSelector == null)
                throw Error.ArgumentNull(nameof(resultSelector));
            return source.Provider.CreateQuery<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TResult)), source.Expression, (Expression)Expression.Quote((Expression)keySelector), (Expression)Expression.Quote((Expression)resultSelector)));
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. Keys are compared by using a specified comparer.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in <paramref name="keySelector" />.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector" />.</typeparam>
        /// <returns>An T:System.Linq.IQueryable`1 that has a type argument of <paramref name="TResult" /> and where each element represents a projection over a group and its key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="resultSelector" /> or <paramref name="comparer" /> is <see langword="null" />.</exception>
        public static IQueryable<TResult> GroupBy<TSource, TKey, TResult>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector,
          Expression<Func<TKey, IEnumerable<TSource>, TResult>> resultSelector,
          IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            if (resultSelector == null)
                throw Error.ArgumentNull(nameof(resultSelector));
            return source.Provider.CreateQuery<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TResult)), source.Expression, (Expression)Expression.Quote((Expression)keySelector), (Expression)Expression.Quote((Expression)resultSelector), (Expression)Expression.Constant((object)comparer, typeof(IEqualityComparer<TKey>))));
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. Keys are compared by using a specified comparer and the elements of each group are projected by using a specified function.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to map each source element to an element in an <see cref="T:System.Linq.IGrouping`2" />.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in <paramref name="keySelector" />.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each <see cref="T:System.Linq.IGrouping`2" />.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector" />.</typeparam>
        /// <returns>An T:System.Linq.IQueryable`1 that has a type argument of <paramref name="TResult" /> and where each element represents a projection over a group and its key.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> or <paramref name="resultSelector" /> or <paramref name="comparer" /> is <see langword="null" />.</exception>
        public static IQueryable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TKey>> keySelector,
          Expression<Func<TSource, TElement>> elementSelector,
          Expression<Func<TKey, IEnumerable<TElement>, TResult>> resultSelector,
          IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (keySelector == null)
                throw Error.ArgumentNull(nameof(keySelector));
            if (elementSelector == null)
                throw Error.ArgumentNull(nameof(elementSelector));
            if (resultSelector == null)
                throw Error.ArgumentNull(nameof(resultSelector));
            return source.Provider.CreateQuery<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement), typeof(TResult)), source.Expression, (Expression)Expression.Quote((Expression)keySelector), (Expression)Expression.Quote((Expression)elementSelector), (Expression)Expression.Quote((Expression)resultSelector), (Expression)Expression.Constant((object)comparer, typeof(IEqualityComparer<TKey>))));
        }

        /// <summary>Returns distinct elements from a sequence by using the default equality comparer to compare values.</summary>
        /// <param name="source">The <see cref="T:System.Linq.IQueryable`1" /> to remove duplicates from.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains distinct elements from <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> Distinct<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        /// <summary>Returns distinct elements from a sequence by using a specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</summary>
        /// <param name="source">The <see cref="T:System.Linq.IQueryable`1" /> to remove duplicates from.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains distinct elements from <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="comparer" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> Distinct<TSource>(
          this IQueryable<TSource> source,
          IEqualityComparer<TSource> comparer)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Constant((object)comparer, typeof(IEqualityComparer<TSource>))));
        }

        /// <summary>Concatenates two sequences.</summary>
        /// <param name="source1">The first sequence to concatenate.</param>
        /// <param name="source2">The sequence to concatenate to the first sequence.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains the concatenated elements of the two input sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source1" /> or <paramref name="source2" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> Concat<TSource>(
          this IQueryable<TSource> source1,
          IEnumerable<TSource> source2)
        {
            if (source1 == null)
                throw Error.ArgumentNull(nameof(source1));
            if (source2 == null)
                throw Error.ArgumentNull(nameof(source2));
            return source1.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source1.Expression, Queryable.GetSourceExpression<TSource>(source2)));
        }

        /// <summary>Produces the set union of two sequences by using the default equality comparer.</summary>
        /// <param name="source1">A sequence whose distinct elements form the first set for the union operation.</param>
        /// <param name="source2">A sequence whose distinct elements form the second set for the union operation.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains the elements from both input sequences, excluding duplicates.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source1" /> or <paramref name="source2" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> Union<TSource>(
          this IQueryable<TSource> source1,
          IEnumerable<TSource> source2)
        {
            if (source1 == null)
                throw Error.ArgumentNull(nameof(source1));
            if (source2 == null)
                throw Error.ArgumentNull(nameof(source2));
            return source1.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source1.Expression, Queryable.GetSourceExpression<TSource>(source2)));
        }

        /// <summary>Produces the set union of two sequences by using a specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
        /// <param name="source1">A sequence whose distinct elements form the first set for the union operation.</param>
        /// <param name="source2">A sequence whose distinct elements form the second set for the union operation.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains the elements from both input sequences, excluding duplicates.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source1" /> or <paramref name="source2" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> Union<TSource>(
          this IQueryable<TSource> source1,
          IEnumerable<TSource> source2,
          IEqualityComparer<TSource> comparer)
        {
            if (source1 == null)
                throw Error.ArgumentNull(nameof(source1));
            if (source2 == null)
                throw Error.ArgumentNull(nameof(source2));
            return source1.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source1.Expression, Queryable.GetSourceExpression<TSource>(source2), (Expression)Expression.Constant((object)comparer, typeof(IEqualityComparer<TSource>))));
        }

        /// <summary>Produces the set intersection of two sequences by using the default equality comparer to compare values.</summary>
        /// <param name="source1">A sequence whose distinct elements that also appear in <paramref name="source2" /> are returned.</param>
        /// <param name="source2">A sequence whose distinct elements that also appear in the first sequence are returned.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>A sequence that contains the set intersection of the two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source1" /> or <paramref name="source2" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> Intersect<TSource>(
          this IQueryable<TSource> source1,
          IEnumerable<TSource> source2)
        {
            if (source1 == null)
                throw Error.ArgumentNull(nameof(source1));
            if (source2 == null)
                throw Error.ArgumentNull(nameof(source2));
            return source1.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source1.Expression, Queryable.GetSourceExpression<TSource>(source2)));
        }

        /// <summary>Produces the set intersection of two sequences by using the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</summary>
        /// <param name="source1">An <see cref="T:System.Linq.IQueryable`1" /> whose distinct elements that also appear in <paramref name="source2" /> are returned.</param>
        /// <param name="source2">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose distinct elements that also appear in the first sequence are returned.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains the set intersection of the two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source1" /> or <paramref name="source2" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> Intersect<TSource>(
          this IQueryable<TSource> source1,
          IEnumerable<TSource> source2,
          IEqualityComparer<TSource> comparer)
        {
            if (source1 == null)
                throw Error.ArgumentNull(nameof(source1));
            if (source2 == null)
                throw Error.ArgumentNull(nameof(source2));
            return source1.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source1.Expression, Queryable.GetSourceExpression<TSource>(source2), (Expression)Expression.Constant((object)comparer, typeof(IEqualityComparer<TSource>))));
        }

        /// <summary>Produces the set difference of two sequences by using the default equality comparer to compare values.</summary>
        /// <param name="source1">An <see cref="T:System.Linq.IQueryable`1" /> whose elements that are not also in <paramref name="source2" /> will be returned.</param>
        /// <param name="source2">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements that also occur in the first sequence will not appear in the returned sequence.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains the set difference of the two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source1" /> or <paramref name="source2" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> Except<TSource>(
          this IQueryable<TSource> source1,
          IEnumerable<TSource> source2)
        {
            if (source1 == null)
                throw Error.ArgumentNull(nameof(source1));
            if (source2 == null)
                throw Error.ArgumentNull(nameof(source2));
            return source1.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source1.Expression, Queryable.GetSourceExpression<TSource>(source2)));
        }

        /// <summary>Produces the set difference of two sequences by using the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</summary>
        /// <param name="source1">An <see cref="T:System.Linq.IQueryable`1" /> whose elements that are not also in <paramref name="source2" /> will be returned.</param>
        /// <param name="source2">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements that also occur in the first sequence will not appear in the returned sequence.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains the set difference of the two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source1" /> or <paramref name="source2" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> Except<TSource>(
          this IQueryable<TSource> source1,
          IEnumerable<TSource> source2,
          IEqualityComparer<TSource> comparer)
        {
            if (source1 == null)
                throw Error.ArgumentNull(nameof(source1));
            if (source2 == null)
                throw Error.ArgumentNull(nameof(source2));
            return source1.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source1.Expression, Queryable.GetSourceExpression<TSource>(source2), (Expression)Expression.Constant((object)comparer, typeof(IEqualityComparer<TSource>))));
        }

        /// <summary>Returns the first element of a sequence.</summary>
        /// <param name="source">The <see cref="T:System.Linq.IQueryable`1" /> to return the first element of.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The first element in <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">The source sequence is empty.</exception>
        public static TSource First<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        /// <summary>Returns the first element of a sequence that satisfies a specified condition.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The first element in <paramref name="source" /> that passes the test in <paramref name="predicate" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">No element satisfies the condition in <paramref name="predicate" />.-or-The source sequence is empty.</exception>
        public static TSource First<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Returns the first element of a sequence, or a default value if the sequence contains no elements.</summary>
        /// <param name="source">The <see cref="T:System.Linq.IQueryable`1" /> to return the first element of.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>
        /// default(<paramref name="TSource" />) if <paramref name="source" /> is empty; otherwise, the first element in <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static TSource FirstOrDefault<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        /// <summary>Returns the first element of a sequence that satisfies a specified condition or a default value if no such element is found.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>
        /// default(<paramref name="TSource" />) if <paramref name="source" /> is empty or if no element passes the test specified by <paramref name="predicate" />; otherwise, the first element in <paramref name="source" /> that passes the test specified by <paramref name="predicate" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        public static TSource FirstOrDefault<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Returns the last element in a sequence.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return the last element of.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The value at the last position in <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">The source sequence is empty.</exception>
        public static TSource Last<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        /// <summary>Returns the last element of a sequence that satisfies a specified condition.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The last element in <paramref name="source" /> that passes the test specified by <paramref name="predicate" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">No element satisfies the condition in <paramref name="predicate" />.-or-The source sequence is empty.</exception>
        public static TSource Last<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Returns the last element in a sequence, or a default value if the sequence contains no elements.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return the last element of.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>
        /// default(<paramref name="TSource" />) if <paramref name="source" /> is empty; otherwise, the last element in <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static TSource LastOrDefault<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        /// <summary>Returns the last element of a sequence that satisfies a condition or a default value if no such element is found.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>
        /// default(<paramref name="TSource" />) if <paramref name="source" /> is empty or if no elements pass the test in the predicate function; otherwise, the last element of <paramref name="source" /> that passes the test in the predicate function.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        public static TSource LastOrDefault<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return the single element of.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The single element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> has more than one element.</exception>
        public static TSource Single<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        /// <summary>Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The single element of the input sequence that satisfies the condition in <paramref name="predicate" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">No element satisfies the condition in <paramref name="predicate" />.-or-More than one element satisfies the condition in <paramref name="predicate" />.-or-The source sequence is empty.</exception>
        public static TSource Single<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Returns the only element of a sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return the single element of.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The single element of the input sequence, or default(<paramref name="TSource" />) if the sequence contains no elements.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> has more than one element.</exception>
        public static TSource SingleOrDefault<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        /// <summary>Returns the only element of a sequence that satisfies a specified condition or a default value if no such element exists; this method throws an exception if more than one element satisfies the condition.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The single element of the input sequence that satisfies the condition in <paramref name="predicate" />, or default(<paramref name="TSource" />) if no such element is found.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">More than one element satisfies the condition in <paramref name="predicate" />.</exception>
        public static TSource SingleOrDefault<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Returns the element at a specified index in a sequence.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return an element from.</param>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The element at the specified position in <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than zero.</exception>
        public static TSource ElementAt<TSource>(this IQueryable<TSource> source, int index)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (index < 0)
                throw Error.ArgumentOutOfRange(nameof(index));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Constant((object)index)));
        }

        /// <summary>Returns the element at a specified index in a sequence or a default value if the index is out of range.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return an element from.</param>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>
        /// default(<paramref name="TSource" />) if <paramref name="index" /> is outside the bounds of <paramref name="source" />; otherwise, the element at the specified position in <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static TSource ElementAtOrDefault<TSource>(this IQueryable<TSource> source, int index)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Constant((object)index)));
        }

        /// <summary>Returns the elements of the specified sequence or the type parameter's default value in a singleton collection if the sequence is empty.</summary>
        /// <param name="source">The <see cref="T:System.Linq.IQueryable`1" /> to return a default value for if empty.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains <see langword="default" />(<paramref name="TSource" />) if <paramref name="source" /> is empty; otherwise, <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> DefaultIfEmpty<TSource>(
          this IQueryable<TSource> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        /// <summary>Returns the elements of the specified sequence or the specified value in a singleton collection if the sequence is empty.</summary>
        /// <param name="source">The <see cref="T:System.Linq.IQueryable`1" /> to return the specified value for if empty.</param>
        /// <param name="defaultValue">The value to return if the sequence is empty.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> that contains <paramref name="defaultValue" /> if <paramref name="source" /> is empty; otherwise, <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> DefaultIfEmpty<TSource>(
          this IQueryable<TSource> source,
          TSource defaultValue)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Constant((object)defaultValue, typeof(TSource))));
        }

        /// <summary>Determines whether a sequence contains a specified element by using the default equality comparer.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> in which to locate <paramref name="item" />.</param>
        /// <param name="item">The object to locate in the sequence.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>
        /// <see langword="true" /> if the input sequence contains an element that has the specified value; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static bool Contains<TSource>(this IQueryable<TSource> source, TSource item)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<bool>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Constant((object)item, typeof(TSource))));
        }

        /// <summary>Determines whether a sequence contains a specified element by using a specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> in which to locate <paramref name="item" />.</param>
        /// <param name="item">The object to locate in the sequence.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>
        /// <see langword="true" /> if the input sequence contains an element that has the specified value; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static bool Contains<TSource>(
          this IQueryable<TSource> source,
          TSource item,
          IEqualityComparer<TSource> comparer)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<bool>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Constant((object)item, typeof(TSource)), (Expression)Expression.Constant((object)comparer, typeof(IEqualityComparer<TSource>))));
        }

        /// <summary>Inverts the order of the elements in a sequence.</summary>
        /// <param name="source">A sequence of values to reverse.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1" /> whose elements correspond to those of the input sequence in reverse order.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static IQueryable<TSource> Reverse<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.CreateQuery<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        /// <summary>Determines whether two sequences are equal by using the default equality comparer to compare elements.</summary>
        /// <param name="source1">An <see cref="T:System.Linq.IQueryable`1" /> whose elements to compare to those of <paramref name="source2" />.</param>
        /// <param name="source2">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements to compare to those of the first sequence.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>
        /// <see langword="true" /> if the two source sequences are of equal length and their corresponding elements compare equal; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source1" /> or <paramref name="source2" /> is <see langword="null" />.</exception>
        public static bool SequenceEqual<TSource>(
          this IQueryable<TSource> source1,
          IEnumerable<TSource> source2)
        {
            if (source1 == null)
                throw Error.ArgumentNull(nameof(source1));
            if (source2 == null)
                throw Error.ArgumentNull(nameof(source2));
            return source1.Provider.Execute<bool>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source1.Expression, Queryable.GetSourceExpression<TSource>(source2)));
        }

        /// <summary>Determines whether two sequences are equal by using a specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare elements.</summary>
        /// <param name="source1">An <see cref="T:System.Linq.IQueryable`1" /> whose elements to compare to those of <paramref name="source2" />.</param>
        /// <param name="source2">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements to compare to those of the first sequence.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to use to compare elements.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>
        /// <see langword="true" /> if the two source sequences are of equal length and their corresponding elements compare equal; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source1" /> or <paramref name="source2" /> is <see langword="null" />.</exception>
        public static bool SequenceEqual<TSource>(
          this IQueryable<TSource> source1,
          IEnumerable<TSource> source2,
          IEqualityComparer<TSource> comparer)
        {
            if (source1 == null)
                throw Error.ArgumentNull(nameof(source1));
            if (source2 == null)
                throw Error.ArgumentNull(nameof(source2));
            return source1.Provider.Execute<bool>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source1.Expression, Queryable.GetSourceExpression<TSource>(source2), (Expression)Expression.Constant((object)comparer, typeof(IEqualityComparer<TSource>))));
        }

        /// <summary>Determines whether a sequence contains any elements.</summary>
        /// <param name="source">A sequence to check for being empty.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>
        /// <see langword="true" /> if the source sequence contains any elements; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static bool Any<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<bool>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        /// <summary>Determines whether any element of a sequence satisfies a condition.</summary>
        /// <param name="source">A sequence whose elements to test for a condition.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>
        /// <see langword="true" /> if any elements in the source sequence pass the test in the specified predicate; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        public static bool Any<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.Execute<bool>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Determines whether all the elements of a sequence satisfy a condition.</summary>
        /// <param name="source">A sequence whose elements to test for a condition.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>
        /// <see langword="true" /> if every element of the source sequence passes the test in the specified predicate, or if the sequence is empty; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        public static bool All<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.Execute<bool>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Returns the number of elements in a sequence.</summary>
        /// <param name="source">The <see cref="T:System.Linq.IQueryable`1" /> that contains the elements to be counted.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The number of elements in the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The number of elements in <paramref name="source" /> is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
        public static int Count<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<int>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        /// <summary>Returns the number of elements in the specified sequence that satisfies a condition.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> that contains the elements to be counted.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The number of elements in the sequence that satisfies the condition in the predicate function.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The number of elements in <paramref name="source" /> is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
        public static int Count<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.Execute<int>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Returns an <see cref="T:System.Int64" /> that represents the total number of elements in a sequence.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> that contains the elements to be counted.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The number of elements in <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The number of elements exceeds <see cref="F:System.Int64.MaxValue" />.</exception>
        public static long LongCount<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<long>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        /// <summary>Returns an <see cref="T:System.Int64" /> that represents the number of elements in a sequence that satisfy a condition.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> that contains the elements to be counted.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The number of elements in <paramref name="source" /> that satisfy the condition in the predicate function.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The number of matching elements exceeds <see cref="F:System.Int64.MaxValue" />.</exception>
        public static long LongCount<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, bool>> predicate)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (predicate == null)
                throw Error.ArgumentNull(nameof(predicate));
            return source.Provider.Execute<long>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)predicate)));
        }

        /// <summary>Returns the minimum value of a generic <see cref="T:System.Linq.IQueryable`1" />.</summary>
        /// <param name="source">A sequence of values to determine the minimum of.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static TSource Min<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        /// <summary>Invokes a projection function on each element of a generic <see cref="T:System.Linq.IQueryable`1" /> and returns the minimum resulting value.</summary>
        /// <param name="source">A sequence of values to determine the minimum of.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by the function represented by <paramref name="selector" />.</typeparam>
        /// <returns>The minimum value in the sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static TResult Min<TSource, TResult>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TResult>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Returns the maximum value in a generic <see cref="T:System.Linq.IQueryable`1" />.</summary>
        /// <param name="source">A sequence of values to determine the maximum of.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static TSource Max<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        /// <summary>Invokes a projection function on each element of a generic <see cref="T:System.Linq.IQueryable`1" /> and returns the maximum resulting value.</summary>
        /// <param name="source">A sequence of values to determine the maximum of.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by the function represented by <paramref name="selector" />.</typeparam>
        /// <returns>The maximum value in the sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static TResult Max<TSource, TResult>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TResult>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the sum of a sequence of <see cref="T:System.Int32" /> values.</summary>
        /// <param name="source">A sequence of <see cref="T:System.Int32" /> values to calculate the sum of.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
        public static int Sum(this IQueryable<int> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<int>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the sum of a sequence of nullable <see cref="T:System.Int32" /> values.</summary>
        /// <param name="source">A sequence of nullable <see cref="T:System.Int32" /> values to calculate the sum of.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
        public static int? Sum(this IQueryable<int?> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<int?>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the sum of a sequence of <see cref="T:System.Int64" /> values.</summary>
        /// <param name="source">A sequence of <see cref="T:System.Int64" /> values to calculate the sum of.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
        public static long Sum(this IQueryable<long> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<long>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the sum of a sequence of nullable <see cref="T:System.Int64" /> values.</summary>
        /// <param name="source">A sequence of nullable <see cref="T:System.Int64" /> values to calculate the sum of.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
        public static long? Sum(this IQueryable<long?> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<long?>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the sum of a sequence of <see cref="T:System.Single" /> values.</summary>
        /// <param name="source">A sequence of <see cref="T:System.Single" /> values to calculate the sum of.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static float Sum(this IQueryable<float> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<float>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the sum of a sequence of nullable <see cref="T:System.Single" /> values.</summary>
        /// <param name="source">A sequence of nullable <see cref="T:System.Single" /> values to calculate the sum of.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static float? Sum(this IQueryable<float?> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<float?>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the sum of a sequence of <see cref="T:System.Double" /> values.</summary>
        /// <param name="source">A sequence of <see cref="T:System.Double" /> values to calculate the sum of.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static double Sum(this IQueryable<double> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<double>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the sum of a sequence of nullable <see cref="T:System.Double" /> values.</summary>
        /// <param name="source">A sequence of nullable <see cref="T:System.Double" /> values to calculate the sum of.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static double? Sum(this IQueryable<double?> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<double?>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the sum of a sequence of <see cref="T:System.Decimal" /> values.</summary>
        /// <param name="source">A sequence of <see cref="T:System.Decimal" /> values to calculate the sum of.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
        public static Decimal Sum(this IQueryable<Decimal> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<Decimal>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the sum of a sequence of nullable <see cref="T:System.Decimal" /> values.</summary>
        /// <param name="source">A sequence of nullable <see cref="T:System.Decimal" /> values to calculate the sum of.</param>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
        public static Decimal? Sum(this IQueryable<Decimal?> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<Decimal?>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the sum of the sequence of <see cref="T:System.Int32" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values of type <paramref name="TSource" />.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
        public static int Sum<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, int>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<int>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the sum of the sequence of nullable <see cref="T:System.Int32" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values of type <paramref name="TSource" />.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
        public static int? Sum<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, int?>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<int?>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the sum of the sequence of <see cref="T:System.Int64" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values of type <paramref name="TSource" />.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
        public static long Sum<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, long>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<long>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the sum of the sequence of nullable <see cref="T:System.Int64" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values of type <paramref name="TSource" />.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
        public static long? Sum<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, long?>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<long?>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the sum of the sequence of <see cref="T:System.Single" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values of type <paramref name="TSource" />.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static float Sum<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, float>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<float>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the sum of the sequence of nullable <see cref="T:System.Single" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values of type <paramref name="TSource" />.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static float? Sum<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, float?>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<float?>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the sum of the sequence of <see cref="T:System.Double" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values of type <paramref name="TSource" />.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static double Sum<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, double>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<double>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the sum of the sequence of nullable <see cref="T:System.Double" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values of type <paramref name="TSource" />.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static double? Sum<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, double?>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<double?>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the sum of the sequence of <see cref="T:System.Decimal" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values of type <paramref name="TSource" />.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
        public static Decimal Sum<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, Decimal>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<Decimal>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the sum of the sequence of nullable <see cref="T:System.Decimal" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values of type <paramref name="TSource" />.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The sum of the projected values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
        public static Decimal? Sum<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, Decimal?>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<Decimal?>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the average of a sequence of <see cref="T:System.Int32" /> values.</summary>
        /// <param name="source">A sequence of <see cref="T:System.Int32" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static double Average(this IQueryable<int> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<double>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the average of a sequence of nullable <see cref="T:System.Int32" /> values.</summary>
        /// <param name="source">A sequence of nullable <see cref="T:System.Int32" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values, or <see langword="null" /> if the source sequence is empty or contains only <see langword="null" /> values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static double? Average(this IQueryable<int?> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<double?>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the average of a sequence of <see cref="T:System.Int64" /> values.</summary>
        /// <param name="source">A sequence of <see cref="T:System.Int64" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static double Average(this IQueryable<long> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<double>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the average of a sequence of nullable <see cref="T:System.Int64" /> values.</summary>
        /// <param name="source">A sequence of nullable <see cref="T:System.Int64" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values, or <see langword="null" /> if the source sequence is empty or contains only <see langword="null" /> values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static double? Average(this IQueryable<long?> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<double?>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the average of a sequence of <see cref="T:System.Single" /> values.</summary>
        /// <param name="source">A sequence of <see cref="T:System.Single" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static float Average(this IQueryable<float> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<float>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the average of a sequence of nullable <see cref="T:System.Single" /> values.</summary>
        /// <param name="source">A sequence of nullable <see cref="T:System.Single" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values, or <see langword="null" /> if the source sequence is empty or contains only <see langword="null" /> values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static float? Average(this IQueryable<float?> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<float?>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the average of a sequence of <see cref="T:System.Double" /> values.</summary>
        /// <param name="source">A sequence of <see cref="T:System.Double" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static double Average(this IQueryable<double> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<double>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the average of a sequence of nullable <see cref="T:System.Double" /> values.</summary>
        /// <param name="source">A sequence of nullable <see cref="T:System.Double" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values, or <see langword="null" /> if the source sequence is empty or contains only <see langword="null" /> values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static double? Average(this IQueryable<double?> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<double?>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the average of a sequence of <see cref="T:System.Decimal" /> values.</summary>
        /// <param name="source">A sequence of <see cref="T:System.Decimal" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static Decimal Average(this IQueryable<Decimal> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<Decimal>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the average of a sequence of nullable <see cref="T:System.Decimal" /> values.</summary>
        /// <param name="source">A sequence of nullable <see cref="T:System.Decimal" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values, or <see langword="null" /> if the source sequence is empty or contains only <see langword="null" /> values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        public static Decimal? Average(this IQueryable<Decimal?> source)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            return source.Provider.Execute<Decimal?>((Expression)Expression.Call((Expression)null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
        }

        /// <summary>Computes the average of a sequence of <see cref="T:System.Int32" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static double Average<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, int>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<double>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the average of a sequence of nullable <see cref="T:System.Int32" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values, or <see langword="null" /> if the <paramref name="source" /> sequence is empty or contains only <see langword="null" /> values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static double? Average<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, int?>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<double?>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the average of a sequence of <see cref="T:System.Single" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static float Average<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, float>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<float>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the average of a sequence of nullable <see cref="T:System.Single" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values, or <see langword="null" /> if the <paramref name="source" /> sequence is empty or contains only <see langword="null" /> values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static float? Average<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, float?>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<float?>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the average of a sequence of <see cref="T:System.Int64" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static double Average<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, long>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<double>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the average of a sequence of nullable <see cref="T:System.Int64" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values, or <see langword="null" /> if the <paramref name="source" /> sequence is empty or contains only <see langword="null" /> values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static double? Average<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, long?>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<double?>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the average of a sequence of <see cref="T:System.Double" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static double Average<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, double>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<double>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the average of a sequence of nullable <see cref="T:System.Double" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values, or <see langword="null" /> if the <paramref name="source" /> sequence is empty or contains only <see langword="null" /> values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static double? Average<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, double?>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<double?>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the average of a sequence of <see cref="T:System.Decimal" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values that are used to calculate an average.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static Decimal Average<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, Decimal>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<Decimal>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Computes the average of a sequence of nullable <see cref="T:System.Decimal" /> values that is obtained by invoking a projection function on each element of the input sequence.</summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values, or <see langword="null" /> if the <paramref name="source" /> sequence is empty or contains only <see langword="null" /> values.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static Decimal? Average<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, Decimal?>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<Decimal?>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)selector)));
        }

        /// <summary>Applies an accumulator function over a sequence.</summary>
        /// <param name="source">A sequence to aggregate over.</param>
        /// <param name="func">An accumulator function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The final accumulator value.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="func" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static TSource Aggregate<TSource>(
          this IQueryable<TSource> source,
          Expression<Func<TSource, TSource, TSource>> func)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (func == null)
                throw Error.ArgumentNull(nameof(func));
            return source.Provider.Execute<TSource>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, (Expression)Expression.Quote((Expression)func)));
        }

        /// <summary>Applies an accumulator function over a sequence. The specified seed value is used as the initial accumulator value.</summary>
        /// <param name="source">A sequence to aggregate over.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to invoke on each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <returns>The final accumulator value.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="func" /> is <see langword="null" />.</exception>
        public static TAccumulate Aggregate<TSource, TAccumulate>(
          this IQueryable<TSource> source,
          TAccumulate seed,
          Expression<Func<TAccumulate, TSource, TAccumulate>> func)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (func == null)
                throw Error.ArgumentNull(nameof(func));
            return source.Provider.Execute<TAccumulate>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TAccumulate)), source.Expression, (Expression)Expression.Constant((object)seed), (Expression)Expression.Quote((Expression)func)));
        }

        /// <summary>Applies an accumulator function over a sequence. The specified seed value is used as the initial accumulator value, and the specified function is used to select the result value.</summary>
        /// <param name="source">A sequence to aggregate over.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to invoke on each element.</param>
        /// <param name="selector">A function to transform the final accumulator value into the result value.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <returns>The transformed final accumulator value.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="func" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
        public static TResult Aggregate<TSource, TAccumulate, TResult>(
          this IQueryable<TSource> source,
          TAccumulate seed,
          Expression<Func<TAccumulate, TSource, TAccumulate>> func,
          Expression<Func<TAccumulate, TResult>> selector)
        {
            if (source == null)
                throw Error.ArgumentNull(nameof(source));
            if (func == null)
                throw Error.ArgumentNull(nameof(func));
            if (selector == null)
                throw Error.ArgumentNull(nameof(selector));
            return source.Provider.Execute<TResult>((Expression)Expression.Call((Expression)null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TAccumulate), typeof(TResult)), source.Expression, (Expression)Expression.Constant((object)seed), (Expression)Expression.Quote((Expression)func), (Expression)Expression.Quote((Expression)selector)));
        }
    }
}
