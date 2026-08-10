using Fallout.Common;

namespace Automation.Fallout.Components.Parameters;

public interface IHasVelopack: IFalloutBuild
{
    [Parameter] string VelopackProjectName => TryGetValue(() => VelopackProjectName);
    [Parameter] string VelopackPackTitle => TryGetValue(() => VelopackPackTitle) ??
                                            "";
    [Parameter] string VelopackIconPath => TryGetValue(() => VelopackIconPath) ??
                                           "";
    [Parameter] string VelopackChannel => TryGetValue(() => VelopackChannel) ?? 
                                          "win";
    [Parameter] string VelopackBlobContainer => TryGetValue(() => VelopackBlobContainer) ??
                                                DefaultBlobContainer;

    /// <summary>The container Azure DevOps releases go to.</summary>
    const string AzureDevOpsBlobContainer = "staftrinstallers";

    /// <summary>The container GitHub releases go to, and what a local run uses.</summary>
    const string GitHubBlobContainer = "installers";

    // The two containers hold separate SAS tokens, so the container is not interchangeable: a build
    // pointed at the wrong one fails the upload with 403 rather than writing somewhere harmless.
    // Detected from the host rather than configured per repository, because every repository that
    // gets migrated would otherwise have to remember to pass --velopack-blob-container.
    private static string DefaultBlobContainer =>
        string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD"))
            ? GitHubBlobContainer
            : AzureDevOpsBlobContainer;
    [Parameter] string AzureBlobAccount => TryGetValue(() => AzureBlobAccount) ?? 
                                           "staftrinstallers";
    [Parameter] string AzureBlobEndpoint => TryGetValue(() => AzureBlobEndpoint) ?? 
                                            "https://staftrinstallers.blob.core.windows.net";
    [Parameter, Secret] string AzureBlobSasToken => TryGetValue(() => AzureBlobSasToken);

    // The SAS token the Velopack targets actually use. There is deliberately no default: a SAS
    // token is a live credential and does not belong in source. When it is missing the Velopack
    // targets log a warning and skip the download/upload rather than failing the build.
    //
    // Several sources are checked because the CI definitions in this org do not agree on one:
    // pipelines pass --azure-blob-sas-token, Azure DevOps maps AZUREBLOBSASTOKEN, and GitHub
    // Actions maps AZURE_BLOB_SAS_TOKEN. Before this fallback existed only AzureBlobSasTokenLocal
    // was read, so every one of those wirings resolved to empty and the upload silently skipped.
    [Parameter] string AzureBlobSasTokenLocal =>
        Coalesce(
            TryGetValue(() => AzureBlobSasTokenLocal),
            Environment.GetEnvironmentVariable("AZURE_BLOB_SAS_TOKEN_LOCAL"),
            AzureBlobSasToken,
            Environment.GetEnvironmentVariable("AZUREBLOBSASTOKEN"),
            Environment.GetEnvironmentVariable("AZURE_BLOB_SAS_TOKEN"));

    /// <summary>
    /// First non-blank value, or "". Plain ?? is not enough here: an unset Azure DevOps macro
    /// arrives as the literal string "$(SomeName)" and an unmapped one as "", and both are
    /// non-null, so they would shadow a later source that actually holds the token.
    /// </summary>
    private static string Coalesce(params string?[] values)
    {
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            // An unexpanded Azure DevOps macro, e.g. "$(AzureBlobToken)" when the variable group
            // has no such variable. Treat it as absent rather than handing it to vpk as a token.
            if (value.StartsWith("$(", StringComparison.Ordinal) && value.EndsWith(")", StringComparison.Ordinal))
                continue;

            return value;
        }

        return "";
    }
    [Parameter] string AzureBlobTimeout => TryGetValue(() => AzureBlobTimeout) ?? 
                                           "30";
    [Parameter] int KeepMaxReleases => TryGetValue<int?>(() => KeepMaxReleases) ?? 3;
}