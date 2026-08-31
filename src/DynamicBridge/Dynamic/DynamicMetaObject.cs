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
using System.Linq.Expressions;

namespace System.Dynamic
{
    /// <summary>Represents the dynamic binding and a binding logic of an object participating in the dynamic binding.</summary>
    public class DynamicMetaObject
    {
        /// <summary>Represents an empty array of type <see cref="T:System.Dynamic.DynamicMetaObject" />.</summary>
        public static readonly DynamicMetaObject[] EmptyMetaObjects = new DynamicMetaObject[0];

        private static readonly object NoValueSentinel = new object();

        private readonly Expression _expression;
        private readonly BindingRestrictions _restrictions;
        private readonly object _value;

        /// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.DynamicMetaObject" /> class.</summary>
        public DynamicMetaObject(Expression expression, BindingRestrictions restrictions)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (restrictions == null)
                throw new ArgumentNullException("restrictions");
            _expression = expression;
            _restrictions = restrictions;
            _value = NoValueSentinel;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.DynamicMetaObject" /> class.</summary>
        public DynamicMetaObject(Expression expression, BindingRestrictions restrictions, object value)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (restrictions == null)
                throw new ArgumentNullException("restrictions");
            _expression = expression;
            _restrictions = restrictions;
            _value = value;
        }

        /// <summary>The expression representing the <see cref="T:System.Dynamic.DynamicMetaObject" /> during the dynamic binding process.</summary>
        public Expression Expression
        {
            get { return _expression; }
        }

        /// <summary>The binding restrictions under which the binding is valid.</summary>
        public BindingRestrictions Restrictions
        {
            get { return _restrictions; }
        }

        /// <summary>The runtime value represented by this <see cref="T:System.Dynamic.DynamicMetaObject" />.</summary>
        public object Value
        {
            get { return ReferenceEquals(_value, NoValueSentinel) ? null : _value; }
        }

        /// <summary>Gets a value indicating whether the <see cref="T:System.Dynamic.DynamicMetaObject" /> has the runtime value.</summary>
        public bool HasValue
        {
            get { return !ReferenceEquals(_value, NoValueSentinel); }
        }

        /// <summary>Gets the <see cref="T:System.Type" /> of the runtime value or null if the <see cref="T:System.Dynamic.DynamicMetaObject" /> has no value associated with it.</summary>
        public Type RuntimeType
        {
            get
            {
                if (!HasValue)
                    return null;
                return _value == null ? null : _value.GetType();
            }
        }

        /// <summary>Gets the limit type of the <see cref="T:System.Dynamic.DynamicMetaObject" />.</summary>
        public Type LimitType
        {
            get { return RuntimeType ?? Expression.Type; }
        }

        /// <summary>Performs the binding of the dynamic conversion operation.</summary>
        public virtual DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            return binder.FallbackConvert(this);
        }

        /// <summary>Performs the binding of the dynamic get member operation.</summary>
        public virtual DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            return binder.FallbackGetMember(this);
        }

        /// <summary>Performs the binding of the dynamic set member operation.</summary>
        public virtual DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            return binder.FallbackSetMember(this, value);
        }

        /// <summary>Performs the binding of the dynamic delete member operation.</summary>
        public virtual DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            return binder.FallbackDeleteMember(this);
        }

        /// <summary>Performs the binding of the dynamic get index operation.</summary>
        public virtual DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            return binder.FallbackGetIndex(this, indexes);
        }

        /// <summary>Performs the binding of the dynamic set index operation.</summary>
        public virtual DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            return binder.FallbackSetIndex(this, indexes, value);
        }

        /// <summary>Performs the binding of the dynamic delete index operation.</summary>
        public virtual DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            return binder.FallbackDeleteIndex(this, indexes);
        }

        /// <summary>Performs the binding of the dynamic invoke member operation.</summary>
        public virtual DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            return binder.FallbackInvokeMember(this, args);
        }

        /// <summary>Performs the binding of the dynamic invoke operation.</summary>
        public virtual DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            return binder.FallbackInvoke(this, args);
        }

        /// <summary>Performs the binding of the dynamic create instance operation.</summary>
        public virtual DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            return binder.FallbackCreateInstance(this, args);
        }

        /// <summary>Performs the binding of the dynamic unary operation.</summary>
        public virtual DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            return binder.FallbackUnaryOperation(this);
        }

        /// <summary>Performs the binding of the dynamic binary operation.</summary>
        public virtual DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            return binder.FallbackBinaryOperation(this, arg);
        }

        /// <summary>Returns the enumeration of all dynamic member names.</summary>
        public virtual IEnumerable<string> GetDynamicMemberNames()
        {
            return new string[0];
        }

        /// <summary>Creates a meta-object for the specified object.</summary>
        public static DynamicMetaObject Create(object value, Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            var provider = value as IDynamicMetaObjectProvider;
            if (provider != null)
            {
                var metaObject = provider.GetMetaObject(expression);
                if (metaObject == null)
                    throw new InvalidOperationException("GetMetaObject returned null");
                return metaObject;
            }

            return new DynamicMetaObject(expression, BindingRestrictions.Empty, value);
        }
    }
}
