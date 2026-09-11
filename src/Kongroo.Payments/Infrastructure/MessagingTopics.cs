namespace Kongroo.Payments.Infrastructure;

/// <summary>
/// SNS topic names used with the Amazon SQS transport. Shared by contract with Kongroo.Catalog and
/// the Kongroo.Notifications SAM template — change all three together.
/// </summary>
public static class MessagingTopics
{
    public const string OrderPlaced = "kongroo-order-placed";
    public const string PaymentProcessed = "kongroo-payment-processed";
}
