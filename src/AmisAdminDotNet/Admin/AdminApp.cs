using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Services;

namespace AmisAdminDotNet.Admin;

/// <summary>
/// Groups a set of <see cref="RouterAdmin"/> / <see cref="ModelAdmin{,,}"/> instances
/// under a common label and URL prefix, and can mount all of their routes.
/// Mirrors Python's <c>AdminApp</c> from <c>fastapi_amis_admin/admin/admin.py</c>.
///
/// <para>
/// Python usage:
/// <code>
///   app = AdminApp(app=site)
///   app.register_admin(UserAdmin)
/// </code>
/// C# equivalent:
/// <code>
///   var adminApp = site.CreateApp("Users");
///   adminApp.RegisterAdmin&lt;UserAdmin&gt;();
/// </code>
/// </para>
/// </summary>
public class AdminApp
{
    private readonly IServiceProvider _services;
    private readonly List<RouterAdmin> _admins = [];

    /// <summary>Display name of this app group (used in tab labels).</summary>
    public string Name { get; }

    /// <summary>
    /// Settings shared across all admins in this app.
    /// Sourced from the parent <see cref="AdminSite"/>.
    /// </summary>
    public AdminSiteSettings Settings { get; }

    /// <summary>
    /// URL prefix under which this app's admins are mounted
    /// (e.g. <c>"/admin"</c>).
    /// </summary>
    public string MountPrefix { get; }

    /// <summary>
    /// Admins registered in this app, in registration order.
    /// </summary>
    public IReadOnlyList<RouterAdmin> Admins => _admins;

    public AdminApp(string name, string mountPrefix, AdminSiteSettings settings, IServiceProvider services)
    {
        Name         = name;
        MountPrefix  = mountPrefix;
        Settings     = settings;
        _services    = services;
    }

    /// <summary>
    /// Resolves a <typeparamref name="TAdmin"/> instance from the DI container,
    /// links it to this app, and adds it to <see cref="Admins"/>.
    /// Mirrors Python's <c>AdminApp.register_admin(AdminClass)</c>.
    /// </summary>
    /// <typeparam name="TAdmin">Concrete <see cref="RouterAdmin"/> sub-type.</typeparam>
    public void RegisterAdmin<TAdmin>() where TAdmin : RouterAdmin
    {
        var admin = (TAdmin)_services.GetRequiredService(typeof(TAdmin));
        admin.App  = this;
        _admins.Add(admin);
    }

    /// <summary>
    /// Registers all HTTP routes for every admin in <see cref="Admins"/> on the
    /// given <see cref="WebApplication"/>. Mirrors Python's
    /// <c>app.include_router(admin.router)</c> calls.
    /// </summary>
    public void Mount(WebApplication app)
    {
        foreach (var admin in _admins)
            admin.RegisterRoutes(app);
    }

    /// <summary>
    /// Builds an amis <see cref="Tabs"/> node containing one tab per registered admin.
    /// Tabs are ordered by <see cref="PageSchemaOptions.Sort"/> (descending); admins
    /// with <see cref="PageSchemaOptions.IsDefaultPage"/> set to <c>true</c> are always
    /// placed first. Each tab body is produced by the admin's own <c>BuildPageSchema()</c>.
    /// </summary>
    public Tabs BuildTabsSchema()
    {
        var sorted = _admins
            .OrderByDescending(a => a.PageSchema.IsDefaultPage)
            .ThenByDescending(a => a.PageSchema.Sort)
            .ToList();

        return new Tabs
        {
            TabList = sorted
                .Select(a => new Tab { Title = a.PageSchema.Label, Body = a.BuildPageSchema() })
                .ToList()
        };
    }
}
