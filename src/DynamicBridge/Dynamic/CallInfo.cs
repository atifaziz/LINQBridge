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

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Dynamic
{
    /// <summary>Describes arguments in the dynamic binding process.</summary>
    public sealed class CallInfo
    {
        private readonly int _argCount;
        private readonly ReadOnlyCollection<string> _argNames;

        /// <summary>Creates a new <see cref="T:System.Dynamic.CallInfo" /> that represents arguments in the dynamic binding process.</summary>
        public CallInfo(int argCount, params string[] argNames)
            : this(argCount, (IEnumerable<string>)(argNames ?? new string[0]))
        {
        }

        /// <summary>Creates a new <see cref="T:System.Dynamic.CallInfo" /> that represents arguments in the dynamic binding process.</summary>
        public CallInfo(int argCount, IEnumerable<string> argNames)
        {
            if (argNames == null)
                throw new ArgumentNullException("argNames");

            var names = new List<string>();
            foreach (var name in argNames)
            {
                if (name == null)
                    throw new ArgumentNullException("argNames");
                names.Add(name);
            }

            if (argCount < names.Count)
                throw new ArgumentException("Argument count must be greater than number of named arguments", "argCount");

            _argCount = argCount;
            _argNames = new ReadOnlyCollection<string>(names);
        }

        /// <summary>The number of arguments.</summary>
        public int ArgumentCount
        {
            get { return _argCount; }
        }

        /// <summary>The argument names.</summary>
        public ReadOnlyCollection<string> ArgumentNames
        {
            get { return _argNames; }
        }

        /// <summary>Serves as a hash function for the current <see cref="T:System.Dynamic.CallInfo" />.</summary>
        public override int GetHashCode()
        {
            var hash = _argCount;
            foreach (var name in _argNames)
                hash = hash * 31 + name.GetHashCode();
            return hash;
        }

        /// <summary>Determines whether the specified <see cref="T:System.Dynamic.CallInfo" /> instance is considered equal to the current instance.</summary>
        public override bool Equals(object obj)
        {
            var other = obj as CallInfo;
            if (other == null || other._argCount != _argCount || other._argNames.Count != _argNames.Count)
                return false;

            for (var i = 0; i < _argNames.Count; i++)
            {
                if (!string.Equals(_argNames[i], other._argNames[i], StringComparison.Ordinal))
                    return false;
            }

            return true;
        }
    }
}
