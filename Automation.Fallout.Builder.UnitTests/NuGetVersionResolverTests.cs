using Automation.Fallout.Builder.Services;

namespace Automation.Fallout.Builder.UnitTests;

public class NuGetVersionResolverTests
{
    /// <summary>
    /// 'dotnet package search Automation.Fallout.Components --exact-match --format json' against the
    /// AFTR feeds: every configured source answers, and only the one hosting the package has hits.
    /// </summary>
    private const string ExactMatchResponse = """
                                              {
                                                "version": 2,
                                                "problems": [],
                                                "searchResult": [
                                                  { "sourceName": "AFTR Prerelease", "packages": [] },
                                                  {
                                                    "sourceName": "AFTR Production",
                                                    "packages": [
                                                      { "id": "Automation.Fallout.Components", "version": "1.0.2" },
                                                      { "id": "Automation.Fallout.Components", "version": "1.0.1" },
                                                      { "id": "Automation.Fallout.Components", "version": "0.0.1" }
                                                    ]
                                                  },
                                                  { "sourceName": "nuget.org", "packages": [] }
                                                ]
                                              }
                                              """;

    [Test]
    public async Task SelectLatestStableVersion_ExactMatchResponse_TakesTheHighestVersion()
    {
        var version = NuGetVersionResolver.SelectLatestStableVersion(ExactMatchResponse);

        await Assert.That(version).IsEqualTo("1.0.2");
    }

    [Test]
    public async Task SelectLatestStableVersion_VersionsOutOfOrder_StillTakesTheHighest()
    {
        var json = """
                   {
                     "searchResult": [
                       {
                         "sourceName": "AFTR Production",
                         "packages": [
                           { "id": "Automation.Fallout.Components", "version": "1.0.9" },
                           { "id": "Automation.Fallout.Components", "version": "1.0.10" }
                         ]
                       }
                     ]
                   }
                   """;

        var version = NuGetVersionResolver.SelectLatestStableVersion(json);

        await Assert.That(version).IsEqualTo("1.0.10");
    }

    [Test]
    public async Task SelectLatestStableVersion_PrereleaseOnTheFeed_IsIgnored()
    {
        var json = """
                   {
                     "searchResult": [
                       {
                         "sourceName": "AFTR Prerelease",
                         "packages": [
                           { "id": "Automation.Fallout.Components", "version": "1.0.3-beta.2" }
                         ]
                       },
                       {
                         "sourceName": "AFTR Production",
                         "packages": [
                           { "id": "Automation.Fallout.Components", "version": "1.0.2" }
                         ]
                       }
                     ]
                   }
                   """;

        var version = NuGetVersionResolver.SelectLatestStableVersion(json);

        await Assert.That(version).IsEqualTo("1.0.2");
    }

    [Test]
    public async Task SelectLatestStableVersion_PlainSearchResponse_ReadsLatestVersion()
    {
        var json = """
                   {
                     "searchResult": [
                       {
                         "sourceName": "AFTR Production",
                         "packages": [
                           { "id": "Automation.Fallout.Components", "latestVersion": "1.0.2", "totalDownloads": 12 }
                         ]
                       }
                     ]
                   }
                   """;

        var version = NuGetVersionResolver.SelectLatestStableVersion(json);

        await Assert.That(version).IsEqualTo("1.0.2");
    }

    [Test]
    public async Task SelectLatestStableVersion_NoSourceHasThePackage_ReturnsNull()
    {
        var json = """
                   {
                     "version": 2,
                     "problems": [],
                     "searchResult": [
                       { "sourceName": "AFTR Production", "packages": [] }
                     ]
                   }
                   """;

        var version = NuGetVersionResolver.SelectLatestStableVersion(json);

        await Assert.That(version).IsNull();
    }

    [Test]
    public async Task SelectLatestStableVersion_ResponseWithoutSearchResult_ReturnsNull()
    {
        var version = NuGetVersionResolver.SelectLatestStableVersion("""{ "version": 2 }""");

        await Assert.That(version).IsNull();
    }
}
