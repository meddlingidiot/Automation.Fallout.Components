using Fallout.Common;

namespace Automation.Fallout.Components.Parameters;

public interface IHasVelopack: IFalloutBuild
{
    [Parameter] string VelopackProjectName => TryGetValue(() => VelopackProjectName);
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
    [Parameter] string AzureBlobSasTokenLocal => TryGetValue(() => AzureBlobSasTokenLocal) ?? 
                                                 "sp=racwdl&st=2025-11-15T07:03:42Z&se=2125-11-15T15:18:42Z&spr=https&sv=2024-11-04&sr=c&sig=obovzU6WgCnQAMioHdJkG2Pxpq8UP2tqb7CakPAfCU8%3D";
    [Parameter] string AzureBlobTimeout => TryGetValue(() => AzureBlobTimeout) ?? 
                                           "30";
    [Parameter] int KeepMaxReleases => TryGetValue<int?>(() => KeepMaxReleases) ?? 3;
}