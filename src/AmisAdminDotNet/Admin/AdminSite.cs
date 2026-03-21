using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Services;

namespace AmisAdminDotNet.Admin;

/// <summary>
/// Top-level admin site that owns one or more <see cref="AdminApp"/> groups, mounts
/// all their HTTP routes, and exposes the amis page schema for the entire admin UI.
/// Mirrors Python's <c>AdminSite</c> from <c>fastapi_amis_admin/admin/site.py</c>.
///
/// <para>
/// Python usage:
/// <code>
///   site = AdminSite(settings=Settings(database_url="sqlite+aiosqlite:///admin.db"))
///   app  = site.create_admin_app(name="Users")
///   app.register_admin(UserAdmin)
///   site.mount_app(app)
/// </code>
/// C# equivalent:
/// <code>
///   var site    = new AdminSite(settings, services);
///   var userApp = site.CreateApp("Users");
///   userApp.RegisterAdmin&lt;UserAdmin&gt;();
///   site.MountApp(webApp);
/// </code>
/// </para>
/// </summary>
public class AdminSite
{
    private readonly IServiceProvider _services;
    private readonly List<AdminApp> _apps = [];
    private AdminApp? _builtinApp;

    /// <summary>
    /// Runtime settings for this admin site (database URL, admin path, CORS origins, etc.).
    /// Maps to Python <c>Settings</c> nested inside <c>AdminSite</c>.
    /// </summary>
    public AdminSiteSettings Settings { get; }

    public AdminSite(AdminSiteSettings settings, IServiceProvider services)
    {
        Settings  = settings;
        _services = services;
    }

    /// <summary>
    /// Creates a new <see cref="AdminApp"/> group, links it to this site, and returns it
    /// so that admins can be registered on it.
    /// Maps to Python <c>AdminSite.create_admin_app()</c>.
    /// </summary>
    /// <param name="name">Display label for the app group.</param>
    public AdminApp CreateApp(string name)
    {
        var adminApp = new AdminApp(name, Settings.AdminPath, Settings, _services);
        _apps.Add(adminApp);
        return adminApp;
    }

    /// <summary>
    /// Registers built-in admin pages — <see cref="HomeAdmin"/>, <see cref="FileAdmin"/>,
    /// and <see cref="APIDocsApp"/> — in a dedicated <c>"Built-in"</c> app group.
    /// Mirrors Python <c>AdminSite.__init__</c> which calls
    /// <c>self.register_admin(HomeAdmin, APIDocsApp, FileAdmin)</c>.
    ///
    /// <para>
    /// This method is idempotent: calling it more than once has no effect after the
    /// first call.
    /// </para>
    /// </summary>
    /// <param name="includeApiDocs">
    /// When <c>true</c> (default), the <see cref="APIDocsApp"/> group with
    /// <see cref="DocsAdmin"/> and <see cref="ReDocsAdmin"/> is also registered.
    /// Set to <c>false</c> when Swagger is not enabled.
    /// </param>
    public void RegisterBuiltinAdmins(bool includeApiDocs = true)
    {
        if (_builtinApp is not null)
            return;

        _builtinApp = new AdminApp("Built-in", Settings.AdminPath, Settings, _services);

        // Home dashboard — always first
        _builtinApp.AddAdmin(new HomeAdmin(Settings));

        // File upload
        _builtinApp.AddAdmin(new FileAdmin());

        // API docs (optional)
        if (includeApiDocs)
        {
            var docsApp = new APIDocsApp(Settings.AdminPath, Settings, _services);
            // Flatten docs admins into the built-in app directly so they appear as tabs
            foreach (var docsAdmin in docsApp.Admins)
                _builtinApp.AddAdmin(docsAdmin);
        }

        // Prepend so built-in admins appear before user-registered apps
        _apps.Insert(0, _builtinApp);
    }

    /// <summary>
    /// Registers the admin schema endpoint and mounts every <see cref="AdminApp"/>'s
    /// CRUD routes on the given <see cref="WebApplication"/>.
    /// Optionally serves the amis HTML host page at <c><see cref="AdminSiteSettings.AdminPath"/></c>.
    /// Maps to Python <c>AdminSite.mount_app(app)</c>.
    /// </summary>
    /// <param name="app">The ASP.NET Core <see cref="WebApplication"/>.</param>
    /// <param name="serveHostPage">
    /// When <c>true</c> (default), registers a <c>GET {AdminPath}</c> endpoint that
    /// returns the amis HTML host page. The host page schema URL is set to
    /// <c>{AdminPath}/schema</c> automatically.
    /// </param>
    /// <param name="appSettings">
    /// Optional <see cref="AppSettings"/> for the HTML host page (CDN URL, etc.).
    /// When <c>null</c>, the method tries to resolve <see cref="AppSettings"/> from the
    /// DI container, falling back to default settings.
    /// </param>
    public void MountApp(WebApplication app, bool serveHostPage = false,
        AppSettings? appSettings = null)
    {
        // Schema endpoint — serves the full amis JSON page schema
        app.MapGet(Settings.AdminPath + "/schema", () =>
            Results.Json(BuildPageSchema(), AmisJsonOptions.Default));

        // Optionally serve the HTML admin host page
        if (serveHostPage)
        {
            var settings = appSettings
                ?? app.Services.GetService<AppSettings>()
                ?? new AppSettings();

            // Override schema path to point at this site's schema endpoint
            var siteSettings = new AppSettings
            {
                DatabaseUrl   = settings.DatabaseUrl,
                AdminPath     = settings.AdminPath,
                CorsOrigins   = settings.CorsOrigins,
                EnableSwagger = settings.EnableSwagger,
                AmisCdn       = settings.AmisCdn,
                SchemaApiPath = settings.AdminPath + "/schema"
            };

            app.MapGet(settings.AdminPath, () =>
                Results.Content(
                    AdminHostPage.RenderHtml(siteSettings),
                    "text/html; charset=utf-8"));
        }

        // Register routes for every app group
        foreach (var adminApp in _apps)
            adminApp.Mount(app);
    }

    /// <summary>
    /// Builds the root amis <see cref="Page"/> schema for the entire admin site,
    /// assembling one tab per registered <see cref="AdminApp"/>.
    /// The site title is taken from <see cref="AdminSiteSettings.SiteTitle"/>.
    /// Maps to Python <c>AdminSite.get_page()</c>.
    /// </summary>
    public Page BuildPageSchema()
    {
        var tabs = _apps.Count == 1
            ? (object)_apps[0].BuildTabsSchema()
            : (object)new Tabs
            {
                TabList = _apps
                    .Select(a => new Tab { Title = a.Name, Body = a.BuildTabsSchema() })
                    .ToList()
            };

        return new Page
        {
            Title = Settings.SiteTitle,
            Body  = tabs
        };
    }
}
