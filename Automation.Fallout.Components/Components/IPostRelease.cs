// using Automation.Fallout.Components.Parameters;
// using Fallout.Common;
// using Fallout.Common.Tooling;
//
namespace Automation.Fallout.Components.Components;
//
// public interface IPostRelease : IFalloutBuild, IHasGitVersion, IDoTag
// {
//     Target TagRelease => t => t
//         .DependsOn<ITest>(x => x.Test)
//         .Triggers(Announce)
//         .Description("Tag Release in Git")
//         .OnlyWhenDynamic(() => IsServerBuild || ForceTagRelease)
//         .OnlyWhenDynamic(() => GitVersion.BranchName.Equals("main", StringComparison.OrdinalIgnoreCase))
//         .Executes(() =>
//         {
//             // SETUP GIT IDENTITY
//             // This prevents "Please tell me who you are" errors
//             ProcessTasks.StartProcess("git", "config --local user.name \"Azure DevOps Pipeline\"")
//                 .AssertZeroExitCode();
//             ProcessTasks.StartProcess("git", "config --local user.email \"pipeline@azure.com\"")
//                 .AssertZeroExitCode();
//
//             var version = GitVersion.MajorMinorPatch;
//             var tag = $"v{version}";
//             Serilog.Log.Information("Tagging release with version {Version}", version);
//
//             // helper to run git and return captured stdout as string (last line)
//             string? RunGitSingle(string args)
//             {
//                 var p = ProcessTasks.StartProcess("git", args);
//                 p.AssertWaitForExit();
//                 return p.Output.Select(x => x.Text).LastOrDefault()?.Trim();
//             }
//
//             // check local tag
//             var localCheck = ProcessTasks.StartProcess("git", $"rev-parse -q --verify refs/tags/{tag}");
//             localCheck.AssertWaitForExit();
//             var localExists = localCheck.ExitCode == 0;
//             string? localSha = null;
//             if (localExists)
//                 localSha = RunGitSingle($"rev-list -n 1 {tag}");
//
//             // check remote tag (origin)
//             var remoteProc = ProcessTasks.StartProcess("git", $"ls-remote --tags origin refs/tags/{tag}");
//             remoteProc.AssertWaitForExit();
//             var remoteLine = remoteProc.Output.Select(x => x.Text).LastOrDefault();
//             var remoteExists = !string.IsNullOrWhiteSpace(remoteLine);
//             string? remoteSha = null;
//             if (remoteExists)
//                 remoteSha = remoteLine?.Split('\t')[0];
//
//             // current HEAD
//             var headSha = RunGitSingle("rev-parse HEAD");
//
//             if (localExists)
//             {
//                 if (localSha == headSha)
//                 {
//                     Serilog.Log.Information("Local tag {Tag} already exists and points at HEAD. Skipping creation.",
//                         tag);
//                 }
//                 else
//                 {
//                     Serilog.Log.Warning(
//                         "Local tag {Tag} already exists and points at {localSha}. Refusing to overwrite.", tag,
//                         localSha);
//                 }
//             }
//             else
//             {
//                 // create annotated tag
//                 Serilog.Log.Information("Creating local tag {Tag}", tag);
//                 ProcessTasks.StartProcess("git", $"tag -a {tag} -m \"Release {tag}\"")
//                     .AssertZeroExitCode();
//             }
//
//             // decide whether to push
//             if (remoteExists)
//             {
//                 if (remoteSha == headSha)
//                 {
//                     Serilog.Log.Information("Remote tag {Tag} already exists and matches HEAD. Skipping push.",
//                         tag);
//                 }
//                 else
//                 {
//                     Serilog.Log.Warning(
//                         "Remote tag {Tag} already exists and points at {RemoteSha}. Refusing to push. Resolve manually.",
//                         tag, remoteSha);
//                 }
//             }
//             else
//             {
//                 Serilog.Log.Information("Pushing tag {Tag} to origin", tag);
//                 var result = ProcessTasks.StartProcess("git", $"push --quiet origin {tag}");
//                 result.WaitForExit();
//                 if (result.ExitCode != 0)
//                 {
//                     throw new Exception($"Failed to push tag {tag}");
//                 }
//
//                 Serilog.Log.Information("Tag {Tag} pushed", tag);
//             }
//         });
//
// }