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

namespace System.Runtime.CompilerServices
{
    /// <summary>Indicates that the use of <see cref="T:System.Object" /> on a member is meant to be treated as a dynamically dispatched type.</summary>
    /// <remarks>
    /// The C# compiler emits this attribute for every <c>dynamic</c> that appears in a signature;
    /// without it, <c>dynamic</c> in a field, parameter or return type is a compile error (CS1980).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field |
                    AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue |
                    AttributeTargets.GenericParameter | AttributeTargets.Delegate | AttributeTargets.Interface |
                    AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method)]
    public sealed class DynamicAttribute : Attribute
    {
        private readonly bool[] _transformFlags;

        /// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.DynamicAttribute" /> class.</summary>
        public DynamicAttribute()
        {
            _transformFlags = new bool[] { true };
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.DynamicAttribute" /> class.</summary>
        /// <param name="transformFlags">Specifies, in a prefix traversal of a type's construction, which occurrences of <see cref="T:System.Object" /> are meant to be treated as a dynamically dispatched type.</param>
        public DynamicAttribute(bool[] transformFlags)
        {
            if (transformFlags == null)
                throw new ArgumentNullException("transformFlags");
            _transformFlags = transformFlags;
        }

        /// <summary>Specifies, in a prefix traversal of a type's construction, which occurrences of <see cref="T:System.Object" /> are meant to be treated as a dynamically dispatched type.</summary>
        public IList<bool> TransformFlags
        {
            get { return Array.AsReadOnly(_transformFlags); }
        }
    }
}
