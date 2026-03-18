using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Services;
using Microsoft.Extensions.Configuration;
using CrudComponent = AmisAdminDotNet.AmisComponents.Crud;

namespace AmisAdminDotNet.Tests;

public sealed class MigrationUtilityTests
{
    [Fact]
    public void Crud_ToJson_MatchesExpectedJsonSchema()
    {
        var crud = new CrudComponent
        {
            Name = "userCrud",
            Api = "get:/api/admin/users",
            PerPage = 10,
            SyncLocation = false,
            Columns =
            [
                new TableColumn { Name = "id", Label = "ID" }
            ]
        };

        using var doc = JsonDocument.Parse(crud.ToJson());
        var root = doc.RootElement;

        Assert.Equal("crud", root.GetProperty("type").GetString());
        Assert.Equal("userCrud", root.GetProperty("name").GetString());
        Assert.Equal("get:/api/admin/users", root.GetProperty("api").GetString());
        Assert.Equal(10, root.GetProperty("perPage").GetInt32());
        Assert.False(root.GetProperty("syncLocation").GetBoolean());
        Assert.Equal("id", root.GetProperty("columns")[0].GetProperty("name").GetString());
    }

    [Fact]
    public void AppSettings_FromConfiguration_BindsAmisCdnAndSchemaPath()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:DatabaseUrl"] = "Data Source=test.db",
                ["AppSettings:AmisCdn"] = "https://cdn.example.com/amis/sdk",
                ["AppSettings:SchemaApiPath"] = "/schema.json"
            })
            .Build();

        var settings = AppSettings.FromConfiguration(configuration);

        Assert.Equal("Data Source=test.db", settings.DatabaseUrl);
        Assert.Equal("https://cdn.example.com/amis/sdk", settings.AmisCdn);
        Assert.Equal("/schema.json", settings.SchemaApiPath);
    }

    [Fact]
    public void I18nService_Translate_ReturnsFallbackWhenMissing()
    {
        var i18n = new I18nService(new Dictionary<string, string>
        {
            ["hello"] = "你好"
        });

        Assert.Equal("你好", i18n.Translate("hello"));
        Assert.Equal("Default value", i18n.Translate("missing", "Default value"));
    }

    [Fact]
    public void DataAnnotationsModelValidator_ReturnsErrorsForInvalidModel()
    {
        var model = new AnnotatedModel();

        var valid = DataAnnotationsModelValidator.TryValidate(model, out var errors);

        Assert.False(valid);
        Assert.Contains("Name", errors.Keys);
        Assert.Contains("Email", errors.Keys);
    }

    private sealed class AnnotatedModel
    {
        [Required]
        public string? Name { get; set; }

        [EmailAddress]
        public string Email { get; set; } = "not-an-email";
    }
}
