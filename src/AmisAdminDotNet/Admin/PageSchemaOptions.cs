namespace AmisAdminDotNet.Admin;

/// <summary>
/// Navigation metadata for an admin page entry in the tab schema.
/// Controls how the admin appears in the generated <see cref="AmisAdminDotNet.AmisComponents.Tabs"/> structure.
/// Mirrors Python's <c>PageSchema</c> from <c>fastapi_amis_admin</c>.
/// </summary>
public record PageSchemaOptions
{
    /// <summary>Tab label displayed in the navigation.</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// Icon CSS class applied to the tab (e.g. <c>"fa fa-users"</c>).
    /// Maps to Python <c>PageSchema.icon</c>.
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// Sort priority (descending). Admins with a higher <see cref="Sort"/> value appear first.
    /// Maps to Python <c>PageSchema.sort</c>.
    /// </summary>
    public int Sort { get; init; } = 0;

    /// <summary>
    /// When <c>true</c>, this admin is placed at the very beginning of the tab list,
    /// ahead of all sort-based ordering.
    /// Maps to Python <c>PageSchema.is_default_page</c>.
    /// </summary>
    public bool IsDefaultPage { get; init; } = false;

    /// <summary>
    /// When set, this admin renders as a hyperlink in the menu rather than a page tab.
    /// Maps to Python <c>PageSchema.link</c>.
    /// </summary>
    public string? Link { get; init; }
}
