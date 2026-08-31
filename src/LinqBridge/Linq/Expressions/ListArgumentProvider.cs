using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions
{
    internal class ListArgumentProvider : IList<Expression>, ICollection<Expression>, IEnumerable<Expression>, IEnumerable
    {
        private readonly IArgumentProvider _provider;

        private readonly Expression _arg0;

        public int Count
        {
            get
            {
                return this._provider.ArgumentCount;
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
                return this._provider.GetArgument(index);
            }
            set
            {
                throw ContractUtils.Unreachable;
            }
        }

        internal ListArgumentProvider(IArgumentProvider provider, Expression arg0)
        {
            this._provider = provider;
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
            for (int i = 1; i < this._provider.ArgumentCount; i++)
            {
                int num1 = arrayIndex;
                arrayIndex = num1 + 1;
                array[num1] = this._provider.GetArgument(i);
            }
        }

        public IEnumerator<Expression> GetEnumerator()
        {
            ListArgumentProvider listArgumentProviders = null;
            yield return listArgumentProviders._arg0;
            for (int i = 1; i < listArgumentProviders._provider.ArgumentCount; i++)
            {
                yield return listArgumentProviders._provider.GetArgument(i);
            }
        }

        public int IndexOf(Expression item)
        {
            if (this._arg0 == item)
            {
                return 0;
            }
            for (int i = 1; i < this._provider.ArgumentCount; i++)
            {
                if (this._provider.GetArgument(i) == item)
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
            ListArgumentProvider listArgumentProviders = null;
            yield return listArgumentProviders._arg0;
            for (int i = 1; i < listArgumentProviders._provider.ArgumentCount; i++)
            {
                yield return listArgumentProviders._provider.GetArgument(i);
            }
        }
    }
}