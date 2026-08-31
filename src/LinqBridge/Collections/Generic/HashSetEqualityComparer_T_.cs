using System;

namespace System.Collections.Generic
{
	[Serializable]
	internal class HashSetEqualityComparer<T> : IEqualityComparer<HashSet<T>>
	{
		private IEqualityComparer<T> m_comparer;

		public HashSetEqualityComparer()
		{
			this.m_comparer = EqualityComparer<T>.Default;
		}

		public HashSetEqualityComparer(IEqualityComparer<T> comparer)
		{
			if (this.m_comparer == null)
			{
				this.m_comparer = EqualityComparer<T>.Default;
				return;
			}
			this.m_comparer = comparer;
		}

		public bool Equals(HashSet<T> x, HashSet<T> y)
		{
			return HashSet<T>.HashSetEquals(x, y, this.m_comparer);
		}

		public override bool Equals(object obj)
		{
			HashSetEqualityComparer<T> hashSetEqualityComparer = obj as HashSetEqualityComparer<T>;
			if (hashSetEqualityComparer == null)
			{
				return false;
			}
			return this.m_comparer == hashSetEqualityComparer.m_comparer;
		}

		public int GetHashCode(HashSet<T> obj)
		{
			int hashCode = 0;
			if (obj != null)
			{
				foreach (T t in obj)
				{
					hashCode = hashCode ^ this.m_comparer.GetHashCode(t) & 2147483647;
				}
			}
			return hashCode;
		}

		public override int GetHashCode()
		{
			return this.m_comparer.GetHashCode();
		}
	}
}