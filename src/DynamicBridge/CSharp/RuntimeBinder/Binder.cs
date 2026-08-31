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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Microsoft.CSharp.RuntimeBinder
{
    /// <summary>Contains factory methods to create dynamic call site binders for CSharp.</summary>
    /// <remarks>
    /// Every one of these methods is called from compiler-generated code: the C# compiler lowers each
    /// dynamic operation into a call site whose binder comes from here. Their signatures are therefore
    /// fixed by the compiler and must not change.
    /// </remarks>
    public static class Binder
    {
        /// <summary>Initializes a new C# binary operation binder.</summary>
        public static CallSiteBinder BinaryOperation(CSharpBinderFlags flags, ExpressionType operation, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo)
        {
            return new CSharpBinaryOperationBinder(operation, context, argumentInfo,
                (flags & CSharpBinderFlags.CheckedContext) != 0,
                (flags & CSharpBinderFlags.BinaryOperationLogical) != 0);
        }

        /// <summary>Initializes a new C# convert binder.</summary>
        public static CallSiteBinder Convert(CSharpBinderFlags flags, Type type, Type context)
        {
            return new CSharpConvertBinder(type, context,
                (flags & CSharpBinderFlags.ConvertExplicit) != 0,
                (flags & CSharpBinderFlags.CheckedContext) != 0);
        }

        /// <summary>Initializes a new C# get index binder.</summary>
        public static CallSiteBinder GetIndex(CSharpBinderFlags flags, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo)
        {
            return new CSharpGetIndexBinder(context, argumentInfo, (flags & CSharpBinderFlags.CheckedContext) != 0);
        }

        /// <summary>Initializes a new C# get member binder.</summary>
        public static CallSiteBinder GetMember(CSharpBinderFlags flags, string name, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo)
        {
            return new CSharpGetMemberBinder(name, context, argumentInfo);
        }

        /// <summary>Initializes a new C# invoke binder.</summary>
        public static CallSiteBinder Invoke(CSharpBinderFlags flags, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo)
        {
            return new CSharpInvokeBinder(context, argumentInfo, (flags & CSharpBinderFlags.CheckedContext) != 0);
        }

        /// <summary>Initializes a new C# invoke constructor binder.</summary>
        public static CallSiteBinder InvokeConstructor(CSharpBinderFlags flags, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo)
        {
            return new CSharpInvokeConstructorBinder(context, argumentInfo, (flags & CSharpBinderFlags.CheckedContext) != 0);
        }

        /// <summary>Initializes a new C# invoke member binder.</summary>
        public static CallSiteBinder InvokeMember(CSharpBinderFlags flags, string name, IEnumerable<Type> typeArguments, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo)
        {
            return new CSharpInvokeMemberBinder(name, typeArguments, context, argumentInfo,
                (flags & CSharpBinderFlags.CheckedContext) != 0);
        }

        /// <summary>Initializes a new C# is event binder.</summary>
        public static CallSiteBinder IsEvent(CSharpBinderFlags flags, string name, Type context)
        {
            return new CSharpIsEventBinder(name, context);
        }

        /// <summary>Initializes a new C# set index binder.</summary>
        public static CallSiteBinder SetIndex(CSharpBinderFlags flags, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo)
        {
            return new CSharpSetIndexBinder(context, argumentInfo, (flags & CSharpBinderFlags.CheckedContext) != 0);
        }

        /// <summary>Initializes a new C# set member binder.</summary>
        public static CallSiteBinder SetMember(CSharpBinderFlags flags, string name, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo)
        {
            return new CSharpSetMemberBinder(name, context, argumentInfo);
        }

        /// <summary>Initializes a new C# unary operation binder.</summary>
        public static CallSiteBinder UnaryOperation(CSharpBinderFlags flags, ExpressionType operation, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo)
        {
            return new CSharpUnaryOperationBinder(operation, context, argumentInfo,
                (flags & CSharpBinderFlags.CheckedContext) != 0);
        }
    }
}
