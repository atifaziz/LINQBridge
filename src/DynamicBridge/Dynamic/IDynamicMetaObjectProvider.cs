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

using System.Linq.Expressions;

namespace System.Dynamic
{
    /// <summary>Represents a dynamic object, that can have its operations bound at runtime.</summary>
    public interface IDynamicMetaObjectProvider
    {
        /// <summary>Returns the <see cref="T:System.Dynamic.DynamicMetaObject" /> responsible for binding operations performed on this object.</summary>
        /// <param name="parameter">The expression tree representation of the runtime value.</param>
        /// <returns>The <see cref="T:System.Dynamic.DynamicMetaObject" /> to bind this object.</returns>
        DynamicMetaObject GetMetaObject(Expression parameter);
    }
}
