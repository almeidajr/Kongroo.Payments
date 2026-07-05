using System.Collections.Concurrent;
using Kongroo.Payments.Contracts;

namespace Kongroo.Payments.Specs.Support;

public static class PublishedEvents
{
    public static ConcurrentBag<PaymentProcessedIntegrationEvent> PaymentProcessed { get; } = [];
}
