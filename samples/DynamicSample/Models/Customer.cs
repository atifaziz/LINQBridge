namespace DynamicSample.Models;

public class Customer
{
    public string Name { get; set; }
    public int Age;

    public string Greet() { return "Hello, " + Name; }
    public string Greet(string greeting) { return greeting + ", " + Name; }
    public string Greet(string greeting, int times) { return greeting + " x" + times; }
    public T Echo<T>(T value) { return value; }
    public int Sum(params int[] numbers)
    {
        var total = 0;
        foreach (var n in numbers) total += n;
        return total;
    }
    public string Describe(string prefix, string suffix = "!") { return prefix + Name + suffix; }
    public void Swap(ref int left, ref int right) { var t = left; left = right; right = t; }
    public string this[int index] { get { return Name + "[" + index + "]"; } }
}