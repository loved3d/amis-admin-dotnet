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
        Assert.Contains("/admin/site.css", AdminHostPage.Html);
        Assert.Contains("Official amis SDK could not be loaded", AdminHostPage.Html);
    }

    [Fact]
    public void RenderHtml_UsesConfiguredCdnAndSchemaPath()
    {
        var html = AdminHostPage.RenderHtml(new AppSettings
        {
            AmisCdn = "https://cdn.example.com/amis/sdk",
            SchemaApiPath = "/custom/schema"
        });

        Assert.Contains("https://cdn.example.com/amis/sdk/sdk.js", html);
        Assert.Contains("https://cdn.example.com/amis/sdk/sdk.css", html);
        Assert.Contains("fetch('/custom/schema')", html);
    }
}
