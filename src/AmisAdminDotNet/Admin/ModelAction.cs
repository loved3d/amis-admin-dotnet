using Microsoft.AspNetCore.Http;

namespace AmisAdminDotNet.Admin;

/// <summary>
/// Row-level admin action. Appears as a button in the operation column of the CRUD
/// table, alongside the built-in Edit and Delete buttons.
/// Corresponds to Python <c>ModelAction</c> from <c>fastapi_amis_admin</c>.
///
/// <para>
/// The action handler receives the row's primary key via the route parameter
/// <c>{id}</c>: <c>POST {adminRouterPrefix}/row-actions/{ActionName}/{id}</c>.
/// </para>
///
/// Subclasses must implement <see cref="AdminAction.ActionName"/>,
/// <see cref="AdminAction.Label"/>, and <see cref="HandleAsync"/>.
///
/// Example:
/// <code>
/// public class PublishAction : ModelAction
/// {
///     public override string ActionName => "publish";
///     public override string Label      => "Publish";
///     public override string Level      => "primary";
///
///     public override async Task&lt;object&gt; HandleAsync(HttpContext context)
///     {
///         var id = context.Request.RouteValues["id"]?.ToString();
///         // ... publish logic ...
///         return new { ok = true, id };
///     }
/// }
/// </code>
/// </summary>
public abstract class ModelAction : AdminAction
{
    /// <summary>
    /// Whether to show a confirmation dialog before executing the action.
    /// When <c>null</c>, no confirmation is shown.
    /// </summary>
    public virtual string? ConfirmText => null;

    /// <summary>
    /// Builds the amis button schema that appears in the CRUD row operation column.
    /// Default implementation returns an <c>ajax</c>-type button that POSTs to
    /// <c>{adminRouterPrefix}/row-actions/{ActionName}/${id}</c>, passing the row id.
    /// </summary>
    /// <param name="adminRouterPrefix">
    /// The <see cref="RouterAdmin.RouterPrefix"/> of the owning admin.
    /// </param>
    public virtual object BuildRowActionButton(string adminRouterPrefix) => new
    {
        type        = "button",
        label       = Label,
        level       = Level,
        icon        = Icon,
        actionType  = "ajax",
        api         = $"post:{adminRouterPrefix}/row-actions/{ActionName}/${{id}}",
        confirmText = ConfirmText
    };

    /// <inheritdoc cref="AdminAction.BuildActionButton"/>
    /// <remarks>
    /// For row-level actions the primary rendering is
    /// <see cref="BuildRowActionButton"/>; <see cref="BuildActionButton"/> is
    /// available for placing the same action in the header toolbar as well.
    /// </remarks>
    public override object BuildActionButton(string adminRouterPrefix) =>
        BuildRowActionButton(adminRouterPrefix);
}
