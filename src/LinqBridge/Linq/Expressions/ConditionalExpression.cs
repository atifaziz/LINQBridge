using System.Text;

namespace System.Linq.Expressions
{
    /// <summary>Represents an expression that has a conditional operator.</summary>
    public sealed class ConditionalExpression : Expression
    {
        private Expression test;
        private Expression ifTrue;
        private Expression ifFalse;

        internal ConditionalExpression(
          Expression test,
          Expression ifTrue,
          Expression ifFalse,
          Type type)
          : base(ExpressionType.Conditional, type)
        {
            this.test = test;
            this.ifTrue = ifTrue;
            this.ifFalse = ifFalse;
        }

        /// <summary>Gets the test of the conditional operation.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the test of the conditional operation.</returns>
        public Expression Test => this.test;

        /// <summary>Gets the expression to execute if the test evaluates to true.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the expression to execute if the test is true.</returns>
        public Expression IfTrue => this.ifTrue;

        /// <summary>Gets the expression to execute if the test evaluates to false.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the expression to execute if the test is false.</returns>
        public Expression IfFalse => this.ifFalse;

        internal override void BuildString(StringBuilder builder)
        {
            if (builder == null)
                throw Error.ArgumentNull(nameof(builder));
            builder.Append("IIF(");
            this.test.BuildString(builder);
            builder.Append(", ");
            this.ifTrue.BuildString(builder);
            builder.Append(", ");
            this.ifFalse.BuildString(builder);
            builder.Append(")");
        }
    }
}
