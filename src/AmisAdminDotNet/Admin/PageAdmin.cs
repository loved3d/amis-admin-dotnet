using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Services;
using Microsoft.AspNetCore.Http;

namespace AmisAdminDotNet.Admin;

/// <summary>
/// An admin that renders an arbitrary amis Page schema, without any CRUD backing.
/// Mirrors Python's <c>PageAdmin</c>.
/// Subclasses override <see cref="GetPage"/> to return a custom <see cref="Page"/>.
/// </summary>
public abstract class PageAdmin : RouterAdmin
{
    /// <summary>
    /// Returns the amis <see cref="Page"/> schema for this admin.
    /// Base implementation returns an empty page titled with <see cref="RouterAdmin.Label"/>.
    /// </summary>
    public virtual Page GetPage() => new() { Title = Label };

    /// <inheritdoc/>
    public override Page BuildPageSchema() => GetPage();

    /// <summary>
    /// Registers GET <c>{RouterPrefix}/schema</c> — returns the page schema JSON.
    /// Maps to Python <c>PageAdmin.register_router()</c>.
    /// </summary>
    public override void RegisterRoutes(WebApplication app)
    {
        app.MapGet(RouterPrefix + "/schema", async (HttpContext ctx) =>
        {
            if (!await HasPagePermissionAsync(ctx))
                return Results.Json(AdminApiResponse.Fail("Unauthorized"), statusCode: 401);
            return Results.Json(AdminApiResponse.Ok(BuildPageSchema()));
        });
    }
}
