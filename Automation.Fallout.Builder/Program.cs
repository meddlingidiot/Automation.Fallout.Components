using System.CommandLine;
using Automation.Fallout.Builder.Commands;

namespace Automation.Fallout.Builder;

class Program
{
    static int Main(string[] args)
    {
        var rootCommand = new RootCommand("Automation Fallout Builder - Simplify Fallout build setup");

        var setupCommand = new Command("setup", "Setup Fallout build for the current project");
        setupCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            return await SetupCommand.ExecuteAsync();
        });

        rootCommand.Subcommands.Add(setupCommand);

        return rootCommand.Parse(args).Invoke();
    }
}
