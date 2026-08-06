using Automation.Fallout.Builder.Services;
using static Automation.Fallout.Builder.Services.MigrationPackageVersionResolver;

namespace Automation.Fallout.Builder.UnitTests;

public class MigrationPackageVersionResolverTests
{
    [Test]
    public async Task Choose_PrefersTheFeed()
    {
        var (version, source) = Choose(feedVersion: "0.0.4", existingPin: "0.0.1", shippedDefault: "1.0.2");

        using (Assert.Multiple())
        {
            await Assert.That(version).IsEqualTo("0.0.4");
            await Assert.That(source).IsEqualTo(VersionSource.Feed);
        }
    }

    [Test]
    public async Task Choose_KeepsWhatTheProjectPinsWhenTheFeedsAreUnreachable()
    {
        // The reported bug: a GitHub Packages repository sitting on a working 0.0.4 was overwritten
        // with 1.0.2, the version this tool shipped with off the Azure DevOps feed, and stopped
        // restoring with NU1102.
        var (version, source) = Choose(feedVersion: null, existingPin: "0.0.4", shippedDefault: "1.0.2");

        using (Assert.Multiple())
        {
            await Assert.That(version).IsEqualTo("0.0.4");
            await Assert.That(source).IsEqualTo(VersionSource.ExistingPin);
        }
    }

    [Test]
    public async Task Choose_FallsBackToTheShippedDefaultOnly()
    {
        var (version, source) = Choose(feedVersion: null, existingPin: null, shippedDefault: "1.0.2");

        using (Assert.Multiple())
        {
            await Assert.That(version).IsEqualTo("1.0.2");
            await Assert.That(source).IsEqualTo(VersionSource.ShippedDefault);
        }
    }

    [Test]
    [Arguments("Response status code does not indicate success: 401 (Unauthorized).")]
    [Arguments("error: Response status code does not indicate success: 403 (Forbidden).")]
    public async Task NeedsCredentials_RecognisesARejectedFeed(string output)
    {
        await Assert.That(NuGetVersionResolver.NeedsCredentials(output)).IsTrue();
    }

    [Test]
    public async Task NeedsCredentials_IsFalseForAnOrdinaryFailure()
    {
        await Assert.That(NuGetVersionResolver.NeedsCredentials("No packages found.")).IsFalse();
    }

    [Test]
    public async Task SelectLatestStableVersion_TakesTheHighestAcrossEveryFeed()
    {
        // One block per configured source, each listing its own versions.
        var json = """
                   {
                     "searchResult": [
                       { "sourceName": "nuget.org", "packages": [] },
                       { "sourceName": "github", "packages": [
                           { "id": "Automation.Fallout.Components", "version": "0.0.3" },
                           { "id": "Automation.Fallout.Components", "version": "0.0.4" }
                       ] }
                     ]
                   }
                   """;

        await Assert.That(NuGetVersionResolver.SelectLatestStableVersion(json)).IsEqualTo("0.0.4");
    }
}
