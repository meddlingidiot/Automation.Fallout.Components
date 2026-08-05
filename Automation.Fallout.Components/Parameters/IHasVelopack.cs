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
                                                "installers";
    [Parameter] string AzureBlobAccount => TryGetValue(() => AzureBlobAccount) ?? 
                                           "staftrinstallers";
    [Parameter] string AzureBlobEndpoint => TryGetValue(() => AzureBlobEndpoint) ?? 
                                            "https://staftrinstallers.blob.core.windows.net";
    [Secret] string AzureBlobSasToken => TryGetValue(() => AzureBlobSasToken);

    // Supply the token with --azure-blob-sas-token-local or the AZURE_BLOB_SAS_TOKEN_LOCAL
    // environment variable. There is deliberately no default: a SAS token is a live credential and
    // does not belong in source. When it is missing the Velopack targets log a warning and skip the
    // download/upload rather than failing the build.
    [Parameter] string AzureBlobSasTokenLocal => TryGetValue(() => AzureBlobSasTokenLocal) ??
                                                 Environment.GetEnvironmentVariable("AZURE_BLOB_SAS_TOKEN_LOCAL") ??
                                                 "";
    [Parameter] string AzureBlobTimeout => TryGetValue(() => AzureBlobTimeout) ?? 
                                           "30";
    [Parameter] int KeepMaxReleases => TryGetValue<int?>(() => KeepMaxReleases) ?? 3;
}