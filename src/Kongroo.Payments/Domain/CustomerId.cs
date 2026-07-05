using Kongroo.BuildingBlocks.Domain;

namespace Kongroo.Payments.Domain;

public record CustomerId(Guid Value) : IGuidId<CustomerId>
{
    public static CustomerId Create() => new(Guid.CreateVersion7());

    public static CustomerId From(Guid value) => new(value);
}
