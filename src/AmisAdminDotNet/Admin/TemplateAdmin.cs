using AmisAdminDotNet.AmisComponents;

namespace AmisAdminDotNet.Admin;

/// <summary>
/// An admin that renders a page whose body is built from a template string via the
/// amis <c>tpl</c> component.
/// Mirrors Python's <c>TemplateAdmin</c> from <c>fastapi_amis_admin/admin/admin.py</c>.
///
/// <para>
/// Subclasses must provide <see cref="RouterAdmin.RouterPath"/> and
/// <see cref="Template"/>. The template is a <see href="https://lodash.com/docs/#template">
/// Lodash template string</see> — plain HTML is also valid.
/// </para>
///
/// Example:
/// <code>
/// public class WelcomePage : TemplateAdmin
/// {
///     public override string RouterPath => "welcome";
///     public override string Label      => "Welcome";
///     public override string Template   =>
///         "&lt;h1&gt;Hello, ${user}!&lt;/h1&gt;&lt;p&gt;Current time: ${now}&lt;/p&gt;";
/// }
/// </code>
/// </summary>
public abstract class TemplateAdmin : PageAdmin
{
    /// <summary>
    /// Lodash template string (or plain HTML) rendered as the page body.
    /// Maps to Python <c>TemplateAdmin.template_name</c>.
    /// </summary>
    public abstract string Template { get; }

    /// <summary>
    /// Optional API URL called on page init to fetch data variables for template rendering.
    /// When set, the amis <c>tpl</c> component can access response data via <c>${variableName}</c>.
    /// Maps to Python <c>TemplateAdmin.template_context</c>.
    /// </summary>
    public virtual string? InitApi { get; }

    /// <inheritdoc/>
    public override Page GetPage() => new()
    {
        Title   = Label,
        InitApi = InitApi,
        Body    = new Tpl { Template = Template }
    };
}
