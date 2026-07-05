using Kongroo.BuildingBlocks.Domain;

namespace Kongroo.Payments.Domain;

public record OrderId(Guid Value) : IGuidId<OrderId>
{
    public static OrderId Create() => new(Guid.CreateVersion7());

    public static OrderId From(Guid value) => new(value);
}
