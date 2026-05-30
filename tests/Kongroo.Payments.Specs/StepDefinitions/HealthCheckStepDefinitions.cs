using Kongroo.Payments.Specs.Support;
using Reqnroll;
using Shouldly;

namespace Kongroo.Payments.Specs.StepDefinitions;

[Binding]
public sealed class HealthCheckStepDefinitions(ApiScenarioContext scenarioContext)
{
    [When("the health endpoint is requested")]
    public async Task WhenTheHealthEndpointIsRequested()
    {
        var response = await scenarioContext.Client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        scenarioContext.SetLastResponse(response, content);
    }

    [Then("the response status code is {int}")]
    public void ThenTheResponseStatusCodeIs(int statusCode) =>
        ((int)scenarioContext.LastResponse.ShouldNotBeNull().StatusCode).ShouldBe(statusCode);
}
