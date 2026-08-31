#region License, Terms and Author(s)
//
// LINQBridge
// Copyright (c) 2007 Atif Aziz, Joseph Albahari. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it
// under the terms of the New BSD License, a copy of which should have
// been delivered along with this distribution.
//
#endregion

namespace LinqBridge.Tests
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using NUnit.Framework;

    #endregion

    /// <summary>
    /// Exercises the <see cref="Expression"/> factory methods and, through
    /// <see cref="Expression{TDelegate}.Compile"/>, the IL emitting compiler
    /// behind them.
    /// </summary>
    /// <remarks>
    /// This fixture is compiled into both LINQBridge.Tests (against LINQBridge)
    /// and LINQ.Tests (against Framework 3.5's System.Core), so it must stay
    /// within the Framework 3.5 expression API. Anything introduced in
    /// Framework 4.0 (Block, Assign, public ExpressionVisitor, ...) belongs in
    /// ExpressionExtrasFixture instead.
    /// </remarks>

    [TestFixture]
    public sealed class ExpressionFixture
    {
        // ReSharper disable InconsistentNaming

        #region Constant, Parameter and Lambda

        [Test]
        public void Constant_Value_HasValueAndType()
        {
            var e = Expression.Constant(42);
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.Constant));
            Assert.That(e.Value, Is.EqualTo(42));
            Assert.That(e.Type, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void Constant_NullWithExplicitType_KeepsType()
        {
            var e = Expression.Constant(null, typeof(string));
            Assert.That(e.Value, Is.Null);
            Assert.That(e.Type, Is.EqualTo(typeof(string)));
        }

        [Test]
        public void Constant_NullValueForValueType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Expression.Constant(null, typeof(int)));
        }

        [Test]
        public void Constant_ValueNotAssignableToType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Expression.Constant("str", typeof(int)));
        }

        [Test]
        public void Parameter_TypeAndName_Retained()
        {
            var p = Expression.Parameter(typeof(string), "s");
            Assert.That(p.NodeType, Is.EqualTo(ExpressionType.Parameter));
            Assert.That(p.Type, Is.EqualTo(typeof(string)));
            Assert.That(p.Name, Is.EqualTo("s"));
        }

        [Test]
        public void Parameter_NullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Expression.Parameter(null, "x"));
        }

        [Test]
        public void Lambda_Constant_CompilesToConstantFunction()
        {
            var f = Expression.Lambda<Func<int>>(Expression.Constant(7)).Compile();
            Assert.That(f(), Is.EqualTo(7));
        }

        [Test]
        public void Lambda_Identity_CompilesToIdentityFunction()
        {
            var p = Expression.Parameter(typeof(int), "x");
            var f = Expression.Lambda<Func<int, int>>(p, p).Compile();
            Assert.That(f(3), Is.EqualTo(3));
        }

        [Test]
        public void Lambda_ExposesBodyAndParameters()
        {
            var p = Expression.Parameter(typeof(int), "x");
            var lambda = Expression.Lambda<Func<int, int>>(p, p);
            Assert.That(lambda.NodeType, Is.EqualTo(ExpressionType.Lambda));
            Assert.That(lambda.Body, Is.SameAs(p));
            Assert.That(lambda.Parameters.Count, Is.EqualTo(1));
            Assert.That(lambda.Parameters[0], Is.SameAs(p));
            Assert.That(lambda.Type, Is.EqualTo(typeof(Func<int, int>)));
        }

        [Test]
        public void Lambda_NullBody_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Expression.Lambda<Func<int>>(null));
        }

        [Test]
        public void Lambda_WrongParameterCount_ThrowsArgumentException()
        {
            var p = Expression.Parameter(typeof(int), "x");
            Assert.Throws<ArgumentException>(() => Expression.Lambda<Func<int>>(p, p));
        }

        [Test]
        public void Lambda_BodyNotAssignableToReturnType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => Expression.Lambda<Func<string>>(Expression.Constant(1)));
        }

        [Test]
        public void Lambda_NonGeneric_ProducesRequestedDelegateType()
        {
            var p = Expression.Parameter(typeof(int), "x");
            var lambda = Expression.Lambda(typeof(Func<int, int>), p, p);
            var f = (Func<int, int>) lambda.Compile();
            Assert.That(f(9), Is.EqualTo(9));
        }

        [Test]
        public void Lambda_ManyParameters_CompilesAndBinds()
        {
            var a = Expression.Parameter(typeof(int), "a");
            var b = Expression.Parameter(typeof(int), "b");
            var c = Expression.Parameter(typeof(int), "c");
            var body = Expression.Add(Expression.Add(a, b), c);
            var f = Expression.Lambda<Func<int, int, int, int>>(body, a, b, c).Compile();
            Assert.That(f(1, 2, 3), Is.EqualTo(6));
        }

        #endregion

        #region Arithmetic

        [Test]
        public void Add_Integers_Sums()
        {
            Assert.That(Eval<int>(Expression.Add(Constant(2), Constant(3))), Is.EqualTo(5));
        }

        [Test]
        public void Add_NodeTypeAndOperands()
        {
            var left = Constant(2);
            var right = Constant(3);
            var e = Expression.Add(left, right);
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.Add));
            Assert.That(e.Left, Is.SameAs(left));
            Assert.That(e.Right, Is.SameAs(right));
            Assert.That(e.Type, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void Add_MismatchedOperandTypes_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(
                () => Expression.Add(Expression.Constant(1), Expression.Constant("s")));
        }

        [Test]
        public void AddChecked_Overflow_ThrowsOverflowException()
        {
            var f = Expression.Lambda<Func<int>>(
                Expression.AddChecked(Constant(int.MaxValue), Constant(1))).Compile();
            Assert.Throws<OverflowException>(() => f());
        }

        [Test]
        public void Add_Overflow_WrapsAround()
        {
            Assert.That(Eval<int>(Expression.Add(Constant(int.MaxValue), Constant(1))),
                        Is.EqualTo(int.MinValue));
        }

        [Test]
        public void Subtract_Integers_Subtracts()
        {
            Assert.That(Eval<int>(Expression.Subtract(Constant(7), Constant(4))), Is.EqualTo(3));
        }

        [Test]
        public void SubtractChecked_Overflow_ThrowsOverflowException()
        {
            var f = Expression.Lambda<Func<int>>(
                Expression.SubtractChecked(Constant(int.MinValue), Constant(1))).Compile();
            Assert.Throws<OverflowException>(() => f());
        }

        [Test]
        public void Multiply_Integers_Multiplies()
        {
            Assert.That(Eval<int>(Expression.Multiply(Constant(6), Constant(7))), Is.EqualTo(42));
        }

        [Test]
        public void MultiplyChecked_Overflow_ThrowsOverflowException()
        {
            var f = Expression.Lambda<Func<int>>(
                Expression.MultiplyChecked(Constant(int.MaxValue), Constant(2))).Compile();
            Assert.Throws<OverflowException>(() => f());
        }

        [Test]
        public void Divide_Integers_Divides()
        {
            Assert.That(Eval<int>(Expression.Divide(Constant(9), Constant(2))), Is.EqualTo(4));
        }

        [Test]
        public void Divide_ByZero_ThrowsDivideByZeroException()
        {
            var f = Expression.Lambda<Func<int>>(Expression.Divide(Constant(1), Constant(0))).Compile();
            Assert.Throws<DivideByZeroException>(() => f());
        }

        [Test]
        public void Modulo_Integers_Remainders()
        {
            Assert.That(Eval<int>(Expression.Modulo(Constant(9), Constant(4))), Is.EqualTo(1));
        }

        [Test]
        public void Power_Doubles_Exponentiates()
        {
            var e = Expression.Power(Expression.Constant(2.0), Expression.Constant(10.0));
            Assert.That(Eval<double>(e), Is.EqualTo(1024.0));
        }

        [Test]
        public void Negate_Integer_Negates()
        {
            var e = Expression.Negate(Constant(5));
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.Negate));
            Assert.That(Eval<int>(e), Is.EqualTo(-5));
        }

        [Test]
        public void NegateChecked_MinValue_ThrowsOverflowException()
        {
            var f = Expression.Lambda<Func<int>>(
                Expression.NegateChecked(Constant(int.MinValue))).Compile();
            Assert.Throws<OverflowException>(() => f());
        }

        [Test]
        public void UnaryPlus_Integer_ReturnsOperand()
        {
            Assert.That(Eval<int>(Expression.UnaryPlus(Constant(5))), Is.EqualTo(5));
        }

        [Test]
        public void Arithmetic_OnDoubles_UsesFloatingPointOpcodes()
        {
            var e = Expression.Divide(Expression.Constant(1.0), Expression.Constant(4.0));
            Assert.That(Eval<double>(e), Is.EqualTo(0.25));
        }

        [Test]
        public void Arithmetic_OnDecimals_UsesOperatorMethod()
        {
            var e = Expression.Add(Expression.Constant(1.5m), Expression.Constant(2.25m));
            Assert.That(Eval<decimal>(e), Is.EqualTo(3.75m));
        }

        [Test]
        public void Add_Strings_ThrowsInvalidOperationExceptionWithoutMethod()
        {
            Assert.Throws<InvalidOperationException>(
                () => Expression.Add(Expression.Constant("a"), Expression.Constant("b")));
        }

        [Test]
        public void Add_WithUserSuppliedMethod_UsesThatMethod()
        {
            var concat = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
            var e = Expression.Add(Expression.Constant("a"), Expression.Constant("b"), concat);
            Assert.That(e.Method, Is.EqualTo(concat));
            Assert.That(Eval<string>(e), Is.EqualTo("ab"));
        }

        #endregion

        #region Comparison and logic

        [Test]
        public void Equal_EqualIntegers_IsTrue()
        {
            Assert.That(Eval<bool>(Expression.Equal(Constant(1), Constant(1))), Is.True);
        }

        [Test]
        public void NotEqual_EqualIntegers_IsFalse()
        {
            Assert.That(Eval<bool>(Expression.NotEqual(Constant(1), Constant(1))), Is.False);
        }

        [Test]
        public void LessThan_SmallerLeft_IsTrue()
        {
            Assert.That(Eval<bool>(Expression.LessThan(Constant(1), Constant(2))), Is.True);
        }

        [Test]
        public void LessThanOrEqual_EqualOperands_IsTrue()
        {
            Assert.That(Eval<bool>(Expression.LessThanOrEqual(Constant(2), Constant(2))), Is.True);
        }

        [Test]
        public void GreaterThan_SmallerLeft_IsFalse()
        {
            Assert.That(Eval<bool>(Expression.GreaterThan(Constant(1), Constant(2))), Is.False);
        }

        [Test]
        public void GreaterThanOrEqual_EqualOperands_IsTrue()
        {
            Assert.That(Eval<bool>(Expression.GreaterThanOrEqual(Constant(2), Constant(2))), Is.True);
        }

        [Test]
        public void Comparison_ResultTypeIsBoolean()
        {
            Assert.That(Expression.LessThan(Constant(1), Constant(2)).Type, Is.EqualTo(typeof(bool)));
        }

        [Test]
        public void And_Booleans_IsBitwiseAnd()
        {
            Assert.That(Eval<bool>(Expression.And(True, False)), Is.False);
        }

        [Test]
        public void And_Integers_IsBitwiseAnd()
        {
            Assert.That(Eval<int>(Expression.And(Constant(6), Constant(3))), Is.EqualTo(2));
        }

        [Test]
        public void Or_Integers_IsBitwiseOr()
        {
            Assert.That(Eval<int>(Expression.Or(Constant(6), Constant(3))), Is.EqualTo(7));
        }

        [Test]
        public void ExclusiveOr_Integers_IsXor()
        {
            Assert.That(Eval<int>(Expression.ExclusiveOr(Constant(6), Constant(3))), Is.EqualTo(5));
        }

        [Test]
        public void AndAlso_FalseLeft_ShortCircuits()
        {
            var e = Expression.AndAlso(False, ThrowingBoolean());
            Assert.That(Eval<bool>(e), Is.False);
        }

        [Test]
        public void OrElse_TrueLeft_ShortCircuits()
        {
            var e = Expression.OrElse(True, ThrowingBoolean());
            Assert.That(Eval<bool>(e), Is.True);
        }

        [Test]
        public void AndAlso_TrueLeft_EvaluatesRight()
        {
            Assert.That(Eval<bool>(Expression.AndAlso(True, False)), Is.False);
            Assert.That(Eval<bool>(Expression.AndAlso(True, True)), Is.True);
        }

        [Test]
        public void OrElse_FalseLeft_EvaluatesRight()
        {
            Assert.That(Eval<bool>(Expression.OrElse(False, True)), Is.True);
            Assert.That(Eval<bool>(Expression.OrElse(False, False)), Is.False);
        }

        [Test]
        public void Not_Boolean_Negates()
        {
            Assert.That(Eval<bool>(Expression.Not(True)), Is.False);
        }

        [Test]
        public void Not_Integer_IsOnesComplement()
        {
            Assert.That(Eval<int>(Expression.Not(Constant(0))), Is.EqualTo(-1));
        }

        [Test]
        public void LeftShift_Integer_Shifts()
        {
            Assert.That(Eval<int>(Expression.LeftShift(Constant(1), Constant(4))), Is.EqualTo(16));
        }

        [Test]
        public void RightShift_Integer_Shifts()
        {
            Assert.That(Eval<int>(Expression.RightShift(Constant(16), Constant(4))), Is.EqualTo(1));
        }

        #endregion

        #region Conditional and coalesce

        [Test]
        public void Condition_TrueTest_ReturnsIfTrue()
        {
            var e = Expression.Condition(True, Constant(1), Constant(2));
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.Conditional));
            Assert.That(Eval<int>(e), Is.EqualTo(1));
        }

        [Test]
        public void Condition_FalseTest_ReturnsIfFalse()
        {
            Assert.That(Eval<int>(Expression.Condition(False, Constant(1), Constant(2))),
                        Is.EqualTo(2));
        }

        [Test]
        public void Condition_NonBooleanTest_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => Expression.Condition(Constant(1), Constant(1), Constant(2)));
        }

        [Test]
        public void Condition_BranchTypesDiffer_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => Expression.Condition(True, Constant(1), Expression.Constant("s")));
        }

        [Test]
        public void Coalesce_NonNullLeft_ReturnsLeft()
        {
            var e = Expression.Coalesce(Expression.Constant("a"), Expression.Constant("b"));
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.Coalesce));
            Assert.That(Eval<string>(e), Is.EqualTo("a"));
        }

        [Test]
        public void Coalesce_NullLeft_ReturnsRight()
        {
            var e = Expression.Coalesce(Expression.Constant(null, typeof(string)),
                                        Expression.Constant("b"));
            Assert.That(Eval<string>(e), Is.EqualTo("b"));
        }

        [Test]
        public void Coalesce_NullableLeft_UnwrapsToNonNullable()
        {
            var e = Expression.Coalesce(Expression.Constant(null, typeof(int?)), Constant(5));
            Assert.That(Eval<int>(e), Is.EqualTo(5));
        }

        #endregion

        #region Nullable (lifted) operators

        [Test]
        public void Add_NullableOperands_LiftsToNullable()
        {
            var e = Expression.Add(Nullable(1), Nullable(2));
            Assert.That(e.Type, Is.EqualTo(typeof(int?)));
            Assert.That(Eval<int?>(e), Is.EqualTo(3));
        }

        [Test]
        public void Add_NullOperand_YieldsNull()
        {
            var e = Expression.Add(Nullable(1), Expression.Constant(null, typeof(int?)));
            Assert.That(Eval<int?>(e), Is.Null);
        }

        [Test]
        public void LessThan_NullOperand_YieldsFalse()
        {
            var e = Expression.LessThan(Nullable(1), Expression.Constant(null, typeof(int?)));
            Assert.That(e.Type, Is.EqualTo(typeof(bool)));
            Assert.That(Eval<bool>(e), Is.False);
        }

        [Test]
        public void Equal_BothNull_YieldsTrue()
        {
            var e = Expression.Equal(Expression.Constant(null, typeof(int?)),
                                     Expression.Constant(null, typeof(int?)));
            Assert.That(Eval<bool>(e), Is.True);
        }

        [Test]
        public void Negate_NullOperand_YieldsNull()
        {
            var e = Expression.Negate(Expression.Constant(null, typeof(int?)));
            Assert.That(Eval<int?>(e), Is.Null);
        }

        [Test]
        public void IsLifted_ForLiftedComparison_IsTrueButNotLiftedToNull()
        {
            var e = Expression.LessThan(Nullable(1), Nullable(2));
            Assert.That(e.IsLifted, Is.True);
            Assert.That(e.IsLiftedToNull, Is.False);
        }

        [Test]
        public void LessThan_LiftedToNull_YieldsNullableBoolean()
        {
            var e = Expression.LessThan(Nullable(1), Expression.Constant(null, typeof(int?)),
                                        true, null);
            Assert.That(e.IsLiftedToNull, Is.True);
            Assert.That(e.Type, Is.EqualTo(typeof(bool?)));
            Assert.That(Eval<bool?>(e), Is.Null);
        }

        #endregion

        #region Conversions and type tests

        [Test]
        public void Convert_IntToLong_Widens()
        {
            var e = Expression.Convert(Constant(5), typeof(long));
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.Convert));
            Assert.That(e.Type, Is.EqualTo(typeof(long)));
            Assert.That(Eval<long>(e), Is.EqualTo(5L));
        }

        [Test]
        public void Convert_LongToInt_Truncates()
        {
            var e = Expression.Convert(Expression.Constant(long.MaxValue), typeof(int));
            Assert.That(Eval<int>(e), Is.EqualTo(-1));
        }

        [Test]
        public void ConvertChecked_OutOfRange_ThrowsOverflowException()
        {
            var f = Expression.Lambda<Func<int>>(
                Expression.ConvertChecked(Expression.Constant(long.MaxValue), typeof(int))).Compile();
            Assert.Throws<OverflowException>(() => f());
        }

        [Test]
        public void Convert_ValueTypeToObject_Boxes()
        {
            var e = Expression.Convert(Constant(5), typeof(object));
            Assert.That(Eval<object>(e), Is.EqualTo(5));
        }

        [Test]
        public void Convert_ObjectToValueType_Unboxes()
        {
            var e = Expression.Convert(Expression.Constant(5, typeof(object)), typeof(int));
            Assert.That(Eval<int>(e), Is.EqualTo(5));
        }

        [Test]
        public void Convert_IntToNullableInt_Lifts()
        {
            var e = Expression.Convert(Constant(5), typeof(int?));
            Assert.That(Eval<int?>(e), Is.EqualTo(5));
        }

        [Test]
        public void Convert_ImpossibleConversion_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(
                () => Expression.Convert(Expression.Constant("s"), typeof(int)));
        }

        [Test]
        public void TypeAs_CompatibleReference_ReturnsInstance()
        {
            var e = Expression.TypeAs(Expression.Constant("s", typeof(object)), typeof(string));
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.TypeAs));
            Assert.That(Eval<string>(e), Is.EqualTo("s"));
        }

        [Test]
        public void TypeAs_IncompatibleReference_ReturnsNull()
        {
            var e = Expression.TypeAs(Expression.Constant(5, typeof(object)), typeof(string));
            Assert.That(Eval<string>(e), Is.Null);
        }

        [Test]
        public void TypeIs_MatchingType_IsTrue()
        {
            var e = Expression.TypeIs(Expression.Constant("s", typeof(object)), typeof(string));
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.TypeIs));
            Assert.That(e.TypeOperand, Is.EqualTo(typeof(string)));
            Assert.That(Eval<bool>(e), Is.True);
        }

        [Test]
        public void TypeIs_NonMatchingType_IsFalse()
        {
            var e = Expression.TypeIs(Expression.Constant(5, typeof(object)), typeof(string));
            Assert.That(Eval<bool>(e), Is.False);
        }

        #endregion

        #region Calls, invocation and quoting

        [Test]
        public void Call_StaticMethod_Invokes()
        {
            var max = typeof(Math).GetMethod("Max", new[] { typeof(int), typeof(int) });
            var e = Expression.Call(max, Constant(2), Constant(9));
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.Call));
            Assert.That(e.Object, Is.Null);
            Assert.That(e.Method, Is.EqualTo(max));
            Assert.That(e.Arguments.Count, Is.EqualTo(2));
            Assert.That(Eval<int>(e), Is.EqualTo(9));
        }

        [Test]
        public void Call_InstanceMethod_Invokes()
        {
            var toUpper = typeof(string).GetMethod("ToUpperInvariant", new Type[0]);
            var e = Expression.Call(Expression.Constant("abc"), toUpper);
            Assert.That(Eval<string>(e), Is.EqualTo("ABC"));
        }

        [Test]
        public void Call_InstanceMethodWithArguments_Invokes()
        {
            var substring = typeof(string).GetMethod("Substring", new[] { typeof(int), typeof(int) });
            var e = Expression.Call(Expression.Constant("abcdef"), substring, Constant(1), Constant(3));
            Assert.That(Eval<string>(e), Is.EqualTo("bcd"));
        }

        [Test]
        public void Call_NullMethod_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => Expression.Call(null, (MethodInfo) null, new Expression[0]));
        }

        [Test]
        public void Call_WrongArgumentCount_ThrowsArgumentException()
        {
            var max = typeof(Math).GetMethod("Max", new[] { typeof(int), typeof(int) });
            Assert.Throws<ArgumentException>(() => Expression.Call(max, Constant(1)));
        }

        [Test]
        public void Call_WrongArgumentType_ThrowsArgumentException()
        {
            var max = typeof(Math).GetMethod("Max", new[] { typeof(int), typeof(int) });
            Assert.Throws<ArgumentException>(
                () => Expression.Call(max, Constant(1), Expression.Constant("s")));
        }

        [Test]
        public void Call_VirtualMethodOnBase_DispatchesVirtually()
        {
            var describe = typeof(Animal).GetMethod("Describe");
            var e = Expression.Call(Expression.Constant(new Dog(), typeof(Animal)), describe);
            Assert.That(Eval<string>(e), Is.EqualTo("dog"));
        }

        [Test]
        public void Invoke_CompiledLambda_CallsIt()
        {
            var p = Expression.Parameter(typeof(int), "x");
            var inner = Expression.Lambda<Func<int, int>>(Expression.Multiply(p, Constant(2)), p);
            var e = Expression.Invoke(inner, Constant(21));
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.Invoke));
            Assert.That(Eval<int>(e), Is.EqualTo(42));
        }

        [Test]
        public void Invoke_DelegateConstant_CallsIt()
        {
            Func<int, int> twice = x => x * 2;
            var e = Expression.Invoke(Expression.Constant(twice), Constant(4));
            Assert.That(Eval<int>(e), Is.EqualTo(8));
        }

        [Test]
        public void Quote_Lambda_ProducesQuoteNode()
        {
            var inner = Expression.Lambda<Func<int>>(Constant(1));
            var e = Expression.Quote(inner);
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.Quote));
            Assert.That(e.Operand, Is.SameAs(inner));
        }

        [Test]
        public void Quote_CompiledInsideLambda_YieldsExpressionTree()
        {
            var inner = Expression.Lambda<Func<int>>(Constant(1));
            var outer = Expression.Lambda<Func<Expression<Func<int>>>>(Expression.Quote(inner));
            var tree = outer.Compile()();
            Assert.That(tree.Compile()(), Is.EqualTo(1));
        }

        #endregion

        #region Members, construction and initialisation

        [Test]
        public void Field_OnConstant_ReadsField()
        {
            var field = typeof(Box).GetField("Field");
            var e = Expression.Field(Expression.Constant(new Box { Field = 11 }), field);
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.MemberAccess));
            Assert.That(e.Member, Is.EqualTo(field));
            Assert.That(Eval<int>(e), Is.EqualTo(11));
        }

        [Test]
        public void Field_ByName_ReadsField()
        {
            var e = Expression.Field(Expression.Constant(new Box { Field = 12 }), "Field");
            Assert.That(Eval<int>(e), Is.EqualTo(12));
        }

        [Test]
        public void Field_UnknownName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => Expression.Field(Expression.Constant(new Box()), "NoSuchField"));
        }

        [Test]
        public void Property_ByName_ReadsProperty()
        {
            var e = Expression.Property(Expression.Constant(new Box { Property = 13 }), "Property");
            Assert.That(Eval<int>(e), Is.EqualTo(13));
        }

        [Test]
        public void Property_ByPropertyInfo_ReadsProperty()
        {
            var property = typeof(Box).GetProperty("Property");
            var e = Expression.Property(Expression.Constant(new Box { Property = 14 }), property);
            Assert.That(e.Member, Is.EqualTo(property));
            Assert.That(Eval<int>(e), Is.EqualTo(14));
        }

        [Test]
        public void Property_ByAccessorMethod_ReadsProperty()
        {
            var getter = typeof(Box).GetProperty("Property").GetGetMethod();
            var e = Expression.Property(Expression.Constant(new Box { Property = 15 }), getter);
            Assert.That(Eval<int>(e), Is.EqualTo(15));
        }

        [Test]
        public void PropertyOrField_ResolvesEither()
        {
            var box = Expression.Constant(new Box { Field = 1, Property = 2 });
            Assert.That(Eval<int>(Expression.PropertyOrField(box, "Field")), Is.EqualTo(1));
            Assert.That(Eval<int>(Expression.PropertyOrField(box, "Property")), Is.EqualTo(2));
        }

        [Test]
        public void PropertyOrField_UnknownName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => Expression.PropertyOrField(Expression.Constant(new Box()), "Nope"));
        }

        [Test]
        public void New_DefaultConstructor_Constructs()
        {
            var e = Expression.New(typeof(Box));
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.New));
            Assert.That(Eval<Box>(e), Is.Not.Null);
        }

        [Test]
        public void New_ConstructorWithArguments_Constructs()
        {
            var ctor = typeof(Box).GetConstructor(new[] { typeof(int) });
            var e = Expression.New(ctor, Constant(3));
            Assert.That(e.Constructor, Is.EqualTo(ctor));
            Assert.That(Eval<Box>(e).Field, Is.EqualTo(3));
        }

        [Test]
        public void New_ValueType_ProducesDefaultInstance()
        {
            var e = Expression.New(typeof(int));
            Assert.That(Eval<int>(e), Is.EqualTo(0));
        }

        [Test]
        public void New_TypeWithoutParameterlessConstructor_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Expression.New(typeof(NoDefaultCtor)));
        }

        [Test]
        public void MemberInit_AssignsMembers()
        {
            var e = Expression.MemberInit(
                Expression.New(typeof(Box)),
                Expression.Bind(typeof(Box).GetField("Field"), Constant(4)),
                Expression.Bind(typeof(Box).GetProperty("Property"), Constant(5)));
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.MemberInit));
            Assert.That(e.Bindings.Count, Is.EqualTo(2));

            var box = Eval<Box>(e);
            Assert.That(box.Field, Is.EqualTo(4));
            Assert.That(box.Property, Is.EqualTo(5));
        }

        [Test]
        public void Bind_ValueNotAssignableToMember_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => Expression.Bind(typeof(Box).GetField("Field"), Expression.Constant("s")));
        }

        [Test]
        public void MemberInit_NestedMemberBinding_AssignsThroughGraph()
        {
            var e = Expression.MemberInit(
                Expression.New(typeof(Node)),
                Expression.MemberBind(
                    typeof(Node).GetProperty("Child"),
                    Expression.Bind(typeof(Box).GetField("Field"), Constant(6))));
            Assert.That(Eval<Node>(e).Child.Field, Is.EqualTo(6));
        }

        [Test]
        public void MemberInit_ListBinding_FillsCollection()
        {
            var add = typeof(List<int>).GetMethod("Add");
            var e = Expression.MemberInit(
                Expression.New(typeof(Node)),
                Expression.ListBind(typeof(Node).GetProperty("Items"),
                                    Expression.ElementInit(add, Constant(1)),
                                    Expression.ElementInit(add, Constant(2))));
            var items = Eval<Node>(e).Items;
            Assert.That(items.Count, Is.EqualTo(2));
            Assert.That(items[0], Is.EqualTo(1));
            Assert.That(items[1], Is.EqualTo(2));
        }

        [Test]
        public void ListInit_WithAddMethod_FillsCollection()
        {
            var add = typeof(List<int>).GetMethod("Add");
            var e = Expression.ListInit(Expression.New(typeof(List<int>)),
                                        Expression.ElementInit(add, Constant(7)),
                                        Expression.ElementInit(add, Constant(8)));
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.ListInit));
            Assert.That(e.Initializers.Count, Is.EqualTo(2));

            var list = Eval<List<int>>(e);
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0], Is.EqualTo(7));
        }

        [Test]
        public void ListInit_InferringAddMethod_FillsCollection()
        {
            var e = Expression.ListInit(Expression.New(typeof(List<int>)), Constant(9));
            Assert.That(Eval<List<int>>(e)[0], Is.EqualTo(9));
        }

        [Test]
        public void ListInit_TargetWithoutAddMethod_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(
                () => Expression.ListInit(Expression.New(typeof(Box)), Constant(1)));
        }

        [Test]
        public void ElementInit_NullAddMethod_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Expression.ElementInit(null, Constant(1)));
        }

        #endregion

        #region Arrays

        [Test]
        public void NewArrayInit_CreatesPopulatedArray()
        {
            var e = Expression.NewArrayInit(typeof(int), Constant(1), Constant(2), Constant(3));
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.NewArrayInit));
            Assert.That(e.Type, Is.EqualTo(typeof(int[])));

            var array = Eval<int[]>(e);
            Assert.That(array.Length, Is.EqualTo(3));
            Assert.That(array[2], Is.EqualTo(3));
        }

        [Test]
        public void NewArrayInit_ReferenceElements_CreatesPopulatedArray()
        {
            var e = Expression.NewArrayInit(typeof(string),
                                            Expression.Constant("a"), Expression.Constant("b"));
            Assert.That(Eval<string[]>(e)[1], Is.EqualTo("b"));
        }

        [Test]
        public void NewArrayInit_ElementNotAssignable_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(
                () => Expression.NewArrayInit(typeof(int), Expression.Constant("s")));
        }

        [Test]
        public void NewArrayBounds_CreatesArrayOfLength()
        {
            var e = Expression.NewArrayBounds(typeof(int), Constant(4));
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.NewArrayBounds));
            Assert.That(Eval<int[]>(e).Length, Is.EqualTo(4));
        }

        [Test]
        public void NewArrayBounds_NonIntegerBound_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => Expression.NewArrayBounds(typeof(int), Expression.Constant("s")));
        }

        [Test]
        public void ArrayIndex_ReadsElement()
        {
            var array = Expression.Constant(new[] { 10, 20, 30 });
            var e = Expression.ArrayIndex(array, Constant(1));
            Assert.That(Eval<int>(e), Is.EqualTo(20));
        }

        [Test]
        public void ArrayIndex_OutOfRange_ThrowsIndexOutOfRangeException()
        {
            var array = Expression.Constant(new[] { 1 });
            var f = Expression.Lambda<Func<int>>(Expression.ArrayIndex(array, Constant(5))).Compile();
            Assert.Throws<IndexOutOfRangeException>(() => f());
        }

        [Test]
        public void ArrayLength_ReturnsLength()
        {
            var e = Expression.ArrayLength(Expression.Constant(new[] { 1, 2, 3 }));
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.ArrayLength));
            Assert.That(Eval<int>(e), Is.EqualTo(3));
        }

        [Test]
        public void ArrayLength_NonArray_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Expression.ArrayLength(Expression.Constant(1)));
        }

        #endregion

        #region Factory dispatch

        [Test]
        public void MakeBinary_DispatchesByNodeType()
        {
            var e = Expression.MakeBinary(ExpressionType.Multiply, Constant(3), Constant(4));
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.Multiply));
            Assert.That(Eval<int>(e), Is.EqualTo(12));
        }

        [Test]
        public void MakeUnary_DispatchesByNodeType()
        {
            var e = Expression.MakeUnary(ExpressionType.Negate, Constant(3), null);
            Assert.That(e.NodeType, Is.EqualTo(ExpressionType.Negate));
            Assert.That(Eval<int>(e), Is.EqualTo(-3));
        }

        [Test]
        public void MakeMemberAccess_ResolvesFieldAndProperty()
        {
            var box = Expression.Constant(new Box { Field = 1, Property = 2 });
            var field = Expression.MakeMemberAccess(box, typeof(Box).GetField("Field"));
            var property = Expression.MakeMemberAccess(box, typeof(Box).GetProperty("Property"));
            Assert.That(Eval<int>(field), Is.EqualTo(1));
            Assert.That(Eval<int>(property), Is.EqualTo(2));
        }

        #endregion

        #region Composition

        [Test]
        public void Compile_NestedLambdaOverOuterParameter_Closes()
        {
            var x = Expression.Parameter(typeof(int), "x");
            var y = Expression.Parameter(typeof(int), "y");
            var inner = Expression.Lambda<Func<int, int>>(Expression.Add(x, y), y);
            var outer = Expression.Lambda<Func<int, Func<int, int>>>(inner, x);
            Assert.That(outer.Compile()(10)(5), Is.EqualTo(15));
        }

        [Test]
        public void Compile_DeeplyNestedArithmetic_Evaluates()
        {
            var x = Expression.Parameter(typeof(int), "x");
            Expression body = x;
            for (var i = 0; i < 10; i++)
                body = Expression.Add(body, Constant(1));
            Assert.That(Expression.Lambda<Func<int, int>>(body, x).Compile()(0), Is.EqualTo(10));
        }

        [Test]
        public void Compile_SameLambdaTwice_ProducesIndependentDelegates()
        {
            var x = Expression.Parameter(typeof(int), "x");
            var lambda = Expression.Lambda<Func<int, int>>(Expression.Add(x, Constant(1)), x);
            var first = lambda.Compile();
            var second = lambda.Compile();
            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(first(1), Is.EqualTo(2));
            Assert.That(second(1), Is.EqualTo(2));
        }

        [Test]
        public void CompiledLambda_ConstantReference_ReadsCurrentState()
        {
            var box = new Box { Field = 1 };
            var e = Expression.Field(Expression.Constant(box), "Field");
            var f = Expression.Lambda<Func<int>>(e).Compile();
            Assert.That(f(), Is.EqualTo(1));
            box.Field = 2;
            Assert.That(f(), Is.EqualTo(2));
        }

        [Test]
        public void CompiledLambda_ThrowingBody_PropagatesException()
        {
            var f = Expression.Lambda<Func<int>>(
                Expression.Call(typeof(ExpressionFixture).GetMethod("Throw"))).Compile();
            Assert.Throws<InvalidOperationException>(() => f());
        }

        [Test]
        public void ToString_ReturnsNonEmptyDescription()
        {
            var x = Expression.Parameter(typeof(int), "x");
            var text = Expression.Lambda<Func<int, int>>(Expression.Add(x, Constant(1)), x).ToString();
            Assert.That(string.IsNullOrEmpty(text), Is.False);
        }

        #endregion

        #region Helpers

        private static ConstantExpression Constant(int value)
        {
            return Expression.Constant(value, typeof(int));
        }

        private static ConstantExpression Nullable(int value)
        {
            return Expression.Constant(value, typeof(int?));
        }

        private static ConstantExpression True
        {
            get { return Expression.Constant(true); }
        }

        private static ConstantExpression False
        {
            get { return Expression.Constant(false); }
        }

        private static Expression ThrowingBoolean()
        {
            return Expression.Call(typeof(ExpressionFixture).GetMethod("ThrowBoolean"));
        }

        public static int Throw()
        {
            throw new InvalidOperationException("boom");
        }

        public static bool ThrowBoolean()
        {
            throw new InvalidOperationException("boom");
        }

        private static T Eval<T>(Expression body)
        {
            return Expression.Lambda<Func<T>>(body).Compile()();
        }

        public sealed class Box
        {
            public int Field;

            public Box() {}
            public Box(int field) { Field = field; }

            public int Property { get; set; }
        }

        public sealed class Node
        {
            private readonly List<int> _items = new List<int>();
            private readonly Box _child = new Box();

            public List<int> Items { get { return _items; } }
            public Box Child { get { return _child; } }
        }

        public sealed class NoDefaultCtor
        {
            public NoDefaultCtor(int value) { Value = value; }
            public int Value { get; private set; }
        }

        public class Animal
        {
            public virtual string Describe() { return "animal"; }
        }

        public sealed class Dog : Animal
        {
            public override string Describe() { return "dog"; }
        }

        #endregion
    }
}
