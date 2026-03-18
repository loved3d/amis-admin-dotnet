namespace AmisAdminDotNet.Services;

public interface II18nService
{
    string Translate(string key, string? defaultValue = null);
}

/// <summary>
/// Minimal translation service mirroring the lookup-style helpers from
/// fastapi-amis-admin utilities.
/// </summary>
public sealed class I18nService : II18nService
{
    private readonly IReadOnlyDictionary<string, string> _translations;

    public I18nService()
        : this(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase))
    {
    }

    public I18nService(IReadOnlyDictionary<string, string> translations)
    {
        _translations = new Dictionary<string, string>(translations, StringComparer.OrdinalIgnoreCase);
    }

    public string Translate(string key, string? defaultValue = null) =>
        _translations.TryGetValue(key, out var value) ? value : defaultValue ?? key;
}
