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
    /// Registers the admin schema endpoint and mounts every <see cref="AdminApp"/>'s
    /// CRUD routes on the given <see cref="WebApplication"/>.
    /// Maps to Python <c>AdminSite.mount_app(app)</c>.
    /// </summary>
    public void MountApp(WebApplication app)
    {
        // Schema endpoint — serves the full amis JSON page schema
        app.MapGet(Settings.AdminPath + "/schema", () =>
            Results.Json(BuildPageSchema(), AmisJsonOptions.Default));

        // Register routes for every app group
        foreach (var adminApp in _apps)
            adminApp.Mount(app);
    }

    /// <summary>
    /// Builds the root amis <see cref="Page"/> schema for the entire admin site,
    /// assembling one tab per registered <see cref="AdminApp"/>.
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
            Title = "Admin",
            Body  = tabs
        };
    }
}
