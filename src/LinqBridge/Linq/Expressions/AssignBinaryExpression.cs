using System;

namespace System.Linq.Expressions
{
    internal sealed class AssignBinaryExpression : BinaryExpression
    {
        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Assign;
            }
        }

        public sealed override Type Type
        {
            get
            {
                return base.Left.Type;
            }
        }

        internal AssignBinaryExpression(Expression left, Expression right) : base(ExpressionType.Assign,left, right, left.Type)
        {
        }
    }
}