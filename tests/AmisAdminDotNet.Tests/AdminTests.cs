using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AmisAdminDotNet.Admin;
using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Crud;
using AmisAdminDotNet.Services;
using Microsoft.AspNetCore.Http;
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

public sealed class ValidatedProductEntity
{
    public int Id { get; set; }

    [Required]
    public string? Title { get; set; }
}

public sealed class ValidatedProductDbContext : DbContext
{
    public DbSet<ValidatedProductEntity> Products => Set<ValidatedProductEntity>();

    public ValidatedProductDbContext(DbContextOptions<ValidatedProductDbContext> options) : base(options) { }
}

public sealed class ValidatedProductAdmin : ModelAdmin<ValidatedProductEntity, int, ValidatedProductDbContext>
{
    public override string RouterPath => "validated-products";
    public override string Label => "Validated products";

    public ValidatedProductAdmin(ValidatedProductDbContext db) : base(db) { }

    public bool ValidateForTest(ValidatedProductEntity entity, out string? errorMessage) =>
        TryValidateEntity(entity, out errorMessage);
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
        var ctx   = new DefaultHttpContext();
        Assert.True(admin.HasPagePermission(ctx));
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

    [Fact]
    public void TryValidateEntity_ReturnsErrorMessage_ForInvalidAnnotatedEntity()
    {
        var opts = new DbContextOptionsBuilder<ValidatedProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new ValidatedProductDbContext(opts);
        var admin = new ValidatedProductAdmin(db);

        var isValid = admin.ValidateForTest(new ValidatedProductEntity { Id = 1 }, out var errorMessage);

        Assert.False(isValid);
        Assert.Contains("Title", errorMessage);
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

// ── Permission tests ──────────────────────────────────────────────────────────

/// <summary>ModelAdmin that denies all access via HasPagePermission.</summary>
public sealed class LockedProductAdmin : ModelAdmin<ProductEntity, int, ProductDbContext>
{
    public override string RouterPath => "locked-products";
    public override string Label      => "Locked Products";

    public LockedProductAdmin(ProductDbContext db) : base(db) { }

    public override bool HasPagePermission(HttpContext context) => false;
}

/// <summary>ModelAdmin that only denies CREATE.</summary>
public sealed class ReadOnlyProductAdmin : ModelAdmin<ProductEntity, int, ProductDbContext>
{
    public override string RouterPath => "readonly-products";
    public override string Label      => "ReadOnly Products";

    public ReadOnlyProductAdmin(ProductDbContext db) : base(db) { }

    public override bool HasCreatePermission(HttpContext context) => false;
}

public sealed class PermissionTests
{
    private static ProductDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static TAdmin CreateAdmin<TAdmin>(Func<ProductDbContext, TAdmin> factory)
        where TAdmin : RouterAdmin
    {
        var admin    = factory(CreateDb());
        var settings = new AdminSiteSettings { AdminPath = "/admin" };
        admin.App    = new AdminApp("Main", "/admin", settings,
                            new ServiceCollection().BuildServiceProvider());
        return admin;
    }

    [Fact]
    public void HasPagePermission_Default_ReturnsTrueForUnauthenticatedUser()
    {
        var admin = CreateAdmin(db => new ProductAdmin(db));
        // DefaultHttpContext has no authenticated user → still true (backward-compat)
        Assert.True(admin.HasPagePermission(new DefaultHttpContext()));
    }

    [Fact]
    public void HasPagePermission_OverriddenToFalse_ReturnsFalse()
    {
        var admin = CreateAdmin(db => new LockedProductAdmin(db));
        Assert.False(admin.HasPagePermission(new DefaultHttpContext()));
    }

    [Fact]
    public void GranularHooks_DefaultToHasPagePermission_WhenPermitted()
    {
        var admin = CreateAdmin(db => new ProductAdmin(db));
        var ctx   = new DefaultHttpContext();

        Assert.True(admin.HasReadPermission(ctx));
        Assert.True(admin.HasCreatePermission(ctx));
        Assert.True(admin.HasUpdatePermission(ctx));
        Assert.True(admin.HasDeletePermission(ctx));
    }

    [Fact]
    public void GranularHooks_AllDenied_WhenHasPagePermissionReturnsFalse()
    {
        var admin = CreateAdmin(db => new LockedProductAdmin(db));
        var ctx   = new DefaultHttpContext();

        Assert.False(admin.HasReadPermission(ctx));
        Assert.False(admin.HasCreatePermission(ctx));
        Assert.False(admin.HasUpdatePermission(ctx));
        Assert.False(admin.HasDeletePermission(ctx));
    }

    [Fact]
    public void GranularHooks_IndividualOverride_OnlyAffectsTargetOperation()
    {
        var admin = CreateAdmin(db => new ReadOnlyProductAdmin(db));
        var ctx   = new DefaultHttpContext();

        Assert.True(admin.HasReadPermission(ctx));
        Assert.False(admin.HasCreatePermission(ctx));   // overridden → false
        Assert.True(admin.HasUpdatePermission(ctx));
        Assert.True(admin.HasDeletePermission(ctx));
    }

    [Fact]
    public void RequireAuthenticatedUser_ReturnsFalse_ForUnauthenticatedContext()
    {
        // Use a concrete subclass to expose the protected helper
        var admin = CreateAdmin(db => new AuthAwareProductAdmin(db));
        Assert.False(admin.CheckAuth(new DefaultHttpContext()));
    }
}

/// <summary>Exposes the protected <c>RequireAuthenticatedUser</c> helper for testing.</summary>
public sealed class AuthAwareProductAdmin : ModelAdmin<ProductEntity, int, ProductDbContext>
{
    public override string RouterPath => "auth-products";
    public override string Label      => "Auth Products";

    public AuthAwareProductAdmin(ProductDbContext db) : base(db) { }

    public bool CheckAuth(HttpContext context) => RequireAuthenticatedUser(context);
}

// ── FormAdmin tests ───────────────────────────────────────────────────────────

public sealed class ContactForm
{
    public string Name  { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class TestFormAdmin : FormAdmin<ContactForm>
{
    public override string RouterPath => "contact";
    public override string Label      => "Contact";

    public override Task<object> HandleAsync(ContactForm data, HttpContext context)
        => Task.FromResult<object>(new { received = data.Name });
}

public sealed class CustomPathFormAdmin : FormAdmin<ContactForm>
{
    public override string RouterPath => "feedback";
    public override string Label      => "Feedback";
    public override string FormPath   => "send";

    public override Task<object> HandleAsync(ContactForm data, HttpContext context)
        => Task.FromResult<object>(new { ok = true });
}

public sealed class FormAdminTests
{
    private static TAdmin CreateAdmin<TAdmin>(TAdmin admin)
        where TAdmin : RouterAdmin
    {
        var settings = new AdminSiteSettings { AdminPath = "/admin" };
        admin.App    = new AdminApp("Main", "/admin", settings,
                            new ServiceCollection().BuildServiceProvider());
        return admin;
    }

    [Fact]
    public void BuildPageSchema_ReturnsPageContainingForm()
    {
        var admin = CreateAdmin(new TestFormAdmin());
        var page  = admin.BuildPageSchema();

        Assert.IsType<Page>(page);
        Assert.IsType<Form>(page.Body);
    }

    [Fact]
    public void BuildPageSchema_FormApi_ContainsCorrectSubmitPath()
    {
        var admin = CreateAdmin(new TestFormAdmin());
        var form  = (Form)admin.BuildPageSchema().Body!;

        Assert.Contains("/admin/contact/submit", form.Api);
        Assert.StartsWith("post:", form.Api);
    }

    [Fact]
    public void BuildPageSchema_FormApi_UsesCustomFormPath()
    {
        var admin = CreateAdmin(new CustomPathFormAdmin());
        var form  = (Form)admin.BuildPageSchema().Body!;

        Assert.Contains("/admin/feedback/send", form.Api);
    }

    [Fact]
    public void FormPath_CanBeOverriddenBySubclass()
    {
        var admin = CreateAdmin(new CustomPathFormAdmin());
        Assert.Equal("send", admin.FormPath);
    }

    [Fact]
    public async Task HandleAsync_IsInvokedAndReturnsExpectedData()
    {
        var admin = CreateAdmin(new TestFormAdmin());
        var ctx   = new DefaultHttpContext();

        var result = await admin.HandleAsync(new ContactForm { Name = "Alice" }, ctx);

        var json = System.Text.Json.JsonSerializer.Serialize(result);
        Assert.Contains("Alice", json);
    }
}

// ── PageSchemaOptions tests ───────────────────────────────────────────────────

/// <summary>Admin with non-default <see cref="PageSchemaOptions"/>.</summary>
public sealed class SortedProductAdmin : ModelAdmin<ProductEntity, int, ProductDbContext>
{
    private readonly int _sort;
    private readonly bool _isDefault;

    public override string RouterPath => "sorted-products";
    public override string Label      => "Sorted Products";

    public SortedProductAdmin(ProductDbContext db, int sort, bool isDefault = false)
        : base(db)
    {
        _sort      = sort;
        _isDefault = isDefault;
    }

    public override PageSchemaOptions PageSchema =>
        new() { Label = Label, Sort = _sort, IsDefaultPage = _isDefault };
}

public sealed class PageSchemaOptionsTests
{
    private static ProductDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AdminApp CreateAppWithAdmins(params (int sort, bool isDefault)[] configs)
    {
        var db       = CreateDb();
        var settings = new AdminSiteSettings { AdminPath = "/admin" };

        var services = new ServiceCollection()
            .AddSingleton(db)
            .BuildServiceProvider();

        var adminApp = new AdminApp("Test", "/admin", settings, services);

        foreach (var (sort, isDefault) in configs)
        {
            var admin = new SortedProductAdmin(db, sort, isDefault);
            admin.App = adminApp;
            // Access the internal list via the public property to register manually
            typeof(AdminApp)
                .GetField("_admins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(adminApp, new List<RouterAdmin>(adminApp.Admins) { admin });
        }

        return adminApp;
    }

    [Fact]
    public void PageSchemaOptions_DefaultValues_AreCorrect()
    {
        var opts = new PageSchemaOptions { Label = "Test" };

        Assert.Equal("Test", opts.Label);
        Assert.Null(opts.Icon);
        Assert.Equal(0, opts.Sort);
        Assert.False(opts.IsDefaultPage);
    }

    [Fact]
    public void PageSchemaOptions_IconIsPreserved()
    {
        var opts = new PageSchemaOptions { Label = "Users", Icon = "fa fa-users" };
        Assert.Equal("fa fa-users", opts.Icon);
    }

    [Fact]
    public void RouterAdmin_PageSchema_DefaultsToLabelAndZeroSort()
    {
        var db      = CreateDb();
        var admin   = new ProductAdmin(db);
        var opts    = admin.PageSchema;

        Assert.Equal("Products", opts.Label);
        Assert.Equal(0, opts.Sort);
        Assert.False(opts.IsDefaultPage);
        Assert.Null(opts.Icon);
    }

    [Fact]
    public void BuildTabsSchema_SortsByPageSortDescending()
    {
        var db       = CreateDb();
        var settings = new AdminSiteSettings { AdminPath = "/admin" };
        var services = new ServiceCollection().AddSingleton(db).BuildServiceProvider();
        var app      = new AdminApp("Test", "/admin", settings, services);

        var low  = new SortedProductAdmin(db, sort: 1);
        var high = new SortedProductAdmin(db, sort: 10);
        low.App  = app;
        high.App = app;

        // Register via reflection (list is private)
        var field = typeof(AdminApp)
            .GetField("_admins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        field.SetValue(app, new List<RouterAdmin> { low, high });

        var tabs = app.BuildTabsSchema();

        // high Sort (10) should appear first
        Assert.Equal(high.PageSchema.Label, tabs.TabList![0].Title);
        Assert.Equal(low.PageSchema.Label,  tabs.TabList![1].Title);
    }

    [Fact]
    public void BuildTabsSchema_IsDefaultPage_ComesFirst_RegardlessOfSort()
    {
        var db       = CreateDb();
        var settings = new AdminSiteSettings { AdminPath = "/admin" };
        var services = new ServiceCollection().AddSingleton(db).BuildServiceProvider();
        var app      = new AdminApp("Test", "/admin", settings, services);

        var regular  = new SortedProductAdmin(db, sort: 100);
        var defaults = new SortedProductAdmin(db, sort: 0, isDefault: true);
        regular.App  = app;
        defaults.App = app;

        var field = typeof(AdminApp)
            .GetField("_admins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        field.SetValue(app, new List<RouterAdmin> { regular, defaults });

        var tabs = app.BuildTabsSchema();

        // defaults admin (IsDefaultPage=true) must be first
        Assert.True(tabs.TabList![0].Title == defaults.PageSchema.Label);
    }
}

// ── CrudQueryParams tests ─────────────────────────────────────────────────────

public sealed class CrudQueryParamsTests
{
    [Fact]
    public void FromQuery_Defaults_WhenQueryStringIsEmpty()
    {
        var p = CrudQueryParams.FromQuery(new QueryCollection());

        Assert.Equal(1,     p.Page);
        Assert.Equal(10,    p.PerPage);
        Assert.Null(p.OrderBy);
        Assert.Equal("asc", p.OrderDir);
        Assert.Null(p.Search);
        Assert.Empty(p.Filters);
    }

    [Fact]
    public void FromQuery_ParsesStandardParams()
    {
        var qs = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["page"]     = "3",
            ["perPage"]  = "25",
            ["orderBy"]  = "Name",
            ["orderDir"] = "desc",
            ["search"]   = "widget"
        });

        var p = CrudQueryParams.FromQuery(qs);

        Assert.Equal(3,        p.Page);
        Assert.Equal(25,       p.PerPage);
        Assert.Equal("Name",   p.OrderBy);
        Assert.Equal("desc",   p.OrderDir);
        Assert.Equal("widget", p.Search);
    }

    [Fact]
    public void FromQuery_ParsesFilterPrefix()
    {
        var qs = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["filter_name"]  = "Alice",
            ["filter_active"] = "true"
        });

        var p = CrudQueryParams.FromQuery(qs);

        Assert.Equal("Alice", p.Filters["name"]);
        Assert.Equal("true",  p.Filters["active"]);
    }

    [Fact]
    public void ApplyOrdering_OrdersByPropertyAscending()
    {
        var opts = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new ProductDbContext(opts);
        db.Products.AddRange(
            new ProductEntity { Id = 1, Title = "Zebra", Price = 5m },
            new ProductEntity { Id = 2, Title = "Apple", Price = 1m },
            new ProductEntity { Id = 3, Title = "Mango", Price = 3m });
        db.SaveChanges();

        var settings = new AdminSiteSettings { AdminPath = "/admin" };
        var admin    = new ProductAdmin(db);
        admin.App    = new AdminApp("Main", "/admin", settings,
                           new ServiceCollection().BuildServiceProvider());

        var p = new CrudQueryParams { OrderBy = "Title", OrderDir = "asc" };
        var result = admin.GetItems(p);

        Assert.Equal("Apple", result.Items[0].Title);
        Assert.Equal("Mango", result.Items[1].Title);
        Assert.Equal("Zebra", result.Items[2].Title);
    }

    [Fact]
    public void ApplyOrdering_OrdersByPropertyDescending()
    {
        var opts = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new ProductDbContext(opts);
        db.Products.AddRange(
            new ProductEntity { Id = 1, Title = "Zebra", Price = 5m },
            new ProductEntity { Id = 2, Title = "Apple", Price = 1m });
        db.SaveChanges();

        var settings = new AdminSiteSettings { AdminPath = "/admin" };
        var admin    = new ProductAdmin(db);
        admin.App    = new AdminApp("Main", "/admin", settings,
                           new ServiceCollection().BuildServiceProvider());

        var p = new CrudQueryParams { OrderBy = "Title", OrderDir = "desc" };
        var result = admin.GetItems(p);

        Assert.Equal("Zebra", result.Items[0].Title);
        Assert.Equal("Apple", result.Items[1].Title);
    }

    [Fact]
    public void GetItems_WithCrudQueryParams_RespectsPageAndPerPage()
    {
        var opts = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new ProductDbContext(opts);
        for (var i = 1; i <= 5; i++)
            db.Products.Add(new ProductEntity { Id = i, Title = $"Item{i}" });
        db.SaveChanges();

        var settings = new AdminSiteSettings { AdminPath = "/admin" };
        var admin    = new ProductAdmin(db);
        admin.App    = new AdminApp("Main", "/admin", settings,
                           new ServiceCollection().BuildServiceProvider());

        var p = new CrudQueryParams { Page = 2, PerPage = 2 };
        var result = admin.GetItems(p);

        Assert.Equal(5, result.Total);
        Assert.Equal(2, result.Items.Count);
    }
}
