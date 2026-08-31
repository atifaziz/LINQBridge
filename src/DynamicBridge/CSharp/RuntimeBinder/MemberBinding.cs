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
using System.Globalization;
using System.Reflection;

namespace Microsoft.CSharp.RuntimeBinder
{
    /// <summary>
    /// Member lookup, overload resolution and invocation against the runtime types of the arguments.
    /// </summary>
    internal static class MemberBinding
    {
        /// <summary>
        /// Set by <see cref="M:MemberBinding.InvokeMethod" /> to the parameter array the invocation
        /// actually used, so a caller can copy 'ref'/'out' results back into the call site's
        /// variables. Read (and cleared) immediately after binding, on the same thread.
        /// </summary>
        [ThreadStatic]
        internal static object[] PendingWriteBack;

        internal static BindingFlags Flags(bool isStatic, bool ignoreCase, bool nonPublic)
        {
            var flags = BindingFlags.Public | BindingFlags.FlattenHierarchy;
            if (nonPublic)
                flags |= BindingFlags.NonPublic;
            flags |= isStatic ? BindingFlags.Static : BindingFlags.Instance | BindingFlags.Static;
            if (ignoreCase)
                flags |= BindingFlags.IgnoreCase;
            return flags;
        }

        internal static object GetMember(object target, Type type, string name, bool ignoreCase, bool isStatic, bool nonPublic)
        {
            var flags = Flags(isStatic, ignoreCase, nonPublic);

            var property = FindProperty(type, name, flags);
            if (property != null)
            {
                var getter = property.GetGetMethod();
                if (getter == null)
                    throw new RuntimeBinderException(string.Format(CultureInfo.InvariantCulture,
                        "The property or indexer '{0}.{1}' cannot be used in this context because it lacks the get accessor",
                        Conversions.Format(type), name));
                return Invoke(getter, getter.IsStatic ? null : target, new object[0]);
            }

            var field = type.GetField(name, flags);
            if (field != null)
                return field.GetValue(field.IsStatic ? null : target);

            var nested = type.GetNestedType(name, BindingFlags.Public);
            if (nested != null)
                return nested;

            if (FindEvent(type, name, flags) != null)
                throw new RuntimeBinderException(string.Format(CultureInfo.InvariantCulture,
                    "The event '{0}.{1}' can only appear on the left hand side of += or -=",
                    Conversions.Format(type), name));

            throw MissingMember(type, name);
        }

        internal static object SetMember(object target, Type type, string name, object value, bool ignoreCase, bool isStatic, bool nonPublic)
        {
            var flags = Flags(isStatic, ignoreCase, nonPublic);

            var property = FindProperty(type, name, flags);
            if (property != null)
            {
                var setter = property.GetSetMethod();
                if (setter == null)
                    throw new RuntimeBinderException(string.Format(CultureInfo.InvariantCulture,
                        "Property or indexer '{0}.{1}' cannot be assigned to -- it is read only",
                        Conversions.Format(type), name));

                var converted = Conversions.Convert(value, property.PropertyType, false, false);
                Invoke(setter, setter.IsStatic ? null : target, new[] { converted });
                return value;
            }

            var field = type.GetField(name, flags);
            if (field != null)
            {
                if (field.IsInitOnly || field.IsLiteral)
                    throw new RuntimeBinderException(string.Format(CultureInfo.InvariantCulture,
                        "A readonly field '{0}.{1}' cannot be assigned to", Conversions.Format(type), name));

                field.SetValue(field.IsStatic ? null : target, Conversions.Convert(value, field.FieldType, false, false));
                return value;
            }

            throw MissingMember(type, name);
        }

        internal static bool IsEvent(Type type, string name, bool ignoreCase, bool isStatic)
        {
            return FindEvent(type, name, Flags(isStatic, ignoreCase, false)) != null;
        }

        internal static object InvokeMember(object target, Type type, string name, Type[] typeArguments,
                                            object[] args, string[] argumentNames, bool ignoreCase, bool isStatic,
                                            bool isChecked, bool nonPublic)
        {
            var flags = Flags(isStatic, ignoreCase, nonPublic);
            var candidates = new List<MethodBase>();

            foreach (var method in type.GetMethods(flags))
            {
                if (string.Equals(method.Name, name, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    candidates.Add(method);
            }

            // Interfaces do not inherit object's members, and an interface-typed target reaches us
            // with only its own methods; walk the inherited interfaces too.
            if (type.IsInterface)
            {
                foreach (var contract in type.GetInterfaces())
                {
                    foreach (var method in contract.GetMethods(flags))
                    {
                        if (string.Equals(method.Name, name, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                            candidates.Add(method);
                    }
                }
            }

            if (candidates.Count > 0)
            {
                var resolved = Resolve(candidates, typeArguments, args, argumentNames, isChecked);
                if (resolved != null)
                    return Invoke((MethodInfo)resolved.Method, ((MethodInfo)resolved.Method).IsStatic ? null : target, resolved.Arguments);
            }

            // A delegate-valued property or field is invocable too: d.Handler(1) where Handler is a Func.
            var member = TryGetMemberValue(target, type, name, ignoreCase, isStatic, nonPublic);
            var invocable = member as Delegate;
            if (invocable != null)
                return InvokeDelegate(invocable, args, argumentNames, isChecked);

            if (candidates.Count > 0)
                throw NoOverload(type, name, args);

            throw MissingMember(type, name);
        }

        internal static object InvokeDelegate(Delegate target, object[] args, string[] argumentNames, bool isChecked)
        {
            var resolved = Resolve(new List<MethodBase> { target.Method }, null, args, argumentNames, isChecked);
            if (resolved == null)
                throw new RuntimeBinderException(string.Format(CultureInfo.InvariantCulture,
                    "Delegate '{0}' has some invalid arguments", Conversions.Format(target.GetType())));

            try
            {
                return target.DynamicInvoke(resolved.Arguments);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        internal static object CreateInstance(Type type, object[] args, string[] argumentNames, bool isChecked)
        {
            if (type.IsValueType && args.Length == 0)
                return Activator.CreateInstance(type);

            var candidates = new List<MethodBase>();
            foreach (var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
                candidates.Add(constructor);

            var resolved = Resolve(candidates, null, args, argumentNames, isChecked);
            if (resolved == null)
                throw new RuntimeBinderException(string.Format(CultureInfo.InvariantCulture,
                    "'{0}' does not contain a constructor that takes {1} arguments",
                    Conversions.Format(type), args.Length));

            try
            {
                return ((ConstructorInfo)resolved.Method).Invoke(resolved.Arguments);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        internal static object GetIndex(object target, Type type, object[] indexes, bool isChecked)
        {
            if (type.IsArray)
                return GetArrayElement((Array)target, indexes, isChecked);

            var indexer = FindIndexer(type, indexes, true, null, isChecked);
            if (indexer == null)
                throw new RuntimeBinderException(string.Format(CultureInfo.InvariantCulture,
                    "Cannot apply indexing with [] to an expression of type '{0}'", Conversions.Format(type)));

            var getter = indexer.Property.GetGetMethod();
            if (getter == null)
                throw new RuntimeBinderException(string.Format(CultureInfo.InvariantCulture,
                    "The property or indexer '{0}' cannot be used in this context because it lacks the get accessor",
                    Conversions.Format(type)));

            return Invoke(getter, target, indexer.Arguments);
        }

        internal static object SetIndex(object target, Type type, object[] indexes, object value, bool isChecked)
        {
            if (type.IsArray)
            {
                var array = (Array)target;
                var elementType = type.GetElementType();
                SetArrayElement(array, indexes, Conversions.Convert(value, elementType, false, isChecked), isChecked);
                return value;
            }

            var indexer = FindIndexer(type, indexes, false, value, isChecked);
            if (indexer == null)
                throw new RuntimeBinderException(string.Format(CultureInfo.InvariantCulture,
                    "Cannot apply indexing with [] to an expression of type '{0}'", Conversions.Format(type)));

            var setter = indexer.Property.GetSetMethod();
            if (setter == null)
                throw new RuntimeBinderException(string.Format(CultureInfo.InvariantCulture,
                    "Property or indexer '{0}' cannot be assigned to -- it is read only", Conversions.Format(type)));

            var arguments = new object[indexer.Arguments.Length + 1];
            Array.Copy(indexer.Arguments, arguments, indexer.Arguments.Length);
            arguments[arguments.Length - 1] = Conversions.Convert(value, indexer.Property.PropertyType, false, isChecked);

            Invoke(setter, target, arguments);
            return value;
        }

        private static object GetArrayElement(Array array, object[] indexes, bool isChecked)
        {
            var subscripts = ToSubscripts(indexes, isChecked);
            return subscripts.Length == 1 ? array.GetValue(subscripts[0]) : array.GetValue(subscripts);
        }

        private static void SetArrayElement(Array array, object[] indexes, object value, bool isChecked)
        {
            var subscripts = ToSubscripts(indexes, isChecked);
            if (subscripts.Length == 1)
                array.SetValue(value, subscripts[0]);
            else
                array.SetValue(value, subscripts);
        }

        private static int[] ToSubscripts(object[] indexes, bool isChecked)
        {
            var subscripts = new int[indexes.Length];
            for (var i = 0; i < indexes.Length; i++)
            {
                if (indexes[i] == null)
                    throw new RuntimeBinderException("An array index cannot be null");
                subscripts[i] = (int)Conversions.Convert(indexes[i], typeof(int), false, isChecked);
            }
            return subscripts;
        }

        private sealed class ResolvedIndexer
        {
            internal PropertyInfo Property;
            internal object[] Arguments;
        }

        private static ResolvedIndexer FindIndexer(Type type, object[] indexes, bool forRead, object value, bool isChecked)
        {
            ResolvedIndexer best = null;
            var bestScore = int.MaxValue;

            for (var current = type; current != null; current = current.BaseType)
            {
                foreach (var property in current.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    var parameters = property.GetIndexParameters();
                    if (parameters.Length == 0 || parameters.Length != indexes.Length)
                        continue;
                    if (forRead ? property.GetGetMethod() == null : property.GetSetMethod() == null)
                        continue;

                    var score = 0;
                    var arguments = new object[indexes.Length];
                    var applicable = true;

                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var rank = Conversions.Rank(indexes[i] == null ? null : indexes[i].GetType(), parameters[i].ParameterType);
                        if (rank == Conversions.NotApplicable)
                        {
                            applicable = false;
                            break;
                        }
                        score += rank;
                        arguments[i] = Conversions.Convert(indexes[i], parameters[i].ParameterType, false, isChecked);
                    }

                    if (!applicable || score >= bestScore)
                        continue;

                    best = new ResolvedIndexer { Property = property, Arguments = arguments };
                    bestScore = score;
                }

                if (best != null)
                    break;
            }

            return best;
        }

        private static object TryGetMemberValue(object target, Type type, string name, bool ignoreCase, bool isStatic, bool nonPublic)
        {
            var flags = Flags(isStatic, ignoreCase, nonPublic);

            var property = FindProperty(type, name, flags);
            if (property != null && property.GetGetMethod() != null)
                return Invoke(property.GetGetMethod(), property.GetGetMethod().IsStatic ? null : target, new object[0]);

            var field = type.GetField(name, flags);
            return field != null ? field.GetValue(field.IsStatic ? null : target) : null;
        }

        private static PropertyInfo FindProperty(Type type, string name, BindingFlags flags)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                var property = current.GetProperty(name, flags | BindingFlags.DeclaredOnly, null, null, new Type[0], null);
                if (property != null)
                    return property;
            }

            if (type.IsInterface)
            {
                foreach (var contract in type.GetInterfaces())
                {
                    var property = contract.GetProperty(name, flags);
                    if (property != null)
                        return property;
                }
            }

            return null;
        }

        private static EventInfo FindEvent(Type type, string name, BindingFlags flags)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                var found = current.GetEvent(name, flags | BindingFlags.DeclaredOnly);
                if (found != null)
                    return found;
            }
            return null;
        }

        internal sealed class Candidate
        {
            internal MethodBase Method;
            internal object[] Arguments;
            internal int Score;
        }

        /// <summary>Picks the best applicable overload and produces its converted argument list.</summary>
        internal static Candidate Resolve(List<MethodBase> candidates, Type[] typeArguments, object[] args, string[] argumentNames, bool isChecked)
        {
            Candidate best = null;

            foreach (var candidate in candidates)
            {
                var method = candidate;

                if (method.IsGenericMethodDefinition)
                {
                    var definition = (MethodInfo)method;
                    var inferred = typeArguments != null && typeArguments.Length > 0
                        ? typeArguments
                        : Inference.Infer(definition, args);

                    if (inferred == null || inferred.Length != definition.GetGenericArguments().Length)
                        continue;

                    try
                    {
                        method = definition.MakeGenericMethod(inferred);
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }
                }
                else if (typeArguments != null && typeArguments.Length > 0)
                {
                    continue;
                }

                var applicable = TryBuildArguments(method, args, argumentNames, isChecked);
                if (applicable == null)
                    continue;

                if (best == null || applicable.Score < best.Score)
                    best = applicable;
            }

            return best;
        }

        private static Candidate TryBuildArguments(MethodBase method, object[] args, string[] argumentNames, bool isChecked)
        {
            var parameters = method.GetParameters();
            var mapped = new object[parameters.Length];
            var supplied = new bool[parameters.Length];
            var score = 0;

            var hasParamArray = parameters.Length > 0 &&
                                parameters[parameters.Length - 1].ParameterType.IsArray &&
                                IsParamArray(parameters[parameters.Length - 1]);

            var positional = 0;
            for (var i = 0; i < args.Length; i++)
            {
                var name = argumentNames != null && i < argumentNames.Length ? argumentNames[i] : null;
                if (name != null)
                {
                    var index = IndexOfParameter(parameters, name);
                    if (index < 0 || supplied[index])
                        return null;
                    var rank = RankArgument(args[i], parameters[index].ParameterType);
                    if (rank == Conversions.NotApplicable)
                        return null;
                    score += rank;
                    mapped[index] = Conversions.Convert(args[i], parameters[index].ParameterType, false, isChecked);
                    supplied[index] = true;
                }
                else
                {
                    if (positional >= parameters.Length)
                    {
                        if (!hasParamArray)
                            return null;
                        positional = parameters.Length - 1;
                    }

                    if (hasParamArray && positional == parameters.Length - 1)
                        break; // the tail is handled below

                    var rank = RankArgument(args[i], parameters[positional].ParameterType);
                    if (rank == Conversions.NotApplicable)
                        return null;
                    score += rank;
                    mapped[positional] = Conversions.Convert(args[i], parameters[positional].ParameterType, false, isChecked);
                    supplied[positional] = true;
                    positional++;
                }
            }

            if (hasParamArray && !supplied[parameters.Length - 1])
            {
                var last = parameters.Length - 1;
                var tail = new List<object>();
                for (var i = positional; i < args.Length; i++)
                {
                    var name = argumentNames != null && i < argumentNames.Length ? argumentNames[i] : null;
                    if (name != null)
                        continue;
                    tail.Add(args[i]);
                }

                var arrayType = parameters[last].ParameterType;
                var elementType = arrayType.GetElementType();

                // Normal form: a single argument that is already the array itself.
                if (tail.Count == 1 && tail[0] != null && arrayType.IsAssignableFrom(tail[0].GetType()))
                {
                    mapped[last] = tail[0];
                    supplied[last] = true;
                }
                else
                {
                    var expanded = Array.CreateInstance(elementType, tail.Count);
                    for (var i = 0; i < tail.Count; i++)
                    {
                        var rank = RankArgument(tail[i], elementType);
                        if (rank == Conversions.NotApplicable)
                            return null;
                        score += rank;
                        expanded.SetValue(Conversions.Convert(tail[i], elementType, false, isChecked), i);
                    }
                    mapped[last] = expanded;
                    supplied[last] = true;
                    score += 1; // prefer the normal form over the expanded one
                }
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                if (supplied[i])
                    continue;

                if (!parameters[i].IsOptional)
                    return null;

                mapped[i] = parameters[i].DefaultValue == DBNull.Value
                    ? DefaultOf(parameters[i].ParameterType)
                    : parameters[i].DefaultValue;
                score += 2; // an explicitly supplied argument beats a defaulted one
            }

            return new Candidate { Method = method, Arguments = mapped, Score = score };
        }

        private static bool IsParamArray(ParameterInfo parameter)
        {
            return parameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
        }

        private static int RankArgument(object value, Type parameterType)
        {
            if (parameterType.IsByRef)
                parameterType = parameterType.GetElementType();
            return Conversions.Rank(value == null ? null : value.GetType(), parameterType);
        }

        private static int IndexOfParameter(ParameterInfo[] parameters, string name)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                if (string.Equals(parameters[i].Name, name, StringComparison.Ordinal))
                    return i;
            }
            return -1;
        }

        private static object DefaultOf(Type type)
        {
            if (type.IsByRef)
                type = type.GetElementType();
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static object Invoke(MethodBase method, object target, object[] arguments)
        {
            try
            {
                var result = method.Invoke(target, arguments);
                PendingWriteBack = arguments;
                return result;
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        internal static RuntimeBinderException MissingMember(Type type, string name)
        {
            return new RuntimeBinderException(string.Format(CultureInfo.InvariantCulture,
                "'{0}' does not contain a definition for '{1}'", Conversions.Format(type), name));
        }

        internal static RuntimeBinderException NoOverload(Type type, string name, object[] args)
        {
            return new RuntimeBinderException(string.Format(CultureInfo.InvariantCulture,
                "The best overloaded method match for '{0}.{1}' has some invalid arguments",
                Conversions.Format(type), name));
        }
    }
}
