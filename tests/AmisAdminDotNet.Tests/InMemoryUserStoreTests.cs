using AmisAdminDotNet.Models;
using AmisAdminDotNet.Services;

namespace AmisAdminDotNet.Tests;

public sealed class InMemoryUserStoreTests
{
    [Fact]
    public void QueryCreateUpdateDelete_WorksForBasicCrudFlow()
    {
        var store = new InMemoryUserStore();

        var initial = store.Query(null, page: 1, perPage: 10);
        Assert.True(initial.Total >= 3);

        var created = store.Create(new SaveUserRequest("Dora", "dora@example.com", "Operator", true));
        var filtered = store.Query("dora@example.com", page: 1, perPage: 10);

        Assert.Contains(filtered.Items, user => user.Id == created.Id);

        var updated = store.Update(created.Id, new SaveUserRequest("Dora Xu", "dora.xu@example.com", "Administrator", false));

        Assert.NotNull(updated);
        Assert.Equal("Dora Xu", updated!.Name);
        Assert.False(updated.Enabled);

        Assert.True(store.Delete(created.Id));

        var afterDelete = store.Query("dora.xu@example.com", page: 1, perPage: 10);
        Assert.DoesNotContain(afterDelete.Items, user => user.Id == created.Id);
    }

    [Fact]
    public void Query_AppliesPagingAndKeywordFiltering()
    {
        var store = new InMemoryUserStore();
        var fullResult = store.Query("example.com", page: 1, perPage: 100);

        var page = store.Query("example.com", page: 1, perPage: 2);

        Assert.Equal(fullResult.Total, page.Total);
        Assert.True(page.Total >= 3);
        Assert.Equal(2, page.Items.Count);
    }
}
