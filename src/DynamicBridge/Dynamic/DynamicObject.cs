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
    /// <summary>Provides a base class for specifying dynamic behavior at run time. This class must be inherited from; you cannot instantiate it directly.</summary>
    public class DynamicObject : IDynamicMetaObjectProvider
    {
        /// <summary>Enables derived types to initialize a new instance of the <see cref="T:System.Dynamic.DynamicObject" /> type.</summary>
        protected DynamicObject()
        {
        }

        /// <summary>Provides the implementation for operations that get member values.</summary>
        public virtual bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            return false;
        }

        /// <summary>Provides the implementation for operations that set member values.</summary>
        public virtual bool TrySetMember(SetMemberBinder binder, object value)
        {
            return false;
        }

        /// <summary>Provides the implementation for operations that delete an object member.</summary>
        public virtual bool TryDeleteMember(DeleteMemberBinder binder)
        {
            return false;
        }

        /// <summary>Provides the implementation for operations that invoke a member.</summary>
        public virtual bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            return false;
        }

        /// <summary>Provides implementation for type conversion operations.</summary>
        public virtual bool TryConvert(ConvertBinder binder, out object result)
        {
            result = null;
            return false;
        }

        /// <summary>Provides the implementation for operations that initialize a new instance of a dynamic object.</summary>
        public virtual bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result)
        {
            result = null;
            return false;
        }

        /// <summary>Provides the implementation for operations that invoke an object.</summary>
        public virtual bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = null;
            return false;
        }

        /// <summary>Provides implementation for binary operations.</summary>
        public virtual bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            result = null;
            return false;
        }

        /// <summary>Provides implementation for unary operations.</summary>
        public virtual bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
        {
            result = null;
            return false;
        }

        /// <summary>Provides the implementation for operations that get a value by index.</summary>
        public virtual bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = null;
            return false;
        }

        /// <summary>Provides the implementation for operations that set a value by index.</summary>
        public virtual bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            return false;
        }

        /// <summary>Provides the implementation for operations that delete an object by index.</summary>
        public virtual bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes)
        {
            return false;
        }

        /// <summary>Returns the enumeration of all dynamic member names.</summary>
        public virtual IEnumerable<string> GetDynamicMemberNames()
        {
            return new string[0];
        }

        /// <summary>Provides a <see cref="T:System.Dynamic.DynamicMetaObject" /> that dispatches to the dynamic virtual methods.</summary>
        public virtual DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new MetaDynamic(parameter, this);
        }

        private sealed class MetaDynamic : DynamicMetaObject
        {
            private readonly DynamicObject _target;

            internal MetaDynamic(Expression expression, DynamicObject target)
                : base(expression, BindingRestrictions.GetTypeRestriction(expression, target.GetType()), target)
            {
                _target = target;
            }

            private DynamicMetaObject Result(object value)
            {
                return new DynamicMetaObject(
                    Expression.Constant(value, typeof(object)),
                    BindingRestrictions.GetTypeRestriction(Expression, _target.GetType()),
                    value);
            }

            /// <summary>
            /// The meta-object handed to a binder's Fallback* method: a plain view of the same value,
            /// so the language binder resolves against the CLR type instead of coming back here.
            /// </summary>
            private DynamicMetaObject Fallback()
            {
                return new DynamicMetaObject(Expression, BindingRestrictions.Empty, _target);
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return _target.GetDynamicMemberNames();
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                if (binder == null) throw new ArgumentNullException("binder");
                object result;
                return _target.TryGetMember(binder, out result)
                    ? Result(result)
                    : binder.FallbackGetMember(Fallback());
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                if (binder == null) throw new ArgumentNullException("binder");
                var newValue = value == null ? null : value.Value;
                return _target.TrySetMember(binder, newValue)
                    ? Result(newValue)
                    : binder.FallbackSetMember(Fallback(), value);
            }

            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
            {
                if (binder == null) throw new ArgumentNullException("binder");
                return _target.TryDeleteMember(binder)
                    ? Result(null)
                    : binder.FallbackDeleteMember(Fallback());
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
            {
                if (binder == null) throw new ArgumentNullException("binder");
                object result;
                return _target.TryInvokeMember(binder, Values(args), out result)
                    ? Result(result)
                    : binder.FallbackInvokeMember(Fallback(), args);
            }

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
            {
                if (binder == null) throw new ArgumentNullException("binder");
                object result;
                return _target.TryInvoke(binder, Values(args), out result)
                    ? Result(result)
                    : binder.FallbackInvoke(Fallback(), args);
            }

            public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
            {
                if (binder == null) throw new ArgumentNullException("binder");
                object result;
                return _target.TryCreateInstance(binder, Values(args), out result)
                    ? Result(result)
                    : binder.FallbackCreateInstance(Fallback(), args);
            }

            public override DynamicMetaObject BindConvert(ConvertBinder binder)
            {
                if (binder == null) throw new ArgumentNullException("binder");
                object result;
                return _target.TryConvert(binder, out result)
                    ? Result(result)
                    : binder.FallbackConvert(Fallback());
            }

            public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
            {
                if (binder == null) throw new ArgumentNullException("binder");
                object result;
                return _target.TryGetIndex(binder, Values(indexes), out result)
                    ? Result(result)
                    : binder.FallbackGetIndex(Fallback(), indexes);
            }

            public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
            {
                if (binder == null) throw new ArgumentNullException("binder");
                var newValue = value == null ? null : value.Value;
                return _target.TrySetIndex(binder, Values(indexes), newValue)
                    ? Result(newValue)
                    : binder.FallbackSetIndex(Fallback(), indexes, value);
            }

            public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
            {
                if (binder == null) throw new ArgumentNullException("binder");
                return _target.TryDeleteIndex(binder, Values(indexes))
                    ? Result(null)
                    : binder.FallbackDeleteIndex(Fallback(), indexes);
            }

            public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
            {
                if (binder == null) throw new ArgumentNullException("binder");
                object result;
                return _target.TryBinaryOperation(binder, arg == null ? null : arg.Value, out result)
                    ? Result(result)
                    : binder.FallbackBinaryOperation(Fallback(), arg);
            }

            public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
            {
                if (binder == null) throw new ArgumentNullException("binder");
                object result;
                return _target.TryUnaryOperation(binder, out result)
                    ? Result(result)
                    : binder.FallbackUnaryOperation(Fallback());
            }

            private static object[] Values(DynamicMetaObject[] metaObjects)
            {
                if (metaObjects == null)
                    return new object[0];

                var values = new object[metaObjects.Length];
                for (var i = 0; i < metaObjects.Length; i++)
                    values[i] = metaObjects[i] == null ? null : metaObjects[i].Value;
                return values;
            }
        }
    }
}
