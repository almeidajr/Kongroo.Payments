namespace Kongroo.Payments.Domain;

public record OrderId(Guid Value)
{
    public static OrderId From(Guid value) => new(value);
}
