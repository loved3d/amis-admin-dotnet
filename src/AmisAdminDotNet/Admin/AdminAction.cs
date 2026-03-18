using Microsoft.AspNetCore.Http;

namespace AmisAdminDotNet.Admin;

/// <summary>
/// Custom admin action base class. Corresponds to Python <c>AdminAction</c> / <c>FormAction</c>.
/// Instances are returned by <see cref="ModelAdmin{TEntity,TKey,TDbContext}.GetAdminActions"/>,
/// rendered as buttons in the list-page header toolbar, and handled via dedicated POST routes.
/// </summary>
public abstract class AdminAction
{
    /// <summary>
    /// Unique machine-readable name used as the URL path segment for this action.
    /// The action is accessible at <c>POST {adminRouterPrefix}/actions/{ActionName}</c>.
    /// </summary>
    public abstract string ActionName { get; }

    /// <summary>Button label displayed in the UI.</summary>
    public abstract string Label { get; }

    /// <summary>
    /// Amis button <c>level</c> style (e.g. <c>"default"</c>, <c>"primary"</c>, <c>"danger"</c>).
    /// </summary>
    public virtual string Level => "default";

    /// <summary>Optional icon CSS class (e.g. <c>"fa fa-download"</c>).</summary>
    public virtual string? Icon => null;

    /// <summary>
    /// Builds the amis button schema object for this action.
    /// Default implementation returns an <c>ajax</c>-type button that POSTs to the action route.
    /// Subclasses may override to return a <c>dialog</c>-type button or any other schema.
    /// </summary>
    /// <param name="adminRouterPrefix">The <c>RouterPrefix</c> of the owning admin (e.g. <c>"/admin/products"</c>).</param>
    public virtual object BuildActionButton(string adminRouterPrefix) => new
    {
        type = "button",
        label = Label,
        level = Level,
        icon = Icon,
        actionType = "ajax",
        api = $"post:{adminRouterPrefix}/actions/{ActionName}",
        confirmText = (string?)null
    };

    /// <summary>
    /// Handles the action request.
    /// Registered at <c>POST {adminRouterPrefix}/actions/{ActionName}</c> by
    /// <see cref="ModelAdmin{TEntity,TKey,TDbContext}.RegisterRoutes"/>.
    /// </summary>
    /// <param name="context">Current HTTP context.</param>
    /// <returns>Response object serialised as JSON.</returns>
    public abstract Task<object> HandleAsync(HttpContext context);
}

/// <summary>
/// Dialog-based admin action that displays an inline form before submitting.
/// Corresponds to Python <c>FormAction</c>.
/// <c>BuildActionButton</c> returns a <c>dialog</c>-type button containing a form
/// whose API posts to the same action route.
/// </summary>
/// <typeparam name="TSchema">Type of the form's data model (unused at runtime; for documentation).</typeparam>
public abstract class FormAction<TSchema> : AdminAction where TSchema : class
{
    /// <summary>Dialog title. Defaults to <see cref="AdminAction.Label"/>.</summary>
    public virtual string FormTitle => Label;

    /// <summary>Form layout mode (e.g. <c>"horizontal"</c>, <c>"inline"</c>).</summary>
    public virtual string FormMode => "horizontal";

    /// <summary>
    /// Builds a <c>dialog</c>-type amis button that opens a modal form.
    /// The form submits a POST request to the action's handler route.
    /// </summary>
    public override object BuildActionButton(string adminRouterPrefix) => new
    {
        type = "button",
        label = Label,
        level = Level,
        icon = Icon,
        actionType = "dialog",
        dialog = new
        {
            title = FormTitle,
            body = new
            {
                type = "form",
                mode = FormMode,
                api = $"post:{adminRouterPrefix}/actions/{ActionName}",
                body = GetFormFields().ToList()
            }
        }
    };

    /// <summary>Returns the amis form field components rendered inside the dialog.</summary>
    public virtual IEnumerable<object> GetFormFields() => [];

    /// <inheritdoc/>
    public override abstract Task<object> HandleAsync(HttpContext context);
}
