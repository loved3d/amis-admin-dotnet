using AmisAdminDotNet.AmisComponents;

namespace AmisAdminDotNet.Admin;

/// <summary>
/// An admin that only contributes a navigation link to the menu — it has no page body.
/// Mirrors Python's <c>LinkAdmin</c>.
/// </summary>
public abstract class LinkAdmin : RouterAdmin
{
    /// <summary>The URL this menu entry links to.</summary>
    public abstract string Link { get; }

    /// <summary>
    /// PageSchema override to carry the link URL in navigation metadata.
    /// </summary>
    public override PageSchemaOptions PageSchema =>
        new() { Label = Label, Link = Link };

    /// <summary>No routes to register — LinkAdmin is navigation-only.</summary>
    public override void RegisterRoutes(WebApplication app) { }

    /// <inheritdoc/>
    public override Page BuildPageSchema() => new() { Title = Label };
}
