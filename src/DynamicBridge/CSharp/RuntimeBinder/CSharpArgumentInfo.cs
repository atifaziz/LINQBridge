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

using System;

namespace Microsoft.CSharp.RuntimeBinder
{
    /// <summary>Represents information about C# dynamic operations that are specific to particular arguments at a call site.</summary>
    public sealed class CSharpArgumentInfo
    {
        private readonly CSharpArgumentInfoFlags _flags;
        private readonly string _name;

        private CSharpArgumentInfo(CSharpArgumentInfoFlags flags, string name)
        {
            _flags = flags;
            _name = name;
        }

        /// <summary>Initializes a new instance of the <see cref="T:Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo" /> class.</summary>
        /// <param name="flags">The flags for the argument.</param>
        /// <param name="name">The name of the argument, if named; otherwise null.</param>
        public static CSharpArgumentInfo Create(CSharpArgumentInfoFlags flags, string name)
        {
            return new CSharpArgumentInfo(flags, name);
        }

        internal CSharpArgumentInfoFlags Flags
        {
            get { return _flags; }
        }

        internal string Name
        {
            get { return _name; }
        }

        internal bool IsNamed
        {
            get { return (_flags & CSharpArgumentInfoFlags.NamedArgument) != 0 && !string.IsNullOrEmpty(_name); }
        }

        internal bool IsStaticType
        {
            get { return (_flags & CSharpArgumentInfoFlags.IsStaticType) != 0; }
        }

        internal bool IsByRef
        {
            get { return (_flags & (CSharpArgumentInfoFlags.IsRef | CSharpArgumentInfoFlags.IsOut)) != 0; }
        }

        internal bool IsOut
        {
            get { return (_flags & CSharpArgumentInfoFlags.IsOut) != 0; }
        }
    }

    /// <summary>Represents information about C# dynamic operations that are specific to particular arguments at a call site.</summary>
    [Flags]
    public enum CSharpArgumentInfoFlags
    {
        /// <summary>No additional information to represent.</summary>
        None = 0,
        /// <summary>The argument's compile-time type should be considered during binding.</summary>
        UseCompileTimeType = 1,
        /// <summary>The argument is a constant.</summary>
        Constant = 2,
        /// <summary>The argument is a named argument.</summary>
        NamedArgument = 4,
        /// <summary>The argument is passed by reference.</summary>
        IsRef = 8,
        /// <summary>The argument is an out parameter.</summary>
        IsOut = 16,
        /// <summary>The argument is a <see cref="T:System.Type" /> indicating an actual type name used in source.</summary>
        IsStaticType = 32,
    }

    /// <summary>Represents information about C# dynamic operations that are not specific to particular arguments at a call site.</summary>
    [Flags]
    public enum CSharpBinderFlags
    {
        /// <summary>There is no information about the represented dynamic operation.</summary>
        None = 0,
        /// <summary>The evaluation of this dynamic operation is checked at run-time.</summary>
        CheckedContext = 1,
        /// <summary>The dynamic operation is part of an invocation of a simple name.</summary>
        InvokeSimpleName = 2,
        /// <summary>The dynamic operation is part of an invocation of a specially named member.</summary>
        InvokeSpecialName = 4,
        /// <summary>The binary operation is part of a logical operation.</summary>
        BinaryOperationLogical = 8,
        /// <summary>The conversion is an explicit conversion.</summary>
        ConvertExplicit = 16,
        /// <summary>The conversion is used in an array index operation.</summary>
        ConvertArrayIndex = 32,
        /// <summary>The dynamic operation is used in a position that is indexed.</summary>
        ResultIndexed = 64,
        /// <summary>The value of the dynamic operation comes from a compound assignment.</summary>
        ValueFromCompoundAssignment = 128,
        /// <summary>The result of the dynamic operation is discarded.</summary>
        ResultDiscarded = 256,
    }
}
