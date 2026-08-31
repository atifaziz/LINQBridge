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

namespace Microsoft.CSharp.RuntimeBinder
{
    /// <summary>
    /// The Framework 4.0 additions to <see cref="T:System.Linq.Expressions.ExpressionType" />, as
    /// constants rather than enum members.
    /// </summary>
    /// <remarks>
    /// <para>
    /// On net20/net30 the enum comes from LinqBridge and does carry these members; on net35 it comes
    /// from the real 3.5 System.Core, which stops at TypeIs = 45. Naming them as constants is what
    /// lets one copy of this source compile for all three.
    /// </para>
    /// <para>
    /// Consumers are unaffected either way: the C# compiler lowers <c>~d</c>, <c>d++</c> and
    /// <c>d += 1</c> into calls that pass the node kind as a numeric constant from its own table, so
    /// it never looks the member up on the referenced enum — which is also why these values have to
    /// be the Framework's, exactly.
    /// </para>
    /// </remarks>
    internal static class NodeKinds
    {
        internal const ExpressionType Decrement = (ExpressionType)49;
        internal const ExpressionType Increment = (ExpressionType)54;
        internal const ExpressionType AddAssign = (ExpressionType)63;
        internal const ExpressionType AndAssign = (ExpressionType)64;
        internal const ExpressionType DivideAssign = (ExpressionType)65;
        internal const ExpressionType ExclusiveOrAssign = (ExpressionType)66;
        internal const ExpressionType LeftShiftAssign = (ExpressionType)67;
        internal const ExpressionType ModuloAssign = (ExpressionType)68;
        internal const ExpressionType MultiplyAssign = (ExpressionType)69;
        internal const ExpressionType OrAssign = (ExpressionType)70;
        internal const ExpressionType PowerAssign = (ExpressionType)71;
        internal const ExpressionType RightShiftAssign = (ExpressionType)72;
        internal const ExpressionType SubtractAssign = (ExpressionType)73;
        internal const ExpressionType AddAssignChecked = (ExpressionType)74;
        internal const ExpressionType MultiplyAssignChecked = (ExpressionType)75;
        internal const ExpressionType SubtractAssignChecked = (ExpressionType)76;
        internal const ExpressionType PreIncrementAssign = (ExpressionType)77;
        internal const ExpressionType PreDecrementAssign = (ExpressionType)78;
        internal const ExpressionType PostIncrementAssign = (ExpressionType)79;
        internal const ExpressionType PostDecrementAssign = (ExpressionType)80;
        internal const ExpressionType OnesComplement = (ExpressionType)82;
        internal const ExpressionType IsTrue = (ExpressionType)83;
        internal const ExpressionType IsFalse = (ExpressionType)84;
    }
}
