namespace Kongroo.Payments.Application;

public sealed record ProcessPaymentCommand(
    Guid OrderId,
    Guid CustomerId,
    string Email,
    string CustomerName,
    decimal Amount,
    string Currency
);
