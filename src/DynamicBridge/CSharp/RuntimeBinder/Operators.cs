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
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.CSharp.RuntimeBinder
{
    /// <summary>C# operator semantics for values whose types are only known at run time.</summary>
    internal static class Operators
    {
        /// <summary>
        /// A compound assignment on a dynamic value ('d += 1') reaches the binder as AddAssign rather
        /// than Add, and '++'/'--' as Pre/PostIncrementAssign. The operand semantics are the same, so
        /// both forms fold onto the plain operation before anything else looks at them.
        /// </summary>
        internal static ExpressionType Normalize(ExpressionType operation)
        {
            switch (operation)
            {
                case NodeKinds.AddAssign: return ExpressionType.Add;
                case NodeKinds.AddAssignChecked: return ExpressionType.AddChecked;
                case NodeKinds.SubtractAssign: return ExpressionType.Subtract;
                case NodeKinds.SubtractAssignChecked: return ExpressionType.SubtractChecked;
                case NodeKinds.MultiplyAssign: return ExpressionType.Multiply;
                case NodeKinds.MultiplyAssignChecked: return ExpressionType.MultiplyChecked;
                case NodeKinds.DivideAssign: return ExpressionType.Divide;
                case NodeKinds.ModuloAssign: return ExpressionType.Modulo;
                case NodeKinds.AndAssign: return ExpressionType.And;
                case NodeKinds.OrAssign: return ExpressionType.Or;
                case NodeKinds.ExclusiveOrAssign: return ExpressionType.ExclusiveOr;
                case NodeKinds.LeftShiftAssign: return ExpressionType.LeftShift;
                case NodeKinds.RightShiftAssign: return ExpressionType.RightShift;
                case NodeKinds.PowerAssign: return ExpressionType.Power;
                case NodeKinds.PreIncrementAssign:
                case NodeKinds.PostIncrementAssign: return NodeKinds.Increment;
                case NodeKinds.PreDecrementAssign:
                case NodeKinds.PostDecrementAssign: return NodeKinds.Decrement;
                default: return operation;
            }
        }

        internal static object Binary(ExpressionType operation, object left, object right, bool isChecked)
        {
            operation = Normalize(operation);
            var leftType = left == null ? null : left.GetType();
            var rightType = right == null ? null : right.GetType();

            // A null operand lifts the whole operation, exactly as an operand of a nullable type does.
            if (left == null || right == null)
            {
                switch (operation)
                {
                    case ExpressionType.Equal:
                        return left == null && right == null;
                    case ExpressionType.NotEqual:
                        return !(left == null && right == null);
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                        return false;
                    case ExpressionType.Add:
                        // string + null is still concatenation.
                        if (leftType == typeof(string) || rightType == typeof(string))
                            return Concat(left, right);
                        return null;
                    default:
                        return null;
                }
            }

            if (operation == ExpressionType.Add && (leftType == typeof(string) || rightType == typeof(string)))
                return Concat(left, right);

            if (left is Delegate && right is Delegate && leftType == rightType)
            {
                if (operation == ExpressionType.Add)
                    return Delegate.Combine((Delegate)left, (Delegate)right);
                if (operation == ExpressionType.Subtract)
                    return Delegate.Remove((Delegate)left, (Delegate)right);
            }

            if (left is bool && right is bool)
            {
                var l = (bool)left;
                var r = (bool)right;
                switch (operation)
                {
                    case ExpressionType.And:
                    case ExpressionType.AndAlso: return l & r;
                    case ExpressionType.Or:
                    case ExpressionType.OrElse: return l | r;
                    case ExpressionType.ExclusiveOr: return l ^ r;
                    case ExpressionType.Equal: return l == r;
                    case ExpressionType.NotEqual: return l != r;
                }
                throw OperatorError(operation, leftType, rightType);
            }

            if (leftType.IsEnum || rightType.IsEnum)
            {
                var result = EnumOperation(operation, left, right, leftType, rightType, isChecked);
                if (result != null)
                    return result;
            }

            var userDefined = FindBinaryOperator(operation, leftType, rightType);
            if (userDefined != null)
                return InvokeOperator(userDefined, new[] { left, right }, isChecked);

            if (Conversions.IsNumeric(leftType) && Conversions.IsNumeric(rightType))
                return Numeric(operation, left, right, leftType, rightType, isChecked);

            switch (operation)
            {
                case ExpressionType.Equal:
                    return ReferenceEquals(left, right) || left.Equals(right);
                case ExpressionType.NotEqual:
                    return !(ReferenceEquals(left, right) || left.Equals(right));
            }

            throw OperatorError(operation, leftType, rightType);
        }

        internal static object Unary(ExpressionType operation, object operand, bool isChecked)
        {
            operation = Normalize(operation);

            if (operand == null)
            {
                switch (operation)
                {
                    case NodeKinds.IsTrue:
                    case NodeKinds.IsFalse:
                        throw new RuntimeBinderException("Cannot apply operator '" + operation + "' to an operand of type '<null>'");
                    default:
                        return null;
                }
            }

            var type = operand.GetType();

            if (operand is bool)
            {
                var value = (bool)operand;
                switch (operation)
                {
                    case ExpressionType.Not: return !value;
                    case NodeKinds.IsTrue: return value;
                    case NodeKinds.IsFalse: return !value;
                }
            }

            var userDefined = FindUnaryOperator(operation, type);
            if (userDefined != null)
                return InvokeOperator(userDefined, new[] { operand }, isChecked);

            if (type.IsEnum)
            {
                var underlying = Enum.GetUnderlyingType(type);
                var raw = System.Convert.ChangeType(operand, underlying, CultureInfo.InvariantCulture);
                switch (operation)
                {
                    case NodeKinds.OnesComplement:
                    case ExpressionType.Not:
                        return Enum.ToObject(type, Unary(NodeKinds.OnesComplement, raw, isChecked));
                    case NodeKinds.Increment:
                        return Enum.ToObject(type, Binary(ExpressionType.Add, raw, 1, isChecked));
                    case NodeKinds.Decrement:
                        return Enum.ToObject(type, Binary(ExpressionType.Subtract, raw, 1, isChecked));
                }
            }

            if (Conversions.IsNumeric(type))
            {
                var code = PromoteUnary(Type.GetTypeCode(type));
                var value = Conversions.ConvertPrimitive(operand, code, isChecked);

                switch (operation)
                {
                    case ExpressionType.UnaryPlus:
                        return value;
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                        return Negate(value, code, isChecked || operation == ExpressionType.NegateChecked);
                    case NodeKinds.OnesComplement:
                    case ExpressionType.Not:
                        return OnesComplement(value, code);
                    case NodeKinds.Increment:
                        return Conversions.ConvertPrimitive(Binary(ExpressionType.Add, value, System.Convert.ChangeType(1, code, CultureInfo.InvariantCulture), isChecked), Type.GetTypeCode(type), isChecked);
                    case NodeKinds.Decrement:
                        return Conversions.ConvertPrimitive(Binary(ExpressionType.Subtract, value, System.Convert.ChangeType(1, code, CultureInfo.InvariantCulture), isChecked), Type.GetTypeCode(type), isChecked);
                }
            }

            throw new RuntimeBinderException(string.Format(
                CultureInfo.InvariantCulture,
                "Operator '{0}' cannot be applied to operand of type '{1}'",
                Symbol(operation), Conversions.Format(type)));
        }

        private static string Concat(object left, object right)
        {
            return string.Concat(
                left == null ? string.Empty : left.ToString(),
                right == null ? string.Empty : right.ToString());
        }

        private static object EnumOperation(ExpressionType operation, object left, object right, Type leftType, Type rightType, bool isChecked)
        {
            var enumType = leftType.IsEnum ? leftType : rightType;
            var underlying = Enum.GetUnderlyingType(enumType);
            var l = System.Convert.ChangeType(left, underlying, CultureInfo.InvariantCulture);
            var r = System.Convert.ChangeType(right, underlying, CultureInfo.InvariantCulture);

            switch (operation)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    return Numeric(operation, l, r, underlying, underlying, isChecked);

                case ExpressionType.And:
                case ExpressionType.Or:
                case ExpressionType.ExclusiveOr:
                    return Enum.ToObject(enumType, Numeric(operation, l, r, underlying, underlying, isChecked));

                case ExpressionType.Add:
                    // enum + integral, integral + enum
                    if (leftType.IsEnum && rightType.IsEnum)
                        return null;
                    return Enum.ToObject(enumType, Numeric(operation, l, r, underlying, underlying, isChecked));

                case ExpressionType.Subtract:
                    if (leftType.IsEnum && rightType.IsEnum)
                        return Numeric(operation, l, r, underlying, underlying, isChecked);
                    return Enum.ToObject(enumType, Numeric(operation, l, r, underlying, underlying, isChecked));
            }

            return null;
        }

        private static object Numeric(ExpressionType operation, object left, object right, Type leftType, Type rightType, bool isChecked)
        {
            if (operation == ExpressionType.LeftShift || operation == ExpressionType.RightShift)
                return Shift(operation, left, leftType, right, isChecked);

            var code = Promote(Type.GetTypeCode(leftType), Type.GetTypeCode(rightType), operation);
            var l = Conversions.ConvertPrimitive(left, code, isChecked);
            var r = Conversions.ConvertPrimitive(right, code, isChecked);

            switch (code)
            {
                case TypeCode.Int32: return Int32Op(operation, (int)l, (int)r, isChecked);
                case TypeCode.UInt32: return UInt32Op(operation, (uint)l, (uint)r, isChecked);
                case TypeCode.Int64: return Int64Op(operation, (long)l, (long)r, isChecked);
                case TypeCode.UInt64: return UInt64Op(operation, (ulong)l, (ulong)r, isChecked);
                case TypeCode.Single: return SingleOp(operation, (float)l, (float)r);
                case TypeCode.Double: return DoubleOp(operation, (double)l, (double)r);
                case TypeCode.Decimal: return DecimalOp(operation, (decimal)l, (decimal)r);
            }

            throw OperatorError(operation, leftType, rightType);
        }

        private static object Shift(ExpressionType operation, object left, Type leftType, object right, bool isChecked)
        {
            var code = PromoteUnary(Type.GetTypeCode(leftType));
            var value = Conversions.ConvertPrimitive(left, code, isChecked);
            var count = (int)Conversions.ConvertPrimitive(right, TypeCode.Int32, isChecked);

            unchecked
            {
                switch (code)
                {
                    case TypeCode.Int32:
                        return operation == ExpressionType.LeftShift ? (object)((int)value << (count & 31)) : (int)value >> (count & 31);
                    case TypeCode.UInt32:
                        return operation == ExpressionType.LeftShift ? (object)((uint)value << (count & 31)) : (uint)value >> (count & 31);
                    case TypeCode.Int64:
                        return operation == ExpressionType.LeftShift ? (object)((long)value << (count & 63)) : (long)value >> (count & 63);
                    case TypeCode.UInt64:
                        return operation == ExpressionType.LeftShift ? (object)((ulong)value << (count & 63)) : (ulong)value >> (count & 63);
                }
            }

            throw OperatorError(operation, leftType, right == null ? null : right.GetType());
        }

        /// <summary>C# 7.3.6.1 binary numeric promotion.</summary>
        private static TypeCode Promote(TypeCode left, TypeCode right, ExpressionType operation)
        {
            if (left == TypeCode.Decimal || right == TypeCode.Decimal)
            {
                if (left == TypeCode.Double || right == TypeCode.Double || left == TypeCode.Single || right == TypeCode.Single)
                    throw new RuntimeBinderException("Operator '" + Symbol(operation) + "' cannot be applied to operands of type 'decimal' and 'double'");
                return TypeCode.Decimal;
            }
            if (left == TypeCode.Double || right == TypeCode.Double) return TypeCode.Double;
            if (left == TypeCode.Single || right == TypeCode.Single) return TypeCode.Single;
            if (left == TypeCode.UInt64 || right == TypeCode.UInt64)
            {
                if (IsSigned(left) || IsSigned(right))
                    throw new RuntimeBinderException("Operator '" + Symbol(operation) + "' cannot be applied to operands of type 'ulong' and a signed integral type");
                return TypeCode.UInt64;
            }
            if (left == TypeCode.Int64 || right == TypeCode.Int64) return TypeCode.Int64;
            if (left == TypeCode.UInt32 || right == TypeCode.UInt32)
                return IsSigned(left) || IsSigned(right) ? TypeCode.Int64 : TypeCode.UInt32;
            return TypeCode.Int32;
        }

        private static TypeCode PromoteUnary(TypeCode code)
        {
            switch (code)
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Char:
                case TypeCode.Int32:
                    return TypeCode.Int32;
                default:
                    return code;
            }
        }

        private static bool IsSigned(TypeCode code)
        {
            switch (code)
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }

        private static object Int32Op(ExpressionType op, int l, int r, bool isChecked)
        {
            switch (op)
            {
                case ExpressionType.Add: return isChecked ? checked(l + r) : unchecked(l + r);
                case ExpressionType.AddChecked: return checked(l + r);
                case ExpressionType.Subtract: return isChecked ? checked(l - r) : unchecked(l - r);
                case ExpressionType.SubtractChecked: return checked(l - r);
                case ExpressionType.Multiply: return isChecked ? checked(l * r) : unchecked(l * r);
                case ExpressionType.MultiplyChecked: return checked(l * r);
                case ExpressionType.Divide: return l / r;
                case ExpressionType.Modulo: return l % r;
                case ExpressionType.And: return l & r;
                case ExpressionType.Or: return l | r;
                case ExpressionType.ExclusiveOr: return l ^ r;
                case ExpressionType.Equal: return l == r;
                case ExpressionType.NotEqual: return l != r;
                case ExpressionType.LessThan: return l < r;
                case ExpressionType.LessThanOrEqual: return l <= r;
                case ExpressionType.GreaterThan: return l > r;
                case ExpressionType.GreaterThanOrEqual: return l >= r;
            }
            throw OperatorError(op, typeof(int), typeof(int));
        }

        private static object UInt32Op(ExpressionType op, uint l, uint r, bool isChecked)
        {
            switch (op)
            {
                case ExpressionType.Add: return isChecked ? checked(l + r) : unchecked(l + r);
                case ExpressionType.AddChecked: return checked(l + r);
                case ExpressionType.Subtract: return isChecked ? checked(l - r) : unchecked(l - r);
                case ExpressionType.SubtractChecked: return checked(l - r);
                case ExpressionType.Multiply: return isChecked ? checked(l * r) : unchecked(l * r);
                case ExpressionType.MultiplyChecked: return checked(l * r);
                case ExpressionType.Divide: return l / r;
                case ExpressionType.Modulo: return l % r;
                case ExpressionType.And: return l & r;
                case ExpressionType.Or: return l | r;
                case ExpressionType.ExclusiveOr: return l ^ r;
                case ExpressionType.Equal: return l == r;
                case ExpressionType.NotEqual: return l != r;
                case ExpressionType.LessThan: return l < r;
                case ExpressionType.LessThanOrEqual: return l <= r;
                case ExpressionType.GreaterThan: return l > r;
                case ExpressionType.GreaterThanOrEqual: return l >= r;
            }
            throw OperatorError(op, typeof(uint), typeof(uint));
        }

        private static object Int64Op(ExpressionType op, long l, long r, bool isChecked)
        {
            switch (op)
            {
                case ExpressionType.Add: return isChecked ? checked(l + r) : unchecked(l + r);
                case ExpressionType.AddChecked: return checked(l + r);
                case ExpressionType.Subtract: return isChecked ? checked(l - r) : unchecked(l - r);
                case ExpressionType.SubtractChecked: return checked(l - r);
                case ExpressionType.Multiply: return isChecked ? checked(l * r) : unchecked(l * r);
                case ExpressionType.MultiplyChecked: return checked(l * r);
                case ExpressionType.Divide: return l / r;
                case ExpressionType.Modulo: return l % r;
                case ExpressionType.And: return l & r;
                case ExpressionType.Or: return l | r;
                case ExpressionType.ExclusiveOr: return l ^ r;
                case ExpressionType.Equal: return l == r;
                case ExpressionType.NotEqual: return l != r;
                case ExpressionType.LessThan: return l < r;
                case ExpressionType.LessThanOrEqual: return l <= r;
                case ExpressionType.GreaterThan: return l > r;
                case ExpressionType.GreaterThanOrEqual: return l >= r;
            }
            throw OperatorError(op, typeof(long), typeof(long));
        }

        private static object UInt64Op(ExpressionType op, ulong l, ulong r, bool isChecked)
        {
            switch (op)
            {
                case ExpressionType.Add: return isChecked ? checked(l + r) : unchecked(l + r);
                case ExpressionType.AddChecked: return checked(l + r);
                case ExpressionType.Subtract: return isChecked ? checked(l - r) : unchecked(l - r);
                case ExpressionType.SubtractChecked: return checked(l - r);
                case ExpressionType.Multiply: return isChecked ? checked(l * r) : unchecked(l * r);
                case ExpressionType.MultiplyChecked: return checked(l * r);
                case ExpressionType.Divide: return l / r;
                case ExpressionType.Modulo: return l % r;
                case ExpressionType.And: return l & r;
                case ExpressionType.Or: return l | r;
                case ExpressionType.ExclusiveOr: return l ^ r;
                case ExpressionType.Equal: return l == r;
                case ExpressionType.NotEqual: return l != r;
                case ExpressionType.LessThan: return l < r;
                case ExpressionType.LessThanOrEqual: return l <= r;
                case ExpressionType.GreaterThan: return l > r;
                case ExpressionType.GreaterThanOrEqual: return l >= r;
            }
            throw OperatorError(op, typeof(ulong), typeof(ulong));
        }

        private static object SingleOp(ExpressionType op, float l, float r)
        {
            switch (op)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked: return l + r;
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked: return l - r;
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked: return l * r;
                case ExpressionType.Divide: return l / r;
                case ExpressionType.Modulo: return l % r;
                case ExpressionType.Equal: return l == r;
                case ExpressionType.NotEqual: return l != r;
                case ExpressionType.LessThan: return l < r;
                case ExpressionType.LessThanOrEqual: return l <= r;
                case ExpressionType.GreaterThan: return l > r;
                case ExpressionType.GreaterThanOrEqual: return l >= r;
            }
            throw OperatorError(op, typeof(float), typeof(float));
        }

        private static object DoubleOp(ExpressionType op, double l, double r)
        {
            switch (op)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked: return l + r;
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked: return l - r;
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked: return l * r;
                case ExpressionType.Divide: return l / r;
                case ExpressionType.Modulo: return l % r;
                case ExpressionType.Equal: return l == r;
                case ExpressionType.NotEqual: return l != r;
                case ExpressionType.LessThan: return l < r;
                case ExpressionType.LessThanOrEqual: return l <= r;
                case ExpressionType.GreaterThan: return l > r;
                case ExpressionType.GreaterThanOrEqual: return l >= r;
            }
            throw OperatorError(op, typeof(double), typeof(double));
        }

        private static object DecimalOp(ExpressionType op, decimal l, decimal r)
        {
            switch (op)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked: return l + r;
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked: return l - r;
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked: return l * r;
                case ExpressionType.Divide: return l / r;
                case ExpressionType.Modulo: return l % r;
                case ExpressionType.Equal: return l == r;
                case ExpressionType.NotEqual: return l != r;
                case ExpressionType.LessThan: return l < r;
                case ExpressionType.LessThanOrEqual: return l <= r;
                case ExpressionType.GreaterThan: return l > r;
                case ExpressionType.GreaterThanOrEqual: return l >= r;
            }
            throw OperatorError(op, typeof(decimal), typeof(decimal));
        }

        private static object Negate(object value, TypeCode code, bool isChecked)
        {
            switch (code)
            {
                case TypeCode.Int32: return isChecked ? checked(-(int)value) : unchecked(-(int)value);
                case TypeCode.Int64: return isChecked ? checked(-(long)value) : unchecked(-(long)value);
                case TypeCode.UInt32: return -(long)(uint)value;
                case TypeCode.Single: return -(float)value;
                case TypeCode.Double: return -(double)value;
                case TypeCode.Decimal: return -(decimal)value;
                case TypeCode.UInt64: throw new RuntimeBinderException("Operator '-' cannot be applied to operand of type 'ulong'");
            }
            throw new RuntimeBinderException("Operator '-' cannot be applied to this operand");
        }

        private static object OnesComplement(object value, TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Int32: return ~(int)value;
                case TypeCode.UInt32: return ~(uint)value;
                case TypeCode.Int64: return ~(long)value;
                case TypeCode.UInt64: return ~(ulong)value;
            }
            throw new RuntimeBinderException("Operator '~' cannot be applied to an operand of a non-integral type");
        }

        private static object InvokeOperator(MethodInfo method, object[] args, bool isChecked)
        {
            var parameters = method.GetParameters();
            for (var i = 0; i < args.Length; i++)
                args[i] = Conversions.Convert(args[i], parameters[i].ParameterType, false, isChecked);

            try
            {
                return method.Invoke(null, args);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        internal static MethodInfo FindBinaryOperator(ExpressionType operation, Type left, Type right)
        {
            var name = BinaryMethodName(operation);
            if (name == null)
                return null;

            return FindOperatorOn(left, name, new[] { left, right })
                ?? FindOperatorOn(right, name, new[] { left, right });
        }

        internal static MethodInfo FindUnaryOperator(ExpressionType operation, Type operand)
        {
            var name = UnaryMethodName(operation);
            return name == null ? null : FindOperatorOn(operand, name, new[] { operand });
        }

        private static MethodInfo FindOperatorOn(Type declaring, string name, Type[] argumentTypes)
        {
            if (declaring == null || declaring.IsPrimitive || declaring == typeof(object) || declaring == typeof(string))
                return null;

            MethodInfo best = null;
            var bestScore = int.MaxValue;

            foreach (var method in declaring.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (method.Name != name)
                    continue;

                var parameters = method.GetParameters();
                if (parameters.Length != argumentTypes.Length)
                    continue;

                var score = 0;
                var applicable = true;
                for (var i = 0; i < parameters.Length; i++)
                {
                    var rank = Conversions.Rank(argumentTypes[i], parameters[i].ParameterType);
                    if (rank == Conversions.NotApplicable)
                    {
                        applicable = false;
                        break;
                    }
                    score += rank;
                }

                if (applicable && score < bestScore)
                {
                    best = method;
                    bestScore = score;
                }
            }

            return best;
        }

        private static string BinaryMethodName(ExpressionType operation)
        {
            switch (operation)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked: return "op_Addition";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked: return "op_Subtraction";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked: return "op_Multiply";
                case ExpressionType.Divide: return "op_Division";
                case ExpressionType.Modulo: return "op_Modulus";
                case ExpressionType.And: return "op_BitwiseAnd";
                case ExpressionType.Or: return "op_BitwiseOr";
                case ExpressionType.ExclusiveOr: return "op_ExclusiveOr";
                case ExpressionType.LeftShift: return "op_LeftShift";
                case ExpressionType.RightShift: return "op_RightShift";
                case ExpressionType.Equal: return "op_Equality";
                case ExpressionType.NotEqual: return "op_Inequality";
                case ExpressionType.LessThan: return "op_LessThan";
                case ExpressionType.LessThanOrEqual: return "op_LessThanOrEqual";
                case ExpressionType.GreaterThan: return "op_GreaterThan";
                case ExpressionType.GreaterThanOrEqual: return "op_GreaterThanOrEqual";
                default: return null;
            }
        }

        private static string UnaryMethodName(ExpressionType operation)
        {
            switch (operation)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked: return "op_UnaryNegation";
                case ExpressionType.UnaryPlus: return "op_UnaryPlus";
                case ExpressionType.Not: return "op_LogicalNot";
                case NodeKinds.OnesComplement: return "op_OnesComplement";
                case NodeKinds.Increment: return "op_Increment";
                case NodeKinds.Decrement: return "op_Decrement";
                case NodeKinds.IsTrue: return "op_True";
                case NodeKinds.IsFalse: return "op_False";
                default: return null;
            }
        }

        internal static string Symbol(ExpressionType operation)
        {
            switch (operation)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked: return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked: return "-";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked: return "*";
                case ExpressionType.Divide: return "/";
                case ExpressionType.Modulo: return "%";
                case ExpressionType.And: return "&";
                case ExpressionType.Or: return "|";
                case ExpressionType.ExclusiveOr: return "^";
                case ExpressionType.LeftShift: return "<<";
                case ExpressionType.RightShift: return ">>";
                case ExpressionType.Equal: return "==";
                case ExpressionType.NotEqual: return "!=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked: return "-";
                case ExpressionType.UnaryPlus: return "+";
                case ExpressionType.Not: return "!";
                case NodeKinds.OnesComplement: return "~";
                case NodeKinds.Increment: return "++";
                case NodeKinds.Decrement: return "--";
                default: return operation.ToString();
            }
        }

        private static RuntimeBinderException OperatorError(ExpressionType operation, Type left, Type right)
        {
            return new RuntimeBinderException(string.Format(
                CultureInfo.InvariantCulture,
                "Operator '{0}' cannot be applied to operands of type '{1}' and '{2}'",
                Symbol(operation), Conversions.Format(left), Conversions.Format(right)));
        }
    }
}
