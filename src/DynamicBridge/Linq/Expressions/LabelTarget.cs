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

namespace System.Linq.Expressions
{
    /// <summary>Used to represent the target of a <see cref="T:System.Linq.Expressions.GotoExpression" />.</summary>
    /// <remarks>
    /// The Framework declares this type in System.Core from 4.0 onwards. It does not exist in
    /// 2.0/3.0 (no System.Core at all) nor in 3.5 (System.Core has only the 3.5 expression tree),
    /// so DynamicBridge supplies it on every target it compiles for. It is part of the signature
    /// of <see cref="M:System.Runtime.CompilerServices.CallSiteBinder.Bind(System.Object[],System.Collections.ObjectModel.ReadOnlyCollection{System.Linq.Expressions.ParameterExpression},System.Linq.Expressions.LabelTarget)" />,
    /// which is why it is needed even though this assembly does not yet emit goto nodes.
    /// </remarks>
    public sealed class LabelTarget
    {
        private readonly Type _type;
        private readonly string _name;

        internal LabelTarget(Type type, string name)
        {
            _type = type;
            _name = name;
        }

        /// <summary>Gets the name of the label.</summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>The type of value that is passed when jumping to the label, or <see cref="T:System.Void" />.</summary>
        public Type Type
        {
            get { return _type; }
        }

        /// <summary>Returns a <see cref="T:System.String" /> that represents the current object.</summary>
        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? "UnamedLabel" : Name;
        }

        // The BCL creates LabelTargets through Expression.Label(...). Expression itself belongs to
        // LinqBridge (net20/net30) or to System.Core (net35), so it cannot gain factories here;
        // these stay internal rather than widening the public surface beyond the Framework's.
        internal static LabelTarget Create(Type type, string name)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return new LabelTarget(type, name);
        }
    }
}
