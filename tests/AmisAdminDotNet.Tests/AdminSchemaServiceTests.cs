using System.Text.Json;
using AmisAdminDotNet.Services;

namespace AmisAdminDotNet.Tests;

public sealed class AdminSchemaServiceTests
{
    [Fact]
    public void BuildAdminPageSchema_ContainsCrudAndMutationApis()
    {
        using var service = new AdminSchemaService();

        var json = JsonSerializer.Serialize(service.BuildAdminPageSchema());

        Assert.Contains("\"type\":\"page\"", json);
        Assert.Contains("\"type\":\"crud\"", json);
        Assert.Contains("get:/api/admin/users", json);
        Assert.Contains("post:/api/admin/users", json);
        Assert.Contains("put:/api/admin/users/${id}", json);
        Assert.Contains("delete:/api/admin/users/${id}", json);
    }

    [Fact]
    public void BuildAdminPageSchema_IsCached()
    {
        using var service = new AdminSchemaService();

        var first = service.BuildAdminPageSchema();
        var second = service.BuildAdminPageSchema();

        Assert.Same(first, second);
    }
}
