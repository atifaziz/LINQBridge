using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq
{
    /// <summary>Represents an <see cref="T:System.Collections.IEnumerable" /> as an <see cref="T:System.Linq.EnumerableQuery" /> data source. </summary>
    internal abstract class EnumerableQuery
    {
        internal abstract Expression Expression { get; }

        internal abstract IEnumerable Enumerable { get; }

        internal static IQueryable Create(Type elementType, IEnumerable sequence) => (IQueryable)Activator.CreateInstance(typeof(EnumerableQuery<>).MakeGenericType(elementType), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, new object[1]
        {
            (object) sequence
        }, (CultureInfo)null);

        internal static IQueryable Create(Type elementType, Expression expression) => (IQueryable)Activator.CreateInstance(typeof(EnumerableQuery<>).MakeGenericType(elementType), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, new object[1]
        {
            (object) expression
        }, (CultureInfo)null);
    }

    /// <summary>Represents an <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection as an <see cref="T:System.Linq.IQueryable`1" /> data source.</summary>
    /// <typeparam name="T">The type of the data in the collection.</typeparam>
    internal class EnumerableQuery<T> :
      EnumerableQuery,
      IOrderedQueryable<T>,
      IQueryable<T>,
      IOrderedQueryable,
      IQueryable,
      IQueryProvider,
      IEnumerable<T>,
      IEnumerable
    {
        private Expression expression;
        private IEnumerable<T> enumerable;

        /// <summary>Gets the query provider that is associated with this instance.</summary>
        /// <returns>The query provider that is associated with this instance.</returns>
        IQueryProvider IQueryable.Provider => (IQueryProvider)this;

        /// <summary>Initializes a new instance of the <see cref="T:System.Linq.EnumerableQuery`1" /> class and associates it with an <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection.</summary>
        /// <param name="enumerable">A collection to associate with the new instance.</param>
        internal EnumerableQuery(IEnumerable<T> enumerable)
        {
            this.enumerable = enumerable;
            this.expression = (Expression)Expression.Constant((object)this);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Linq.EnumerableQuery`1" /> class and associates the instance with an expression tree.</summary>
        /// <param name="expression">An expression tree to associate with the new instance.</param>
        internal EnumerableQuery(Expression expression) => this.expression = expression;

        internal override Expression Expression => this.expression;

        internal override IEnumerable Enumerable => (IEnumerable)this.enumerable;

        /// <summary>Gets the expression tree that is associated with or that represents this instance.</summary>
        /// <returns>The expression tree that is associated with or that represents this instance.</returns>
        Expression IQueryable.Expression => this.expression;

        /// <summary>Gets the type of the data in the collection that this instance represents.</summary>
        /// <returns>The type of the data in the collection that this instance represents.</returns>
        Type IQueryable.ElementType => typeof(T);

        /// <summary>Constructs a new <see cref="T:System.Linq.EnumerableQuery`1" /> object and associates it with a specified expression tree that represents an <see cref="T:System.Linq.IQueryable" /> collection of data.</summary>
        /// <param name="expression">An expression tree that represents an <see cref="T:System.Linq.IQueryable" /> collection of data.</param>
        /// <returns>An <see cref="T:System.Linq.EnumerableQuery`1" /> object that is associated with <paramref name="expression" />.</returns>
        IQueryable IQueryProvider.CreateQuery(Expression expression) => EnumerableQuery.Create(((expression != null ? TypeHelper.FindGenericType(typeof(IQueryable<>), expression.Type) : throw Error.ArgumentNull(nameof(expression))) ?? throw new ArgumentException(nameof(expression))).GetGenericArguments()[0], expression);

        /// <summary>Constructs a new <see cref="T:System.Linq.EnumerableQuery`1" /> object and associates it with a specified expression tree that represents an <see cref="T:System.Linq.IQueryable`1" /> collection of data.</summary>
        /// <param name="expression">An expression tree to execute.</param>
        /// <typeparam name="S">The type of the data in the collection that <paramref name="expression" /> represents.</typeparam>
        /// <returns>An EnumerableQuery object that is associated with <paramref name="expression" />.</returns>
        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            return typeof(IQueryable<S>).IsAssignableFrom(expression.Type) ? (IQueryable<S>)new EnumerableQuery<S>(expression) : throw new ArgumentException(nameof(expression));
        }

        /// <summary>Executes an expression after rewriting it to call <see cref="T:System.Linq.Enumerable" /> methods instead of <see cref="T:System.Linq.Queryable" /> methods on any enumerable data sources that cannot be queried by <see cref="T:System.Linq.Queryable" /> methods.</summary>
        /// <param name="expression">An expression tree to execute.</param>
        /// <returns>The value that results from executing <paramref name="expression" />.</returns>
        object IQueryProvider.Execute(Expression expression)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            typeof(EnumerableExecutor<>).MakeGenericType(expression.Type);
            return EnumerableExecutor.Create(expression).ExecuteBoxed();
        }

        /// <summary>Executes an expression after rewriting it to call <see cref="T:System.Linq.Enumerable" /> methods instead of <see cref="T:System.Linq.Queryable" /> methods on any enumerable data sources that cannot be queried by <see cref="T:System.Linq.Queryable" /> methods.</summary>
        /// <param name="expression">An expression tree to execute.</param>
        /// <typeparam name="S">The type of the data in the collection that <paramref name="expression" /> represents.</typeparam>
        /// <returns>The value that results from executing <paramref name="expression" />.</returns>
        S IQueryProvider.Execute<S>(Expression expression)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            return typeof(S).IsAssignableFrom(expression.Type) ? new EnumerableExecutor<S>(expression).Execute() : throw new ArgumentException(nameof(expression));
        }

        /// <summary>Returns an enumerator that can iterate through the associated <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection, or, if it is null, through the collection that results from rewriting the associated expression tree as a query on an <see cref="T:System.Collections.Generic.IEnumerable`1" /> data source and executing it.</summary>
        /// <returns>An enumerator that can be used to iterate through the associated data source.</returns>
        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();

        /// <summary>Returns an enumerator that can iterate through the associated <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection, or, if it is null, through the collection that results from rewriting the associated expression tree as a query on an <see cref="T:System.Collections.Generic.IEnumerable`1" /> data source and executing it.</summary>
        /// <returns>An enumerator that can be used to iterate through the associated data source.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();

        private IEnumerator<T> GetEnumerator()
        {
            if (this.enumerable == null)
                this.enumerable = new ExpressionCompiler().Compile<Func<IEnumerable<T>>>(Expression.Lambda<Func<IEnumerable<T>>>(new EnumerableRewriter().Visit(this.expression), (IEnumerable<ParameterExpression>)null))();
            return this.enumerable.GetEnumerator();
        }

        /// <summary>Returns a textual representation of the enumerable collection or, if it is null, of the expression tree that is associated with this instance.</summary>
        /// <returns>A textual representation of the enumerable collection or, if it is null, of the expression tree that is associated with this instance.</returns>
        public override string ToString()
        {
            if (!(this.expression is ConstantExpression expression) || expression.Value != this)
                return this.expression.ToString();
            return this.enumerable != null ? this.enumerable.ToString() : "null";
        }
    }

}