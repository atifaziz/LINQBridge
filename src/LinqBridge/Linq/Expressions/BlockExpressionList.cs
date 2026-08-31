using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions
{
    internal class BlockExpressionList : IList<Expression>, ICollection<Expression>, IEnumerable<Expression>, IEnumerable
    {
        private readonly BlockExpression _block;

        private readonly Expression _arg0;

        public int Count
        {
            get
            {
                return this._block.ExpressionCount;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public Expression this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return this._arg0;
                }
                return this._block.GetExpression(index);
            }
            set
            {
                throw ContractUtils.Unreachable;
            }
        }

        internal BlockExpressionList(BlockExpression provider, Expression arg0)
        {
            this._block = provider;
            this._arg0 = arg0;
        }

        public void Add(Expression item)
        {
            throw ContractUtils.Unreachable;
        }

        public void Clear()
        {
            throw ContractUtils.Unreachable;
        }

        public bool Contains(Expression item)
        {
            return this.IndexOf(item) != -1;
        }

        public void CopyTo(Expression[] array, int arrayIndex)
        {
            int num = arrayIndex;
            arrayIndex = num + 1;
            array[num] = this._arg0;
            for (int i = 1; i < this._block.ExpressionCount; i++)
            {
                int num1 = arrayIndex;
                arrayIndex = num1 + 1;
                array[num1] = this._block.GetExpression(i);
            }
        }

        public IEnumerator<Expression> GetEnumerator()
        {
            BlockExpressionList blockExpressionLists = null;
            yield return blockExpressionLists._arg0;
            for (int i = 1; i < blockExpressionLists._block.ExpressionCount; i++)
            {
                yield return blockExpressionLists._block.GetExpression(i);
            }
        }

        public int IndexOf(Expression item)
        {
            if (this._arg0 == item)
            {
                return 0;
            }
            for (int i = 1; i < this._block.ExpressionCount; i++)
            {
                if (this._block.GetExpression(i) == item)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, Expression item)
        {
            throw ContractUtils.Unreachable;
        }

        public bool Remove(Expression item)
        {
            throw ContractUtils.Unreachable;
        }

        public void RemoveAt(int index)
        {
            throw ContractUtils.Unreachable;
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            BlockExpressionList blockExpressionLists = null;
            yield return blockExpressionLists._arg0;
            for (int i = 1; i < blockExpressionLists._block.ExpressionCount; i++)
            {
                yield return blockExpressionLists._block.GetExpression(i);
            }
        }
    }
}