namespace DynamicSample.Models;

public readonly struct Money
{
    public readonly decimal Amount;
    public Money(decimal amount) { Amount = amount; }
    public static Money operator +(Money left, Money right) { return new Money(left.Amount + right.Amount); }
    public static explicit operator decimal(Money money) { return money.Amount; }
    public override string ToString() { return Amount.ToString("0.00"); }
}