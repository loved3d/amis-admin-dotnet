using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Crud;
using AmisAdminDotNet.Services;

namespace AmisAdminDotNet.Admin;

/// <summary>
/// Abstract base for all admin panel classes.
/// Mirrors Python's <c>BaseAdmin</c> from <c>fastapi_amis_admin/admin/admin.py</c>.
///
/// <para>
/// Every admin belongs to an <see cref="AdminApp"/> (set by <see cref="AdminApp.RegisterAdmin{TAdmin}"/>),
/// exposes a <see cref="RouterPath"/> segment used to compose its URL prefix, and
/// optionally restricts access via <see cref="HasPagePermission"/>.
/// </para>
/// </summary>
public abstract class BaseAdmin
{
    /// <summary>
    /// The <see cref="AdminApp"/> this admin belongs to.
    /// Set automatically when the admin is registered via
    /// <see cref="AdminApp.RegisterAdmin{TAdmin}"/>.
    /// Maps to Python <c>self.app</c>.
    /// </summary>
    public AdminApp App { get; set; } = null!;

    /// <summary>
    /// URL path segment for this admin's routes (e.g. <c>"users"</c>).
    /// Combined with <see cref="AdminApp.MountPrefix"/> to form the full route prefix.
    /// Maps to Python <c>router.prefix</c>.
    /// </summary>
    public abstract string RouterPath { get; }

    /// <summary>
    /// Checks whether the current request context is permitted to view this admin page.
    /// Override to add custom authorization logic.
    /// Maps to Python <c>BaseAdmin.has_page_permission()</c>.
    /// </summary>
    /// <returns><c>true</c> by default (open to all authenticated users).</returns>
    public virtual bool HasPagePermission() => true;
}

/// <summary>
/// An admin that can register its own HTTP routes on an ASP.NET Core
/// <see cref="WebApplication"/>. Combines <see cref="BaseAdmin"/> with
/// <see cref="IRouterMixin"/>.
/// Mirrors Python's <c>RouterAdmin</c> from <c>fastapi_amis_admin/admin/admin.py</c>.
/// </summary>
public abstract class RouterAdmin : BaseAdmin, IRouterMixin
{
    /// <summary>
    /// Full URL prefix for the routes of this admin, composed as
    /// <c>{App.MountPrefix}/{RouterPath}</c>.
    /// Maps to Python's <c>router.prefix</c> after <c>include_router()</c>.
    /// </summary>
    public string RouterPrefix =>
        (App?.MountPrefix.TrimEnd('/') ?? string.Empty) + "/" + RouterPath.TrimStart('/');

    /// <inheritdoc/>
    public abstract void RegisterRoutes(WebApplication app);

    /// <summary>
    /// Returns a tab label used by <see cref="AdminApp.BuildPageSchema"/>.
    /// Defaults to <see cref="BaseAdmin.RouterPath"/> when not overridden.
    /// </summary>
    public virtual string Label => RouterPath;

    /// <summary>
    /// Returns the amis <see cref="Page"/> schema for this admin's page body.
    /// Override in concrete admins (e.g. <see cref="ModelAdmin{TEntity,TKey,TDbContext}"/>)
    /// to supply a CRUD panel, form, etc.
    /// Maps to Python <c>RouterAdmin.get_page()</c>.
    /// </summary>
    public virtual Page BuildPageSchema() =>
        new() { Title = Label, Body = $"No schema defined for {GetType().Name}." };
}
