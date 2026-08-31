using System.Collections.ObjectModel;
using System.Text;

namespace System.Linq.Expressions
{
    /// <summary>Represents creating a new array and possibly initializing the elements of the new array.</summary>
    public sealed class NewArrayExpression : Expression
    {
        private ReadOnlyCollection<Expression> expressions;

        internal NewArrayExpression(
          ExpressionType eType,
          Type type,
          ReadOnlyCollection<Expression> expressions)
          : base(eType, type)
        {
            this.expressions = expressions;
        }

        /// <summary>Gets the bounds of the array if the value of the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property is <see cref="F:System.Linq.Expressions.ExpressionType.NewArrayBounds" />, or the values to initialize the elements of the new array if the value of the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property is <see cref="F:System.Linq.Expressions.ExpressionType.NewArrayInit" />.</summary>
        /// <returns>A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> of <see cref="T:System.Linq.Expressions.Expression" /> objects which represent either the bounds of the array or the initialization values.</returns>
        public ReadOnlyCollection<Expression> Expressions => this.expressions;

        internal override void BuildString(StringBuilder builder)
        {
            switch (this.NodeType)
            {
                case ExpressionType.NewArrayInit:
                    builder.Append("new ");
                    builder.Append("[] {");
                    int index1 = 0;
                    for (int count = this.expressions.Count; index1 < count; ++index1)
                    {
                        if (index1 > 0)
                            builder.Append(", ");
                        this.expressions[index1].BuildString(builder);
                    }
                    builder.Append("}");
                    break;
                case ExpressionType.NewArrayBounds:
                    builder.Append("new ");
                    builder.Append(this.Type.ToString());
                    builder.Append("(");
                    int index2 = 0;
                    for (int count = this.expressions.Count; index2 < count; ++index2)
                    {
                        if (index2 > 0)
                            builder.Append(", ");
                        this.expressions[index2].BuildString(builder);
                    }
                    builder.Append(")");
                    break;
            }
        }
    }
}
