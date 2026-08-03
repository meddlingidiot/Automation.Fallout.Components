using Fallout.Common;

namespace Automation.Fallout.Components.Components;

/// <summary>
/// Runs integration tests. Inherits from <see cref="ITestExecution"/> for shared test logic.
/// Override <see cref="ITestExecution.RunTests"/> to customize test execution behavior.
/// </summary>
public interface IRunIntegrationTests : ITestExecution
{
    Target IntegrationTests => t => t
        .DependsOn<ICompile>(x => x.Compile)
        .Description("Run integration tests")
        .Executes(() => RunTests("IntegrationTests"));
}
