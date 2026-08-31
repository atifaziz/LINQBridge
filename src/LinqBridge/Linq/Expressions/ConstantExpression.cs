using System.Text;

namespace System.Linq.Expressions
{
    /// <summary>Represents an expression that has a constant value.</summary>
    public sealed class ConstantExpression : Expression
    {
        private object value;

        internal ConstantExpression(object value, Type type)
            : base(ExpressionType.Constant, type)
        {
            this.value = value;
        }

        /// <summary>Gets the value of the constant expression.</summary>
        /// <returns>An <see cref="T:System.Object" /> equal to the value of the represented expression.</returns>
        public object Value => this.value;

        internal override void BuildString(StringBuilder builder)
        {
            if (builder == null)
                throw Error.ArgumentNull(nameof(builder));
            if (this.value != null)
            {
                if (this.value is string)
                {
                    builder.Append("\"");
                    builder.Append(this.value);
                    builder.Append("\"");
                }
                else if (this.value.ToString() == this.value.GetType().ToString())
                {
                    builder.Append("value(");
                    builder.Append(this.value);
                    builder.Append(")");
                }
                else
                    builder.Append(this.value);
            }
            else
                builder.Append("null");
        }
    }
}