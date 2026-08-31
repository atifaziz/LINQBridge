using System;
using System.Collections.ObjectModel;

namespace System.Linq.Expressions
{
    internal sealed class Block2 : BlockExpression
    {
        private object _arg0;

        private readonly Expression _arg1;

        internal override int ExpressionCount
        {
            get
            {
                return 2;
            }
        }

        internal Block2(Expression arg0, Expression arg1): base()
        {

            this._arg0 = arg0;
            this._arg1 = arg1;
        }

        internal override Expression GetExpression(int index)
        {
            if (index == 0)
            {
                return Expression.ReturnObject<Expression>(this._arg0);
            }
            if (index != 1)
            {
                throw new InvalidOperationException();
            }
            return this._arg1;
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeExpressions()
        {
            return BlockExpression.ReturnReadOnlyExpressions(this, ref this._arg0);
        }

        internal override BlockExpression Rewrite(ReadOnlyCollection<ParameterExpression> variables, Expression[] args)
        {
            return new Block2(args[0], args[1]);
        }
    }
}