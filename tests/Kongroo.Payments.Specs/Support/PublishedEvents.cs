using System.Collections.Concurrent;
using Kongroo.BuildingBlocks.Contracts;

namespace Kongroo.Payments.Specs.Support;

public static class PublishedEvents
{
    public static ConcurrentBag<PaymentProcessedIntegrationEvent> PaymentProcessed { get; } = [];
}
