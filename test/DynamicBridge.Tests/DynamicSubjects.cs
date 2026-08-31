#region License, Terms and Author(s)
//
// DynamicBridge tests
//
// These types are compiled into both test projects: DynamicBridge.Tests (net30, against
// Net20.DynamicBridge) and Dynamic.Tests (net40, against the real Microsoft.CSharp). Keep them
// inside the API surface both frameworks share.
//
#endregion

using System;
using System.Collections.Generic;
using System.Dynamic;

namespace DynamicBridge.Tests
{
    public class Person
    {
        public string Name { get; set; }
        public int Age;
        public readonly string Id = "fixed";

        public Person() { Name = "anonymous"; }
        public Person(string name) { Name = name; }
        public Person(string name, int age) { Name = name; Age = age; }

        public string ReadOnlyName { get { return Name; } }

        public string Greet() { return "hello " + Name; }
        public string Greet(string greeting) { return greeting + " " + Name; }
        public string Greet(string greeting, int times) { return greeting + "x" + times; }
        public string Greet(int number) { return "n" + number; }

        public T Echo<T>(T value) { return value; }
        public string Describe<T>(T value) { return typeof(T).Name; }

        public int Sum(params int[] values)
        {
            var total = 0;
            foreach (var value in values) total += value;
            return total;
        }

        public string Optional(string first, string second = "b", string third = "c")
        {
            return first + second + third;
        }

        public void Swap(ref int left, ref int right)
        {
            var temp = left;
            left = right;
            right = temp;
        }

        public bool TryDouble(int value, out int result)
        {
            result = value * 2;
            return true;
        }

        public string this[int index] { get { return "item" + index; } }
        public string this[string key, int index] { get { return key + index; } }

        public static string Shout(string what) { return what.ToUpper(); }

        public event EventHandler Changed;

        public void RaiseChanged() { if (Changed != null) Changed(this, EventArgs.Empty); }

        public Func<int, int> Twice = value => value * 2;

        public string Throws() { throw new InvalidOperationException("boom"); }
    }

    public class Employee : Person
    {
        public Employee() { }
        public Employee(string name) : base(name) { }
        public string Department = "none";
    }

    public interface INamed
    {
        string Name { get; }
        string Describe(string prefix);
    }

    public class Named : INamed
    {
        public string Name { get { return "named"; } }
        public string Describe(string prefix) { return prefix + Name; }
    }

    public struct Money
    {
        public readonly decimal Amount;
        public Money(decimal amount) { Amount = amount; }

        public static Money operator +(Money left, Money right) { return new Money(left.Amount + right.Amount); }
        public static Money operator -(Money left, Money right) { return new Money(left.Amount - right.Amount); }
        public static Money operator -(Money money) { return new Money(-money.Amount); }
        public static bool operator ==(Money left, Money right) { return left.Amount == right.Amount; }
        public static bool operator !=(Money left, Money right) { return left.Amount != right.Amount; }
        public static bool operator >(Money left, Money right) { return left.Amount > right.Amount; }
        public static bool operator <(Money left, Money right) { return left.Amount < right.Amount; }

        public static implicit operator decimal(Money money) { return money.Amount; }
        public static explicit operator int(Money money) { return (int)money.Amount; }

        public override bool Equals(object obj) { return obj is Money && ((Money)obj).Amount == Amount; }
        public override int GetHashCode() { return Amount.GetHashCode(); }
        public override string ToString() { return Amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture); }
    }

    public enum Colour
    {
        Red = 1,
        Green = 2,
        Blue = 4,
    }

    /// <summary>A DynamicObject that answers everything from a dictionary.</summary>
    public class Bag : DynamicObject
    {
        private readonly Dictionary<string, object> _items = new Dictionary<string, object>();

        public int GetMemberCalls;
        public int SetMemberCalls;
        public int InvokeMemberCalls;

        public string Fixed { get { return "clr-property"; } }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            GetMemberCalls++;
            return _items.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            SetMemberCalls++;
            _items[binder.Name] = value;
            return true;
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            return _items.Remove(binder.Name);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            InvokeMemberCalls++;
            if (binder.Name == "Unhandled")
            {
                result = null;
                return false;
            }
            result = binder.Name + ":" + args.Length;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = "index:" + indexes.Length;
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            _items["[" + indexes[0] + "]"] = value;
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type == typeof(string))
            {
                result = "converted";
                return true;
            }
            result = null;
            return false;
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            result = "binary:" + binder.Operation;
            return true;
        }

        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
        {
            result = "unary:" + binder.Operation;
            return true;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = "invoked:" + args.Length;
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _items.Keys;
        }

        public int Count { get { return _items.Count; } }
    }

    /// <summary>A DynamicObject that never handles anything, so binding falls back to its CLR type.</summary>
    public class PassThrough : DynamicObject
    {
        public string Real { get { return "real"; } }
        public string RealMethod(int value) { return "real" + value; }
    }
}
