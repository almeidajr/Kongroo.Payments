using Kongroo.BuildingBlocks.Domain;

namespace Kongroo.Payments.Domain;

public sealed record PaymentProcessedDomainEvent(
    PaymentId PaymentId,
    OrderId OrderId,
    CustomerId CustomerId,
    string Email,
    string CustomerName,
    Money Total,
    bool Approved,
    DateTimeOffset ProcessedAt
) : DomainEvent;
