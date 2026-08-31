#region License, Terms and Author(s)
//
// DynamicBridge sample
//
// Proves that the C# 'dynamic' keyword both compiles and binds when the target is CLR 2.0.
//
#endregion

using System;
using System.Dynamic;
using DynamicSample.Models;

namespace DynamicSample
{
    public static class Program
    {
        private static int _failures;

        public static int Main()
        {
            Console.WriteLine("Net20.DynamicBridge sample — CLR " + Environment.Version);
            Console.WriteLine();

            var customer = new Customer { Name = "Ada", Age = 36 };
            dynamic d = customer;

            Check("get property", d.Name, "Ada");
            Check("get field", d.Age, 36);
            d.Name = "Grace";
            Check("set property", customer.Name, "Grace");
            d.Age = 45;
            Check("set field", customer.Age, 45);

            Check("invoke", d.Greet(), "Hello, Grace");
            Check("overload (string)", d.Greet("Hi"), "Hi, Grace");
            Check("overload (string,int)", d.Greet("Hi", 3), "Hi x3");
            Check("generic inference", d.Echo(42), 42);
            Check("params array", d.Sum(1, 2, 3, 4), 10);
            Check("optional argument", d.Describe("<"), "<Grace!");
            Check("named argument", d.Describe(suffix: ">", prefix: "<"), "<Grace>");
            Check("indexer", d[7], "Grace[7]");

            int left = 1, right = 2;
            d.Swap(ref left, ref right);
            Check("ref write-back", left + "," + right, "2,1");

            dynamic a = 6, b = 7;
            Check("arithmetic", a * b, 42);
            Check("mixed arithmetic", a + 0.5, 6.5);
            Check("comparison", a < b, true);
            Check("bitwise", a & b, 6);
            Check("shift", a << 2, 24);
            Check("negate", -a, -6);
            Check("ones complement", ~a, -7);

            dynamic text = "abc";
            Check("string concat", text + 1, "abc1");
            Check("string equality", text == "abc", true);
            Check("string member", text.Length, 3);

            dynamic money = new Money(1.5m);
            Check("user-defined operator", (money + new Money(2m)).ToString(), "3.50");
            Check("explicit conversion", (decimal)money, 1.5m);

            dynamic number = 3;
            int asInt = number;
            Check("implicit conversion", asInt, 3);
            double asDouble = number;
            Check("widening conversion", asDouble, 3d);

            dynamic expando = new ExpandoObject();
            expando.First = "Alan";
            expando.Last = "Turing";
            Check("expando get", expando.First, "Alan");
            Check("expando dictionary", ((System.Collections.Generic.IDictionary<string, object>)expando).Count, 2);
            Check("expando remove", ((System.Collections.Generic.IDictionary<string, object>)expando).Remove("First"), true);

            dynamic bag = new Bag();
            bag.Colour = "red";
            Check("DynamicObject get", bag.Colour, "red");
            Check("DynamicObject invoke", bag.Whatever(1, 2), "Whatever(2 args)");

            dynamic nothing = null;
            Check("null equality", nothing == null, true);

            try
            {
                var oops = d.NoSuchMember;
                Report("missing member throws", false, "no exception, got " + oops);
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException e)
            {
                Report("missing member throws", true, e.Message);
            }

            Console.WriteLine();
            Console.WriteLine(_failures == 0 ? "All checks passed." : _failures + " check(s) FAILED.");
            return _failures == 0 ? 0 : 1;
        }

        private static void Check(string what, object actual, object expected)
        {
            var ok = Equals(actual, expected);
            Report(what, ok, ok ? Format(actual) : "expected " + Format(expected) + ", got " + Format(actual));
        }

        private static void Report(string what, bool ok, string detail)
        {
            if (!ok) _failures++;
            Console.WriteLine("  {0} {1,-24} {2}", ok ? "[ok]  " : "[FAIL]", what, detail);
        }

        private static string Format(object value)
        {
            if (value == null) return "null";
            return value + " (" + value.GetType().Name + ")";
        }
    }
}
