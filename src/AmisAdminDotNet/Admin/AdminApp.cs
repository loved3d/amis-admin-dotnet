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
    private readonly List<AdminGroup> _groups  = [];

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
    /// Creates a named <see cref="AdminGroup"/> within this app.
    /// Maps to Python's nested <c>AdminGroup</c> registration.
    /// </summary>
    public AdminGroup CreateGroup(string name, string? icon = null, int sort = 0)
    {
        var group = new AdminGroup(name, this) { Icon = icon, Sort = sort };
        _groups.Add(group);
        return group;
    }

    /// <summary>
    /// Registers all HTTP routes for every admin in <see cref="Admins"/> (and all
    /// groups) on the given <see cref="WebApplication"/>. Mirrors Python's
    /// <c>app.include_router(admin.router)</c> calls.
    /// </summary>
    public void Mount(WebApplication app)
    {
        foreach (var admin in _admins)
            admin.RegisterRoutes(app);
        foreach (var group in _groups)
            group.Mount(app);
    }

    /// <summary>
    /// Builds an amis <see cref="Tabs"/> node containing one tab per registered admin
    /// plus one nested <see cref="Tabs"/> per <see cref="AdminGroup"/>.
    /// Direct admins are ordered by <see cref="PageSchemaOptions.Sort"/> (descending);
    /// admins with <see cref="PageSchemaOptions.IsDefaultPage"/> set to <c>true</c> are
    /// always placed first. Groups are appended after, ordered by
    /// <see cref="AdminGroup.Sort"/> descending.
    /// </summary>
    public Tabs BuildTabsSchema()
    {
        var tabs = new List<Tab>();

        // Direct admins (not in any group)
        var sortedAdmins = _admins
            .OrderByDescending(a => a.PageSchema.IsDefaultPage)
            .ThenByDescending(a => a.PageSchema.Sort);
        tabs.AddRange(sortedAdmins.Select(a =>
            new Tab { Title = a.PageSchema.Label, Body = a.BuildPageSchema() }));

        // Groups — each renders as a nested Tabs
        var sortedGroups = _groups.OrderByDescending(g => g.Sort);
        tabs.AddRange(sortedGroups.Select(g =>
            new Tab { Title = g.Name, Body = g.BuildTabsSchema() }));

        return new Tabs { TabList = tabs };
    }
}
