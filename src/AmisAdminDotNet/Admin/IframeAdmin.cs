using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Services;
using Microsoft.AspNetCore.Http;

namespace AmisAdminDotNet.Admin;

/// <summary>
/// An admin whose page body is an amis <c>iframe</c> component pointing at <see cref="Src"/>.
/// Mirrors Python's <c>IframeAdmin</c>.
/// </summary>
public abstract class IframeAdmin : RouterAdmin
{
    /// <summary>The URL to embed in the iframe.</summary>
    public abstract string Src { get; }

    /// <inheritdoc/>
    public override Page BuildPageSchema() => new()
    {
        Title = Label,
        Body  = new Iframe { Src = Src }
    };

    /// <summary>
    /// Registers GET <c>{RouterPrefix}/schema</c> — returns the page schema JSON.
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
