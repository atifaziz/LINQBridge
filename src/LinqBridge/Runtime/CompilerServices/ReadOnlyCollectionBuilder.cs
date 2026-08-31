using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace System.Runtime.CompilerServices
{
    [Serializable]
    public sealed class ReadOnlyCollectionBuilder<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection
    {
        private const int DefaultCapacity = 4;

        private T[] _items;

        private int _size;

        private int _version;

        [NonSerialized]
        private object _syncRoot;

        private readonly static T[] _emptyArray;

        public int Capacity
        {
            get
            {
                return (int)this._items.Length;
            }
            set
            {
                ContractUtils.Requires(value >= this._size, "value");
                if (value != (int)this._items.Length)
                {
                    if (value > 0)
                    {
                        T[] tArray = new T[value];
                        if (this._size > 0)
                        {
                            Array.Copy(this._items, 0, tArray, 0, this._size);
                        }
                        this._items = tArray;
                        return;
                    }
                    this._items = ReadOnlyCollectionBuilder<T>._emptyArray;
                }
            }
        }

        public int Count
        {
            get
            {
                return this._size;
            }
        }

        public T this[int index]
        {
            get
            {
                ContractUtils.Requires(index < this._size, "index");
                return this._items[index];
            }
            set
            {
                ContractUtils.Requires(index < this._size, "index");
                this._items[index] = value;
                this._version++;
            }
        }

        bool System.Collections.Generic.ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool System.Collections.ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object System.Collections.ICollection.SyncRoot
        {
            get
            {
                if (this._syncRoot == null)
                {
                    System.Threading.Net20Interlocked.CompareExchange<object>(ref this._syncRoot, new object(), null);
                }
                return this._syncRoot;
            }
        }

        bool System.Collections.IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool System.Collections.IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object System.Collections.IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                ReadOnlyCollectionBuilder<T>.ValidateNullValue(value, "value");
                try
                {
                    this[index] = (T)value;
                }
                catch (InvalidCastException invalidCastException)
                {
                    ReadOnlyCollectionBuilder<T>.ThrowInvalidTypeException(value, "value");
                }
            }
        }

        static ReadOnlyCollectionBuilder()
        {
            ReadOnlyCollectionBuilder<T>._emptyArray = new T[0];
        }

        public ReadOnlyCollectionBuilder()
        {
            this._items = ReadOnlyCollectionBuilder<T>._emptyArray;
        }

        public ReadOnlyCollectionBuilder(int capacity)
        {
            ContractUtils.Requires(capacity >= 0, "capacity");
            this._items = new T[capacity];
        }

        public ReadOnlyCollectionBuilder(IEnumerable<T> collection)
        {
            ContractUtils.Requires(collection != null, "collection");
            ICollection<T> ts = collection as ICollection<T>;
            if (ts != null)
            {
                int count = ts.Count;
                this._items = new T[count];
                ts.CopyTo(this._items, 0);
                this._size = count;
                return;
            }
            this._size = 0;
            this._items = new T[4];
            foreach (T t in collection)
            {
                this.Add(t);
            }
        }

        public void Add(T item)
        {
            if (this._size == (int)this._items.Length)
            {
                this.EnsureCapacity(this._size + 1);
            }
            T[] tArray = this._items;
            int num = this._size;
            this._size = num + 1;
            tArray[num] = item;
            this._version++;
        }

        public void Clear()
        {
            if (this._size > 0)
            {
                Array.Clear(this._items, 0, this._size);
                this._size = 0;
            }
            this._version++;
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int i = 0; i < this._size; i++)
                {
                    if (this._items[i] == null)
                    {
                        return true;
                    }
                }
                return false;
            }
            EqualityComparer<T> @default = EqualityComparer<T>.Default;
            for (int j = 0; j < this._size; j++)
            {
                if (@default.Equals(this._items[j], item))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(this._items, 0, array, arrayIndex, this._size);
        }

        private void EnsureCapacity(int min)
        {
            if ((int)this._items.Length < min)
            {
                int length = 4;
                if (this._items.Length != 0)
                {
                    length = (int)this._items.Length * 2;
                }
                if (length < min)
                {
                    length = min;
                }
                this.Capacity = length;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ReadOnlyCollectionBuilder<T>.Enumerator(this);
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf<T>(this._items, item, 0, this._size);
        }

        public void Insert(int index, T item)
        {
            ContractUtils.Requires(index <= this._size, "index");
            if (this._size == (int)this._items.Length)
            {
                this.EnsureCapacity(this._size + 1);
            }
            if (index < this._size)
            {
                Array.Copy(this._items, index, this._items, index + 1, this._size - index);
            }
            this._items[index] = item;
            this._size++;
            this._version++;
        }

        private static bool IsCompatibleObject(object value)
        {
            if (value is T)
            {
                return true;
            }
            if (value != null)
            {
                return false;
            }
            return default(T) == null;
        }

        public bool Remove(T item)
        {
            int num = this.IndexOf(item);
            if (num < 0)
            {
                return false;
            }
            this.RemoveAt(num);
            return true;
        }

        public void RemoveAt(int index)
        {
            ContractUtils.Requires((index < 0 ? false : index < this._size), "index");
            this._size--;
            if (index < this._size)
            {
                Array.Copy(this._items, index + 1, this._items, index, this._size - index);
            }
            this._items[this._size] = default(T);
            this._version++;
        }

        public void Reverse()
        {
            this.Reverse(0, this.Count);
        }

        public void Reverse(int index, int count)
        {
            ContractUtils.Requires(index >= 0, "index");
            ContractUtils.Requires(count >= 0, "count");
            Array.Reverse(this._items, index, count);
            this._version++;
        }

        void System.Collections.ICollection.CopyTo(Array array, int index)
        {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.Requires(array.Rank == 1, "array");
            Array.Copy(this._items, 0, array, index, this._size);
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        int System.Collections.IList.Add(object value)
        {
            ReadOnlyCollectionBuilder<T>.ValidateNullValue(value, "value");
            try
            {
                this.Add((T)value);
            }
            catch (InvalidCastException invalidCastException)
            {
                ReadOnlyCollectionBuilder<T>.ThrowInvalidTypeException(value, "value");
            }
            return this.Count - 1;
        }

        bool System.Collections.IList.Contains(object value)
        {
            if (!ReadOnlyCollectionBuilder<T>.IsCompatibleObject(value))
            {
                return false;
            }
            return this.Contains((T)value);
        }

        int System.Collections.IList.IndexOf(object value)
        {
            if (!ReadOnlyCollectionBuilder<T>.IsCompatibleObject(value))
            {
                return -1;
            }
            return this.IndexOf((T)value);
        }

        void System.Collections.IList.Insert(int index, object value)
        {
            ReadOnlyCollectionBuilder<T>.ValidateNullValue(value, "value");
            try
            {
                this.Insert(index, (T)value);
            }
            catch (InvalidCastException invalidCastException)
            {
                ReadOnlyCollectionBuilder<T>.ThrowInvalidTypeException(value, "value");
            }
        }

        void System.Collections.IList.Remove(object value)
        {
            if (ReadOnlyCollectionBuilder<T>.IsCompatibleObject(value))
            {
                this.Remove((T)value);
            }
        }

        private static void ThrowInvalidTypeException(object value, string argument)
        {
            object type;
            if (value != null)
            {
                type = value.GetType();
            }
            else
            {
                type = "null";
            }
            throw new ArgumentException(argument);
        }

        public T[] ToArray()
        {
            T[] tArray = new T[this._size];
            Array.Copy(this._items, 0, tArray, 0, this._size);
            return tArray;
        }

        public ReadOnlyCollection<T> ToReadOnlyCollection()
        {
            T[] tArray;
            tArray = (this._size != (int)this._items.Length ? this.ToArray() : this._items);
            this._items = ReadOnlyCollectionBuilder<T>._emptyArray;
            this._size = 0;
            this._version++;
            return new TrueReadOnlyCollection<T>(tArray);
        }

        private static void ValidateNullValue(object value, string argument)
        {
            if (value == null)
            {
                if (default(T) != null)
                {
                    throw new ArgumentException( argument);
                }
            }
        }

        [Serializable]
        private class Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private readonly ReadOnlyCollectionBuilder<T> _builder;

            private readonly int _version;

            private int _index;

            private T _current;

            public T Current
            {
                get
                {
                    return this._current;
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (this._index == 0 || this._index > this._builder._size)
                    {
                        throw new CompilerServicesException();
                    }
                    return this._current;
                }
            }

            internal Enumerator(ReadOnlyCollectionBuilder<T> builder)
            {
                this._builder = builder;
                this._version = builder._version;
                this._index = 0;
                this._current = default(T);
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            public bool MoveNext()
            {
                if (this._version != this._builder._version)
                {
                    throw new ArgumentException(nameof(_version));
                }
                if (this._index >= this._builder._size)
                {
                    this._index = this._builder._size + 1;
                    this._current = default(T);
                    return false;
                }
                T[] tArray = this._builder._items;
                int num = this._index;
                this._index = num + 1;
                this._current = tArray[num];
                return true;
            }

            void System.Collections.IEnumerator.Reset()
            {
                if (this._version != this._builder._version)
                {
                    throw new ArgumentException(nameof(_version));
                }
                this._index = 0;
                this._current = default(T);
            }
        }
    }
}