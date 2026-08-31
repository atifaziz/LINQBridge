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
    /// C# conversion rules, as far as a runtime binder needs them: classification (used to rank
    /// candidates during overload resolution) and the conversion itself.
    /// </summary>
    internal static class Conversions
    {
        // C# 5.1.2 implicit numeric conversions, keyed by source type code.
        private static readonly Dictionary<TypeCode, TypeCode[]> ImplicitNumeric = BuildImplicitNumeric();

        private static Dictionary<TypeCode, TypeCode[]> BuildImplicitNumeric()
        {
            var map = new Dictionary<TypeCode, TypeCode[]>();
            map[TypeCode.SByte] = new[] { TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal };
            map[TypeCode.Byte] = new[] { TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal };
            map[TypeCode.Int16] = new[] { TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal };
            map[TypeCode.UInt16] = new[] { TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal };
            map[TypeCode.Int32] = new[] { TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal };
            map[TypeCode.UInt32] = new[] { TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal };
            map[TypeCode.Int64] = new[] { TypeCode.Single, TypeCode.Double, TypeCode.Decimal };
            map[TypeCode.UInt64] = new[] { TypeCode.Single, TypeCode.Double, TypeCode.Decimal };
            map[TypeCode.Char] = new[] { TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal };
            map[TypeCode.Single] = new[] { TypeCode.Double };
            return map;
        }

        internal const int NotApplicable = -1;

        /// <summary>
        /// Ranks the conversion of a value of <paramref name="from" /> (null for a null literal) to
        /// <paramref name="to" />. Lower is better; <see cref="F:NotApplicable" /> means there is no
        /// implicit conversion and the candidate is not applicable.
        /// </summary>
        internal static int Rank(Type from, Type to)
        {
            if (to == null)
                return NotApplicable;

            if (from == null)
            {
                // A null argument is applicable to anything that can hold null.
                if (!to.IsValueType || IsNullable(to))
                    return 3;
                return NotApplicable;
            }

            if (from == to)
                return 0;

            if (to == typeof(object))
                return 12; // applicable to everything, but the worst possible match

            if (IsNullable(to))
            {
                var underlying = Nullable.GetUnderlyingType(to);
                if (from == underlying)
                    return 1;
                var lifted = Rank(from, underlying);
                return lifted == NotApplicable ? NotApplicable : lifted + 1;
            }

            if (to.IsAssignableFrom(from))
                return 2 + InheritanceDistance(from, to);

            var fromCode = Type.GetTypeCode(from);
            var toCode = Type.GetTypeCode(to);
            if (!from.IsEnum && !to.IsEnum && ImplicitNumeric.ContainsKey(fromCode))
            {
                var targets = ImplicitNumeric[fromCode];
                for (var i = 0; i < targets.Length; i++)
                {
                    if (targets[i] == toCode)
                        return 6 + i;
                }
            }

            if (FindUserDefinedConversion(from, to, false) != null)
                return 10;

            return NotApplicable;
        }

        internal static bool IsImplicit(Type from, Type to)
        {
            return Rank(from, to) != NotApplicable;
        }

        private static int InheritanceDistance(Type from, Type to)
        {
            if (to.IsInterface)
                return 1;

            var distance = 0;
            for (var type = from; type != null; type = type.BaseType)
            {
                if (type == to)
                    return distance;
                distance++;
            }
            return 1;
        }

        internal static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>Converts a runtime value to <paramref name="to" /> using C# conversion rules.</summary>
        internal static object Convert(object value, Type to, bool isExplicit, bool isChecked)
        {
            if (to == null)
                throw new ArgumentNullException("to");

            if (to == typeof(object))
                return value;

            if (to.IsByRef)
                to = to.GetElementType();

            if (value == null)
            {
                if (!to.IsValueType || IsNullable(to))
                    return null;
                throw new RuntimeBinderException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Cannot convert null to '{0}' because it is a non-nullable value type", Format(to)));
            }

            if (IsNullable(to))
                to = Nullable.GetUnderlyingType(to);

            var from = value.GetType();
            if (to.IsAssignableFrom(from))
                return value;

            if (to.IsEnum)
            {
                if (from.IsEnum || IsNumeric(from))
                {
                    if (!isExplicit && !(from.IsEnum && from == to))
                    {
                        // enum <- numeric is an explicit conversion, except for the literal 0.
                        if (!IsZero(value))
                            throw ConversionError(from, to, false);
                    }
                    return Enum.ToObject(to, ConvertPrimitive(from.IsEnum ? System.Convert.ChangeType(value, Enum.GetUnderlyingType(from), CultureInfo.InvariantCulture) : value,
                                                              Type.GetTypeCode(Enum.GetUnderlyingType(to)), isChecked));
                }
            }

            if (from.IsEnum && IsNumeric(to))
            {
                if (!isExplicit)
                    throw ConversionError(from, to, false);
                var underlying = System.Convert.ChangeType(value, Enum.GetUnderlyingType(from), CultureInfo.InvariantCulture);
                return ConvertPrimitive(underlying, Type.GetTypeCode(to), isChecked);
            }

            if (IsNumeric(from) && IsNumeric(to))
            {
                if (!isExplicit && !IsImplicitNumeric(Type.GetTypeCode(from), Type.GetTypeCode(to)))
                    throw ConversionError(from, to, false);
                return ConvertPrimitive(value, Type.GetTypeCode(to), isChecked);
            }

            var conversion = FindUserDefinedConversion(from, to, isExplicit);
            if (conversion != null)
            {
                try
                {
                    return conversion.Invoke(null, new[] { Convert(value, conversion.GetParameters()[0].ParameterType, isExplicit, isChecked) });
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }

            if (isExplicit && from.IsAssignableFrom(to))
            {
                // Downcast of a value that is not actually of the target type.
                throw new InvalidCastException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Unable to cast object of type '{0}' to type '{1}'.", Format(from), Format(to)));
            }

            throw ConversionError(from, to, isExplicit);
        }

        private static bool IsZero(object value)
        {
            try
            {
                return System.Convert.ToInt64(value, CultureInfo.InvariantCulture) == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static RuntimeBinderException ConversionError(Type from, Type to, bool isExplicit)
        {
            return new RuntimeBinderException(string.Format(
                CultureInfo.InvariantCulture,
                isExplicit
                    ? "Cannot convert type '{0}' to '{1}'"
                    : "Cannot implicitly convert type '{0}' to '{1}'",
                Format(from), Format(to)));
        }

        internal static bool IsNumeric(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.Char:
                    return !type.IsEnum;
                default:
                    return false;
            }
        }

        internal static bool IsImplicitNumeric(TypeCode from, TypeCode to)
        {
            if (from == to)
                return true;
            TypeCode[] targets;
            if (!ImplicitNumeric.TryGetValue(from, out targets))
                return false;
            return Array.IndexOf(targets, to) >= 0;
        }

        /// <summary>Numeric conversion honouring the checked/unchecked context of the call site.</summary>
        internal static object ConvertPrimitive(object value, TypeCode target, bool isChecked)
        {
            if (isChecked)
                return System.Convert.ChangeType(value, target, CultureInfo.InvariantCulture);

            var source = Type.GetTypeCode(value.GetType());
            switch (source)
            {
                case TypeCode.Single:
                case TypeCode.Double:
                    return FromDouble(System.Convert.ToDouble(value, CultureInfo.InvariantCulture), target);
                case TypeCode.Decimal:
                    // C# defines decimal -> integral as a checked-like truncation; ChangeType matches.
                    return System.Convert.ChangeType(decimal.Truncate((decimal)value), target, CultureInfo.InvariantCulture);
                case TypeCode.UInt64:
                    return FromUInt64((ulong)value, target);
                default:
                    return FromInt64(System.Convert.ToInt64(value, CultureInfo.InvariantCulture), target);
            }
        }

        private static object FromInt64(long value, TypeCode target)
        {
            unchecked
            {
                switch (target)
                {
                    case TypeCode.SByte: return (sbyte)value;
                    case TypeCode.Byte: return (byte)value;
                    case TypeCode.Int16: return (short)value;
                    case TypeCode.UInt16: return (ushort)value;
                    case TypeCode.Int32: return (int)value;
                    case TypeCode.UInt32: return (uint)value;
                    case TypeCode.Int64: return value;
                    case TypeCode.UInt64: return (ulong)value;
                    case TypeCode.Char: return (char)value;
                    case TypeCode.Single: return (float)value;
                    case TypeCode.Double: return (double)value;
                    case TypeCode.Decimal: return (decimal)value;
                    default: return System.Convert.ChangeType(value, target, CultureInfo.InvariantCulture);
                }
            }
        }

        private static object FromUInt64(ulong value, TypeCode target)
        {
            unchecked
            {
                switch (target)
                {
                    case TypeCode.SByte: return (sbyte)value;
                    case TypeCode.Byte: return (byte)value;
                    case TypeCode.Int16: return (short)value;
                    case TypeCode.UInt16: return (ushort)value;
                    case TypeCode.Int32: return (int)value;
                    case TypeCode.UInt32: return (uint)value;
                    case TypeCode.Int64: return (long)value;
                    case TypeCode.UInt64: return value;
                    case TypeCode.Char: return (char)value;
                    case TypeCode.Single: return (float)value;
                    case TypeCode.Double: return (double)value;
                    case TypeCode.Decimal: return (decimal)value;
                    default: return System.Convert.ChangeType(value, target, CultureInfo.InvariantCulture);
                }
            }
        }

        private static object FromDouble(double value, TypeCode target)
        {
            unchecked
            {
                switch (target)
                {
                    case TypeCode.SByte: return (sbyte)value;
                    case TypeCode.Byte: return (byte)value;
                    case TypeCode.Int16: return (short)value;
                    case TypeCode.UInt16: return (ushort)value;
                    case TypeCode.Int32: return (int)value;
                    case TypeCode.UInt32: return (uint)value;
                    case TypeCode.Int64: return (long)value;
                    case TypeCode.UInt64: return (ulong)value;
                    case TypeCode.Char: return (char)value;
                    case TypeCode.Single: return (float)value;
                    case TypeCode.Double: return value;
                    case TypeCode.Decimal: return (decimal)value;
                    default: return System.Convert.ChangeType(value, target, CultureInfo.InvariantCulture);
                }
            }
        }

        internal static MethodInfo FindUserDefinedConversion(Type from, Type to, bool includeExplicit)
        {
            var candidate = FindConversionOn(from, from, to, includeExplicit);
            return candidate ?? FindConversionOn(to, from, to, includeExplicit);
        }

        private static MethodInfo FindConversionOn(Type declaring, Type from, Type to, bool includeExplicit)
        {
            if (declaring.IsPrimitive || declaring == typeof(object))
                return null;

            foreach (var method in declaring.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (method.Name != "op_Implicit" && (!includeExplicit || method.Name != "op_Explicit"))
                    continue;

                var parameters = method.GetParameters();
                if (parameters.Length != 1)
                    continue;
                if (!parameters[0].ParameterType.IsAssignableFrom(from) && parameters[0].ParameterType != from)
                    continue;
                if (method.ReturnType != to && !to.IsAssignableFrom(method.ReturnType))
                    continue;

                return method;
            }

            return null;
        }

        internal static string Format(Type type)
        {
            if (type == null)
                return "<null>";
            if (IsNullable(type))
                return Format(Nullable.GetUnderlyingType(type)) + "?";

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean: return "bool";
                case TypeCode.Byte: return "byte";
                case TypeCode.SByte: return "sbyte";
                case TypeCode.Char: return "char";
                case TypeCode.Int16: return "short";
                case TypeCode.UInt16: return "ushort";
                case TypeCode.Int32: return "int";
                case TypeCode.UInt32: return "uint";
                case TypeCode.Int64: return "long";
                case TypeCode.UInt64: return "ulong";
                case TypeCode.Single: return "float";
                case TypeCode.Double: return "double";
                case TypeCode.Decimal: return "decimal";
                case TypeCode.String: return "string";
                default:
                    return type == typeof(object) ? "object" : type.FullName ?? type.Name;
            }
        }
    }
}
