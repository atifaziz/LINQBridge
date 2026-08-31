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

namespace System.Runtime.CompilerServices
{
    /// <summary>Class responsible for runtime binding of the dynamic operations on the dynamic call site.</summary>
    public abstract class CallSiteBinder
    {
        private static LabelTarget _updateLabel;

        /// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" /> class.</summary>
        protected CallSiteBinder()
        {
        }

        /// <summary>Gets a label that can be used to cause the binding to be updated.</summary>
        public static LabelTarget UpdateLabel
        {
            get
            {
                if (_updateLabel == null)
                    _updateLabel = LabelTarget.Create(typeof(void), "CallSiteBinder.UpdateLabel");
                return _updateLabel;
            }
        }

        /// <summary>Performs the runtime binding of the dynamic operation on a set of arguments.</summary>
        /// <param name="args">An array of arguments to the dynamic operation.</param>
        /// <param name="parameters">The array of <see cref="T:System.Linq.Expressions.ParameterExpression" /> instances that represent the parameters of the call site in the binding process.</param>
        /// <param name="returnLabel">A LabelTarget used to return the result of the dynamic binding.</param>
        /// <returns>An Expression that performs tests on the dynamic operation arguments, and performs the dynamic operation if the tests are valid.</returns>
        /// <remarks>
        /// DynamicBridge does not compile binding rules into the call site: see
        /// <see cref="M:System.Runtime.CompilerServices.CallSiteBinder.BindAndInvoke(System.Object[])" />,
        /// which is the path every binder in this assembly actually takes. The member is kept
        /// because it is part of the Framework's public contract for this type.
        /// </remarks>
        public abstract Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel);

        /// <summary>Provides low-level runtime binding support. Callers should generally not access this member.</summary>
        public virtual T BindDelegate<T>(CallSite<T> site, object[] args) where T : class
        {
            return null;
        }

        /// <summary>Adds a target to the cache of known targets.</summary>
        protected void CacheTarget<T>(T target) where T : class
        {
            // DynamicBridge caches resolved members inside the binders themselves rather than
            // caching compiled rule delegates on the site, so there is nothing to do here.
        }

        /// <summary>
        /// Binds and performs the dynamic operation directly, without producing a rule.
        /// </summary>
        /// <param name="args">The arguments of the call site; <c>args[0]</c> is the target.</param>
        /// <returns>The result of the operation, or <c>null</c> for a void operation.</returns>
        /// <remarks>
        /// This is the interpretive entry point <see cref="T:System.Runtime.CompilerServices.CallSite`1" />
        /// uses. A binder that only implements <see cref="M:System.Runtime.CompilerServices.CallSiteBinder.Bind(System.Object[],System.Collections.ObjectModel.ReadOnlyCollection{System.Linq.Expressions.ParameterExpression},System.Linq.Expressions.LabelTarget)" />
        /// — i.e. a third-party DLR binder that returns a rule expression — is not supported,
        /// because executing such a rule needs the 4.0 expression tree nodes and compiler that
        /// LinqBridge does not have yet.
        /// </remarks>
        internal virtual object BindAndInvoke(object[] args)
        {
            throw new NotSupportedException(
                "This CallSiteBinder only implements Bind(), which returns a rule expression tree. " +
                "DynamicBridge executes dynamic operations directly and cannot compile rule trees " +
                "on this framework. Derive from one of the System.Dynamic binders instead.");
        }
    }
}
