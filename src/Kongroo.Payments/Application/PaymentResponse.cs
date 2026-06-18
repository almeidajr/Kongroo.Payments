namespace Kongroo.Payments.Application;

public sealed record PaymentResponse(
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    string Status,
    decimal Amount,
    string Currency,
    DateTimeOffset? ProcessedAt
);
