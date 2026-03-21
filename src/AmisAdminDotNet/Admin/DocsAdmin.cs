using AmisAdminDotNet.Services;

namespace AmisAdminDotNet.Admin;

/// <summary>
/// Built-in <see cref="IframeAdmin"/> that embeds the Swagger/OpenAPI interactive
/// documentation page in an iframe.
/// Mirrors Python's <c>DocsAdmin</c> from <c>fastapi_amis_admin/admin/site.py</c>.
/// </summary>
public class DocsAdmin : IframeAdmin
{
    /// <inheritdoc/>
    public override string RouterPath => "docs-admin";

    /// <inheritdoc/>
    public override string Label => "API Docs";

    /// <summary>
    /// URL of the Swagger UI page. Defaults to <c>"/swagger"</c>.
    /// Override to point at a different OpenAPI UI host.
    /// </summary>
    public override string Src => "/swagger";

    /// <inheritdoc/>
    public override PageSchemaOptions PageSchema => new()
    {
        Label = "API Docs",
        Icon  = "fa fa-book",
        Sort  = 10
    };
}

/// <summary>
/// Built-in <see cref="IframeAdmin"/> that embeds the ReDoc API documentation page.
/// Mirrors Python's <c>ReDocsAdmin</c> from <c>fastapi_amis_admin/admin/site.py</c>.
/// </summary>
public class ReDocsAdmin : IframeAdmin
{
    /// <inheritdoc/>
    public override string RouterPath => "redocs-admin";

    /// <inheritdoc/>
    public override string Label => "ReDoc";

    /// <summary>
    /// URL of the ReDoc page. Defaults to <c>"/redoc"</c>.
    /// Override to point at a different ReDoc host.
    /// </summary>
    public override string Src => "/redoc";

    /// <inheritdoc/>
    public override PageSchemaOptions PageSchema => new()
    {
        Label = "ReDoc",
        Icon  = "fa fa-file-alt",
        Sort  = 9
    };
}

/// <summary>
/// Built-in <see cref="AdminApp"/> that groups API documentation admins
/// (<see cref="DocsAdmin"/> and <see cref="ReDocsAdmin"/>) under a single navigation
/// entry.
/// Mirrors Python's <c>APIDocsApp</c> from <c>fastapi_amis_admin/admin/site.py</c>.
/// <para>
/// Use <see cref="AdminSite.RegisterBuiltinAdmins"/> to auto-register this, or add
/// it explicitly:
/// <code>
/// var docsApp = site.CreateDocsApp();
/// site.MountApp(webApp);
/// </code>
/// </para>
/// </summary>
public class APIDocsApp : AdminApp
{
    /// <summary>
    /// Initialises an <see cref="APIDocsApp"/> and pre-registers
    /// <see cref="DocsAdmin"/> and <see cref="ReDocsAdmin"/> as its children.
    /// </summary>
    public APIDocsApp(string mountPrefix, AdminSiteSettings settings, IServiceProvider services)
        : base("API Docs", mountPrefix, settings, services)
    {
        AddAdmin(new DocsAdmin());
        AddAdmin(new ReDocsAdmin());
    }
}
