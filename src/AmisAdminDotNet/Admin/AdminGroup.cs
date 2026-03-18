using AmisAdminDotNet.AmisComponents;

namespace AmisAdminDotNet.Admin;

/// <summary>
/// A named group of <see cref="RouterAdmin"/> instances, rendered as a nested
/// <see cref="Tabs"/> within the parent <see cref="AdminApp"/>.
/// Mirrors Python's <c>AdminGroup</c>.
///
/// Usage:
/// <code>
/// var group = adminApp.CreateGroup("Settings");
/// group.Add(userSettingsAdmin);
/// group.Add(systemSettingsAdmin);
/// </code>
/// </summary>
public sealed class AdminGroup
{
    private readonly List<RouterAdmin> _admins = [];
    private readonly AdminApp _app;

    /// <summary>Display name used as the group tab label.</summary>
    public string Name { get; }

    /// <summary>Icon CSS class shown on the group tab (e.g. <c>"fa fa-cog"</c>).</summary>
    public string? Icon { get; init; }

    /// <summary>Sort priority (descending). Higher values appear first.</summary>
    public int Sort { get; init; }

    /// <summary>Admins registered in this group, in registration order.</summary>
    public IReadOnlyList<RouterAdmin> Admins => _admins;

    internal AdminGroup(string name, AdminApp app)
    {
        Name = name;
        _app = app;
    }

    /// <summary>
    /// Adds an admin to this group, linking it to the parent <see cref="AdminApp"/>.
    /// </summary>
    public void Add(RouterAdmin admin)
    {
        admin.App = _app;
        _admins.Add(admin);
    }

    /// <summary>Builds an amis <see cref="Tabs"/> for this group's admins.</summary>
    public Tabs BuildTabsSchema() => new()
    {
        TabList = _admins
            .OrderByDescending(a => a.PageSchema.IsDefaultPage)
            .ThenByDescending(a => a.PageSchema.Sort)
            .Select(a => new Tab { Title = a.PageSchema.Label, Body = a.BuildPageSchema() })
            .ToList()
    };

    /// <summary>Mounts all admins' routes on the web application.</summary>
    public void Mount(WebApplication app)
    {
        foreach (var admin in _admins)
            admin.RegisterRoutes(app);
    }
}
