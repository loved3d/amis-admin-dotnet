using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Crud;
using AmisAdminDotNet.Services;
using Microsoft.AspNetCore.Http;

namespace AmisAdminDotNet.Admin;

/// <summary>
/// An admin that renders a single standalone form page and handles its submission.
/// Mirrors Python's <c>FormAdmin</c> from <c>fastapi_amis_admin/admin/admin.py</c>.
///
/// <para>
/// Subclasses must:
/// <list type="bullet">
///   <item>Provide a concrete <typeparamref name="TSchema"/> type whose properties
///         become the form fields (via <see cref="TableModelParser"/>).</item>
///   <item>Implement <see cref="HandleAsync"/> to process the posted data.</item>
///   <item>Set <see cref="RouterAdmin.RouterPath"/> to expose the form under the
///         correct URL prefix.</item>
/// </list>
/// </para>
/// </summary>
/// <typeparam name="TSchema">
/// A plain C# class whose public, settable properties model the form fields.
/// Must have a parameterless constructor.
/// </typeparam>
public abstract class FormAdmin<TSchema> : RouterAdmin
    where TSchema : class, new()
{
    /// <summary>
    /// URL path segment appended to <see cref="RouterAdmin.RouterPrefix"/> for the
    /// form-submission endpoint. Defaults to <c>"submit"</c>.
    /// Maps to Python <c>FormAdmin.schema_path</c>.
    /// </summary>
    public virtual string FormPath => "submit";

    /// <summary>
    /// Builds the amis <see cref="Page"/> schema that contains the generated form.
    /// The form's <c>api</c> is set to <c>POST {RouterPrefix}/{FormPath}</c> so that
    /// the amis front-end posts to the correct handler.
    /// Maps to Python <c>FormAdmin.get_page()</c>.
    /// </summary>
    public override Page BuildPageSchema()
    {
        var fields = TableModelParser.ParseFormFields(typeof(TSchema));

        return new Page
        {
            Title = Label,
            Body  = new Form
            {
                Api  = $"post:{RouterPrefix}/{FormPath}",
                Body = fields.ToList()
            }
        };
    }

    /// <summary>
    /// Processes the submitted form data and returns a result object that is
    /// serialised into the amis <c>{ "status": 0, "data": ... }</c> response.
    /// Subclasses must implement this.
    /// Maps to Python <c>FormAdmin.handle(request, data)</c>.
    /// </summary>
    /// <param name="data">The deserialized form payload.</param>
    /// <param name="context">The current HTTP context.</param>
    public abstract Task<object> HandleAsync(TSchema data, HttpContext context);

    /// <summary>
    /// Registers two routes on the <see cref="WebApplication"/>:
    /// <list type="bullet">
    ///   <item>
    ///     <c>GET {RouterPrefix}/schema</c> — returns the full amis page schema JSON.
    ///   </item>
    ///   <item>
    ///     <c>POST {RouterPrefix}/{FormPath}</c> — deserialises the request body into
    ///     <typeparamref name="TSchema"/>, calls <see cref="HandleAsync"/>, and returns a
    ///     success response.
    ///   </item>
    /// </list>
    /// Both routes are protected by <see cref="BaseAdmin.HasPagePermission"/>.
    /// </summary>
    public override void RegisterRoutes(WebApplication app)
    {
        var prefix = RouterPrefix;

        app.MapGet($"{prefix}/schema", (HttpContext ctx) =>
        {
            if (!HasPagePermission(ctx))
                return Results.Json(AdminApiResponse.Fail("Unauthorized"), statusCode: 401);

            return Results.Json(AdminApiResponse.Ok(BuildPageSchema()));
        });

        app.MapPost($"{prefix}/{FormPath}", async (TSchema data, HttpContext ctx) =>
        {
            if (!HasPagePermission(ctx))
                return Results.Json(AdminApiResponse.Fail("Unauthorized"), statusCode: 401);

            var result = await HandleAsync(data, ctx);
            return Results.Json(AdminApiResponse.Ok(result));
        });
    }
}
