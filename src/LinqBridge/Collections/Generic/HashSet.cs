using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace System.Collections.Generic
{
    /// <summary>Represents a set of values.</summary>
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public class HashSet<T> :
      ICollection<T>,
      IEnumerable<T>,
      IEnumerable,
      ISerializable,
      IDeserializationCallback
    {
        private const int Lower31BitMask = 2147483647;
        private const int GrowthFactor = 2;
        private const int StackAllocThreshold = 100;
        private const int ShrinkThreshold = 3;
        private const string CapacityName = "Capacity";
        private const string ElementsName = "Elements";
        private const string ComparerName = "Comparer";
        private const string VersionName = "Version";
        private int[] m_buckets;
        private HashSet<T>.Slot[] m_slots;
        private int m_count;
        private int m_lastIndex;
        private int m_freeList;
        private IEqualityComparer<T> m_comparer;
        private int m_version;
        private SerializationInfo m_siInfo;

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty and uses the default equality comparer for the set type.</summary>
        public HashSet()
          : this((IEqualityComparer<T>)EqualityComparer<T>.Default)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty and uses the specified equality comparer for the set type.</summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing values in the set, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> implementation for the set type.</param>
        public HashSet(IEqualityComparer<T> comparer)
        {
            if (comparer == null)
                comparer = (IEqualityComparer<T>)EqualityComparer<T>.Default;
            this.m_comparer = comparer;
            this.m_lastIndex = 0;
            this.m_count = 0;
            this.m_freeList = -1;
            this.m_version = 0;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the default equality comparer for the set type, contains elements copied from the specified collection, and has sufficient capacity to accommodate the number of elements copied.</summary>
        /// <param name="collection">The collection whose elements are copied to the new set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="collection" /> is null.</exception>
        public HashSet(IEnumerable<T> collection)
          : this(collection, (IEqualityComparer<T>)EqualityComparer<T>.Default)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the specified equality comparer for the set type, contains elements copied from the specified collection, and has sufficient capacity to accommodate the number of elements copied.</summary>
        /// <param name="collection">The collection whose elements are copied to the new set.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing values in the set, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> implementation for the set type.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="collection" /> is null.</exception>
        public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
          : this(comparer)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            int capacity = 0;
            if (collection is ICollection<T> objs)
                capacity = objs.Count;
            this.Initialize(capacity);
            this.UnionWith(collection);
            if ((this.m_count != 0 || this.m_slots.Length <= HashHelpers.GetMinPrime()) && (this.m_count <= 0 || this.m_slots.Length / this.m_count <= 3))
                return;
            this.TrimExcess();
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class with serialized data.</summary>
        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
        /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure that contains the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
        protected HashSet(SerializationInfo info, StreamingContext context) => this.m_siInfo = info;

        void ICollection<T>.Add(T item) => this.AddIfNotPresent(item);

        /// <summary>Removes all elements from a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
        public void Clear()
        {
            if (this.m_lastIndex > 0)
            {
                Array.Clear((Array)this.m_slots, 0, this.m_lastIndex);
                Array.Clear((Array)this.m_buckets, 0, this.m_buckets.Length);
                this.m_lastIndex = 0;
                this.m_count = 0;
                this.m_freeList = -1;
            }
            ++this.m_version;
        }

        /// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object contains the specified element.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.HashSet`1" /> object contains the specified element; otherwise, false.</returns>
        /// <param name="item">The element to locate in the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
        public bool Contains(T item)
        {
            if (this.m_buckets != null)
            {
                int hashCode = this.InternalGetHashCode(item);
                for (int index = this.m_buckets[hashCode % this.m_buckets.Length] - 1; index >= 0; index = this.m_slots[index].next)
                {
                    if (this.m_slots[index].hashCode == hashCode && this.m_comparer.Equals(this.m_slots[index].value, item))
                        return true;
                }
            }
            return false;
        }

        /// <summary>Copies the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array, starting at the specified array index.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.HashSet`1" /> object. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="arrayIndex" /> is greater than the length of the destination <paramref name="array" />.-or-<paramref name="count" /> is larger than the size of the destination <paramref name="array" />.</exception>
        public void CopyTo(T[] array, int arrayIndex) => this.CopyTo(array, arrayIndex, this.m_count);

        /// <summary>Removes the specified element from a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
        /// <returns>true if the element is successfully found and removed; otherwise, false.  This method returns false if <paramref name="item" /> is not found in the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</returns>
        /// <param name="item">The element to remove.</param>
        public bool Remove(T item)
        {
            if (this.m_buckets != null)
            {
                int hashCode = this.InternalGetHashCode(item);
                int index1 = hashCode % this.m_buckets.Length;
                int index2 = -1;
                for (int index3 = this.m_buckets[index1] - 1; index3 >= 0; index3 = this.m_slots[index3].next)
                {
                    if (this.m_slots[index3].hashCode == hashCode && this.m_comparer.Equals(this.m_slots[index3].value, item))
                    {
                        if (index2 < 0)
                            this.m_buckets[index1] = this.m_slots[index3].next + 1;
                        else
                            this.m_slots[index2].next = this.m_slots[index3].next;
                        this.m_slots[index3].hashCode = -1;
                        this.m_slots[index3].value = default(T);
                        this.m_slots[index3].next = this.m_freeList;
                        this.m_freeList = index3;
                        --this.m_count;
                        ++this.m_version;
                        return true;
                    }
                    index2 = index3;
                }
            }
            return false;
        }

        /// <summary>Gets the number of elements that are contained in a set.</summary>
        /// <returns>The number of elements that are contained in the set.</returns>
        public int Count => this.m_count;

        bool ICollection<T>.IsReadOnly => false;

        /// <summary>Returns an enumerator that iterates through a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
        /// <returns>A <see cref="T:System.Collections.Generic.HashSet`1.Enumerator" /> object for the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</returns>
        public HashSet<T>.Enumerator GetEnumerator() => new HashSet<T>.Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => (IEnumerator<T>)new HashSet<T>.Enumerator(this);

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)new HashSet<T>.Enumerator(this);

        /// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
        /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure that contains the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="info" /> is null.</exception>
        /// <filterpriority>2</filterpriority>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            info.AddValue("Version", this.m_version);
            info.AddValue("Comparer", (object)this.m_comparer, typeof(IEqualityComparer<T>));
            info.AddValue("Capacity", this.m_buckets == null ? 0 : this.m_buckets.Length);
            if (this.m_buckets == null)
                return;
            T[] array = new T[this.m_count];
            this.CopyTo(array);
            info.AddValue("Elements", (object)array, typeof(T[]));
        }

        /// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and raises the deserialization event when the deserialization is complete.</summary>
        /// <param name="sender">The source of the deserialization event.</param>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object associated with the current <see cref="T:System.Collections.Generic.HashSet`1" /> object is invalid.</exception>
        /// <filterpriority>2</filterpriority>
        public virtual void OnDeserialization(object sender)
        {
            if (this.m_siInfo == null)
                return;
            int int32 = this.m_siInfo.GetInt32("Capacity");
            this.m_comparer = (IEqualityComparer<T>)this.m_siInfo.GetValue("Comparer", typeof(IEqualityComparer<T>));
            this.m_freeList = -1;
            if (int32 != 0)
            {
                this.m_buckets = new int[int32];
                this.m_slots = new HashSet<T>.Slot[int32];
                T[] objArray = (T[])this.m_siInfo.GetValue("Elements", typeof(T[]));
                if (objArray == null)
                    throw new SerializationException(CoreStringResources.GetString("Serialization_MissingKeys"));
                for (int index = 0; index < objArray.Length; ++index)
                    this.AddIfNotPresent(objArray[index]);
            }
            else
                this.m_buckets = (int[])null;
            this.m_version = this.m_siInfo.GetInt32("Version");
            this.m_siInfo = (SerializationInfo)null;
        }

        /// <summary>Adds the specified element to a set.</summary>
        /// <returns>true if the element is added to the <see cref="T:System.Collections.Generic.HashSet`1" /> object; false if the element is already present.</returns>
        /// <param name="item">The element to add to the set.</param>
        public bool Add(T item) => this.AddIfNotPresent(item);

        /// <summary>Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain all elements that are present in both itself and in the specified collection.</summary>
        /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="other" /> is null.</exception>
        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            foreach (T obj in other)
                this.AddIfNotPresent(obj);
        }

        /// <summary>Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain only elements that are present in that object and in the specified collection.</summary>
        /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="other" /> is null.</exception>
        [SecurityCritical]
        public void IntersectWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (this.m_count == 0)
                return;
            if (other is ICollection<T> objs)
            {
                if (objs.Count == 0)
                {
                    this.Clear();
                    return;
                }
                if (other is HashSet<T> objSet && HashSet<T>.AreEqualityComparersEqual(this, objSet))
                {
                    this.IntersectWithHashSetWithSameEC(objSet);
                    return;
                }
            }
            this.IntersectWithEnumerable(other);
        }

        /// <summary>Removes all elements in the specified collection from the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
        /// <param name="other">The collection of items to remove from the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="other" /> is null.</exception>
        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (this.m_count == 0)
                return;
            if (other == this)
            {
                this.Clear();
            }
            else
            {
                foreach (T obj in other)
                    this.Remove(obj);
            }
        }

        /// <summary>Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain only elements that are present either in that object or in the specified collection, but not both.</summary>
        /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="other" /> is null.</exception>
        [SecurityCritical]
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (this.m_count == 0)
                this.UnionWith(other);
            else if (other == this)
                this.Clear();
            else if (other is HashSet<T> objSet && HashSet<T>.AreEqualityComparersEqual(this, objSet))
                this.SymmetricExceptWithUniqueHashSet(objSet);
            else
                this.SymmetricExceptWithEnumerable(other);
        }

        /// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a subset of the specified collection.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a subset of <paramref name="other" />; otherwise, false.</returns>
        /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="other" /> is null.</exception>
        [SecurityCritical]
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (this.m_count == 0)
                return true;
            if (other is HashSet<T> objSet && HashSet<T>.AreEqualityComparersEqual(this, objSet))
                return this.m_count <= objSet.Count && this.IsSubsetOfHashSetWithSameEC(objSet);
            HashSet<T>.ElementCount elementCount = this.CheckUniqueAndUnfoundElements(other, false);
            return elementCount.uniqueCount == this.m_count && elementCount.unfoundCount >= 0;
        }

        /// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper subset of the specified collection.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper subset of <paramref name="other" />; otherwise, false.</returns>
        /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="other" /> is null.</exception>
        [SecurityCritical]
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (other is ICollection<T> objs)
            {
                if (this.m_count == 0)
                    return objs.Count > 0;
                if (other is HashSet<T> objSet && HashSet<T>.AreEqualityComparersEqual(this, objSet))
                    return this.m_count < objSet.Count && this.IsSubsetOfHashSetWithSameEC(objSet);
            }
            HashSet<T>.ElementCount elementCount = this.CheckUniqueAndUnfoundElements(other, false);
            return elementCount.uniqueCount == this.m_count && elementCount.unfoundCount > 0;
        }

        /// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a superset of the specified collection.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a superset of <paramref name="other" />; otherwise, false.</returns>
        /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="other" /> is null.</exception>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (other is ICollection<T> objs)
            {
                if (objs.Count == 0)
                    return true;
                if (other is HashSet<T> set2 && HashSet<T>.AreEqualityComparersEqual(this, set2) && set2.Count > this.m_count)
                    return false;
            }
            return this.ContainsAllElements(other);
        }

        /// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper superset of the specified collection.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper superset of <paramref name="other" />; otherwise, false.</returns>
        /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object. </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="other" /> is null.</exception>
        [SecurityCritical]
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (this.m_count == 0)
                return false;
            if (other is ICollection<T> objs)
            {
                if (objs.Count == 0)
                    return true;
                if (other is HashSet<T> objSet && HashSet<T>.AreEqualityComparersEqual(this, objSet))
                    return objSet.Count < this.m_count && this.ContainsAllElements((IEnumerable<T>)objSet);
            }
            HashSet<T>.ElementCount elementCount = this.CheckUniqueAndUnfoundElements(other, true);
            return elementCount.uniqueCount < this.m_count && elementCount.unfoundCount == 0;
        }

        /// <summary>Determines whether the current <see cref="T:System.Collections.Generic.HashSet`1" /> object overlaps the specified collection.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.HashSet`1" /> object and <paramref name="other" /> share at least one common element; otherwise, false.</returns>
        /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="other" /> is null.</exception>
        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (this.m_count == 0)
                return false;
            foreach (T obj in other)
            {
                if (this.Contains(obj))
                    return true;
            }
            return false;
        }

        /// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object and the specified collection contain the same elements.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is equal to <paramref name="other" />; otherwise, false.</returns>
        /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="other" /> is null.</exception>
        [SecurityCritical]
        public bool SetEquals(IEnumerable<T> other)
        {
            switch (other)
            {
                case null:
                    throw new ArgumentNullException(nameof(other));
                case HashSet<T> objSet when HashSet<T>.AreEqualityComparersEqual(this, objSet):
                    return this.m_count == objSet.Count && this.ContainsAllElements((IEnumerable<T>)objSet);
                case ICollection<T> objs when this.m_count == 0 && objs.Count > 0:
                    return false;
                default:
                    HashSet<T>.ElementCount elementCount = this.CheckUniqueAndUnfoundElements(other, true);
                    return elementCount.uniqueCount == this.m_count && elementCount.unfoundCount == 0;
            }
        }

        /// <summary>Copies the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.HashSet`1" /> object. The array must have zero-based indexing.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is null.</exception>
        public void CopyTo(T[] array) => this.CopyTo(array, 0, this.m_count);

        /// <summary>Copies the specified number of elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array, starting at the specified array index.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.HashSet`1" /> object. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <param name="count">The number of elements to copy to <paramref name="array" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex" /> is less than 0.-or-<paramref name="count" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="arrayIndex" /> is greater than the length of the destination <paramref name="array" />.-or-<paramref name="count" /> is greater than the available space from the <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), CoreStringResources.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), CoreStringResources.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            if (arrayIndex > array.Length || count > array.Length - arrayIndex)
                throw new ArgumentException(CoreStringResources.GetString("Arg_ArrayPlusOffTooSmall"));
            int num = 0;
            for (int index = 0; index < this.m_lastIndex && num < count; ++index)
            {
                if (this.m_slots[index].hashCode >= 0)
                {
                    array[arrayIndex + num] = this.m_slots[index].value;
                    ++num;
                }
            }
        }

        /// <summary>Removes all elements that match the conditions defined by the specified predicate from a <see cref="T:System.Collections.Generic.HashSet`1" /> collection.</summary>
        /// <returns>The number of elements that were removed from the <see cref="T:System.Collections.Generic.HashSet`1" /> collection.</returns>
        /// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the elements to remove.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="match" /> is null.</exception>
        public int RemoveWhere(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));
            int num = 0;
            for (int index = 0; index < this.m_lastIndex; ++index)
            {
                if (this.m_slots[index].hashCode >= 0)
                {
                    T obj = this.m_slots[index].value;
                    if (match(obj) && this.Remove(obj))
                        ++num;
                }
            }
            return num;
        }

        /// <summary>Gets the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> object that is used to determine equality for the values in the set.</summary>
        /// <returns>The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> object that is used to determine equality for the values in the set.</returns>
        public IEqualityComparer<T> Comparer => this.m_comparer;

        /// <summary>Sets the capacity of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to the actual number of elements it contains, rounded up to a nearby, implementation-specific value.</summary>
        public void TrimExcess()
        {
            if (this.m_count == 0)
            {
                this.m_buckets = (int[])null;
                this.m_slots = (HashSet<T>.Slot[])null;
                ++this.m_version;
            }
            else
            {
                int prime = HashHelpers.GetPrime(this.m_count);
                HashSet<T>.Slot[] slotArray = new HashSet<T>.Slot[prime];
                int[] numArray = new int[prime];
                int index1 = 0;
                for (int index2 = 0; index2 < this.m_lastIndex; ++index2)
                {
                    if (this.m_slots[index2].hashCode >= 0)
                    {
                        slotArray[index1] = this.m_slots[index2];
                        int index3 = slotArray[index1].hashCode % prime;
                        slotArray[index1].next = numArray[index3] - 1;
                        numArray[index3] = index1 + 1;
                        ++index1;
                    }
                }
                this.m_lastIndex = index1;
                this.m_slots = slotArray;
                this.m_buckets = numArray;
                this.m_freeList = -1;
            }
        }

        /// <summary>Returns an <see cref="T:System.Collections.IEqualityComparer" /> object that can be used for deep equality testing of a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
        /// <returns>An <see cref="T:System.Collections.IEqualityComparer" /> object that can be used for deep equality testing of the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</returns>
        public static IEqualityComparer<HashSet<T>> CreateSetComparer() => (IEqualityComparer<HashSet<T>>)new HashSetEqualityComparer<T>();

        private void Initialize(int capacity)
        {
            int prime = HashHelpers.GetPrime(capacity);
            this.m_buckets = new int[prime];
            this.m_slots = new HashSet<T>.Slot[prime];
        }

        private void IncreaseCapacity()
        {
            int min = this.m_count * 2;
            if (min < 0)
                min = this.m_count;
            int prime = HashHelpers.GetPrime(min);
            HashSet<T>.Slot[] destinationArray = prime > this.m_count ? new HashSet<T>.Slot[prime] : throw new ArgumentException(CoreStringResources.GetString("Arg_HSCapacityOverflow"));
            if (this.m_slots != null)
                Array.Copy((Array)this.m_slots, 0, (Array)destinationArray, 0, this.m_lastIndex);
            int[] numArray = new int[prime];
            for (int index1 = 0; index1 < this.m_lastIndex; ++index1)
            {
                int index2 = destinationArray[index1].hashCode % prime;
                destinationArray[index1].next = numArray[index2] - 1;
                numArray[index2] = index1 + 1;
            }
            this.m_slots = destinationArray;
            this.m_buckets = numArray;
        }

        private bool AddIfNotPresent(T value)
        {
            if (this.m_buckets == null)
                this.Initialize(0);
            int hashCode = this.InternalGetHashCode(value);
            int index1 = hashCode % this.m_buckets.Length;
            for (int index2 = this.m_buckets[hashCode % this.m_buckets.Length] - 1; index2 >= 0; index2 = this.m_slots[index2].next)
            {
                if (this.m_slots[index2].hashCode == hashCode && this.m_comparer.Equals(this.m_slots[index2].value, value))
                    return false;
            }
            int index3;
            if (this.m_freeList >= 0)
            {
                index3 = this.m_freeList;
                this.m_freeList = this.m_slots[index3].next;
            }
            else
            {
                if (this.m_lastIndex == this.m_slots.Length)
                {
                    this.IncreaseCapacity();
                    index1 = hashCode % this.m_buckets.Length;
                }
                index3 = this.m_lastIndex;
                ++this.m_lastIndex;
            }
            this.m_slots[index3].hashCode = hashCode;
            this.m_slots[index3].value = value;
            this.m_slots[index3].next = this.m_buckets[index1] - 1;
            this.m_buckets[index1] = index3 + 1;
            ++this.m_count;
            ++this.m_version;
            return true;
        }

        private bool ContainsAllElements(IEnumerable<T> other)
        {
            foreach (T obj in other)
            {
                if (!this.Contains(obj))
                    return false;
            }
            return true;
        }

        private bool IsSubsetOfHashSetWithSameEC(HashSet<T> other)
        {
            foreach (T obj in this)
            {
                if (!other.Contains(obj))
                    return false;
            }
            return true;
        }

        private void IntersectWithHashSetWithSameEC(HashSet<T> other)
        {
            for (int index = 0; index < this.m_lastIndex; ++index)
            {
                if (this.m_slots[index].hashCode >= 0)
                {
                    T obj = this.m_slots[index].value;
                    if (!other.Contains(obj))
                        this.Remove(obj);
                }
            }
        }

        [SecurityCritical]
        private unsafe void IntersectWithEnumerable(IEnumerable<T> other)
        {
            int lastIndex = this.m_lastIndex;
            int intArrayLength = BitHelper.ToIntArrayLength(lastIndex);
            // ISSUE: untyped stack allocation
            BitHelper bitHelper = intArrayLength > 100 ? new BitHelper(new int[intArrayLength], intArrayLength) : new BitHelper(new int[4 * intArrayLength], intArrayLength);
            foreach (T obj in other)
            {
                int bitPosition = this.InternalIndexOf(obj);
                if (bitPosition >= 0)
                    bitHelper.MarkBit(bitPosition);
            }
            for (int bitPosition = 0; bitPosition < lastIndex; ++bitPosition)
            {
                if (this.m_slots[bitPosition].hashCode >= 0 && !bitHelper.IsMarked(bitPosition))
                    this.Remove(this.m_slots[bitPosition].value);
            }
        }

        private int InternalIndexOf(T item)
        {
            int hashCode = this.InternalGetHashCode(item);
            for (int index = this.m_buckets[hashCode % this.m_buckets.Length] - 1; index >= 0; index = this.m_slots[index].next)
            {
                if (this.m_slots[index].hashCode == hashCode && this.m_comparer.Equals(this.m_slots[index].value, item))
                    return index;
            }
            return -1;
        }

        private void SymmetricExceptWithUniqueHashSet(HashSet<T> other)
        {
            foreach (T obj in other)
            {
                if (!this.Remove(obj))
                    this.AddIfNotPresent(obj);
            }
        }

        [SecurityCritical]
        private unsafe void SymmetricExceptWithEnumerable(IEnumerable<T> other)
        {
            int lastIndex = this.m_lastIndex;
            int intArrayLength = BitHelper.ToIntArrayLength(lastIndex);
            BitHelper bitHelper1;
            BitHelper bitHelper2;
            if (intArrayLength <= 50)
            {
                // ISSUE: untyped stack allocation
                bitHelper1 = new BitHelper(new int[4 * intArrayLength], intArrayLength);
                // ISSUE: untyped stack allocation
                bitHelper2 = new BitHelper(new int[4 * intArrayLength], intArrayLength);
            }
            else
            {
                bitHelper1 = new BitHelper(new int[intArrayLength], intArrayLength);
                bitHelper2 = new BitHelper(new int[intArrayLength], intArrayLength);
            }
            foreach (T obj in other)
            {
                int location = 0;
                if (this.AddOrGetLocation(obj, out location))
                    bitHelper2.MarkBit(location);
                else if (location < lastIndex && !bitHelper2.IsMarked(location))
                    bitHelper1.MarkBit(location);
            }
            for (int bitPosition = 0; bitPosition < lastIndex; ++bitPosition)
            {
                if (bitHelper1.IsMarked(bitPosition))
                    this.Remove(this.m_slots[bitPosition].value);
            }
        }

        private bool AddOrGetLocation(T value, out int location)
        {
            int hashCode = this.InternalGetHashCode(value);
            int index1 = hashCode % this.m_buckets.Length;
            for (int index2 = this.m_buckets[hashCode % this.m_buckets.Length] - 1; index2 >= 0; index2 = this.m_slots[index2].next)
            {
                if (this.m_slots[index2].hashCode == hashCode && this.m_comparer.Equals(this.m_slots[index2].value, value))
                {
                    location = index2;
                    return false;
                }
            }
            int index3;
            if (this.m_freeList >= 0)
            {
                index3 = this.m_freeList;
                this.m_freeList = this.m_slots[index3].next;
            }
            else
            {
                if (this.m_lastIndex == this.m_slots.Length)
                {
                    this.IncreaseCapacity();
                    index1 = hashCode % this.m_buckets.Length;
                }
                index3 = this.m_lastIndex;
                ++this.m_lastIndex;
            }
            this.m_slots[index3].hashCode = hashCode;
            this.m_slots[index3].value = value;
            this.m_slots[index3].next = this.m_buckets[index1] - 1;
            this.m_buckets[index1] = index3 + 1;
            ++this.m_count;
            ++this.m_version;
            location = index3;
            return true;
        }

        [SecurityCritical]
        private unsafe HashSet<T>.ElementCount CheckUniqueAndUnfoundElements(
          IEnumerable<T> other,
          bool returnIfUnfound)
        {
            if (this.m_count == 0)
            {
                int num = 0;
                using (IEnumerator<T> enumerator = other.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        T current = enumerator.Current;
                        ++num;
                    }
                }
                HashSet<T>.ElementCount elementCount;
                elementCount.uniqueCount = 0;
                elementCount.unfoundCount = num;
                return elementCount;
            }
            int intArrayLength = BitHelper.ToIntArrayLength(this.m_lastIndex);
            // ISSUE: untyped stack allocation
            BitHelper bitHelper = intArrayLength > 100 ? new BitHelper(new int[intArrayLength], intArrayLength) : new BitHelper(new int[4 * intArrayLength], intArrayLength);
            int num1 = 0;
            int num2 = 0;
            foreach (T obj in other)
            {
                int bitPosition = this.InternalIndexOf(obj);
                if (bitPosition >= 0)
                {
                    if (!bitHelper.IsMarked(bitPosition))
                    {
                        bitHelper.MarkBit(bitPosition);
                        ++num2;
                    }
                }
                else
                {
                    ++num1;
                    if (returnIfUnfound)
                        break;
                }
            }
            HashSet<T>.ElementCount elementCount1;
            elementCount1.uniqueCount = num2;
            elementCount1.unfoundCount = num1;
            return elementCount1;
        }

        internal T[] ToArray()
        {
            T[] array = new T[this.Count];
            this.CopyTo(array);
            return array;
        }

        internal static bool HashSetEquals(
          HashSet<T> set1,
          HashSet<T> set2,
          IEqualityComparer<T> comparer)
        {
            if (set1 == null)
                return set2 == null;
            if (set2 == null)
                return false;
            if (HashSet<T>.AreEqualityComparersEqual(set1, set2))
            {
                if (set1.Count != set2.Count)
                    return false;
                foreach (T obj in set2)
                {
                    if (!set1.Contains(obj))
                        return false;
                }
                return true;
            }
            foreach (T x in set2)
            {
                bool flag = false;
                foreach (T y in set1)
                {
                    if (comparer.Equals(x, y))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    return false;
            }
            return true;
        }

        private static bool AreEqualityComparersEqual(HashSet<T> set1, HashSet<T> set2) => set1.Comparer.Equals((object)set2.Comparer);

        private int InternalGetHashCode(T item) => (object)item == null ? 0 : this.m_comparer.GetHashCode(item) & int.MaxValue;

        internal struct ElementCount
        {
            internal int uniqueCount;
            internal int unfoundCount;
        }

        internal struct Slot
        {
            internal int hashCode;
            internal T value;
            internal int next;
        }

        /// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
        /// <filterpriority>2</filterpriority>
        [Serializable]
        [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private HashSet<T> set;
            private int index;
            private int version;
            private T current;

            internal Enumerator(HashSet<T> set)
            {
                this.set = set;
                this.index = 0;
                this.version = set.m_version;
                this.current = default(T);
            }

            /// <summary>Releases all resources used by a <see cref="T:System.Collections.Generic.HashSet`1.Enumerator" /> object.</summary>
            public void Dispose()
            {
            }

            /// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.HashSet`1" /> collection.</summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
            public bool MoveNext()
            {
                if (this.version != this.set.m_version)
                    throw new InvalidOperationException(CoreStringResources.GetString("InvalidOperation_EnumFailedVersion"));
                for (; this.index < this.set.m_lastIndex; ++this.index)
                {
                    if (this.set.m_slots[this.index].hashCode >= 0)
                    {
                        this.current = this.set.m_slots[this.index].value;
                        ++this.index;
                        return true;
                    }
                }
                this.index = this.set.m_lastIndex + 1;
                this.current = default(T);
                return false;
            }

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element in the <see cref="T:System.Collections.Generic.HashSet`1" /> collection at the current position of the enumerator.</returns>
            public T Current => this.current;

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element in the collection at the current position of the enumerator, as an <see cref="T:System.Object" />.</returns>
            /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
            object IEnumerator.Current
            {
                get
                {
                    if (this.index == 0 || this.index == this.set.m_lastIndex + 1)
                        throw new InvalidOperationException(CoreStringResources.GetString("InvalidOperation_EnumOpCantHappen"));
                    return (object)this.Current;
                }
            }

            /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
            void IEnumerator.Reset()
            {
                if (this.version != this.set.m_version)
                    throw new InvalidOperationException(CoreStringResources.GetString("InvalidOperation_EnumFailedVersion"));
                this.index = 0;
                this.current = default(T);
            }
        }
    }
}
