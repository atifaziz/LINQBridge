using System.Reflection;
using System.Text;

namespace System.Linq.Expressions
{
    /// <summary>Represents an expression that has a binary operator.</summary>
    public class BinaryExpression : Expression
    {
        private Expression left;
        private Expression right;
        private MethodInfo method;
        private LambdaExpression conversion;

        internal BinaryExpression(ExpressionType nt, Expression left, Expression right, Type type)
          : this(nt, left, right, (MethodInfo)null, (LambdaExpression)null, type)
        {
        }

        internal BinaryExpression(
          ExpressionType nt,
          Expression left,
          Expression right,
          MethodInfo method,
          Type type)
          : this(nt, left, right, method, (LambdaExpression)null, type)
        {
        }

        internal BinaryExpression(
          ExpressionType nt,
          Expression left,
          Expression right,
          LambdaExpression conversion,
          Type type)
          : this(nt, left, right, (MethodInfo)null, conversion, type)
        {
        }

        internal BinaryExpression(
          ExpressionType nt,
          Expression left,
          Expression right,
          MethodInfo method,
          LambdaExpression conversion,
          Type type)
          : base(nt, type)
        {
            this.left = left;
            this.right = right;
            this.method = method;
            this.conversion = conversion;
        }

        /// <summary>Gets the left operand of the binary operation.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the left operand of the binary operation.</returns>
        public Expression Left => this.left;

        /// <summary>Gets the right operand of the binary operation.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the right operand of the binary operation.</returns>
        public Expression Right => this.right;

        /// <summary>Gets the implementing method for the binary operation.</summary>
        /// <returns>The <see cref="T:System.Reflection.MethodInfo" /> that represents the implementing method.</returns>
        public MethodInfo Method => this.method;

        /// <summary>Gets the type conversion function that is used by a coalescing operation.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.LambdaExpression" /> that represents a type conversion function.</returns>
        public LambdaExpression Conversion => this.conversion;

        /// <summary>Gets a value that indicates whether the expression tree node represents a lifted call to an operator.</summary>
        /// <returns>true if the node represents a lifted call; otherwise, false.</returns>
        public bool IsLifted
        {
            get
            {
                if (this.NodeType == ExpressionType.Coalesce)
                    return false;
                bool isLifted = Expression.IsNullableType(this.left.Type);
                if (this.method == null)
                    return isLifted;
                return isLifted && this.method.GetParameters()[0].ParameterType != this.left.Type;
            }
        }

        /// <summary>Gets a value that indicates whether the expression tree node represents a lifted call to an operator whose return type is lifted to a nullable type.</summary>
        /// <returns>true if the operator's return type is lifted to a nullable type; otherwise, false.</returns>
        public bool IsLiftedToNull => this.IsLifted && Expression.IsNullableType(this.Type);

        internal override void BuildString(StringBuilder builder)
        {
            if (builder == null)
                throw Error.ArgumentNull(nameof(builder));
            if (this.NodeType == ExpressionType.ArrayIndex)
            {
                this.left.BuildString(builder);
                builder.Append("[");
                this.right.BuildString(builder);
                builder.Append("]");
            }
            else
            {
                string str = this.GetOperator();
                if (str != null)
                {
                    builder.Append("(");
                    this.left.BuildString(builder);
                    builder.Append(" ");
                    builder.Append(str);
                    builder.Append(" ");
                    this.right.BuildString(builder);
                    builder.Append(")");
                }
                else
                {
                    builder.Append((object)this.NodeType);
                    builder.Append("(");
                    this.left.BuildString(builder);
                    builder.Append(", ");
                    this.right.BuildString(builder);
                    builder.Append(")");
                }
            }
        }

        private string GetOperator()
        {
            switch (this.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.And:
                    return this.Type == typeof(bool) || this.Type == typeof(bool?) ? "And" : "&";
                case ExpressionType.AndAlso:
                    return "&&";
                case ExpressionType.Coalesce:
                    return "??";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LeftShift:
                    return "<<";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.Or:
                    return this.Type == typeof(bool) || this.Type == typeof(bool?) ? "Or" : "|";
                case ExpressionType.OrElse:
                    return "||";
                case ExpressionType.Power:
                    return "^";
                case ExpressionType.RightShift:
                    return ">>";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                default:
                    return (string)null;
            }
        }
    }
}
