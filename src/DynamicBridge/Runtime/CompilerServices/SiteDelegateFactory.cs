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
using System.Reflection.Emit;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Builds the delegate that sits in <see cref="F:System.Runtime.CompilerServices.CallSite`1.Target" />.
    /// </summary>
    /// <remarks>
    /// The C# compiler gives every dynamic call site a strongly typed delegate — typically
    /// <c>Func&lt;CallSite, object, ..., object&gt;</c>, but any signature the site needs, including
    /// value-typed and by-ref parameters. This class emits, for such a signature, a stub that packs
    /// the arguments into an <c>object[]</c>, hands them to
    /// <see cref="M:System.Runtime.CompilerServices.CallSiteOps.Dispatch(System.Object,System.Object[])" />,
    /// writes back any by-ref arguments and unboxes the result.
    ///
    /// The stub is emitted once per site and closes over the site itself, so the per-call cost is an
    /// array allocation plus the binder's own cache lookup.
    /// </remarks>
    internal static class SiteDelegateFactory
    {
        private static readonly MethodInfo DispatchMethod =
            typeof(CallSiteOps).GetMethod("Dispatch", BindingFlags.Public | BindingFlags.Static);

        internal static T CreateDispatcher<T>(CallSite site) where T : class
        {
            var delegateType = typeof(T);
            if (!typeof(Delegate).IsAssignableFrom(delegateType))
                throw new ArgumentException("CallSite<T> requires T to be a delegate type; got " + delegateType.FullName);

            var invoke = delegateType.GetMethod("Invoke");
            var parameters = invoke.GetParameters();
            if (parameters.Length < 1)
                throw new ArgumentException("A call site delegate must take the CallSite as its first parameter: " + delegateType.FullName);

            var returnType = invoke.ReturnType;
            var argumentCount = parameters.Length - 1;

            // [0] is the closure (the CallSite), then the delegate's own parameters.
            var signature = new Type[parameters.Length + 1];
            signature[0] = typeof(object);
            for (var i = 0; i < parameters.Length; i++)
                signature[i + 1] = parameters[i].ParameterType;

            var method = new DynamicMethod(
                "CallSiteDispatch",
                returnType,
                signature,
                typeof(SiteDelegateFactory),
                true);

            var il = method.GetILGenerator();
            var args = il.DeclareLocal(typeof(object[]));
            var result = il.DeclareLocal(typeof(object));

            EmitLdcI4(il, argumentCount);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, args);

            for (var i = 0; i < argumentCount; i++)
            {
                // Delegate parameter i+1 is DynamicMethod argument i+2.
                var parameterType = parameters[i + 1].ParameterType;
                il.Emit(OpCodes.Ldloc, args);
                EmitLdcI4(il, i);
                EmitLdarg(il, i + 2);

                if (parameterType.IsByRef)
                {
                    var elementType = parameterType.GetElementType();
                    EmitLoadIndirect(il, elementType);
                    if (elementType.IsValueType)
                        il.Emit(OpCodes.Box, elementType);
                }
                else if (parameterType.IsValueType)
                {
                    il.Emit(OpCodes.Box, parameterType);
                }

                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc, args);
            il.Emit(OpCodes.Call, DispatchMethod);
            il.Emit(OpCodes.Stloc, result);

            // Copy back by-ref arguments; the binder writes 'out'/'ref' results into the array.
            for (var i = 0; i < argumentCount; i++)
            {
                var parameterType = parameters[i + 1].ParameterType;
                if (!parameterType.IsByRef)
                    continue;

                var elementType = parameterType.GetElementType();
                EmitLdarg(il, i + 2);
                il.Emit(OpCodes.Ldloc, args);
                EmitLdcI4(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                il.Emit(OpCodes.Unbox_Any, elementType);
                EmitStoreIndirect(il, elementType);
            }

            if (returnType != typeof(void))
            {
                il.Emit(OpCodes.Ldloc, result);
                il.Emit(OpCodes.Unbox_Any, returnType);
            }

            il.Emit(OpCodes.Ret);

            return (T)(object)method.CreateDelegate(delegateType, site);
        }

        private static void EmitLoadIndirect(ILGenerator il, Type type)
        {
            if (!type.IsValueType)
                il.Emit(OpCodes.Ldind_Ref);
            else if (type.IsEnum || type.IsPrimitive)
                il.Emit(OpCodes.Ldobj, type);
            else
                il.Emit(OpCodes.Ldobj, type);
        }

        private static void EmitStoreIndirect(ILGenerator il, Type type)
        {
            if (!type.IsValueType)
                il.Emit(OpCodes.Stind_Ref);
            else
                il.Emit(OpCodes.Stobj, type);
        }

        private static void EmitLdarg(ILGenerator il, int index)
        {
            switch (index)
            {
                case 0: il.Emit(OpCodes.Ldarg_0); break;
                case 1: il.Emit(OpCodes.Ldarg_1); break;
                case 2: il.Emit(OpCodes.Ldarg_2); break;
                case 3: il.Emit(OpCodes.Ldarg_3); break;
                default:
                    if (index <= byte.MaxValue)
                        il.Emit(OpCodes.Ldarg_S, (byte)index);
                    else
                        il.Emit(OpCodes.Ldarg, (short)index);
                    break;
            }
        }

        private static void EmitLdcI4(ILGenerator il, int value)
        {
            if (value >= -1 && value <= 8)
            {
                switch (value)
                {
                    case -1: il.Emit(OpCodes.Ldc_I4_M1); return;
                    case 0: il.Emit(OpCodes.Ldc_I4_0); return;
                    case 1: il.Emit(OpCodes.Ldc_I4_1); return;
                    case 2: il.Emit(OpCodes.Ldc_I4_2); return;
                    case 3: il.Emit(OpCodes.Ldc_I4_3); return;
                    case 4: il.Emit(OpCodes.Ldc_I4_4); return;
                    case 5: il.Emit(OpCodes.Ldc_I4_5); return;
                    case 6: il.Emit(OpCodes.Ldc_I4_6); return;
                    case 7: il.Emit(OpCodes.Ldc_I4_7); return;
                    case 8: il.Emit(OpCodes.Ldc_I4_8); return;
                }
            }

            if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
                il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
            else
                il.Emit(OpCodes.Ldc_I4, value);
        }
    }
}
