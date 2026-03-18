namespace AmisAdminDotNet.Crud;

/// <summary>
/// Defines the contract for components that register HTTP routes on an ASP.NET Core
/// <see cref="WebApplication"/>. This is the C# equivalent of Python's
/// <c>RouterMixin</c> from <c>fastapi_amis_admin/admin/admin.py</c>.
///
/// <para>
/// Python pattern:
/// <code>
///   @router.add_api_route("/items", get_items, methods=["GET"])
/// </code>
/// C# equivalent:
/// <code>
///   app.MapGet(RouterPrefix + "/items", GetItems);
/// </code>
/// </para>
/// </summary>
public interface IRouterMixin
{
    /// <summary>
    /// URL path prefix for all routes registered by this mixin.
    /// Maps to Python's <c>router.prefix</c>.
    /// </summary>
    string RouterPrefix { get; }

    /// <summary>
    /// Registers all HTTP routes on the given <see cref="WebApplication"/>.
    /// Maps to Python's <c>app.include_router(router)</c> pattern.
    /// </summary>
    /// <param name="app">The ASP.NET Core web application to register routes on.</param>
    void RegisterRoutes(WebApplication app);
}
