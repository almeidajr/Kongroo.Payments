using System.Net;
using Shouldly;

namespace Kongroo.Payments.IntegrationTests.Support;

public static class TestPolling
{
    public static async Task WaitForHealthyAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(60);
        while (true)
        {
            using var response = await client.GetAsync("/health", cancellationToken);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return;
            }

            if (DateTimeOffset.UtcNow >= deadline)
            {
                response.StatusCode.ShouldBe(HttpStatusCode.OK);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
        }
    }

    public static async Task WaitUntilAsync(Func<Task<bool>> condition, CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(60);
        while (!await condition())
        {
            if (DateTimeOffset.UtcNow >= deadline)
            {
                throw new TimeoutException("The expected state was not observed within the timeout.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(150), cancellationToken);
        }
    }
}
