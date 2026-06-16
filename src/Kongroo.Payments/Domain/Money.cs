namespace Kongroo.Payments.Domain;

public sealed record Money(decimal Amount, Currency Currency)
{
    public static Money From(decimal amount, Currency currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be non-negative.");
        }

        return new Money(amount, currency);
    }
}
