using System.Linq.Expressions;
using System.Reflection.Emit;

namespace System.Runtime.CompilerServices
{
    /// <summary>Represents the runtime state of a dynamically generated method.</summary>
    /// <filterpriority>2</filterpriority>
    public class ExecutionScope
    {
        /// <summary>Represents the execution scope of the calling delegate.</summary>
        public ExecutionScope Parent;
        /// <summary>Represents the non-trivial constants and locally executable expressions that are referenced by a dynamically generated method.</summary>
        public object[] Globals;
        /// <summary>Represents the hoisted local variables from the parent context.</summary>
        public object[] Locals;
        private ExpressionCompiler.LambdaInfo Lambda;

        internal ExecutionScope(
          ExecutionScope parent,
          ExpressionCompiler.LambdaInfo lambda,
          object[] globals,
          object[] locals)
        {
            this.Parent = parent;
            this.Lambda = lambda;
            this.Globals = globals;
            this.Locals = locals;
        }

        /// <summary>Creates an array to store the hoisted local variables.</summary>
        /// <returns>An array to store hoisted local variables.</returns>
        public object[] CreateHoistedLocals() => new object[this.Lambda.HoistedLocals.Count];

        /// <summary>Creates a delegate that can be used to execute a dynamically generated method.</summary>
        /// <returns>A <see cref="T:System.Delegate" /> that can execute a dynamically generated method.</returns>
        /// <param name="indexLambda">The index of the object that stores information about associated lambda expression of the dynamic method.</param>
        /// <param name="locals">An array that contains the hoisted local variables from the parent context.</param>
        public Delegate CreateDelegate(int indexLambda, object[] locals)
        {
            ExpressionCompiler.LambdaInfo lambda = this.Lambda.Lambdas[indexLambda];
            ExecutionScope target = new ExecutionScope(this, lambda, this.Globals, locals);
            return ((DynamicMethod)lambda.Method).CreateDelegate(lambda.Lambda.Type, (object)target);
        }

        /// <summary>Frees a specified expression tree of external parameter references by replacing the parameter with its current value.</summary>
        /// <returns>An expression tree that does not contain external parameter references.</returns>
        /// <param name="expression">An expression tree to free of external parameter references.</param>
        /// <param name="locals">An array that contains the hoisted local variables.</param>
        public Expression IsolateExpression(Expression expression, object[] locals) => new ExecutionScope.ExpressionIsolator(this, locals).Visit(expression);

        private class ExpressionIsolator : ExpressionVisitor
        {
            private ExecutionScope top;
            private object[] toplocals;

            internal ExpressionIsolator(ExecutionScope top, object[] toplocals)
            {
                this.top = top;
                this.toplocals = toplocals;
            }

            internal override Expression VisitParameter(ParameterExpression p)
            {
                ExecutionScope executionScope = this.top;
                object[] objArray = this.toplocals;
                for (; executionScope != null; executionScope = executionScope.Parent)
                {
                    int index;
                    if (executionScope.Lambda.HoistedLocals.TryGetValue(p, out index))
                        return (Expression)Expression.Field((Expression)Expression.Convert((Expression)Expression.ArrayIndex((Expression)Expression.Constant((object)objArray, typeof(object[])), (Expression)Expression.Constant((object)index, typeof(int))), objArray[index].GetType()), "Value");
                    objArray = executionScope.Locals;
                }
                return (Expression)p;
            }
        }
    }
}
