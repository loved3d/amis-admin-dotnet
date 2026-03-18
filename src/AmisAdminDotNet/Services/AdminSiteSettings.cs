namespace AmisAdminDotNet.Services;

/// <summary>
/// Configuration settings for an <see cref="BaseAdminSite"/> instance.
/// Mirrors the Python <c>Settings</c> class used inside <c>AdminSite</c> in
/// <c>fastapi_amis_admin/admin/site.py</c>.
/// </summary>
public sealed class AdminSiteSettings
{
    /// <summary>
    /// Database connection string (EF Core format).
    /// Maps to Python <c>database_url: str</c>.
    /// </summary>
    public string DatabaseUrl { get; set; } = "Data Source=amis_admin.db";

    /// <summary>
    /// URL path prefix under which the admin UI is mounted (e.g. <c>"/admin"</c>).
    /// Maps to Python <c>site_path: str = "/admin"</c>.
    /// </summary>
    public string AdminPath { get; set; } = "/admin";

    /// <summary>
    /// Allowed CORS origins. An empty array disables CORS.
    /// Maps to Python FastAPI CORS middleware settings.
    /// </summary>
    public string[] CorsOrigins { get; set; } = [];

    /// <summary>
    /// Whether to enable the Swagger/OpenAPI documentation endpoint at <c>/swagger</c>.
    /// Maps to Python <c>openapi_url</c> in FastAPI app settings.
    /// </summary>
    public bool EnableSwagger { get; set; } = false;

    /// <summary>
    /// Display title of the admin site shown in the UI.
    /// Maps to Python <c>site_title: str</c>.
    /// </summary>
    public string SiteTitle { get; set; } = "Admin";

    /// <summary>
    /// Version string displayed in the admin UI.
    /// Maps to Python <c>site_version: str</c>.
    /// </summary>
    public string Version { get; set; } = "1.0.0";
}
