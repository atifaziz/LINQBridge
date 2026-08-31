using System.Collections.ObjectModel;
using System.Text;

namespace System.Linq.Expressions
{
    /// <summary>Represents an expression that applies a delegate or lambda expression to a list of argument expressions.</summary>
    public sealed class InvocationExpression : Expression
    {
        private ReadOnlyCollection<Expression> arguments;
        private Expression lambda;

        internal InvocationExpression(
          Expression lambda,
          Type returnType,
          ReadOnlyCollection<Expression> arguments)
          : base(ExpressionType.Invoke, returnType)
        {
            this.lambda = lambda;
            this.arguments = arguments;
        }

        /// <summary>Gets the delegate or lambda expression to be applied.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the delegate to be applied.</returns>
        public Expression Expression => this.lambda;

        /// <summary>Gets the arguments that the delegate is applied to.</summary>
        /// <returns>A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> of <see cref="T:System.Linq.Expressions.Expression" /> objects which represents the arguments that the delegate is applied to.</returns>
        public ReadOnlyCollection<Expression> Arguments => this.arguments;

        internal override void BuildString(StringBuilder builder)
        {
            builder.Append("Invoke(");
            this.lambda.BuildString(builder);
            int index = 0;
            for (int count = this.arguments.Count; index < count; ++index)
            {
                builder.Append(",");
                this.arguments[index].BuildString(builder);
            }
            builder.Append(")");
        }
    }
}
