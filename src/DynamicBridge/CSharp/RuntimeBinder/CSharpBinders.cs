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
using System.Dynamic;
using System.Linq.Expressions;

namespace Microsoft.CSharp.RuntimeBinder
{
    /// <summary>Shared plumbing for the concrete C# binders.</summary>
    internal static class BinderHelpers
    {
        /// <summary>
        /// Works out what a dynamic operation is applied to: a static type (<c>SomeType.Member</c>,
        /// flagged IsStaticType at the call site) or an instance.
        /// </summary>
        internal static void ResolveTarget(DynamicMetaObject target, CSharpArgumentInfo info,
                                           out object instance, out Type type, out bool isStatic)
        {
            var value = target.Value;

            if (info != null && info.IsStaticType && value is Type)
            {
                instance = null;
                type = (Type)value;
                isStatic = true;
                return;
            }

            if (value == null)
                throw new RuntimeBinderException("Cannot perform runtime binding on a null reference");

            instance = value;
            type = value.GetType();
            isStatic = false;
        }

        internal static bool AllowsNonPublic(Type context, Type target)
        {
            return context != null && (context == target || target.IsAssignableFrom(context) || context.IsAssignableFrom(target));
        }

        internal static object[] Values(DynamicMetaObject[] metaObjects)
        {
            if (metaObjects == null)
                return new object[0];

            var values = new object[metaObjects.Length];
            for (var i = 0; i < metaObjects.Length; i++)
                values[i] = metaObjects[i] == null ? null : metaObjects[i].Value;
            return values;
        }

        /// <summary>Argument names for the arguments after the target, or null when none is named.</summary>
        internal static string[] Names(IList<CSharpArgumentInfo> argumentInfo, int skip, int count)
        {
            if (argumentInfo == null)
                return null;

            string[] names = null;
            for (var i = 0; i < count; i++)
            {
                var index = i + skip;
                if (index >= argumentInfo.Count)
                    break;

                var info = argumentInfo[index];
                if (info == null || !info.IsNamed)
                    continue;

                if (names == null)
                    names = new string[count];
                names[i] = info.Name;
            }

            return names;
        }

        internal static CSharpArgumentInfo At(IList<CSharpArgumentInfo> argumentInfo, int index)
        {
            return argumentInfo != null && index < argumentInfo.Count ? argumentInfo[index] : null;
        }

        internal static DynamicMetaObject Result(object value)
        {
            return new DynamicMetaObject(Expression.Constant(value, typeof(object)), BindingRestrictions.Empty, value);
        }
    }

    internal sealed class CSharpGetMemberBinder : GetMemberBinder
    {
        private readonly Type _context;
        private readonly IList<CSharpArgumentInfo> _argumentInfo;

        internal CSharpGetMemberBinder(string name, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo)
            : base(name, false)
        {
            _context = context;
            _argumentInfo = new List<CSharpArgumentInfo>(argumentInfo ?? new CSharpArgumentInfo[0]);
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            object instance;
            Type type;
            bool isStatic;
            BinderHelpers.ResolveTarget(target, BinderHelpers.At(_argumentInfo, 0), out instance, out type, out isStatic);

            return BinderHelpers.Result(MemberBinding.GetMember(instance, type, Name, IgnoreCase, isStatic,
                BinderHelpers.AllowsNonPublic(_context, type)));
        }
    }

    internal sealed class CSharpSetMemberBinder : SetMemberBinder
    {
        private readonly Type _context;
        private readonly IList<CSharpArgumentInfo> _argumentInfo;

        internal CSharpSetMemberBinder(string name, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo)
            : base(name, false)
        {
            _context = context;
            _argumentInfo = new List<CSharpArgumentInfo>(argumentInfo ?? new CSharpArgumentInfo[0]);
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            object instance;
            Type type;
            bool isStatic;
            BinderHelpers.ResolveTarget(target, BinderHelpers.At(_argumentInfo, 0), out instance, out type, out isStatic);

            return BinderHelpers.Result(MemberBinding.SetMember(instance, type, Name, value == null ? null : value.Value,
                IgnoreCase, isStatic, BinderHelpers.AllowsNonPublic(_context, type)));
        }
    }

    internal sealed class CSharpInvokeMemberBinder : InvokeMemberBinder
    {
        private readonly Type _context;
        private readonly IList<Type> _typeArguments;
        private readonly IList<CSharpArgumentInfo> _argumentInfo;
        private readonly bool _isChecked;

        internal CSharpInvokeMemberBinder(string name, IEnumerable<Type> typeArguments, Type context,
                                          IEnumerable<CSharpArgumentInfo> argumentInfo, bool isChecked)
            : base(name, false, new CallInfo(CountArguments(argumentInfo), ArgumentNames(argumentInfo)))
        {
            _context = context;
            _typeArguments = new List<Type>(typeArguments ?? new Type[0]);
            _argumentInfo = new List<CSharpArgumentInfo>(argumentInfo ?? new CSharpArgumentInfo[0]);
            _isChecked = isChecked;
        }

        internal static int CountArguments(IEnumerable<CSharpArgumentInfo> argumentInfo)
        {
            var count = 0;
            if (argumentInfo != null)
            {
                foreach (var info in argumentInfo)
                    count++;
            }
            return Math.Max(0, count - 1); // the target is not an argument
        }

        internal static IEnumerable<string> ArgumentNames(IEnumerable<CSharpArgumentInfo> argumentInfo)
        {
            var names = new List<string>();
            if (argumentInfo == null)
                return names;

            var first = true;
            foreach (var info in argumentInfo)
            {
                if (first)
                {
                    first = false;
                    continue;
                }
                if (info != null && info.IsNamed)
                    names.Add(info.Name);
            }
            return names;
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            object instance;
            Type type;
            bool isStatic;
            BinderHelpers.ResolveTarget(target, BinderHelpers.At(_argumentInfo, 0), out instance, out type, out isStatic);

            var values = BinderHelpers.Values(args);
            var typeArguments = new Type[_typeArguments.Count];
            _typeArguments.CopyTo(typeArguments, 0);

            return BinderHelpers.Result(MemberBinding.InvokeMember(instance, type, Name, typeArguments, values,
                BinderHelpers.Names(_argumentInfo, 1, values.Length), IgnoreCase, isStatic, _isChecked,
                BinderHelpers.AllowsNonPublic(_context, type)));
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            var invocable = target.Value as Delegate;
            if (invocable == null)
                throw new RuntimeBinderException("Cannot invoke a non-delegate type");

            var values = BinderHelpers.Values(args);
            return BinderHelpers.Result(MemberBinding.InvokeDelegate(invocable, values,
                BinderHelpers.Names(_argumentInfo, 1, values.Length), _isChecked));
        }
    }

    internal sealed class CSharpInvokeBinder : InvokeBinder
    {
        private readonly IList<CSharpArgumentInfo> _argumentInfo;
        private readonly bool _isChecked;

        internal CSharpInvokeBinder(Type context, IEnumerable<CSharpArgumentInfo> argumentInfo, bool isChecked)
            : base(new CallInfo(CSharpInvokeMemberBinder.CountArguments(argumentInfo), CSharpInvokeMemberBinder.ArgumentNames(argumentInfo)))
        {
            _argumentInfo = new List<CSharpArgumentInfo>(argumentInfo ?? new CSharpArgumentInfo[0]);
            _isChecked = isChecked;
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            if (target.Value == null)
                throw new RuntimeBinderException("Cannot perform runtime binding on a null reference");

            var invocable = target.Value as Delegate;
            if (invocable == null)
                throw new RuntimeBinderException(string.Format(
                    "Cannot invoke an expression of type '{0}'", Conversions.Format(target.Value.GetType())));

            var values = BinderHelpers.Values(args);
            return BinderHelpers.Result(MemberBinding.InvokeDelegate(invocable, values,
                BinderHelpers.Names(_argumentInfo, 1, values.Length), _isChecked));
        }
    }

    internal sealed class CSharpInvokeConstructorBinder : CreateInstanceBinder
    {
        private readonly IList<CSharpArgumentInfo> _argumentInfo;
        private readonly bool _isChecked;

        internal CSharpInvokeConstructorBinder(Type context, IEnumerable<CSharpArgumentInfo> argumentInfo, bool isChecked)
            : base(new CallInfo(CSharpInvokeMemberBinder.CountArguments(argumentInfo), CSharpInvokeMemberBinder.ArgumentNames(argumentInfo)))
        {
            _argumentInfo = new List<CSharpArgumentInfo>(argumentInfo ?? new CSharpArgumentInfo[0]);
            _isChecked = isChecked;
        }

        public override DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            var type = target.Value as Type;
            if (type == null)
                throw new RuntimeBinderException("The type of the target of a constructor invocation must be a System.Type");

            var values = BinderHelpers.Values(args);
            return BinderHelpers.Result(MemberBinding.CreateInstance(type, values,
                BinderHelpers.Names(_argumentInfo, 1, values.Length), _isChecked));
        }
    }

    internal sealed class CSharpGetIndexBinder : GetIndexBinder
    {
        private readonly bool _isChecked;

        internal CSharpGetIndexBinder(Type context, IEnumerable<CSharpArgumentInfo> argumentInfo, bool isChecked)
            : base(new CallInfo(CSharpInvokeMemberBinder.CountArguments(argumentInfo), CSharpInvokeMemberBinder.ArgumentNames(argumentInfo)))
        {
            _isChecked = isChecked;
        }

        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
        {
            if (target.Value == null)
                throw new RuntimeBinderException("Cannot perform runtime binding on a null reference");

            return BinderHelpers.Result(MemberBinding.GetIndex(target.Value, target.Value.GetType(),
                BinderHelpers.Values(indexes), _isChecked));
        }
    }

    internal sealed class CSharpSetIndexBinder : SetIndexBinder
    {
        private readonly bool _isChecked;

        internal CSharpSetIndexBinder(Type context, IEnumerable<CSharpArgumentInfo> argumentInfo, bool isChecked)
            : base(new CallInfo(CSharpInvokeMemberBinder.CountArguments(argumentInfo), CSharpInvokeMemberBinder.ArgumentNames(argumentInfo)))
        {
            _isChecked = isChecked;
        }

        public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            if (target.Value == null)
                throw new RuntimeBinderException("Cannot perform runtime binding on a null reference");

            return BinderHelpers.Result(MemberBinding.SetIndex(target.Value, target.Value.GetType(),
                BinderHelpers.Values(indexes), value == null ? null : value.Value, _isChecked));
        }
    }

    internal sealed class CSharpConvertBinder : ConvertBinder
    {
        private readonly bool _isChecked;

        internal CSharpConvertBinder(Type type, Type context, bool isExplicit, bool isChecked)
            : base(type, isExplicit)
        {
            _isChecked = isChecked;
        }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            return BinderHelpers.Result(Conversions.Convert(target.Value, Type, Explicit, _isChecked));
        }
    }

    internal sealed class CSharpBinaryOperationBinder : BinaryOperationBinder
    {
        private readonly bool _isChecked;
        private readonly bool _isLogical;

        internal CSharpBinaryOperationBinder(ExpressionType operation, Type context,
                                             IEnumerable<CSharpArgumentInfo> argumentInfo, bool isChecked, bool isLogical)
            : base(operation)
        {
            _isChecked = isChecked;
            _isLogical = isLogical;
        }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            return BinderHelpers.Result(Operators.Binary(Operation, target.Value, arg == null ? null : arg.Value, _isChecked));
        }
    }

    internal sealed class CSharpUnaryOperationBinder : UnaryOperationBinder
    {
        private readonly bool _isChecked;

        internal CSharpUnaryOperationBinder(ExpressionType operation, Type context,
                                            IEnumerable<CSharpArgumentInfo> argumentInfo, bool isChecked)
            : base(operation)
        {
            _isChecked = isChecked;
        }

        public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            return BinderHelpers.Result(Operators.Unary(Operation, target.Value, _isChecked));
        }
    }

    /// <summary>
    /// Answers whether a member is an event, which the compiler asks before it lets <c>+=</c> on a
    /// dynamic expression compile to an event subscription.
    /// </summary>
    internal sealed class CSharpIsEventBinder : DynamicMetaObjectBinder
    {
        private readonly string _name;
        private readonly Type _context;

        internal CSharpIsEventBinder(string name, Type context)
        {
            _name = name;
            _context = context;
        }

        public override Type ReturnType
        {
            get { return typeof(bool); }
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            var value = target.Value;
            if (value == null)
                return BinderHelpers.Result(false);

            var type = value as Type ?? value.GetType();
            var isStatic = value is Type;
            return BinderHelpers.Result(MemberBinding.IsEvent(type, _name, false, isStatic));
        }
    }
}
