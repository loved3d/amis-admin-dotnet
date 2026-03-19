using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Services;

namespace AmisAdminDotNet.Admin;

/// <summary>
/// Default home page admin. Corresponds to Python <c>site.HomeAdmin</c>.
/// Displays system information cards (site settings and runtime information).
/// Registered as the default page (appears first in the tab list).
/// </summary>
public class HomeAdmin : RouterAdmin
{
    private readonly AdminSiteSettings _settings;

    /// <param name="settings">Admin site settings injected from DI.</param>
    public HomeAdmin(AdminSiteSettings settings)
    {
        _settings = settings;
    }

    /// <inheritdoc/>
    public override string RouterPath => "home";

    /// <inheritdoc/>
    public override string Label => "Home";

    /// <inheritdoc/>
    public override PageSchemaOptions PageSchema => new()
    {
        Label         = "Home",
        Icon          = "fa fa-home",
        Sort          = 100,
        IsDefaultPage = true
    };

    /// <summary>
    /// Builds a welcome page with site-info and runtime-info cards
    /// using the amis <c>property</c> component.
    /// </summary>
    public override Page BuildPageSchema()
    {
        return new Page
        {
            Title = "Welcome",
            Body  = new object[]
            {
                new
                {
                    type   = "property",
                    title  = "Site Info",
                    column = 3,
                    items  = new object[]
                    {
                        new { label = "Title",      content = _settings.SiteTitle },
                        new { label = "Version",    content = _settings.Version },
                        new { label = "Admin Path", content = _settings.AdminPath }
                    }
                },
                new
                {
                    type   = "property",
                    title  = "Runtime",
                    column = 3,
                    items  = new object[]
                    {
                        new { label = "Framework", content = $".NET {Environment.Version}" },
                        new { label = "OS",        content = Environment.OSVersion.ToString() },
                        new { label = "Machine",   content = Environment.MachineName }
                    }
                }
            }
        };
    }

    /// <summary>HomeAdmin does not register additional HTTP routes.</summary>
    public override void RegisterRoutes(WebApplication app) { }
}
