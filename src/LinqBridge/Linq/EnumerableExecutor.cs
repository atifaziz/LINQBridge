using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq
{
    /// <summary>Represents an expression tree and provides functionality to execute the expression tree after rewriting it.</summary>
    internal abstract class EnumerableExecutor
    {
        internal abstract object ExecuteBoxed();

        internal static EnumerableExecutor Create(Expression expression) => (EnumerableExecutor)Activator.CreateInstance(typeof(EnumerableExecutor<>).MakeGenericType(expression.Type), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, new object[1]
        {
            (object) expression
        }, (CultureInfo)null);
    }

    /// <summary>Represents an expression tree and provides functionality to execute the expression tree after rewriting it.</summary>
    /// <typeparam name="T">The data type of the value that results from executing the expression tree.</typeparam>
    internal class EnumerableExecutor<T> : EnumerableExecutor
    {
        private Expression expression;
        private Func<T> func;

        /// <summary>Initializes a new instance of the <see cref="T:System.Linq.EnumerableExecutor`1" /> class.</summary>
        /// <param name="expression">An expression tree to associate with the new instance.</param>
        internal EnumerableExecutor(Expression expression) => this.expression = expression;

        internal override object ExecuteBoxed() => (object)this.Execute();

        internal T Execute()
        {
            if (this.func == null)
                this.func = new ExpressionCompiler().Compile<Func<T>>(Expression.Lambda<Func<T>>(new EnumerableRewriter().Visit(this.expression), (IEnumerable<ParameterExpression>)null));
            return this.func();
        }

    }
}