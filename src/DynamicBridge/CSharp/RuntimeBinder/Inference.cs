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
using System.Reflection;

namespace Microsoft.CSharp.RuntimeBinder
{
    /// <summary>
    /// Type inference for a generic method invoked through a dynamic call site, driven by the runtime
    /// types of the arguments.
    /// </summary>
    internal static class Inference
    {
        internal static Type[] Infer(MethodInfo definition, object[] args)
        {
            var typeParameters = definition.GetGenericArguments();
            var parameters = definition.GetParameters();
            var bindings = new Dictionary<Type, Type>();

            var count = Math.Min(parameters.Length, args.Length);
            for (var i = 0; i < count; i++)
            {
                if (args[i] == null)
                    continue;
                Bind(parameters[i].ParameterType, args[i].GetType(), bindings);
            }

            var inferred = new Type[typeParameters.Length];
            for (var i = 0; i < typeParameters.Length; i++)
            {
                Type argument;
                if (!bindings.TryGetValue(typeParameters[i], out argument))
                    return null;
                inferred[i] = argument;
            }

            return inferred;
        }

        private static void Bind(Type parameter, Type argument, Dictionary<Type, Type> bindings)
        {
            if (parameter.IsByRef)
                parameter = parameter.GetElementType();

            if (parameter.IsGenericParameter)
            {
                Type existing;
                if (!bindings.TryGetValue(parameter, out existing))
                {
                    bindings[parameter] = argument;
                }
                else if (existing != argument && existing.IsAssignableFrom(argument) == false && argument.IsAssignableFrom(existing))
                {
                    // Two arguments contributed different types; keep the one that can hold both.
                    bindings[parameter] = argument;
                }
                return;
            }

            if (!parameter.ContainsGenericParameters)
                return;

            if (parameter.IsArray)
            {
                if (argument.IsArray)
                    Bind(parameter.GetElementType(), argument.GetElementType(), bindings);
                else
                    BindThroughInterfaces(parameter, argument, bindings);
                return;
            }

            if (parameter.IsGenericType)
            {
                var construction = FindConstruction(argument, parameter.GetGenericTypeDefinition());
                if (construction == null)
                    return;

                var parameterArguments = parameter.GetGenericArguments();
                var argumentArguments = construction.GetGenericArguments();
                var count = Math.Min(parameterArguments.Length, argumentArguments.Length);
                for (var i = 0; i < count; i++)
                    Bind(parameterArguments[i], argumentArguments[i], bindings);
            }
        }

        private static void BindThroughInterfaces(Type parameter, Type argument, Dictionary<Type, Type> bindings)
        {
            // T[] against, say, List<int>: nothing to infer without variance rules; left deliberately
            // simple, because the resolution step re-checks applicability afterwards.
            foreach (var contract in argument.GetInterfaces())
            {
                if (contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    Bind(parameter.GetElementType(), contract.GetGenericArguments()[0], bindings);
                    return;
                }
            }
        }

        private static Type FindConstruction(Type type, Type definition)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == definition)
                    return current;
            }

            foreach (var contract in type.GetInterfaces())
            {
                if (contract.IsGenericType && contract.GetGenericTypeDefinition() == definition)
                    return contract;
            }

            return null;
        }
    }
}
