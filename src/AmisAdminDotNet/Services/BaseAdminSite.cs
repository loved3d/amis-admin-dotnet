using AmisAdminDotNet.AmisComponents;

namespace AmisAdminDotNet.Services;

/// <summary>
/// Abstract base class for an amis admin site, equivalent to Python's
/// <c>AdminSite</c> class in <c>fastapi_amis_admin/admin/site.py</c>.
///
/// <para>
/// Concrete sub-classes override <see cref="BuildPageSchema"/> to return the root
/// <see cref="Page"/> schema and <see cref="RegisterRoutes"/> to mount their API
/// endpoints onto a <see cref="WebApplication"/> — mirroring FastAPI's
/// <c>app.include_router()</c> pattern.
/// </para>
///
/// <para>
/// Dependency injection is handled by the ASP.NET Core DI container (the
/// <see cref="IServiceProvider"/> passed in the constructor), which replaces
/// SQLAlchemy async session injection used in the Python version.
/// </para>
/// </summary>
public abstract class BaseAdminSite
{
    /// <summary>Runtime settings (database URL, CORS, admin path, etc.).</summary>
    public AdminSiteSettings Settings { get; }

    /// <summary>Application-level DI service provider.</summary>
    protected IServiceProvider Services { get; }

    protected BaseAdminSite(AdminSiteSettings settings, IServiceProvider services)
    {
        Settings = settings;
        Services = services;
    }

    /// <summary>
    /// Returns the root amis <see cref="Page"/> schema for this admin site.
    /// Override to customise the page title, body components, and tab layout.
    /// Maps to Python <c>AdminSite.get_page()</c>.
    /// </summary>
    public abstract Page BuildPageSchema();

    /// <summary>
    /// Registers all HTTP routes (schema endpoint + CRUD APIs) on the given
    /// <see cref="WebApplication"/>. Maps to Python <c>AdminSite.mount_app()</c>.
    /// </summary>
    public abstract void RegisterRoutes(WebApplication app);
}
