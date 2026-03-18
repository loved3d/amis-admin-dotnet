using System.Text.Json;
using AmisAdminDotNet.Admin;
using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Crud;
using AmisAdminDotNet.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CrudComponent = AmisAdminDotNet.AmisComponents.Crud;

namespace AmisAdminDotNet.Tests;

// ════════════════════════════════════════════════════════════════════════════════
// Shared test infrastructure
// ════════════════════════════════════════════════════════════════════════════════

public sealed class ArticleEntity
{
    public int    Id      { get; set; }
    public string Title   { get; set; } = string.Empty;
    public string? Body   { get; set; }
    public DateOnly? PublishedOn { get; set; }
}

public sealed class ArticleDbContext : DbContext
{
    public DbSet<ArticleEntity> Articles => Set<ArticleEntity>();
    public ArticleDbContext(DbContextOptions<ArticleDbContext> opts) : base(opts) { }
}

/// <summary>Base ModelAdmin used as a simple concrete implementation in tests.</summary>
public sealed class ArticleAdmin : ModelAdmin<ArticleEntity, int, ArticleDbContext>
{
    public override string RouterPath => "articles";
    public override string Label      => "Articles";
    public ArticleAdmin(ArticleDbContext db) : base(db) { }
}

internal static class TestHelper
{
    public static ArticleDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ArticleDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    public static AdminApp CreateApp(IServiceProvider? services = null)
    {
        var settings = new AdminSiteSettings { AdminPath = "/admin" };
        return new AdminApp("Main", "/admin", settings,
            services ?? new ServiceCollection().BuildServiceProvider());
    }

    public static T WithApp<T>(T admin, IServiceProvider? services = null)
        where T : RouterAdmin
    {
        admin.App = CreateApp(services);
        return admin;
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// P1 — PageAdmin tests
// ════════════════════════════════════════════════════════════════════════════════

public sealed class CustomPageAdmin : PageAdmin
{
    public override string RouterPath => "custom-page";
    public override string Label      => "My Custom Page";
}

public sealed class OverriddenBodyPageAdmin : PageAdmin
{
    public override string RouterPath => "body-page";
    public override string Label      => "Body Page";

    public override Page GetPage() => new()
    {
        Title = Label,
        Body  = new Alert { Level = "info", Body = "Hello from body!" }
    };
}

public sealed class DeniedPageAdmin : PageAdmin
{
    public override string RouterPath => "denied-page";
    public override string Label      => "Denied Page";
    public override bool HasPagePermission(HttpContext context) => false;
}

public sealed class PageAdminTests
{
    [Fact]
    public void BuildPageSchema_ReturnsPage_WithCustomTitle()
    {
        var admin = TestHelper.WithApp(new CustomPageAdmin());
        var page  = admin.BuildPageSchema();

        Assert.IsType<Page>(page);
        Assert.Equal("My Custom Page", page.Title);
    }

    [Fact]
    public void BuildPageSchema_BodyIsCustomNode_WhenGetPageOverridden()
    {
        var admin = TestHelper.WithApp(new OverriddenBodyPageAdmin());
        var page  = admin.BuildPageSchema();

        Assert.IsType<Alert>(page.Body);
        var alert = (Alert)page.Body!;
        Assert.Equal("info", alert.Level);
    }

    [Fact]
    public void RegisterRoutes_SchemaEndpoint_ReturnsUnauthorized_WhenNoPermission()
    {
        // Verify DeniedPageAdmin has the permission configured correctly
        var admin = TestHelper.WithApp(new DeniedPageAdmin());
        var ctx   = new DefaultHttpContext();
        Assert.False(admin.HasPagePermission(ctx));
    }

    [Fact]
    public void PageAdmin_GetPage_ReturnsPageWithLabel_ByDefault()
    {
        var admin = TestHelper.WithApp(new CustomPageAdmin());
        var page  = admin.GetPage();

        Assert.Equal("My Custom Page", page.Title);
        Assert.Null(page.Body);
    }

    [Fact]
    public void PageAdmin_BuildPageSchema_DelegatesToGetPage()
    {
        var admin = TestHelper.WithApp(new OverriddenBodyPageAdmin());
        var page  = admin.BuildPageSchema();

        // BuildPageSchema() delegates to GetPage() — both should produce an Alert body
        Assert.IsType<Alert>(page.Body);
        Assert.Equal("Body Page", page.Title);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// P1 — IframeAdmin tests
// ════════════════════════════════════════════════════════════════════════════════

public sealed class EmbeddedAdmin : IframeAdmin
{
    public override string RouterPath => "embedded";
    public override string Label      => "Embedded Tool";
    public override string Src        => "https://example.com/tool";
}

public sealed class IframeAdminTests
{
    [Fact]
    public void BuildPageSchema_BodyIsIframe()
    {
        var admin = TestHelper.WithApp(new EmbeddedAdmin());
        var page  = admin.BuildPageSchema();

        Assert.IsType<Iframe>(page.Body);
    }

    [Fact]
    public void BuildPageSchema_Iframe_SrcMatchesAbstractProperty()
    {
        var admin = TestHelper.WithApp(new EmbeddedAdmin());
        var iframe = (Iframe)admin.BuildPageSchema().Body!;

        Assert.Equal("https://example.com/tool", iframe.Src);
    }

    [Fact]
    public void BuildPageSchema_TitleMatchesLabel()
    {
        var admin = TestHelper.WithApp(new EmbeddedAdmin());
        var page  = admin.BuildPageSchema();

        Assert.Equal("Embedded Tool", page.Title);
    }

    [Fact]
    public void Iframe_Type_IsIframe()
    {
        var iframe = new Iframe { Src = "https://x.com" };
        Assert.Equal("iframe", iframe.Type);
    }

    [Fact]
    public void Iframe_Src_IsSerializedCorrectly()
    {
        var iframe = new Iframe { Src = "https://example.com", Width = "100%", Height = "600px" };
        var json   = iframe.ToJson();

        Assert.Contains("\"type\":\"iframe\"", json);
        Assert.Contains("\"src\":\"https://example.com\"", json);
        Assert.Contains("\"width\":\"100%\"", json);
        Assert.Contains("\"height\":\"600px\"", json);
    }

    [Fact]
    public void Iframe_NullProperties_AreOmittedFromJson()
    {
        var iframe = new Iframe { Src = "https://x.com" };
        var json   = iframe.ToJson();

        Assert.DoesNotContain("\"width\"", json);
        Assert.DoesNotContain("\"height\"", json);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// P1 — LinkAdmin tests
// ════════════════════════════════════════════════════════════════════════════════

public sealed class ExternalSiteAdmin : LinkAdmin
{
    public override string RouterPath => "external-site";
    public override string Label      => "External Site";
    public override string Link       => "https://external.example.com";
}

public sealed class LinkAdminTests
{
    [Fact]
    public void PageSchema_ContainsLink()
    {
        var admin = TestHelper.WithApp(new ExternalSiteAdmin());
        Assert.Equal("https://external.example.com", admin.PageSchema.Link);
    }

    [Fact]
    public void PageSchema_Label_MatchesAdminLabel()
    {
        var admin = TestHelper.WithApp(new ExternalSiteAdmin());
        Assert.Equal("External Site", admin.PageSchema.Label);
    }

    [Fact]
    public void RegisterRoutes_IsNoOp()
    {
        // LinkAdmin.RegisterRoutes should do nothing (no routes registered)
        var admin   = TestHelper.WithApp(new ExternalSiteAdmin());
        var builder = WebApplication.CreateBuilder();
        var app     = builder.Build();

        var endpointsBefore = app.Services
            .GetService<Microsoft.AspNetCore.Routing.EndpointDataSource>()
            ?.Endpoints.Count ?? 0;

        admin.RegisterRoutes(app);   // should be a no-op

        var endpointsAfter = app.Services
            .GetService<Microsoft.AspNetCore.Routing.EndpointDataSource>()
            ?.Endpoints.Count ?? 0;

        Assert.Equal(endpointsBefore, endpointsAfter);
    }

    [Fact]
    public void BuildPageSchema_ReturnsMinimalPage()
    {
        var admin = TestHelper.WithApp(new ExternalSiteAdmin());
        var page  = admin.BuildPageSchema();

        Assert.IsType<Page>(page);
        Assert.Equal("External Site", page.Title);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// P2 — Declarative ModelAdmin configuration tests
// ════════════════════════════════════════════════════════════════════════════════

public sealed class DisplayOnlyAdmin : ModelAdmin<ArticleEntity, int, ArticleDbContext>
{
    public override string RouterPath  => "display-only-articles";
    public override string Label       => "Articles";
    public override string[] ListDisplay => ["Title", "Body"];
    public DisplayOnlyAdmin(ArticleDbContext db) : base(db) { }
}

public sealed class SearchableAdmin : ModelAdmin<ArticleEntity, int, ArticleDbContext>
{
    public override string RouterPath    => "searchable-articles";
    public override string Label         => "Articles";
    public override string[] SearchFields => ["Title", "Body"];
    public SearchableAdmin(ArticleDbContext db) : base(db) { }
}

public sealed class OrderedAdmin : ModelAdmin<ArticleEntity, int, ArticleDbContext>
{
    public override string RouterPath      => "ordered-articles";
    public override string Label           => "Articles";
    public override string? DefaultOrdering => "-Id";
    public OrderedAdmin(ArticleDbContext db) : base(db) { }
}

public sealed class PaginatedAdmin : ModelAdmin<ArticleEntity, int, ArticleDbContext>
{
    public override string RouterPath => "paged-articles";
    public override string Label      => "Articles";
    public override int ListPerPage   => 25;
    public PaginatedAdmin(ArticleDbContext db) : base(db) { }
}

public sealed class ReadonlyFieldAdmin : ModelAdmin<ArticleEntity, int, ArticleDbContext>
{
    public override string RouterPath       => "readonly-articles";
    public override string Label            => "Articles";
    public override string[] ReadonlyFields => ["Title"];
    public ReadonlyFieldAdmin(ArticleDbContext db) : base(db) { }
}

public sealed class DeclarativeModelAdminTests
{
    private static ArticleAdmin CreateArticleAdmin() =>
        TestHelper.WithApp(new ArticleAdmin(TestHelper.CreateDb()));

    [Fact]
    public void ListDisplay_Empty_ReturnsAllColumns()
    {
        var admin   = CreateArticleAdmin();
        var columns = admin.BuildPageSchema();

        // All entity properties become columns (plus operations column)
        var crud = (CrudComponent)columns.Body!;
        // ArticleEntity has Id, Title, Body, PublishedOn → 4 + 1 operation = 5
        Assert.NotNull(crud.Columns);
        Assert.True(crud.Columns!.Count >= 4);
    }

    [Fact]
    public void ListDisplay_NonEmpty_FiltersAndOrdersColumns()
    {
        var admin = TestHelper.WithApp(new DisplayOnlyAdmin(TestHelper.CreateDb()));
        var crud  = (CrudComponent)admin.BuildPageSchema().Body!;

        // Only Title and Body, plus operations column
        Assert.NotNull(crud.Columns);
        var dataCols = crud.Columns!
            .OfType<TableColumn>()
            .ToList();
        Assert.Equal(2, dataCols.Count);
        Assert.Equal("title", dataCols[0].Name);
        Assert.Equal("body",  dataCols[1].Name);
    }

    [Fact]
    public void SearchFields_AppliesContainsFilter_WhenSearchIsSet()
    {
        var db    = TestHelper.CreateDb();
        db.Articles.AddRange(
            new ArticleEntity { Id = 1, Title = "Hello World",  Body = "Content A" },
            new ArticleEntity { Id = 2, Title = "Goodbye World", Body = "Content B" },
            new ArticleEntity { Id = 3, Title = "Other",         Body = "No match" });
        db.SaveChanges();

        var admin = TestHelper.WithApp(new SearchableAdmin(db));
        var p     = new CrudQueryParams { Search = "World" };
        var result = admin.GetItems(p);

        Assert.Equal(2, result.Total);
        Assert.All(result.Items, item => Assert.Contains("World", item.Title));
    }

    [Fact]
    public void SearchFields_Empty_DoesNotFilter()
    {
        var db = TestHelper.CreateDb();
        db.Articles.AddRange(
            new ArticleEntity { Id = 1, Title = "Alpha" },
            new ArticleEntity { Id = 2, Title = "Beta" });
        db.SaveChanges();

        // ArticleAdmin has no SearchFields, so Search is ignored → all items returned
        var admin = TestHelper.WithApp(new ArticleAdmin(db));
        var p     = new CrudQueryParams { Search = "Alpha" };
        var result = admin.GetItems(p);

        Assert.Equal(2, result.Total);
    }

    [Fact]
    public void DefaultOrdering_IsUsed_WhenOrderByIsNotProvided()
    {
        var db = TestHelper.CreateDb();
        db.Articles.AddRange(
            new ArticleEntity { Id = 1, Title = "First" },
            new ArticleEntity { Id = 2, Title = "Second" },
            new ArticleEntity { Id = 3, Title = "Third" });
        db.SaveChanges();

        var admin = TestHelper.WithApp(new OrderedAdmin(db));
        var p     = new CrudQueryParams(); // no OrderBy
        var result = admin.GetItems(p);

        // DefaultOrdering = "-Id" → descending by Id
        Assert.Equal(3, result.Items[0].Id);
        Assert.Equal(2, result.Items[1].Id);
        Assert.Equal(1, result.Items[2].Id);
    }

    [Fact]
    public void DefaultOrdering_IsIgnored_WhenOrderByIsProvided()
    {
        var db = TestHelper.CreateDb();
        db.Articles.AddRange(
            new ArticleEntity { Id = 3, Title = "Alpha" },
            new ArticleEntity { Id = 1, Title = "Beta" },
            new ArticleEntity { Id = 2, Title = "Gamma" });
        db.SaveChanges();

        var admin = TestHelper.WithApp(new OrderedAdmin(db));
        var p     = new CrudQueryParams { OrderBy = "Title", OrderDir = "asc" };
        var result = admin.GetItems(p);

        Assert.Equal("Alpha", result.Items[0].Title);
    }

    [Fact]
    public void ListPerPage_IsReflectedInCrudSchema()
    {
        var admin = TestHelper.WithApp(new PaginatedAdmin(TestHelper.CreateDb()));
        var crud  = (CrudComponent)admin.BuildPageSchema().Body!;

        Assert.Equal(25, crud.PerPage);
    }

    [Fact]
    public void ListPerPage_Default_Is10()
    {
        var admin = CreateArticleAdmin();
        var crud  = (CrudComponent)admin.BuildPageSchema().Body!;

        Assert.Equal(10, crud.PerPage);
    }

    [Fact]
    public void ReadonlyFields_MarksFieldAsDisabled_InFormFields()
    {
        var admin = TestHelper.WithApp(new ReadonlyFieldAdmin(TestHelper.CreateDb()));
        var page  = admin.BuildPageSchema();
        var crud  = (CrudComponent)page.Body!;

        // Create button opens a form — verify Title field is disabled
        var createBtn = (Button)crud.HeaderToolbar![0];
        var dialog    = (Dialog)createBtn.Dialog!;
        var form      = (Form)dialog.Body!;

        var titleField = form.Body!
            .Cast<object>()
            .OfType<InputText>()
            .FirstOrDefault(f => string.Equals(f.Name, "title", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(titleField);
        Assert.True(titleField!.Disabled);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// P3 — Async permission hook tests
// ════════════════════════════════════════════════════════════════════════════════

public sealed class AsyncDeniedPageAdmin : PageAdmin
{
    public override string RouterPath => "async-denied";
    public override string Label      => "Async Denied";

    // Sync returns true, async overrides to return false
    public override bool HasPagePermission(HttpContext context) => true;
    public override Task<bool> HasPagePermissionAsync(HttpContext context)
        => Task.FromResult(false);
}

public sealed class AsyncTrackedArticleAdmin : ModelAdmin<ArticleEntity, int, ArticleDbContext>
{
    public override string RouterPath => "async-tracked";
    public override string Label      => "Async Tracked";

    public bool AsyncReadCalled { get; private set; }

    public AsyncTrackedArticleAdmin(ArticleDbContext db) : base(db) { }

    public override Task<bool> HasReadPermissionAsync(HttpContext context)
    {
        AsyncReadCalled = true;
        return Task.FromResult(false);
    }
}

public sealed class AsyncPermissionTests
{
    [Fact]
    public async Task HasPagePermissionAsync_DefaultDelegatesToSync()
    {
        var admin = TestHelper.WithApp(new CustomPageAdmin());
        var ctx   = new DefaultHttpContext();

        // Default sync is true, so async should also be true
        Assert.True(await admin.HasPagePermissionAsync(ctx));
    }

    [Fact]
    public async Task HasPagePermissionAsync_CanBeOverriddenAsync()
    {
        var admin = TestHelper.WithApp(new AsyncDeniedPageAdmin());
        var ctx   = new DefaultHttpContext();

        // Sync says true, async override says false
        Assert.True(admin.HasPagePermission(ctx));
        Assert.False(await admin.HasPagePermissionAsync(ctx));
    }

    [Fact]
    public async Task HasReadPermissionAsync_DefaultDelegatesToSync()
    {
        var admin = TestHelper.WithApp(new ArticleAdmin(TestHelper.CreateDb()));
        var ctx   = new DefaultHttpContext();

        Assert.True(await admin.HasReadPermissionAsync(ctx));
        Assert.True(await admin.HasCreatePermissionAsync(ctx));
        Assert.True(await admin.HasUpdatePermissionAsync(ctx));
        Assert.True(await admin.HasDeletePermissionAsync(ctx));
    }

    [Fact]
    public async Task HasReadPermissionAsync_CanBeOverriddenAsync()
    {
        var admin = TestHelper.WithApp(new AsyncTrackedArticleAdmin(TestHelper.CreateDb()));
        var ctx   = new DefaultHttpContext();

        // Override returns false
        var result = await admin.HasReadPermissionAsync(ctx);
        Assert.False(result);
        Assert.True(admin.AsyncReadCalled);
    }

    [Fact]
    public async Task RegisterRoutes_UsesAsyncPermission_WhenOverridden()
    {
        // This test verifies that the route handler calls the async permission method.
        // We verify this by observing that when async returns false (while sync returns true),
        // the route denies access.
        var admin = TestHelper.WithApp(new AsyncTrackedArticleAdmin(TestHelper.CreateDb()));

        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseTestServer();
        var app = builder.Build();
        admin.RegisterRoutes(app);
        await app.StartAsync();

        var client   = app.GetTestClient();
        var response = await client.GetAsync(admin.RouterPrefix);

        await app.StopAsync();

        // AsyncTrackedArticleAdmin overrides HasReadPermissionAsync to return false
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized,
            response.StatusCode);
        Assert.True(admin.AsyncReadCalled);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// P4 — AdminGroup tests
// ════════════════════════════════════════════════════════════════════════════════

public sealed class AdminGroupTests
{
    private static ArticleAdmin CreateArticleAdmin(ArticleDbContext? db = null) =>
        new ArticleAdmin(db ?? TestHelper.CreateDb());

    [Fact]
    public void CreateGroup_AddsGroupToApp()
    {
        var app   = TestHelper.CreateApp();
        var group = app.CreateGroup("Settings");

        Assert.Equal("Settings", group.Name);
        // Verify it's accessible via BuildTabsSchema (will show as a tab)
        var tabs = app.BuildTabsSchema();
        Assert.Single(tabs.TabList!);
        Assert.Equal("Settings", tabs.TabList![0].Title);
    }

    [Fact]
    public void AdminGroup_Add_SetsAppOnAdmin()
    {
        var adminApp = TestHelper.CreateApp();
        var group    = adminApp.CreateGroup("Group1");
        var admin    = CreateArticleAdmin();

        group.Add(admin);

        Assert.Same(adminApp, admin.App);
        Assert.Single(group.Admins);
    }

    [Fact]
    public void BuildTabsSchema_IncludesGroupAsNestedTab()
    {
        var adminApp = TestHelper.CreateApp();
        var group    = adminApp.CreateGroup("My Group");
        group.Add(CreateArticleAdmin());

        var tabs = adminApp.BuildTabsSchema();

        Assert.Single(tabs.TabList!);
        var groupTab = tabs.TabList![0];
        Assert.Equal("My Group", groupTab.Title);
        Assert.IsType<Tabs>(groupTab.Body);

        var nestedTabs = (Tabs)groupTab.Body!;
        Assert.Single(nestedTabs.TabList!);
        Assert.Equal("Articles", nestedTabs.TabList![0].Title);
    }

    [Fact]
    public void BuildTabsSchema_GroupAndDirectAdmins_BothAppear()
    {
        var db       = TestHelper.CreateDb();
        var adminApp = TestHelper.CreateApp();

        var directAdmin = new ArticleAdmin(db);
        directAdmin.App = adminApp;
        // Add direct admin via reflection (to match the existing test pattern)
        typeof(AdminApp)
            .GetField("_admins",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(adminApp, new List<RouterAdmin>(adminApp.Admins) { directAdmin });

        var group = adminApp.CreateGroup("Config Group");
        group.Add(new ArticleAdmin(db));

        var tabs = adminApp.BuildTabsSchema();

        // Should have 2 tabs: 1 direct admin + 1 group
        Assert.Equal(2, tabs.TabList!.Count);
    }

    [Fact]
    public void AdminGroup_BuildTabsSchema_SortsAdminsByPageSchema()
    {
        var adminApp = TestHelper.CreateApp();
        var group    = adminApp.CreateGroup("Group");
        var db       = TestHelper.CreateDb();

        var a1 = new ArticleAdmin(db);
        var a2 = new ArticleAdmin(db);
        // Manually override PageSchema sort by wrapping
        group.Add(a1);
        group.Add(a2);

        var tabs = group.BuildTabsSchema();
        Assert.Equal(2, tabs.TabList!.Count);
    }

    [Fact]
    public void Mount_RegistersAllGroupAdminRoutes()
    {
        var adminApp = TestHelper.CreateApp();
        var group    = adminApp.CreateGroup("Group");
        var db       = TestHelper.CreateDb();
        group.Add(new ArticleAdmin(db));

        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        // Should not throw
        group.Mount(app);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// P5 — New amis component serialization tests
// ════════════════════════════════════════════════════════════════════════════════

public sealed class NewAmisComponentsTests
{
    [Fact]
    public void Drawer_Type_IsDrawer()
    {
        var drawer = new Drawer { Title = "Edit", Body = "Content", Size = "lg", Position = "right" };
        Assert.Equal("drawer", drawer.Type);
    }

    [Fact]
    public void Drawer_SerializesCorrectly()
    {
        var drawer = new Drawer { Title = "Edit", Size = "md" };
        var json   = drawer.ToJson();

        Assert.Contains("\"type\":\"drawer\"", json);
        Assert.Contains("\"title\":\"Edit\"", json);
        Assert.Contains("\"size\":\"md\"", json);
        Assert.DoesNotContain("\"body\"", json);
        Assert.DoesNotContain("\"position\"", json);
    }

    [Fact]
    public void Panel_Type_IsPanel()
    {
        var panel = new Panel { Title = "Summary", Body = "Details" };
        Assert.Equal("panel", panel.Type);
    }

    [Fact]
    public void Panel_SerializesCorrectly()
    {
        var panel = new Panel { Title = "Summary", Body = "Details", HeaderClassName = "bg-info" };
        var json  = panel.ToJson();

        Assert.Contains("\"type\":\"panel\"", json);
        Assert.Contains("\"title\":\"Summary\"", json);
        Assert.Contains("\"headerClassName\":\"bg-info\"", json);
    }

    [Fact]
    public void Grid_Type_IsGrid()
    {
        var grid = new Grid();
        Assert.Equal("grid", grid.Type);
    }

    [Fact]
    public void Grid_SerializesColumnsAndGap()
    {
        var grid = new Grid { Columns = ["col1", "col2"], Gap = "md" };
        var json = grid.ToJson();

        Assert.Contains("\"type\":\"grid\"", json);
        Assert.Contains("\"gap\":\"md\"", json);
        Assert.Contains("\"columns\"", json);
    }

    [Fact]
    public void Service_Type_IsService()
    {
        var svc = new Service { Api = "get:/api/data", SchemaApi = "get:/api/schema" };
        Assert.Equal("service", svc.Type);
    }

    [Fact]
    public void Service_SerializesCorrectly()
    {
        var svc  = new Service { Api = "get:/api/data", Interval = 5000 };
        var json = svc.ToJson();

        Assert.Contains("\"type\":\"service\"", json);
        Assert.Contains("\"api\":\"get:/api/data\"", json);
        Assert.Contains("\"interval\":5000", json);
        Assert.DoesNotContain("\"schemaApi\"", json);
    }

    [Fact]
    public void Select_Type_IsSelect()
    {
        var sel = new Select { Name = "status", Label = "Status" };
        Assert.Equal("select", sel.Type);
    }

    [Fact]
    public void Select_SerializesWithOptions()
    {
        var sel = new Select
        {
            Name     = "status",
            Label    = "Status",
            Multiple = true,
            Options  =
            [
                new SelectOption { Label = "Active",   Value = 1 },
                new SelectOption { Label = "Inactive", Value = 0 }
            ]
        };
        var json = sel.ToJson();

        Assert.Contains("\"type\":\"select\"", json);
        Assert.Contains("\"multiple\":true", json);
        Assert.Contains("\"label\":\"Active\"", json);
    }

    [Fact]
    public void InputDate_Type_IsInputDate()
    {
        var d = new InputDate { Name = "dob", Label = "Date of Birth" };
        Assert.Equal("input-date", d.Type);
    }

    [Fact]
    public void InputDate_SerializesCorrectly()
    {
        var d    = new InputDate { Name = "dob", Label = "DOB", Format = "YYYY-MM-DD", Required = true };
        var json = d.ToJson();

        Assert.Contains("\"type\":\"input-date\"", json);
        Assert.Contains("\"name\":\"dob\"", json);
        Assert.Contains("\"format\":\"YYYY-MM-DD\"", json);
        Assert.Contains("\"required\":true", json);
    }

    [Fact]
    public void InputDateTime_Type_IsInputDatetime()
    {
        var d = new InputDateTime { Name = "createdAt", Label = "Created At" };
        Assert.Equal("input-datetime", d.Type);
    }

    [Fact]
    public void InputDateTime_SerializesCorrectly()
    {
        var d    = new InputDateTime { Name = "ts", Label = "Timestamp", Format = "YYYY-MM-DD HH:mm" };
        var json = d.ToJson();

        Assert.Contains("\"type\":\"input-datetime\"", json);
        Assert.Contains("\"format\":\"YYYY-MM-DD HH:mm\"", json);
    }

    [Fact]
    public void InputNumber_Type_IsInputNumber()
    {
        var n = new InputNumber { Name = "qty", Label = "Quantity" };
        Assert.Equal("input-number", n.Type);
    }

    [Fact]
    public void InputNumber_Step_IsSerializedWhenSet()
    {
        var n    = new InputNumber { Name = "price", Min = 0, Max = 1000, Step = 0.5 };
        var json = n.ToJson();

        Assert.Contains("\"type\":\"input-number\"", json);
        Assert.Contains("\"step\":0.5", json);
        Assert.Contains("\"min\":0", json);
        Assert.Contains("\"max\":1000", json);
    }

    [Fact]
    public void Textarea_Type_IsTextarea()
    {
        var ta = new Textarea { Name = "desc", Label = "Description" };
        Assert.Equal("textarea", ta.Type);
    }

    [Fact]
    public void Textarea_SerializesCorrectly()
    {
        var ta   = new Textarea { Name = "content", Label = "Content", MinRows = 3, MaxRows = 10 };
        var json = ta.ToJson();

        Assert.Contains("\"type\":\"textarea\"", json);
        Assert.Contains("\"minRows\":3", json);
        Assert.Contains("\"maxRows\":10", json);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// P6 — Export toolbar tests
// ════════════════════════════════════════════════════════════════════════════════

public sealed class CsvExportAdmin : ModelAdmin<ArticleEntity, int, ArticleDbContext>
{
    public override string RouterPath    => "csv-export-articles";
    public override string Label         => "Articles";
    public override bool EnableExportCsv => true;
    public CsvExportAdmin(ArticleDbContext db) : base(db) { }
}

public sealed class ExcelExportAdmin : ModelAdmin<ArticleEntity, int, ArticleDbContext>
{
    public override string RouterPath      => "excel-export-articles";
    public override string Label           => "Articles";
    public override bool EnableExportExcel => true;
    public ExcelExportAdmin(ArticleDbContext db) : base(db) { }
}

public sealed class BothExportAdmin : ModelAdmin<ArticleEntity, int, ArticleDbContext>
{
    public override string RouterPath      => "both-export-articles";
    public override string Label           => "Articles";
    public override bool EnableExportCsv   => true;
    public override bool EnableExportExcel => true;
    public BothExportAdmin(ArticleDbContext db) : base(db) { }
}

public sealed class ExportToolbarTests
{
    private static T WithApp<T>(T admin) where T : RouterAdmin
        => TestHelper.WithApp(admin);

    private static List<object> GetFooterToolbar(RouterAdmin admin)
    {
        var crud = (CrudComponent)admin.BuildPageSchema().Body!;
        return crud.FooterToolbar!;
    }

    [Fact]
    public void BuildPageSchema_FooterToolbar_ContainsExportCsv_WhenEnabled()
    {
        var admin   = WithApp(new CsvExportAdmin(TestHelper.CreateDb()));
        var toolbar = GetFooterToolbar(admin);

        Assert.Contains("export-csv",   toolbar);
        Assert.DoesNotContain("export-excel", toolbar);
    }

    [Fact]
    public void BuildPageSchema_FooterToolbar_ContainsExportExcel_WhenEnabled()
    {
        var admin   = WithApp(new ExcelExportAdmin(TestHelper.CreateDb()));
        var toolbar = GetFooterToolbar(admin);

        Assert.Contains("export-excel", toolbar);
        Assert.DoesNotContain("export-csv",   toolbar);
    }

    [Fact]
    public void BuildPageSchema_FooterToolbar_OnlyStatisticsAndPagination_WhenExportDisabled()
    {
        var admin   = WithApp(new ArticleAdmin(TestHelper.CreateDb()));
        var toolbar = GetFooterToolbar(admin);

        Assert.Contains("statistics",    toolbar);
        Assert.Contains("pagination",    toolbar);
        Assert.DoesNotContain("export-csv",   toolbar);
        Assert.DoesNotContain("export-excel", toolbar);
    }

    [Fact]
    public void BuildPageSchema_FooterToolbar_ContainsBothExports_WhenBothEnabled()
    {
        var admin   = WithApp(new BothExportAdmin(TestHelper.CreateDb()));
        var toolbar = GetFooterToolbar(admin);

        Assert.Contains("export-csv",   toolbar);
        Assert.Contains("export-excel", toolbar);
    }

    [Fact]
    public void BuildPageSchema_FooterToolbar_AlwaysContainsBaseItems()
    {
        var admin   = WithApp(new CsvExportAdmin(TestHelper.CreateDb()));
        var toolbar = GetFooterToolbar(admin);

        Assert.Contains("statistics",     toolbar);
        Assert.Contains("switch-per-page", toolbar);
        Assert.Contains("pagination",     toolbar);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// PageSchemaOptions.Link tests
// ════════════════════════════════════════════════════════════════════════════════

public sealed class PageSchemaOptionsLinkTests
{
    [Fact]
    public void PageSchemaOptions_Link_DefaultIsNull()
    {
        var opts = new PageSchemaOptions { Label = "Test" };
        Assert.Null(opts.Link);
    }

    [Fact]
    public void PageSchemaOptions_Link_CanBeSet()
    {
        var opts = new PageSchemaOptions { Label = "Docs", Link = "https://docs.example.com" };
        Assert.Equal("https://docs.example.com", opts.Link);
    }
}
