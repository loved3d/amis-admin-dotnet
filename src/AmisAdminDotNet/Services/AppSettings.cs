using Microsoft.Extensions.Configuration;

namespace AmisAdminDotNet.Services;

/// <summary>
/// Application-level settings mirroring the Python admin settings module while keeping
/// compatibility with the existing <see cref="AdminSiteSettings"/> shape used by the
/// admin site classes.
/// </summary>
public sealed class AppSettings
{
    /// <summary>Primary database connection string.</summary>
    public string DatabaseUrl { get; set; } = "Data Source=amis_admin.db";

    /// <summary>Admin UI mount path.</summary>
    public string AdminPath { get; set; } = "/admin";

    /// <summary>Allowed CORS origins.</summary>
    public string[] CorsOrigins { get; set; } = [];

    /// <summary>Whether Swagger/OpenAPI endpoints should be enabled.</summary>
    public bool EnableSwagger { get; set; }

    /// <summary>Base CDN path containing the amis sdk assets.</summary>
    public string AmisCdn { get; set; } = "https://unpkg.com/amis@6.11.0/sdk";

    /// <summary>Schema JSON endpoint used by the host page.</summary>
    public string SchemaApiPath { get; set; } = "/api/admin/schema";

    public AdminSiteSettings ToAdminSiteSettings() => new()
    {
        DatabaseUrl = DatabaseUrl,
        AdminPath = AdminPath,
        CorsOrigins = CorsOrigins,
        EnableSwagger = EnableSwagger
    };

    public static AppSettings FromConfiguration(IConfiguration configuration)
    {
        var appSettingsSection = configuration.GetSection("AppSettings");
        var settings = appSettingsSection.Get<AppSettings>() ?? new AppSettings();
        var legacySettings = configuration.GetSection("AdminSite").Get<AdminSiteSettings>();

        if (legacySettings is null)
            return settings;

        static bool HasConfiguredValue(IConfigurationSection section, string key) =>
            section[key] is not null || section.GetSection(key).GetChildren().Any();

        var configuredValues = new Dictionary<string, bool>(StringComparer.Ordinal)
        {
            [nameof(DatabaseUrl)] = HasConfiguredValue(appSettingsSection, nameof(DatabaseUrl)),
            [nameof(AdminPath)] = HasConfiguredValue(appSettingsSection, nameof(AdminPath)),
            [nameof(CorsOrigins)] = HasConfiguredValue(appSettingsSection, nameof(CorsOrigins)),
            [nameof(EnableSwagger)] = HasConfiguredValue(appSettingsSection, nameof(EnableSwagger))
        };

        if (!configuredValues[nameof(DatabaseUrl)])
            settings.DatabaseUrl = legacySettings.DatabaseUrl;

        if (!configuredValues[nameof(AdminPath)])
            settings.AdminPath = legacySettings.AdminPath;

        if (!configuredValues[nameof(CorsOrigins)])
            settings.CorsOrigins = legacySettings.CorsOrigins;

        if (!configuredValues[nameof(EnableSwagger)])
            settings.EnableSwagger = legacySettings.EnableSwagger;

        return settings;
    }
}
