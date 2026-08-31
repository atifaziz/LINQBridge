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
    /// <summary>Represents the invoke dynamic operation at the call site, providing the binding semantic and the details about the operation.</summary>
    public abstract class InvokeBinder : DynamicMetaObjectBinder
    {
        private readonly CallInfo _callInfo;

        /// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.InvokeBinder" /> class.</summary>
        protected InvokeBinder(CallInfo callInfo)
        {
            if (callInfo == null)
                throw new ArgumentNullException("callInfo");
            _callInfo = callInfo;
        }

        /// <summary>Gets the signature of the arguments at the call site.</summary>
        public CallInfo CallInfo
        {
            get { return _callInfo; }
        }

        /// <summary>Performs the binding of the dynamic invoke operation.</summary>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (target == null) throw new ArgumentNullException("target");
            return target.BindInvoke(this, args ?? DynamicMetaObject.EmptyMetaObjects);
        }

        /// <summary>Performs the binding of the dynamic invoke operation if the target dynamic object cannot bind.</summary>
        public DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            return FallbackInvoke(target, args, null);
        }

        /// <summary>When overridden in the derived class, performs the binding of the dynamic invoke operation if the target dynamic object cannot bind.</summary>
        public abstract DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion);
    }

    /// <summary>Represents the invoke member dynamic operation at the call site, providing the binding semantic and the details about the operation.</summary>
    public abstract class InvokeMemberBinder : DynamicMetaObjectBinder
    {
        private readonly string _name;
        private readonly bool _ignoreCase;
        private readonly CallInfo _callInfo;

        /// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.InvokeMemberBinder" /> class.</summary>
        protected InvokeMemberBinder(string name, bool ignoreCase, CallInfo callInfo)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (callInfo == null)
                throw new ArgumentNullException("callInfo");
            _name = name;
            _ignoreCase = ignoreCase;
            _callInfo = callInfo;
        }

        /// <summary>Gets the name of the member to invoke.</summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>Gets the value indicating if the string comparison should ignore the case of the member name.</summary>
        public bool IgnoreCase
        {
            get { return _ignoreCase; }
        }

        /// <summary>Gets the signature of the arguments at the call site.</summary>
        public CallInfo CallInfo
        {
            get { return _callInfo; }
        }

        /// <summary>Performs the binding of the dynamic invoke member operation.</summary>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (target == null) throw new ArgumentNullException("target");
            return target.BindInvokeMember(this, args ?? DynamicMetaObject.EmptyMetaObjects);
        }

        /// <summary>Performs the binding of the dynamic invoke member operation if the target dynamic object cannot bind.</summary>
        public DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            return FallbackInvokeMember(target, args, null);
        }

        /// <summary>When overridden in the derived class, performs the binding of the dynamic invoke member operation if the target dynamic object cannot bind.</summary>
        public abstract DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion);

        /// <summary>When overridden in the derived class, performs the binding of the dynamic invoke operation if the target dynamic object cannot bind.</summary>
        public abstract DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion);
    }

    /// <summary>Represents the dynamic create operation at the call site, providing the binding semantic and the details about the operation.</summary>
    public abstract class CreateInstanceBinder : DynamicMetaObjectBinder
    {
        private readonly CallInfo _callInfo;

        /// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.CreateInstanceBinder" /> class.</summary>
        protected CreateInstanceBinder(CallInfo callInfo)
        {
            if (callInfo == null)
                throw new ArgumentNullException("callInfo");
            _callInfo = callInfo;
        }

        /// <summary>Gets the signature of the arguments at the call site.</summary>
        public CallInfo CallInfo
        {
            get { return _callInfo; }
        }

        /// <summary>Performs the binding of the dynamic create instance operation.</summary>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (target == null) throw new ArgumentNullException("target");
            return target.BindCreateInstance(this, args ?? DynamicMetaObject.EmptyMetaObjects);
        }

        /// <summary>Performs the binding of the dynamic create instance operation if the target dynamic object cannot bind.</summary>
        public DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            return FallbackCreateInstance(target, args, null);
        }

        /// <summary>When overridden in the derived class, performs the binding of the dynamic create instance operation if the target dynamic object cannot bind.</summary>
        public abstract DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion);
    }

    /// <summary>Represents the convert dynamic operation at the call site, providing the binding semantic and the details about the operation.</summary>
    public abstract class ConvertBinder : DynamicMetaObjectBinder
    {
        private readonly Type _type;
        private readonly bool _explicit;

        /// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.ConvertBinder" /> class.</summary>
        protected ConvertBinder(Type type, bool isExplicit)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            _type = type;
            _explicit = isExplicit;
        }

        /// <summary>The type to convert to.</summary>
        public Type Type
        {
            get { return _type; }
        }

        /// <summary>Gets the value indicating if the conversion should consider explicit conversions.</summary>
        public bool Explicit
        {
            get { return _explicit; }
        }

        /// <summary>The result type of the operation.</summary>
        public override Type ReturnType
        {
            get { return _type; }
        }

        /// <summary>Performs the binding of the dynamic convert operation.</summary>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (args != null && args.Length != 0) throw new ArgumentException("Convert takes no arguments", "args");
            return target.BindConvert(this);
        }

        /// <summary>Performs the binding of the dynamic convert operation if the target dynamic object cannot bind.</summary>
        public DynamicMetaObject FallbackConvert(DynamicMetaObject target)
        {
            return FallbackConvert(target, null);
        }

        /// <summary>When overridden in the derived class, performs the binding of the dynamic convert operation if the target dynamic object cannot bind.</summary>
        public abstract DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion);
    }

    /// <summary>Represents the binary dynamic operation at the call site, providing the binding semantic and the details about the operation.</summary>
    public abstract class BinaryOperationBinder : DynamicMetaObjectBinder
    {
        private readonly ExpressionType _operation;

        /// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.BinaryOperationBinder" /> class.</summary>
        protected BinaryOperationBinder(ExpressionType operation)
        {
            _operation = operation;
        }

        /// <summary>The binary operation kind.</summary>
        public ExpressionType Operation
        {
            get { return _operation; }
        }

        /// <summary>Performs the binding of the binary dynamic operation.</summary>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (args == null || args.Length != 1) throw new ArgumentException("A binary operation takes exactly one argument", "args");
            return target.BindBinaryOperation(this, args[0]);
        }

        /// <summary>Performs the binding of the binary dynamic operation if the target dynamic object cannot bind.</summary>
        public DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg)
        {
            return FallbackBinaryOperation(target, arg, null);
        }

        /// <summary>When overridden in the derived class, performs the binding of the binary dynamic operation if the target dynamic object cannot bind.</summary>
        public abstract DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion);
    }

    /// <summary>Represents the unary dynamic operation at the call site, providing the binding semantic and the details about the operation.</summary>
    public abstract class UnaryOperationBinder : DynamicMetaObjectBinder
    {
        private readonly ExpressionType _operation;

        /// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.UnaryOperationBinder" /> class.</summary>
        protected UnaryOperationBinder(ExpressionType operation)
        {
            _operation = operation;
        }

        /// <summary>The unary operation kind.</summary>
        public ExpressionType Operation
        {
            get { return _operation; }
        }

        /// <summary>Performs the binding of the unary dynamic operation.</summary>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (args != null && args.Length != 0) throw new ArgumentException("A unary operation takes no arguments", "args");
            return target.BindUnaryOperation(this);
        }

        /// <summary>Performs the binding of the unary dynamic operation if the target dynamic object cannot bind.</summary>
        public DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target)
        {
            return FallbackUnaryOperation(target, null);
        }

        /// <summary>When overridden in the derived class, performs the binding of the unary dynamic operation if the target dynamic object cannot bind.</summary>
        public abstract DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion);
    }
}
