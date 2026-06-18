using Kongroo.Payments.Domain;

namespace Kongroo.Payments.Application;

internal static class PaymentMapping
{
    public static PaymentResponse ToResponse(Payment payment) =>
        new(
            payment.Id.Value,
            payment.OrderId.Value,
            payment.CustomerId.Value,
            payment.Status.ToString(),
            payment.Total.Amount,
            CurrencyMappings.ToCode(payment.Total.Currency),
            payment.ProcessedAt
        );
}
