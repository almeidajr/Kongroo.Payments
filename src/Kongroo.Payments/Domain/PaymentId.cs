namespace Kongroo.Payments.Domain;

public record PaymentId(Guid Value)
{
    public static PaymentId Create() => new(Guid.CreateVersion7());

    public static PaymentId From(Guid value) => new(value);
}
