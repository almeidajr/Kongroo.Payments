namespace Kongroo.Payments.Domain;

public record CustomerId(Guid Value)
{
    public static CustomerId From(Guid value) => new(value);
}
