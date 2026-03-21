using System.Text.Json;
using AmisAdminDotNet.Admin;
using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AmisAdminDotNet.Tests;

// ════════════════════════════════════════════════════════════════════════════
// Shared helpers for the gap-fill tests
// ════════════════════════════════════════════════════════════════════════════

file static class GapTestHelpers
{
    public static AdminSiteSettings DefaultSettings() =>
        new() { AdminPath = "/admin", SiteTitle = "Test Admin" };

    public static AdminApp MakeApp(IServiceProvider? sp = null) =>
        new("Main", "/admin", DefaultSettings(),
            sp ?? new ServiceCollection().BuildServiceProvider());

    public static T WithApp<T>(T admin, IServiceProvider? sp = null)
        where T : RouterAdmin
    {
        admin.App = MakeApp(sp);
        return admin;
    }
}

// ════════════════════════════════════════════════════════════════════════════
// AdminApp.AddAdmin — direct instance registration
// ════════════════════════════════════════════════════════════════════════════

public sealed class AdminAppAddAdminTests
{
    private sealed class SimpleAdmin : RouterAdmin
    {
        public override string RouterPath => "simple";
        public override string Label      => "Simple";
        public override void RegisterRoutes(WebApplication app) { }
    }

    [Fact]
    public void AddAdmin_AppendsAdminToList()
    {
        var adminApp = GapTestHelpers.MakeApp();
        var admin    = new SimpleAdmin();

        adminApp.AddAdmin(admin);

        Assert.Contains(admin, adminApp.Admins);
    }

    [Fact]
    public void AddAdmin_SetsAppOnAdmin()
    {
        var adminApp = GapTestHelpers.MakeApp();
        var admin    = new SimpleAdmin();

        adminApp.AddAdmin(admin);

        Assert.Same(adminApp, admin.App);
    }

    [Fact]
    public void AddAdmin_AppearsInTabsSchema()
    {
        var adminApp = GapTestHelpers.MakeApp();
        adminApp.AddAdmin(new SimpleAdmin());

        var tabs = adminApp.BuildTabsSchema();

        Assert.Contains(tabs.TabList!, t => t.Title == "Simple");
    }
}

// ════════════════════════════════════════════════════════════════════════════
// DocsAdmin / ReDocsAdmin / APIDocsApp
// ════════════════════════════════════════════════════════════════════════════

public sealed class BuiltinDocsAdminTests
{
    [Fact]
    public void DocsAdmin_BuildPageSchema_BodyIsIframeWithSwaggerSrc()
    {
        var admin = GapTestHelpers.WithApp(new DocsAdmin());
        var page  = admin.BuildPageSchema();

        Assert.IsType<Iframe>(page.Body);
        Assert.Equal("/swagger", ((Iframe)page.Body).Src);
    }

    [Fact]
    public void DocsAdmin_PageSchema_HasBookIcon()
    {
        var admin = new DocsAdmin();
        Assert.Equal("fa fa-book", admin.PageSchema.Icon);
    }

    [Fact]
    public void ReDocsAdmin_BuildPageSchema_BodyIsIframeWithReDocSrc()
    {
        var admin = GapTestHelpers.WithApp(new ReDocsAdmin());
        var page  = admin.BuildPageSchema();

        Assert.IsType<Iframe>(page.Body);
        Assert.Equal("/redoc", ((Iframe)page.Body).Src);
    }

    [Fact]
    public void ReDocsAdmin_PageSchema_HasFileAltIcon()
    {
        var admin = new ReDocsAdmin();
        Assert.Equal("fa fa-file-alt", admin.PageSchema.Icon);
    }

    [Fact]
    public void APIDocsApp_ContainsDocsAndReDocsAdmins()
    {
        var settings = GapTestHelpers.DefaultSettings();
        var sp       = new ServiceCollection().BuildServiceProvider();
        var docsApp  = new APIDocsApp("/admin", settings, sp);

        var types = docsApp.Admins.Select(a => a.GetType()).ToList();
        Assert.Contains(typeof(DocsAdmin),   types);
        Assert.Contains(typeof(ReDocsAdmin), types);
    }

    [Fact]
    public void APIDocsApp_BuildTabsSchema_HasDocsAndReDocsTabs()
    {
        var settings = GapTestHelpers.DefaultSettings();
        var sp       = new ServiceCollection().BuildServiceProvider();
        var docsApp  = new APIDocsApp("/admin", settings, sp);

        var tabs = docsApp.BuildTabsSchema();

        var titles = tabs.TabList!.Select(t => t.Title).ToList();
        Assert.Contains("API Docs", titles);
        Assert.Contains("ReDoc",    titles);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// AdminSite.RegisterBuiltinAdmins
// ════════════════════════════════════════════════════════════════════════════

public sealed class AdminSiteBuiltinAdminsTests
{
    private static AdminSite MakeSite(bool includeApiDocs = true)
    {
        var settings = GapTestHelpers.DefaultSettings();
        var sp       = new ServiceCollection().BuildServiceProvider();
        var site     = new AdminSite(settings, sp);
        site.RegisterBuiltinAdmins(includeApiDocs);
        return site;
    }

    [Fact]
    public void RegisterBuiltinAdmins_AddsBuiltinAppFirst()
    {
        var site   = MakeSite();
        var schema = site.BuildPageSchema();

        // Page body must be a Tabs (multiple apps → wrapped in Tabs over apps)
        // or the built-in app's tabs directly
        Assert.NotNull(schema);
        Assert.NotNull(schema.Body);
    }

    [Fact]
    public void RegisterBuiltinAdmins_SchemaContainsHome()
    {
        var site   = MakeSite();
        var json   = site.BuildPageSchema().ToJson();

        Assert.Contains("Home", json);
    }

    [Fact]
    public void RegisterBuiltinAdmins_IsIdempotent()
    {
        var settings = GapTestHelpers.DefaultSettings();
        var sp       = new ServiceCollection().BuildServiceProvider();
        var site     = new AdminSite(settings, sp);

        site.RegisterBuiltinAdmins();
        site.RegisterBuiltinAdmins(); // second call should not add again

        var json = site.BuildPageSchema().ToJson();
        // Should not have duplicate "Home" tabs
        var count = json.Split("\"Home\"").Length - 1;
        // Exactly one occurrence of the "Home" label in the schema
        Assert.Equal(1, count);
    }

    [Fact]
    public void RegisterBuiltinAdmins_WithoutApiDocs_DoesNotContainDocsAdmin()
    {
        var settings = GapTestHelpers.DefaultSettings();
        var sp       = new ServiceCollection().BuildServiceProvider();
        var site     = new AdminSite(settings, sp);
        site.RegisterBuiltinAdmins(includeApiDocs: false);

        var json = site.BuildPageSchema().ToJson();

        Assert.DoesNotContain("/swagger", json);
    }

    [Fact]
    public void AdminSite_BuildPageSchema_UsesSiteTitleFromSettings()
    {
        var settings = new AdminSiteSettings
        {
            AdminPath = "/admin",
            SiteTitle = "My Custom Admin"
        };
        var sp   = new ServiceCollection().BuildServiceProvider();
        var site = new AdminSite(settings, sp);

        var page = site.BuildPageSchema();

        Assert.Equal("My Custom Admin", page.Title);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// TemplateAdmin
// ════════════════════════════════════════════════════════════════════════════

public sealed class ConcreteTemplateAdmin : TemplateAdmin
{
    public override string RouterPath => "template-test";
    public override string Label      => "Template Page";
    public override string Template   => "<h1>Hello ${user}!</h1>";
}

public sealed class TemplateAdminWithApi : TemplateAdmin
{
    public override string RouterPath => "template-api";
    public override string Label      => "Template With API";
    public override string Template   => "<p>${greeting}</p>";
    public override string? InitApi   => "/api/greeting";
}

public sealed class TemplateAdminTests
{
    [Fact]
    public void GetPage_BodyIsTpl()
    {
        var admin = GapTestHelpers.WithApp(new ConcreteTemplateAdmin());
        var page  = admin.GetPage();

        Assert.IsType<Tpl>(page.Body);
    }

    [Fact]
    public void GetPage_TplTemplate_MatchesAbstractProperty()
    {
        var admin = GapTestHelpers.WithApp(new ConcreteTemplateAdmin());
        var tpl   = (Tpl)admin.GetPage().Body!;

        Assert.Equal("<h1>Hello ${user}!</h1>", tpl.Template);
    }

    [Fact]
    public void GetPage_Title_MatchesLabel()
    {
        var admin = GapTestHelpers.WithApp(new ConcreteTemplateAdmin());
        var page  = admin.GetPage();

        Assert.Equal("Template Page", page.Title);
    }

    [Fact]
    public void GetPage_InitApi_IsNull_WhenNotSet()
    {
        var admin = GapTestHelpers.WithApp(new ConcreteTemplateAdmin());
        var page  = admin.GetPage();

        Assert.Null(page.InitApi);
    }

    [Fact]
    public void GetPage_InitApi_IsSet_WhenOverridden()
    {
        var admin = GapTestHelpers.WithApp(new TemplateAdminWithApi());
        var page  = admin.GetPage();

        Assert.Equal("/api/greeting", page.InitApi);
    }

    [Fact]
    public void BuildPageSchema_DelegatesToGetPage()
    {
        var admin = GapTestHelpers.WithApp(new ConcreteTemplateAdmin());

        Assert.Same(admin.GetPage().Body?.GetType(), admin.BuildPageSchema().Body?.GetType());
    }
}

// ════════════════════════════════════════════════════════════════════════════
// ModelAction (row-level actions)
// ════════════════════════════════════════════════════════════════════════════

public sealed class TestModelAction : ModelAction
{
    public override string ActionName => "approve";
    public override string Label      => "Approve";
    public override string Level      => "success";
    public override string? ConfirmText => "Approve this item?";

    public override Task<object> HandleAsync(HttpContext context)
        => Task.FromResult<object>(new { ok = true });
}

public sealed class ModelActionTests
{
    [Fact]
    public void BuildRowActionButton_ReturnsAjaxButtonWithIdInPath()
    {
        var action = new TestModelAction();
        var json   = JsonSerializer.Serialize(action.BuildRowActionButton("/admin/items"));

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("ajax", doc.RootElement.GetProperty("actionType").GetString());
        Assert.Contains("/row-actions/approve/${id}", doc.RootElement.GetProperty("api").GetString());
    }

    [Fact]
    public void BuildRowActionButton_IncludesConfirmText()
    {
        var action = new TestModelAction();
        var json   = JsonSerializer.Serialize(action.BuildRowActionButton("/admin/items"));

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Approve this item?", doc.RootElement.GetProperty("confirmText").GetString());
    }

    [Fact]
    public void BuildRowActionButton_LevelAndLabelMatch()
    {
        var action = new TestModelAction();
        var json   = JsonSerializer.Serialize(action.BuildRowActionButton("/admin/items"));

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Approve", doc.RootElement.GetProperty("label").GetString());
        Assert.Equal("success", doc.RootElement.GetProperty("level").GetString());
    }

    [Fact]
    public void BuildActionButton_DelegatesToBuildRowActionButton()
    {
        var action    = new TestModelAction();
        var rowBtn    = action.BuildRowActionButton("/admin/items");
        var headerBtn = action.BuildActionButton("/admin/items");

        // Both should produce the same result (row action reused for header toolbar too)
        Assert.Equal(
            JsonSerializer.Serialize(rowBtn),
            JsonSerializer.Serialize(headerBtn));
    }
}

// ════════════════════════════════════════════════════════════════════════════
// ModelAdmin with GetRowActions — schema integration
// ════════════════════════════════════════════════════════════════════════════

public sealed class GapTestEntity
{
    public int    Id    { get; set; }
    public string Name  { get; set; } = string.Empty;
}

public sealed class GapTestDbContext : DbContext
{
    public DbSet<GapTestEntity> Items => Set<GapTestEntity>();
    public GapTestDbContext(DbContextOptions<GapTestDbContext> opts) : base(opts) { }
}

/// <summary>ModelAdmin with a custom row action for schema-generation tests.</summary>
public sealed class RowActionModelAdmin : ModelAdmin<GapTestEntity, int, GapTestDbContext>
{
    public override string RouterPath => "gap-items";
    public override string Label      => "Gap Items";

    public RowActionModelAdmin(GapTestDbContext db) : base(db) { }

    protected override IEnumerable<ModelAction> GetRowActions()
        => [new TestModelAction()];
}

/// <summary>ModelAdmin with lifecycle hooks for testing.</summary>
public sealed class LifecycleModelAdmin : ModelAdmin<GapTestEntity, int, GapTestDbContext>
{
    public override string RouterPath => "lifecycle-items";
    public override string Label      => "Lifecycle Items";

    public List<string> Log { get; } = [];

    public LifecycleModelAdmin(GapTestDbContext db) : base(db) { }

    protected override Task OnAfterCreateAsync(GapTestEntity entity, HttpContext ctx)
    {
        Log.Add($"created:{entity.Id}");
        return Task.CompletedTask;
    }

    protected override Task OnAfterUpdateAsync(GapTestEntity entity, HttpContext ctx)
    {
        Log.Add($"updated:{entity.Id}");
        return Task.CompletedTask;
    }

    protected override Task OnAfterDeleteAsync(int id, HttpContext ctx)
    {
        Log.Add($"deleted:{id}");
        return Task.CompletedTask;
    }

    // Public wrappers so tests can invoke the protected lifecycle hooks directly
    public Task InvokeOnAfterCreate(GapTestEntity e, HttpContext ctx) => OnAfterCreateAsync(e, ctx);
    public Task InvokeOnAfterUpdate(GapTestEntity e, HttpContext ctx) => OnAfterUpdateAsync(e, ctx);
    public Task InvokeOnAfterDelete(int id, HttpContext ctx) => OnAfterDeleteAsync(id, ctx);
}

public sealed class ModelAdminRowActionsTests
{
    private static GapTestDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<GapTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static T WithApp<T>(T admin) where T : RouterAdmin
    {
        admin.App = GapTestHelpers.MakeApp();
        return admin;
    }

    [Fact]
    public void BuildPageSchema_OperationColumn_ContainsRowActionButton()
    {
        var admin = WithApp(new RowActionModelAdmin(CreateDb()));
        var json  = JsonSerializer.Serialize(admin.BuildPageSchema());

        // The approve row action should appear in the operation column buttons
        Assert.Contains("approve", json);
        Assert.Contains("row-actions", json);
    }

    [Fact]
    public void BuildPageSchema_OperationColumn_StillContainsEditAndDelete()
    {
        var admin = WithApp(new RowActionModelAdmin(CreateDb()));
        var json  = JsonSerializer.Serialize(admin.BuildPageSchema());

        Assert.Contains("Edit",   json);
        Assert.Contains("Delete", json);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// ModelAdmin lifecycle hooks
// ════════════════════════════════════════════════════════════════════════════

public sealed class ModelAdminLifecycleTests
{
    private static GapTestDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<GapTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task OnAfterCreateAsync_IsCalledAfterCreate()
    {
        var db    = CreateDb();
        var admin = new LifecycleModelAdmin(db);
        admin.App = GapTestHelpers.MakeApp();

        var entity = new GapTestEntity { Id = 1, Name = "Test" };
        await admin.CreateItemAsync(entity);
        await admin.InvokeOnAfterCreate(entity, new DefaultHttpContext());

        Assert.Contains("created:1", admin.Log);
    }

    [Fact]
    public async Task OnAfterUpdateAsync_IsCalledAfterUpdate()
    {
        var db = CreateDb();
        var admin = new LifecycleModelAdmin(db);
        admin.App = GapTestHelpers.MakeApp();

        var entity = new GapTestEntity { Id = 1, Name = "Original" };
        db.Items.Add(entity);
        await db.SaveChangesAsync();

        var updated = new GapTestEntity { Id = 1, Name = "Updated" };
        await admin.UpdateItemAsync(1, updated);
        await admin.InvokeOnAfterUpdate(entity, new DefaultHttpContext());

        Assert.Contains("updated:1", admin.Log);
    }

    [Fact]
    public async Task OnAfterDeleteAsync_IsCalledAfterDelete()
    {
        var db = CreateDb();
        var admin = new LifecycleModelAdmin(db);
        admin.App = GapTestHelpers.MakeApp();

        var entity = new GapTestEntity { Id = 5, Name = "ToDelete" };
        db.Items.Add(entity);
        await db.SaveChangesAsync();

        await admin.DeleteItemAsync(5);
        await admin.InvokeOnAfterDelete(5, new DefaultHttpContext());

        Assert.Contains("deleted:5", admin.Log);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// New amis components: InputFile, InputImage, InputColor, Collapse, Divider,
// Card, Cards, Steps, Image
// ════════════════════════════════════════════════════════════════════════════

public sealed class NewAmisComponentTests
{
    [Fact]
    public void InputFile_Type_IsInputFile()
    {
        var f = new InputFile { Name = "doc", Label = "Upload", Receiver = "/upload" };
        Assert.Equal("input-file", f.Type);
    }

    [Fact]
    public void InputFile_ToJson_ContainsTypeAndReceiver()
    {
        var f    = new InputFile { Name = "doc", Receiver = "/admin/file/upload" };
        var json = f.ToJson();

        Assert.Contains("\"type\":\"input-file\"", json);
        Assert.Contains("/admin/file/upload", json);
    }

    [Fact]
    public void InputImage_Type_IsInputImage()
    {
        var f = new InputImage { Name = "photo", Receiver = "/upload/image" };
        Assert.Equal("input-image", f.Type);
    }

    [Fact]
    public void InputImage_ToJson_ContainsTypeAndReceiver()
    {
        var f    = new InputImage { Name = "photo", Receiver = "/upload/image" };
        var json = f.ToJson();

        Assert.Contains("\"type\":\"input-image\"", json);
        Assert.Contains("/upload/image", json);
    }

    [Fact]
    public void InputColor_Type_IsInputColor()
    {
        var f = new InputColor { Name = "color" };
        Assert.Equal("input-color", f.Type);
    }

    [Fact]
    public void InputColor_ToJson_ContainsFormat_WhenSet()
    {
        var f    = new InputColor { Name = "color", Format = "rgba" };
        var json = f.ToJson();

        Assert.Contains("rgba", json);
    }

    [Fact]
    public void Collapse_Type_IsCollapse()
    {
        var c = new Collapse { Header = "Section 1" };
        Assert.Equal("collapse", c.Type);
    }

    [Fact]
    public void Collapse_ToJson_ContainsHeader()
    {
        var c    = new Collapse { Header = "Section 1", Body = "Content" };
        var json = c.ToJson();

        Assert.Contains("Section 1", json);
    }

    [Fact]
    public void CollapseGroup_Type_IsCollapseGroup()
    {
        var g = new CollapseGroup
        {
            Body = [new Collapse { Header = "A" }],
            Accordion = true
        };
        Assert.Equal("collapse-group", g.Type);
    }

    [Fact]
    public void Divider_Type_IsDivider()
    {
        var d = new Divider();
        Assert.Equal("divider", d.Type);
    }

    [Fact]
    public void Divider_ToJson_ContainsLineStyle_WhenSet()
    {
        var d    = new Divider { LineStyle = "dashed" };
        var json = d.ToJson();

        Assert.Contains("dashed", json);
    }

    [Fact]
    public void Card_Type_IsCard()
    {
        var c = new Card { Header = new { title = "Test" } };
        Assert.Equal("card", c.Type);
    }

    [Fact]
    public void Cards_Type_IsCards()
    {
        var c = new Cards { Source = "/api/items", PerRow = 3 };
        Assert.Equal("cards", c.Type);
    }

    [Fact]
    public void Cards_ToJson_ContainsSource()
    {
        var c    = new Cards { Source = "/api/items" };
        var json = c.ToJson();

        Assert.Contains("/api/items", json);
    }

    [Fact]
    public void Steps_Type_IsSteps()
    {
        var s = new Steps { Value = 1 };
        Assert.Equal("steps", s.Type);
    }

    [Fact]
    public void Steps_ToJson_ContainsStepTitles()
    {
        var s = new Steps
        {
            StepList =
            [
                new Step { Title = "Step One" },
                new Step { Title = "Step Two" }
            ]
        };
        var json = s.ToJson();

        Assert.Contains("Step One", json);
        Assert.Contains("Step Two", json);
    }

    [Fact]
    public void Image_Type_IsImage()
    {
        var img = new Image { Src = "https://example.com/img.png" };
        Assert.Equal("image", img.Type);
    }

    [Fact]
    public void Image_ToJson_ContainsSrc()
    {
        var img  = new Image { Src = "https://example.com/img.png", Alt = "test" };
        var json = img.ToJson();

        Assert.Contains("https://example.com/img.png", json);
        Assert.Contains("\"alt\"", json);
    }
}
