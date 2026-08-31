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

using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace System.Dynamic
{
    /// <summary>The dynamic call site binder that participates in the <see cref="T:System.Dynamic.DynamicMetaObject" /> binding protocol.</summary>
    /// <remarks>
    /// <para>
    /// In the Framework a binder returns a *rule* — an expression tree that the call site compiles and
    /// caches. DynamicBridge has no 4.0 expression compiler, so it runs the very same binding protocol
    /// eagerly: <see cref="M:System.Dynamic.DynamicMetaObjectBinder.Bind(System.Dynamic.DynamicMetaObject,System.Dynamic.DynamicMetaObject[])" />
    /// is called with meta-objects that carry their runtime values, and the resulting meta-object is
    /// expected to carry the computed value (<see cref="P:System.Dynamic.DynamicMetaObject.HasValue" />).
    /// </para>
    /// <para>
    /// The upshot is that <see cref="T:System.Dynamic.DynamicObject" /> subclasses and custom binders
    /// see exactly the protocol they see on 4.0; only rule caching differs.
    /// </para>
    /// </remarks>
    public abstract class DynamicMetaObjectBinder : CallSiteBinder
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.DynamicMetaObjectBinder" /> class.</summary>
        protected DynamicMetaObjectBinder()
        {
        }

        /// <summary>The result type of the operation.</summary>
        public virtual Type ReturnType
        {
            get { return typeof(object); }
        }

        /// <summary>Performs the runtime binding of the dynamic operation on a set of arguments.</summary>
        public sealed override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
        {
            throw new NotSupportedException(
                "DynamicBridge binds dynamic operations directly rather than producing rule expression " +
                "trees; the 4.0 expression tree nodes a rule needs (Block, Goto, Label) do not exist on " +
                "this framework. Use Bind(DynamicMetaObject, DynamicMetaObject[]) instead.");
        }

        /// <summary>Performs the binding of the dynamic operation.</summary>
        /// <param name="target">The target of the dynamic operation.</param>
        /// <param name="args">An array of arguments of the dynamic operation.</param>
        /// <returns>The <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
        public abstract DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args);

        /// <summary>Defers the binding of the operation until later time when the runtime values of all dynamic operation arguments have been computed.</summary>
        public DynamicMetaObject Defer(DynamicMetaObject target, params DynamicMetaObject[] args)
        {
            // Every meta-object already has its runtime value here, so there is nothing to defer to.
            return Bind(target, args ?? DynamicMetaObject.EmptyMetaObjects);
        }

        /// <summary>Defers the binding of the operation until later time when the runtime values of all dynamic operation arguments have been computed.</summary>
        public DynamicMetaObject Defer(params DynamicMetaObject[] args)
        {
            if (args == null || args.Length == 0)
                throw new ArgumentException("At least one argument is required", "args");

            var rest = new DynamicMetaObject[args.Length - 1];
            Array.Copy(args, 1, rest, 0, rest.Length);
            return Bind(args[0], rest);
        }

        /// <summary>Gets an expression that will cause the binding to be updated.</summary>
        public Expression GetUpdateExpression(Type type)
        {
            // There is no rule to invalidate; the site always re-binds.
            return Expression.Constant(null, typeof(object));
        }

        internal sealed override object BindAndInvoke(object[] args)
        {
            if (args == null || args.Length == 0)
                throw new ArgumentException("A dynamic operation needs at least a target", "args");

            var target = CreateMetaObject(args[0]);
            var rest = new DynamicMetaObject[args.Length - 1];
            for (var i = 1; i < args.Length; i++)
                rest[i - 1] = CreateMetaObject(args[i]);

            Microsoft.CSharp.RuntimeBinder.MemberBinding.PendingWriteBack = null;

            var result = Bind(target, rest);
            if (result == null)
                throw new InvalidOperationException(GetType().Name + ".Bind returned null");

            // An invocation with 'ref'/'out' arguments leaves its parameter array behind; copying it
            // back into args lets the emitted call site stub store the results into the caller's
            // variables. Only the straightforward positional case is written back.
            var writeBack = Microsoft.CSharp.RuntimeBinder.MemberBinding.PendingWriteBack;
            Microsoft.CSharp.RuntimeBinder.MemberBinding.PendingWriteBack = null;
            if (writeBack != null && writeBack.Length == args.Length - 1)
            {
                for (var i = 0; i < writeBack.Length; i++)
                    args[i + 1] = writeBack[i];
            }

            return Unwrap(result);
        }

        internal static DynamicMetaObject CreateMetaObject(object value)
        {
            return DynamicMetaObject.Create(value, Expression.Constant(value, typeof(object)));
        }

        /// <summary>
        /// Extracts the runtime result from the meta-object a binder produced.
        /// </summary>
        internal static object Unwrap(DynamicMetaObject result)
        {
            if (result.HasValue)
                return result.Value;

            var constant = result.Expression as ConstantExpression;
            if (constant != null)
                return constant.Value;

            // Last resort for a meta-object that describes its result purely as an expression. This
            // succeeds only while the tree stays inside the 3.5 node set the available compiler can
            // emit, which is why every binder in this assembly returns a value-carrying meta-object.
            try
            {
                var body = result.Expression.Type == typeof(void)
                    ? null
                    : (result.Expression.Type == typeof(object)
                        ? result.Expression
                        : Expression.Convert(result.Expression, typeof(object)));

                if (body == null)
                    return null;

                return Expression.Lambda<Func<object>>(body).Compile()();
            }
            catch (Exception e)
            {
                throw new NotSupportedException(
                    "The binder produced a rule expression that DynamicBridge cannot execute on this " +
                    "framework. Return a DynamicMetaObject carrying its runtime value instead.", e);
            }
        }
    }
}
