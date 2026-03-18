using AmisAdminDotNet.Services;

namespace AmisAdminDotNet.Tests;

public sealed class AdminHostPageTests
{
    [Fact]
    public void Html_ReferencesOfficialAmisSdkAndSchemaEndpoint()
    {
        Assert.Contains("https://unpkg.com/amis@6.11.0/sdk/sdk.js", AdminHostPage.Html);
        Assert.Contains("amisRequire('amis/embed')", AdminHostPage.Html);
        Assert.Contains("/api/admin/schema", AdminHostPage.Html);
        Assert.Contains("Official amis SDK could not be loaded", AdminHostPage.Html);
    }
}
