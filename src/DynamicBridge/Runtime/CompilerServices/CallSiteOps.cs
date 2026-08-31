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

using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    /// <summary>Creates and caches binding rules. This API supports the product infrastructure and is not intended to be used directly from your code.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class CallSiteOps
    {
        /// <summary>Performs the dynamic operation of a call site. Called by the emitted call site stub.</summary>
        /// <param name="site">The <see cref="T:System.Runtime.CompilerServices.CallSite" /> the operation belongs to.</param>
        /// <param name="args">The arguments of the operation; <c>args[0]</c> is the target.</param>
        /// <returns>The result of the operation, or <c>null</c> when the operation has no value.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static object Dispatch(object site, object[] args)
        {
            var callSite = (CallSite)site;
            return callSite.Binder.BindAndInvoke(args);
        }
    }
}
