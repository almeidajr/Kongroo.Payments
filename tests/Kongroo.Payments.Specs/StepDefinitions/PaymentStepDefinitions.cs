using Kongroo.Catalog.Contracts;
using Kongroo.Payments.Specs.Support;
using Reqnroll;
using Shouldly;

namespace Kongroo.Payments.Specs.StepDefinitions;

[Binding]
public sealed class PaymentStepDefinitions
{
    private Guid _orderId;

    [When("an order is placed for {string} with amount {decimal} {string}")]
    public async Task WhenAnOrderIsPlaced(string email, decimal amount, string currency)
    {
        _orderId = Guid.CreateVersion7();
        await SpecsEnvironment.Bus.Publish(
            new OrderPlacedIntegrationEvent(_orderId, Guid.CreateVersion7(), email, "Grace Hopper", amount, currency)
        );
    }

    [Then("a payment processed event is published with approval {string}")]
    public async Task ThenAPaymentProcessedEventIsPublished(string approved)
    {
        var expectedApproved = bool.Parse(approved);

        await WaitUntilAsync(() => PublishedEvents.PaymentProcessed.Any(processed => processed.OrderId == _orderId));

        var published = PublishedEvents.PaymentProcessed.Single(processed => processed.OrderId == _orderId);
        published.Approved.ShouldBe(expectedApproved);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(60);
        while (!condition())
        {
            if (DateTimeOffset.UtcNow >= deadline)
            {
                throw new TimeoutException("The expected payment processed event was not observed.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(150));
        }
    }
}
