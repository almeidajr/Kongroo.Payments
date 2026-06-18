namespace Kongroo.Payments.Application;

public sealed record GetPaymentQuery(Guid OrderId, Guid CallerId, bool CallerIsAdmin);
