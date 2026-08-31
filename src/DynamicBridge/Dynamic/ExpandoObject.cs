#region License, Terms and Author(s)
//
// DynamicBridge
//
// Brings the C# 'dynamic' keyword to CLR 2.0 targets.
//
// This library is free software; you can redistribute it and/or modify it
// under the terms of the New BSD License, a copy of which should have
// been delivered along with this distribution.
//
#endregion

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace System.Dynamic
{
    /// <summary>Represents an object whose members can be dynamically added and removed at run time.</summary>
    public sealed class ExpandoObject : IDynamicMetaObjectProvider, IDictionary<string, object>, INotifyPropertyChanged
    {
        // Insertion order is part of the observable behaviour of ExpandoObject (enumerating it yields
        // members in the order they were added), which a plain Dictionary<,> does not guarantee.
        private readonly List<string> _keys = new List<string>();
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>(StringComparer.Ordinal);
        private readonly object _lock = new object();

        /// <summary>Initializes a new <see cref="T:System.Dynamic.ExpandoObject" /> that does not have members.</summary>
        public ExpandoObject()
        {
        }

        /// <summary>Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #region IDictionary<string, object>

        /// <summary>Gets or sets the value associated with the specified key.</summary>
        public object this[string key]
        {
            get
            {
                object value;
                if (!TryGetValue(key, out value))
                    throw new KeyNotFoundException("The given key was not present in the ExpandoObject: " + key);
                return value;
            }
            set { Set(key, value); }
        }

        /// <summary>Gets a collection containing the keys of the <see cref="T:System.Dynamic.ExpandoObject" />.</summary>
        public ICollection<string> Keys
        {
            get
            {
                lock (_lock)
                    return new List<string>(_keys).AsReadOnly();
            }
        }

        /// <summary>Gets a collection containing the values of the <see cref="T:System.Dynamic.ExpandoObject" />.</summary>
        public ICollection<object> Values
        {
            get
            {
                lock (_lock)
                {
                    var values = new List<object>(_keys.Count);
                    foreach (var key in _keys)
                        values.Add(_values[key]);
                    return values.AsReadOnly();
                }
            }
        }

        /// <summary>Gets the number of members in the <see cref="T:System.Dynamic.ExpandoObject" />.</summary>
        public int Count
        {
            get { lock (_lock) return _keys.Count; }
        }

        /// <summary>Gets a value indicating whether the <see cref="T:System.Dynamic.ExpandoObject" /> is read-only.</summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>Adds the specified key and value to the <see cref="T:System.Dynamic.ExpandoObject" />.</summary>
        public void Add(string key, object value)
        {
            if (key == null) throw new ArgumentNullException("key");
            lock (_lock)
            {
                if (_values.ContainsKey(key))
                    throw new ArgumentException("An element with the same key already exists: " + key, "key");
            }
            Set(key, value);
        }

        /// <summary>Adds the specified item to the <see cref="T:System.Dynamic.ExpandoObject" />.</summary>
        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>Removes all members from the <see cref="T:System.Dynamic.ExpandoObject" />.</summary>
        public void Clear()
        {
            string[] removed;
            lock (_lock)
            {
                removed = _keys.ToArray();
                _keys.Clear();
                _values.Clear();
            }

            foreach (var key in removed)
                OnPropertyChanged(key);
        }

        /// <summary>Determines whether the <see cref="T:System.Dynamic.ExpandoObject" /> contains the specified item.</summary>
        public bool Contains(KeyValuePair<string, object> item)
        {
            object value;
            return TryGetValue(item.Key, out value) && Equals(value, item.Value);
        }

        /// <summary>Determines whether the <see cref="T:System.Dynamic.ExpandoObject" /> contains a member with the specified key.</summary>
        public bool ContainsKey(string key)
        {
            if (key == null) throw new ArgumentNullException("key");
            lock (_lock) return _values.ContainsKey(key);
        }

        /// <summary>Copies the members of the <see cref="T:System.Dynamic.ExpandoObject" /> to an array, starting at the specified index.</summary>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException("arrayIndex");

            lock (_lock)
            {
                if (array.Length - arrayIndex < _keys.Count)
                    throw new ArgumentException("The destination array is too small", "array");

                foreach (var key in _keys)
                    array[arrayIndex++] = new KeyValuePair<string, object>(key, _values[key]);
            }
        }

        /// <summary>Removes the member with the specified key from the <see cref="T:System.Dynamic.ExpandoObject" />.</summary>
        public bool Remove(string key)
        {
            if (key == null) throw new ArgumentNullException("key");

            lock (_lock)
            {
                if (!_values.Remove(key))
                    return false;
                _keys.Remove(key);
            }

            OnPropertyChanged(key);
            return true;
        }

        /// <summary>Removes the specified item from the <see cref="T:System.Dynamic.ExpandoObject" />.</summary>
        public bool Remove(KeyValuePair<string, object> item)
        {
            return Contains(item) && Remove(item.Key);
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        public bool TryGetValue(string key, out object value)
        {
            if (key == null) throw new ArgumentNullException("key");
            lock (_lock) return _values.TryGetValue(key, out value);
        }

        /// <summary>Returns an enumerator that iterates through the members of the <see cref="T:System.Dynamic.ExpandoObject" />.</summary>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            List<KeyValuePair<string, object>> snapshot;
            lock (_lock)
            {
                snapshot = new List<KeyValuePair<string, object>>(_keys.Count);
                foreach (var key in _keys)
                    snapshot.Add(new KeyValuePair<string, object>(key, _values[key]));
            }
            return snapshot.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        internal void Set(string key, object value)
        {
            if (key == null) throw new ArgumentNullException("key");

            lock (_lock)
            {
                if (!_values.ContainsKey(key))
                    _keys.Add(key);
                _values[key] = value;
            }

            OnPropertyChanged(key);
        }

        internal IEnumerable<string> MemberNames
        {
            get { lock (_lock) return new List<string>(_keys); }
        }

        private void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>Returns the <see cref="T:System.Dynamic.DynamicMetaObject" /> responsible for binding operations performed on this object.</summary>
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new MetaExpando(parameter, this);
        }

        private sealed class MetaExpando : DynamicMetaObject
        {
            private readonly ExpandoObject _expando;

            internal MetaExpando(Expression expression, ExpandoObject expando)
                : base(expression, BindingRestrictions.GetTypeRestriction(expression, typeof(ExpandoObject)), expando)
            {
                _expando = expando;
            }

            private DynamicMetaObject Result(object value)
            {
                return new DynamicMetaObject(
                    Expression.Constant(value, typeof(object)),
                    BindingRestrictions.GetTypeRestriction(Expression, typeof(ExpandoObject)),
                    value);
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return _expando.MemberNames;
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                if (binder == null) throw new ArgumentNullException("binder");

                object value;
                if (_expando.TryGetValue(binder.Name, out value))
                    return Result(value);

                if (binder.IgnoreCase)
                {
                    foreach (var name in _expando.MemberNames)
                    {
                        if (string.Equals(name, binder.Name, StringComparison.OrdinalIgnoreCase))
                            return Result(_expando[name]);
                    }
                }

                // No such member: let the language binder produce its own error, so the exception a
                // caller sees for a missing ExpandoObject member matches the Framework's.
                return binder.FallbackGetMember(new DynamicMetaObject(Expression, BindingRestrictions.Empty, _expando));
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                if (binder == null) throw new ArgumentNullException("binder");
                var newValue = value == null ? null : value.Value;
                _expando.Set(binder.Name, newValue);
                return Result(newValue);
            }

            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
            {
                if (binder == null) throw new ArgumentNullException("binder");
                _expando.Remove(binder.Name);
                return Result(null);
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
            {
                if (binder == null) throw new ArgumentNullException("binder");

                // A member holding a delegate is invoked; anything else falls back to the CLR type.
                object member;
                if (_expando.TryGetValue(binder.Name, out member) && member is Delegate)
                    return binder.FallbackInvoke(new DynamicMetaObject(Expression.Constant(member, typeof(object)), BindingRestrictions.Empty, member), args, null);

                return binder.FallbackInvokeMember(new DynamicMetaObject(Expression, BindingRestrictions.Empty, _expando), args);
            }
        }
    }
}
