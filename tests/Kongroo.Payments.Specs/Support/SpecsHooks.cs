using Reqnroll;

namespace Kongroo.Payments.Specs.Support;

[Binding]
public sealed class SpecsHooks(ApiScenarioContext apiScenarioContext)
{
    [BeforeTestRun]
    public static async Task BeforeTestRunAsync() => await SpecsEnvironment.StartAsync();

    [AfterScenario("@webapi")]
    public void AfterScenario() => apiScenarioContext.Dispose();

    [AfterTestRun]
    public static async Task AfterTestRunAsync() => await SpecsEnvironment.StopAsync();
}
