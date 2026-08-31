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

using System.Reflection;

namespace System.Runtime.CompilerServices
{
    /// <summary>A dynamic call site base class. This type is used as a parameter type to the dynamic site targets.</summary>
    public class CallSite
    {
        private readonly CallSiteBinder _binder;

        internal CallSite(CallSiteBinder binder)
        {
            _binder = binder;
        }

        /// <summary>Class responsible for binding dynamic operations on the dynamic site.</summary>
        public CallSiteBinder Binder
        {
            get { return _binder; }
        }

        /// <summary>Creates a call site with the given delegate type and binder.</summary>
        /// <param name="delegateType">The call site delegate type.</param>
        /// <param name="binder">The call site binder.</param>
        /// <returns>The new call site.</returns>
        public static CallSite Create(Type delegateType, CallSiteBinder binder)
        {
            if (delegateType == null)
                throw new ArgumentNullException("delegateType");
            if (binder == null)
                throw new ArgumentNullException("binder");
            if (!typeof(Delegate).IsAssignableFrom(delegateType))
                throw new ArgumentException("Type must be derived from System.Delegate", "delegateType");

            var siteType = typeof(CallSite<>).MakeGenericType(delegateType);
            var create = siteType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
            try
            {
                return (CallSite)create.Invoke(null, new object[] { binder });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }
    }

    /// <summary>Dynamic site type.</summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    public class CallSite<T> : CallSite where T : class
    {
        /// <summary>The update delegate. Called when the dynamic site experiences cache miss.</summary>
        public T Update
        {
            get { return _update; }
        }

        /// <summary>The Level 0 cache - a delegate specialized based on the site history.</summary>
        public T Target;

        private readonly T _update;

        internal CallSite(CallSiteBinder binder)
            : base(binder)
        {
            _update = SiteDelegateFactory.CreateDispatcher<T>(this);
        }

        /// <summary>Creates an instance of the dynamic call site, initialized with the binder responsible for the runtime binding of the dynamic operations at this call site.</summary>
        /// <param name="binder">The binder responsible for the runtime binding of the dynamic operations at this call site.</param>
        /// <returns>The new instance of dynamic call site.</returns>
        public static CallSite<T> Create(CallSiteBinder binder)
        {
            if (binder == null)
                throw new ArgumentNullException("binder");

            var site = new CallSite<T>(binder);

            // The Framework starts Target at the update stub and lets the binder install
            // specialised rules as the site warms up. DynamicBridge has no rule compiler, so the
            // dispatcher stays installed for the life of the site and caching happens one level
            // down, inside the binder (see RuntimeBinderCache).
            site.Target = site.Update;
            return site;
        }
    }
}
