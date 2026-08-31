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
using System.Runtime.Serialization;

namespace Microsoft.CSharp.RuntimeBinder
{
    /// <summary>Represents an error that occurs when a dynamic bind in the C# runtime binder is processed.</summary>
    [Serializable]
    public class RuntimeBinderException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="T:Microsoft.CSharp.RuntimeBinder.RuntimeBinderException" /> class.</summary>
        public RuntimeBinderException()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:Microsoft.CSharp.RuntimeBinder.RuntimeBinderException" /> class that has a specified error message.</summary>
        public RuntimeBinderException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:Microsoft.CSharp.RuntimeBinder.RuntimeBinderException" /> class that has a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
        public RuntimeBinderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:Microsoft.CSharp.RuntimeBinder.RuntimeBinderException" /> class with serialized data.</summary>
        protected RuntimeBinderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>Represents an error that occurs when a dynamic bind in the C# runtime binder is processed.</summary>
    [Serializable]
    public class RuntimeBinderInternalCompilerException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="T:Microsoft.CSharp.RuntimeBinder.RuntimeBinderInternalCompilerException" /> class.</summary>
        public RuntimeBinderInternalCompilerException()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:Microsoft.CSharp.RuntimeBinder.RuntimeBinderInternalCompilerException" /> class that has a specified error message.</summary>
        public RuntimeBinderInternalCompilerException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:Microsoft.CSharp.RuntimeBinder.RuntimeBinderInternalCompilerException" /> class that has a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
        public RuntimeBinderInternalCompilerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:Microsoft.CSharp.RuntimeBinder.RuntimeBinderInternalCompilerException" /> class with serialized data.</summary>
        protected RuntimeBinderInternalCompilerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
