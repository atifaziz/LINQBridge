#region License, Terms and Author(s)
//
// DynamicBridge tests
//
// Differential fixture: this file is compiled into DynamicBridge.Tests (net30, bound by
// Net20.DynamicBridge) and, by link, into Dynamic.Tests (net40, bound by the real
// Microsoft.CSharp). Every assertion here must hold identically on both.
//
#endregion

using System;
using System.Collections.Generic;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;

namespace DynamicBridge.Tests
{
    [TestFixture]
    public class DynamicFixture
    {
        #region Member access

        [Test]
        public void GetProperty()
        {
            dynamic person = new Person("ada");
            Assert.AreEqual("ada", person.Name);
        }

        [Test]
        public void GetField()
        {
            dynamic person = new Person { Age = 36 };
            Assert.AreEqual(36, person.Age);
        }

        [Test]
        public void GetInheritedMember()
        {
            dynamic employee = new Employee("grace");
            Assert.AreEqual("grace", employee.Name);
            Assert.AreEqual("none", employee.Department);
        }

        [Test]
        public void GetInterfaceMember()
        {
            INamed named = new Named();
            dynamic subject = named;
            Assert.AreEqual("named", subject.Name);
        }

        [Test]
        public void SetProperty()
        {
            var person = new Person();
            dynamic subject = person;
            subject.Name = "alan";
            Assert.AreEqual("alan", person.Name);
        }

        [Test]
        public void SetField()
        {
            var person = new Person();
            dynamic subject = person;
            subject.Age = 41;
            Assert.AreEqual(41, person.Age);
        }

        [Test]
        public void SetPropertyConvertsValue()
        {
            var person = new Person();
            dynamic subject = person;
            dynamic age = (short)7;
            subject.Age = age;
            Assert.AreEqual(7, person.Age);
        }

        [Test]
        public void GetMissingMemberThrows()
        {
            dynamic person = new Person();
            Assert.Throws<RuntimeBinderException>(delegate { var ignored = person.NoSuchThing; });
        }

        [Test]
        public void SetMissingMemberThrows()
        {
            dynamic person = new Person();
            Assert.Throws<RuntimeBinderException>(delegate { person.NoSuchThing = 1; });
        }

        [Test]
        public void SetReadOnlyPropertyThrows()
        {
            dynamic person = new Person();
            Assert.Throws<RuntimeBinderException>(delegate { person.ReadOnlyName = "x"; });
        }

        [Test]
        public void MemberAccessOnNullThrows()
        {
            dynamic nothing = null;
            Assert.Throws<RuntimeBinderException>(delegate { var ignored = nothing.Anything; });
        }

        #endregion

        #region Invocation

        [Test]
        public void InvokeWithoutArguments()
        {
            dynamic person = new Person("ada");
            Assert.AreEqual("hello ada", person.Greet());
        }

        [Test]
        public void InvokeSelectsStringOverload()
        {
            dynamic person = new Person("ada");
            Assert.AreEqual("hi ada", person.Greet("hi"));
        }

        [Test]
        public void InvokeSelectsIntOverload()
        {
            dynamic person = new Person("ada");
            Assert.AreEqual("n3", person.Greet(3));
        }

        [Test]
        public void InvokeSelectsTwoArgumentOverload()
        {
            dynamic person = new Person("ada");
            Assert.AreEqual("hix2", person.Greet("hi", 2));
        }

        [Test]
        public void InvokeWithDynamicArgument()
        {
            dynamic person = new Person("ada");
            dynamic greeting = "hey";
            Assert.AreEqual("hey ada", person.Greet(greeting));
        }

        [Test]
        public void InvokeGenericMethodByInference()
        {
            dynamic person = new Person();
            Assert.AreEqual(42, person.Echo(42));
            Assert.AreEqual("s", person.Echo("s"));
        }

        [Test]
        public void InvokeGenericMethodWithExplicitTypeArgument()
        {
            dynamic person = new Person();
            Assert.AreEqual("Object", person.Describe<object>(1));
        }

        [Test]
        public void InvokeParamsMethod()
        {
            dynamic person = new Person();
            Assert.AreEqual(0, person.Sum());
            Assert.AreEqual(6, person.Sum(1, 2, 3));
            Assert.AreEqual(6, person.Sum(new[] { 1, 2, 3 }));
        }

        [Test]
        public void InvokeWithOptionalArguments()
        {
            dynamic person = new Person();
            Assert.AreEqual("abc", person.Optional("a"));
            Assert.AreEqual("aXc", person.Optional("a", "X"));
        }

        [Test]
        public void InvokeWithNamedArguments()
        {
            dynamic person = new Person();
            Assert.AreEqual("aYc", person.Optional("a", second: "Y"));
            Assert.AreEqual("abZ", person.Optional(third: "Z", first: "a"));
        }

        [Test]
        public void InvokeWithRefArgument()
        {
            dynamic person = new Person();
            int left = 1, right = 2;
            person.Swap(ref left, ref right);
            Assert.AreEqual(2, left);
            Assert.AreEqual(1, right);
        }

        [Test]
        public void InvokeWithOutArgument()
        {
            dynamic person = new Person();
            int result;
            Assert.IsTrue(person.TryDouble(21, out result));
            Assert.AreEqual(42, result);
        }

        [Test]
        public void InvokeStaticMethodWithDynamicArgument()
        {
            dynamic value = 3;
            Assert.AreEqual(5, Math.Max(value, 5));
        }

        [Test]
        public void InvokeMissingMethodThrows()
        {
            dynamic person = new Person();
            Assert.Throws<RuntimeBinderException>(delegate { person.NoSuchMethod(); });
        }

        [Test]
        public void InvokeWithWrongArgumentTypeThrows()
        {
            dynamic person = new Person();
            Assert.Throws<RuntimeBinderException>(delegate { person.Swap("a", "b"); });
        }

        [Test]
        public void ExtensionMethodIsNotVisibleToTheBinder()
        {
            dynamic list = new List<int>();
            Assert.Throws<RuntimeBinderException>(delegate { var ignored = list.FirstOrDefault(); });
        }

        [Test]
        public void ExceptionFromInvokedMethodPropagatesUnwrapped()
        {
            dynamic person = new Person();
            Assert.Throws<InvalidOperationException>(delegate { person.Throws(); });
        }

        [Test]
        public void InvokeDelegateValuedField()
        {
            dynamic person = new Person();
            Assert.AreEqual(10, person.Twice(5));
        }

        [Test]
        public void InvokeDelegate()
        {
            dynamic doubler = new Func<int, int>(value => value * 2);
            Assert.AreEqual(8, doubler(4));
        }

        [Test]
        public void ConstructWithDynamicArgument()
        {
            dynamic name = "zoe";
            var person = new Person(name);
            Assert.AreEqual("zoe", person.Name);
        }

        #endregion

        #region Indexers

        [Test]
        public void GetIndexOnIndexer()
        {
            dynamic person = new Person();
            Assert.AreEqual("item2", person[2]);
        }

        [Test]
        public void GetIndexOnMultiArgumentIndexer()
        {
            dynamic person = new Person();
            Assert.AreEqual("k5", person["k", 5]);
        }

        [Test]
        public void GetIndexOnArray()
        {
            dynamic values = new[] { 10, 20, 30 };
            Assert.AreEqual(20, values[1]);
        }

        [Test]
        public void SetIndexOnArray()
        {
            var array = new int[3];
            dynamic values = array;
            values[1] = 7;
            Assert.AreEqual(7, array[1]);
        }

        [Test]
        public void GetIndexOnList()
        {
            dynamic list = new List<string> { "a", "b" };
            Assert.AreEqual("b", list[1]);
        }

        [Test]
        public void SetIndexOnDictionary()
        {
            var dictionary = new Dictionary<string, int>();
            dynamic subject = dictionary;
            subject["k"] = 3;
            Assert.AreEqual(3, dictionary["k"]);
        }

        [Test]
        public void GetIndexOnNonIndexableThrows()
        {
            dynamic person = new Person();
            dynamic number = 1;
            Assert.Throws<RuntimeBinderException>(delegate { var ignored = number[0]; });
        }

        #endregion

        #region Operators

        [Test]
        public void ArithmeticOnIntegers()
        {
            dynamic a = 6, b = 7;
            Assert.AreEqual(13, a + b);
            Assert.AreEqual(-1, a - b);
            Assert.AreEqual(42, a * b);
            Assert.AreEqual(0, a / b);
            Assert.AreEqual(6, a % b);
        }

        [Test]
        public void ArithmeticPromotesOperands()
        {
            dynamic a = 6;
            Assert.AreEqual(6.5, a + 0.5);
            Assert.AreEqual(7L, a + 1L);
            Assert.AreEqual(6.5m, a + 0.5m);
        }

        [Test]
        public void ArithmeticResultTypeFollowsPromotion()
        {
            dynamic a = (short)6;
            dynamic b = (short)7;
            Assert.AreEqual(typeof(int), ((object)(a + b)).GetType());

            dynamic c = 6;
            Assert.AreEqual(typeof(double), ((object)(c + 1.0)).GetType());
        }

        [Test]
        public void Comparisons()
        {
            dynamic a = 6, b = 7;
            Assert.IsTrue(a < b);
            Assert.IsTrue(a <= b);
            Assert.IsFalse(a > b);
            Assert.IsFalse(a >= b);
            Assert.IsTrue(a != b);
            Assert.IsFalse(a == b);
        }

        [Test]
        public void BitwiseAndShift()
        {
            dynamic a = 6, b = 3;
            Assert.AreEqual(2, a & b);
            Assert.AreEqual(7, a | b);
            Assert.AreEqual(5, a ^ b);
            Assert.AreEqual(48, a << b);
            Assert.AreEqual(0, a >> b);
        }

        [Test]
        public void UnaryOperators()
        {
            dynamic a = 6;
            Assert.AreEqual(-6, -a);
            Assert.AreEqual(6, +a);
            Assert.AreEqual(-7, ~a);

            dynamic flag = true;
            Assert.IsFalse(!flag);
        }

        [Test]
        public void IncrementAndDecrement()
        {
            dynamic a = 6;
            a++;
            Assert.AreEqual(7, a);
            a--;
            Assert.AreEqual(6, a);
        }

        [Test]
        public void CompoundAssignment()
        {
            dynamic a = 6;
            a += 4;
            Assert.AreEqual(10, a);
            a *= 2;
            Assert.AreEqual(20, a);
        }

        [Test]
        public void LogicalOperators()
        {
            dynamic yes = true, no = false;
            Assert.IsTrue(yes && true);
            Assert.IsFalse(no && true);
            Assert.IsTrue(no || true);
            Assert.IsTrue(yes & true);
            Assert.IsTrue(yes | no);
            Assert.IsTrue(yes ^ no);
        }

        [Test]
        public void StringConcatenation()
        {
            dynamic text = "ab";
            Assert.AreEqual("ab1", text + 1);
            Assert.AreEqual("1ab", 1 + text);
            Assert.AreEqual("abcd", text + "cd");
        }

        [Test]
        public void StringEquality()
        {
            dynamic text = "ab";
            Assert.IsTrue(text == "ab");
            Assert.IsFalse(text != "ab");
        }

        [Test]
        public void UserDefinedOperators()
        {
            dynamic one = new Money(1m);
            var two = new Money(2m);

            Assert.AreEqual(new Money(3m), one + two);
            Assert.AreEqual(new Money(-1m), one - two);
            Assert.AreEqual(new Money(-1m), -one);
            Assert.IsTrue(one < two);
            Assert.IsTrue(one != two);
        }

        [Test]
        public void EnumOperators()
        {
            dynamic red = Colour.Red;
            Assert.AreEqual(Colour.Red | Colour.Green, red | Colour.Green);
            Assert.IsTrue(red == Colour.Red);
            Assert.IsFalse(red == Colour.Blue);
        }

        [Test]
        public void NullOperands()
        {
            dynamic nothing = null;
            Assert.IsTrue(nothing == null);
            Assert.IsFalse(nothing != null);
            Assert.AreEqual("a", nothing + "a");
        }

        [Test]
        public void DivideByZeroThrows()
        {
            dynamic a = 6, b = 0;
            Assert.Throws<DivideByZeroException>(delegate { var ignored = a / b; });
        }

        [Test]
        public void CheckedOverflowThrows()
        {
            dynamic a = int.MaxValue;
            Assert.Throws<OverflowException>(delegate { var ignored = checked(a + 1); });
        }

        [Test]
        public void UncheckedOverflowWraps()
        {
            dynamic a = int.MaxValue;
            Assert.AreEqual(int.MinValue, unchecked(a + 1));
        }

        [Test]
        public void OperatorOnIncompatibleOperandsThrows()
        {
            dynamic person = new Person();
            Assert.Throws<RuntimeBinderException>(delegate { var ignored = person - 1; });
        }

        #endregion

        #region Conversions

        [Test]
        public void ImplicitConversionToSameType()
        {
            dynamic value = 3;
            int result = value;
            Assert.AreEqual(3, result);
        }

        [Test]
        public void ImplicitWideningConversion()
        {
            dynamic value = 3;
            double result = value;
            Assert.AreEqual(3d, result);
            long asLong = value;
            Assert.AreEqual(3L, asLong);
        }

        [Test]
        public void ExplicitNarrowingConversion()
        {
            dynamic value = 300;
            var result = (byte)value;
            Assert.AreEqual(44, result);
        }

        [Test]
        public void ConversionToReferenceType()
        {
            dynamic value = new Employee("z");
            Person person = value;
            Assert.AreEqual("z", person.Name);
        }

        [Test]
        public void UserDefinedImplicitConversion()
        {
            dynamic money = new Money(2.5m);
            decimal amount = money;
            Assert.AreEqual(2.5m, amount);
        }

        [Test]
        public void UserDefinedExplicitConversion()
        {
            dynamic money = new Money(2.5m);
            var amount = (int)money;
            Assert.AreEqual(2, amount);
        }

        [Test]
        public void ConversionToNullable()
        {
            dynamic value = 3;
            int? result = value;
            Assert.AreEqual(3, result.Value);

            dynamic nothing = null;
            int? empty = nothing;
            Assert.IsFalse(empty.HasValue);
        }

        [Test]
        public void InvalidConversionThrows()
        {
            dynamic value = "text";
            Assert.Throws<RuntimeBinderException>(delegate { int ignored = value; });
        }

        [Test]
        public void ConversionOfNullToValueTypeThrows()
        {
            dynamic nothing = null;
            Assert.Throws<RuntimeBinderException>(delegate { int ignored = nothing; });
        }

        #endregion

        #region Dynamic in ordinary language constructs

        [Test]
        public void DynamicInForeach()
        {
            dynamic values = new List<int> { 1, 2, 3 };
            var total = 0;
            foreach (int value in values)
                total += value;
            Assert.AreEqual(6, total);
        }

        [Test]
        public void DynamicInCondition()
        {
            dynamic flag = true;
            Assert.IsTrue(flag ? true : false);
        }

        [Test]
        public void DynamicRoundTripsThroughObject()
        {
            dynamic value = 3;
            object boxed = value;
            dynamic back = boxed;
            Assert.AreEqual(4, back + 1);
        }

        [Test]
        public void ChainedDynamicOperations()
        {
            dynamic person = new Person("ada");
            Assert.AreEqual(3, person.Name.Length);
            Assert.AreEqual("ADA", person.Name.ToUpper());
        }

        [Test]
        public void ToStringAndGetTypeAreBoundNormally()
        {
            dynamic value = 3;
            Assert.AreEqual("3", value.ToString());
            Assert.AreEqual(typeof(int), value.GetType());
        }

        #endregion
    }
}
