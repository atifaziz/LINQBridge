using System.Text;

namespace System.Linq.Expressions
{
    /// <summary>Represents an operation between an expression and a type.</summary>
    public sealed class TypeBinaryExpression : Expression
    {
        private Expression expression;
        private Type typeop;

        internal TypeBinaryExpression(
            ExpressionType nt,
            Expression expression,
            Type typeop,
            Type resultType)
            : base(nt, resultType)
        {
            this.expression = expression;
            this.typeop = typeop;
        }

        /// <summary>Gets the expression operand of a type test operation.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the expression operand of a type test operation.</returns>
        public Expression Expression => this.expression;

        /// <summary>Gets the type operand of a type test operation.</summary>
        /// <returns>A <see cref="T:System.Type" /> that represents the type operand of a type test operation.</returns>
        public Type TypeOperand => this.typeop;

        internal override void BuildString(StringBuilder builder)
        {
            if (builder == null)
                throw Error.ArgumentNull(nameof(builder));
            builder.Append("(");
            this.expression.BuildString(builder);
            builder.Append(" Is ");
            builder.Append(this.typeop.Name);
            builder.Append(")");
        }
    }
}