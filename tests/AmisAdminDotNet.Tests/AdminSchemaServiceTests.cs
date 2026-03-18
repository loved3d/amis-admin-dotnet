using System.Text.Json;
using AmisAdminDotNet.Services;

namespace AmisAdminDotNet.Tests;

public sealed class AdminSchemaServiceTests
{
    [Fact]
    public void BuildAdminPageSchema_ContainsCrudAndMutationApis()
    {
        var service = new AdminSchemaService();

        var json = JsonSerializer.Serialize(service.BuildAdminPageSchema());

        Assert.Contains("\"type\":\"page\"", json);
        Assert.Contains("\"type\":\"crud\"", json);
        Assert.Contains("get:/api/admin/users", json);
        Assert.Contains("post:/api/admin/users", json);
        Assert.Contains("put:/api/admin/users/${id}", json);
        Assert.Contains("delete:/api/admin/users/${id}", json);
    }
}
