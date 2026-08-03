using Fallout.Common;

namespace Automation.Fallout.Components.Components;

/// <summary>
/// Runs unit tests. Inherits from <see cref="ITestExecution"/> for shared test logic.
/// Override <see cref="ITestExecution.RunTests"/> to customize test execution behavior.
/// </summary>
public interface IRunUnitTests : ITestExecution
{
    Target UnitTests => t => t
        .DependsOn<ICompile>(x => x.Compile)
        .Description("Run unit tests")
        .Executes(() => RunTests("UnitTests"));
}
