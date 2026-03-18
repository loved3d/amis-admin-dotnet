using System.Text.Json;
using AmisAdminDotNet.Admin;
using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Crud;
using AmisAdminDotNet.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AmisAdminDotNet.Tests;

// ── Test doubles ──────────────────────────────────────────────────────────────

public sealed class ProductEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public sealed class ProductDbContext : DbContext
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
}

/// <summary>
/// Concrete ModelAdmin for testing purposes.
/// Maps to a Python ModelAdmin subclass that provides the model class and router path.
/// </summary>
public sealed class ProductAdmin : ModelAdmin<ProductEntity, int, ProductDbContext>
{
    public override string RouterPath => "products";
    public override string Label      => "Products";

    public ProductAdmin(ProductDbContext db) : base(db) { }
}

// ── ModelAdmin tests ──────────────────────────────────────────────────────────

public sealed class ModelAdminTests
{
    private static ProductDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ProductDbContext(opts);
    }

    private static ProductAdmin CreateAdmin()
    {
        var db    = CreateDb();
        var admin = new ProductAdmin(db);
        // Simulate being registered in an app so RouterPrefix works
        var settings = new AdminSiteSettings { AdminPath = "/admin" };
        var services = new ServiceCollection().BuildServiceProvider();
        admin.App = new AdminApp("Main", "/admin", settings, services);
        return admin;
    }

    [Fact]
    public void RouterPrefix_IsComposedFromAppPrefixAndRouterPath()
    {
        var admin = CreateAdmin();
        Assert.Equal("/admin/products", admin.RouterPrefix);
    }

    [Fact]
    public void HasPagePermission_ReturnsTrueByDefault()
    {
        var admin = CreateAdmin();
        Assert.True(admin.HasPagePermission());
    }

    [Fact]
    public void GetCreateAction_ReturnsButtonWithDialogActionType()
    {
        var admin = CreateAdmin();
        var btn = admin.GetCreateAction();

        Assert.Equal("dialog", btn.ActionType);
        Assert.Contains("Create", btn.Label);
        Assert.IsType<Dialog>(btn.Dialog);
    }

    [Fact]
    public void GetCreateAction_Dialog_ContainsForm()
    {
        var admin = CreateAdmin();
        var btn  = admin.GetCreateAction();
        var dlg  = (Dialog)btn.Dialog!;

        Assert.IsType<Form>(dlg.Body);
        var form = (Form)dlg.Body!;
        Assert.StartsWith("post:", form.Api);
    }

    [Fact]
    public void GetUpdateAction_ReturnsButtonWithDialogAndPutApi()
    {
        var admin = CreateAdmin();
        var btn  = admin.GetUpdateAction();
        var dlg  = (Dialog)btn.Dialog!;
        var form = (Form)dlg.Body!;

        Assert.Equal("dialog", btn.ActionType);
        Assert.StartsWith("put:", form.Api);
    }

    [Fact]
    public void GetDeleteAction_ReturnsAjaxButtonWithDeleteApi()
    {
        var admin = CreateAdmin();
        var btn = admin.GetDeleteAction();

        Assert.Equal("ajax", btn.ActionType);
        Assert.StartsWith("delete:", btn.Api);
    }

    [Fact]
    public void BuildPageSchema_ReturnsPageWithCrudBody()
    {
        var admin = CreateAdmin();
        var page  = admin.BuildPageSchema();

        Assert.IsType<Page>(page);
        Assert.Equal("page", page.Type);
        Assert.NotNull(page.Body);
    }

    [Fact]
    public void BuildPageSchema_Json_ContainsCrudAndApiEndpoints()
    {
        var admin = CreateAdmin();
        var json  = admin.BuildPageSchema().ToJson();

        Assert.Contains("\"type\":\"crud\"", json);
        Assert.Contains("/admin/products", json);
    }

    // ── CRUD operations via ModelAdmin ────────────────────────────────────────

    [Fact]
    public void CreateItem_AndGetItems_WorksThroughModelAdmin()
    {
        var admin = CreateAdmin();

        admin.CreateItem(new ProductEntity { Id = 1, Title = "Widget", Price = 9.99m });
        var result = admin.GetItems(page: 1, perPage: 10);

        Assert.Equal(1, result.Total);
        Assert.Equal("Widget", result.Items[0].Title);
    }

    [Fact]
    public async Task CreateItemAsync_AndGetItemsAsync_WorksThroughModelAdmin()
    {
        var admin = CreateAdmin();

        await admin.CreateItemAsync(new ProductEntity { Id = 1, Title = "Widget", Price = 9.99m });
        var result = await admin.GetItemsAsync(page: 1, perPage: 10);

        Assert.Equal(1, result.Total);
        Assert.Equal("Widget", result.Items[0].Title);
    }
}

// ── AdminApp tests ────────────────────────────────────────────────────────────

public sealed class AdminAppTests
{
    private static (AdminApp app, IServiceProvider services) CreateAdminApp(string name = "TestApp")
    {
        var settings = new AdminSiteSettings { AdminPath = "/admin" };

        var db   = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        var services = new ServiceCollection()
            .AddSingleton(new ProductDbContext(db))
            .AddTransient(sp => new ProductAdmin(sp.GetRequiredService<ProductDbContext>()))
            .BuildServiceProvider();

        var adminApp = new AdminApp(name, "/admin", settings, services);
        return (adminApp, services);
    }

    [Fact]
    public void RegisterAdmin_AddsAdminToList()
    {
        var (app, _) = CreateAdminApp();

        app.RegisterAdmin<ProductAdmin>();

        Assert.Single(app.Admins);
        Assert.IsType<ProductAdmin>(app.Admins[0]);
    }

    [Fact]
    public void RegisterAdmin_SetsAppOnAdmin()
    {
        var (app, _) = CreateAdminApp();

        app.RegisterAdmin<ProductAdmin>();

        Assert.Same(app, app.Admins[0].App);
    }

    [Fact]
    public void BuildTabsSchema_ReturnsTabsWithOneTabPerAdmin()
    {
        var (app, _) = CreateAdminApp();
        app.RegisterAdmin<ProductAdmin>();

        var tabs = app.BuildTabsSchema();

        Assert.IsType<Tabs>(tabs);
        Assert.Single(tabs.TabList!);
        Assert.Equal("Products", tabs.TabList![0].Title);
    }
}

// ── AdminSite tests ───────────────────────────────────────────────────────────

public sealed class AdminSiteTests
{
    private static IServiceProvider BuildServices()
    {
        var db = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        return new ServiceCollection()
            .AddSingleton(new ProductDbContext(db))
            .AddTransient(sp => new ProductAdmin(sp.GetRequiredService<ProductDbContext>()))
            .BuildServiceProvider();
    }

    [Fact]
    public void CreateApp_AddsAppAndReturnsIt()
    {
        var settings = new AdminSiteSettings { AdminPath = "/admin" };
        var site     = new AdminSite(settings, BuildServices());

        var app = site.CreateApp("Products");

        Assert.Equal("Products", app.Name);
    }

    [Fact]
    public void BuildPageSchema_ReturnsSingleAppTabsInBody_WhenOneApp()
    {
        var settings = new AdminSiteSettings { AdminPath = "/admin" };
        var site     = new AdminSite(settings, BuildServices());

        var app = site.CreateApp("Products");
        app.RegisterAdmin<ProductAdmin>();

        var page = site.BuildPageSchema();

        Assert.Equal("page", page.Type);
        Assert.IsType<Tabs>(page.Body);
    }

    [Fact]
    public void BuildPageSchema_ReturnsTwoLevelTabs_WhenMultipleApps()
    {
        var settings = new AdminSiteSettings { AdminPath = "/admin" };
        var site     = new AdminSite(settings, BuildServices());

        site.CreateApp("Group A").RegisterAdmin<ProductAdmin>();
        site.CreateApp("Group B").RegisterAdmin<ProductAdmin>();

        var page = site.BuildPageSchema();
        var tabs = (Tabs)page.Body!;

        Assert.Equal(2, tabs.TabList!.Count);
        Assert.Equal("Group A", tabs.TabList[0].Title);
        Assert.Equal("Group B", tabs.TabList[1].Title);
    }

    [Fact]
    public void AdminSiteSettings_DefaultDatabaseUrl_IsSet()
    {
        var settings = new AdminSiteSettings();
        Assert.False(string.IsNullOrWhiteSpace(settings.DatabaseUrl));
    }
}
