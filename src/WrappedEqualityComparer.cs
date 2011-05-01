using System;
using System.Collections.Generic;

namespace LinqBridge
{
    internal class WrappedEqualityComparer<TKey> : IEqualityComparer<Wrapped<TKey>>
    {
        private readonly IEqualityComparer<TKey> m_InnerComparer;

        public WrappedEqualityComparer() : this(null) {}
 
        public WrappedEqualityComparer(IEqualityComparer<TKey> innerComparer)
        {
            m_InnerComparer = innerComparer ?? EqualityComparer<TKey>.Default;
        }

        #region IEqualityComparer<Wrapped<TKey>> Members

        public bool Equals(Wrapped<TKey> x, Wrapped<TKey> y)
        {
            return m_InnerComparer.Equals(x.Value, y.Value);
        }

        public int GetHashCode(Wrapped<TKey> obj)
        {
            if ((default(TKey) == null) && (obj.Value as object == null))
                return 0;

            return m_InnerComparer.GetHashCode(obj.Value);
        }

        #endregion
    }
}