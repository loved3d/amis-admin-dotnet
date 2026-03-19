using System.Security.Claims;
using System.Text.Json;
using AmisAdminDotNet.Admin;
using AmisAdminDotNet.Admin.Extensions;
using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Crud;
using AmisAdminDotNet.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AmisAdminDotNet.Tests;

// ─── Test entity types (prefixed Ext* to avoid conflicts with NewFeaturesTests) ─

public sealed class ExtArticleEntity
{
    public int      Id         { get; set; }
    public string   Title      { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
}

public sealed class ExtArticleDbContext : DbContext
{
    public DbSet<ExtArticleEntity> Articles => Set<ExtArticleEntity>();
    public ExtArticleDbContext(DbContextOptions<ExtArticleDbContext> opts) : base(opts) { }
}

public sealed class ExtSoftEntity
{
    public int       Id         { get; set; }
    public string    Title      { get; set; } = string.Empty;
    public DateTime  CreateTime { get; set; }
    public DateTime  UpdateTime { get; set; }
    public DateTime? DeleteTime { get; set; }
}

public sealed class ExtSoftDbContext : DbContext
{
    public DbSet<ExtSoftEntity> Items => Set<ExtSoftEntity>();
    public ExtSoftDbContext(DbContextOptions<ExtSoftDbContext> opts) : base(opts) { }
}

public enum ExtArticleStatus { Draft = 0, Published = 1, Archived = 2 }

public sealed class ExtEnumEntity
{
    public int              Id     { get; set; }
    public string           Title  { get; set; } = string.Empty;
    public ExtArticleStatus Status { get; set; }
}

public sealed class ExtEnumDbContext : DbContext
{
    public DbSet<ExtEnumEntity> Items => Set<ExtEnumEntity>();
    public ExtEnumDbContext(DbContextOptions<ExtEnumDbContext> opts) : base(opts) { }
}

public sealed class ExtBoolEntity
{
    public int  Id       { get; set; }
    public bool IsActive { get; set; }
}

// ─── Concrete admin implementations ─────────────────────────────────────────

public sealed class ReadOnlyExtArticleAdmin
    : ReadOnlyModelAdmin<ExtArticleEntity, int, ExtArticleDbContext>
{
    public override string RouterPath => "ext-articles-ro";
    public override string Label      => "Articles (RO)";
    public ReadOnlyExtArticleAdmin(ExtArticleDbContext db) : base(db) { }
}

public sealed class AutoTimeExtArticleAdmin
    : AutoTimeModelAdmin<ExtArticleEntity, int, ExtArticleDbContext>
{
    public override string RouterPath => "ext-articles-at";
    public override string Label      => "Articles (AT)";
    public AutoTimeExtArticleAdmin(ExtArticleDbContext db) : base(db) { }
}

public sealed class SoftDeleteExtArticleAdmin
    : SoftDeleteModelAdmin<ExtSoftEntity, int, ExtSoftDbContext>
{
    public override string RouterPath => "ext-articles-sd";
    public override string Label      => "Articles (SD)";
    public SoftDeleteExtArticleAdmin(ExtSoftDbContext db) : base(db) { }
}

public sealed class FootableExtArticleAdmin
    : FootableModelAdmin<ExtArticleEntity, int, ExtArticleDbContext>
{
    public override string RouterPath => "ext-articles-ft";
    public override string Label      => "Articles (FT)";
    public FootableExtArticleAdmin(ExtArticleDbContext db) : base(db) { }
}

public sealed class AuthenticatedExtArticleAdmin
    : AuthenticatedModelAdmin<ExtArticleEntity, int, ExtArticleDbContext>
{
    public override string RouterPath => "ext-articles-auth";
    public override string Label      => "Articles (Auth)";
    public AuthenticatedExtArticleAdmin(ExtArticleDbContext db) : base(db) { }
}

public sealed class RoleExtArticleAdmin
    : RoleBasedModelAdmin<ExtArticleEntity, int, ExtArticleDbContext>
{
    public override string RouterPath   => "ext-articles-role";
    public override string Label        => "Articles (Role)";
    public override string RequiredRole => "Admin";
    public RoleExtArticleAdmin(ExtArticleDbContext db) : base(db) { }
}

public sealed class PolicyExtArticleAdmin
    : PolicyBasedModelAdmin<ExtArticleEntity, int, ExtArticleDbContext>
{
    public override string RouterPath      => "ext-articles-policy";
    public override string Label           => "Articles (Policy)";
    public override string RequiredPolicy  => "AdminPolicy";
    public PolicyExtArticleAdmin(ExtArticleDbContext db, IAuthorizationService authSvc)
        : base(db, authSvc) { }
}

public sealed class ExtCustomAdminAction : AdminAction
{
    public override string ActionName => "export";
    public override string Label      => "Export";
    public override Task<object> HandleAsync(HttpContext context)
        => Task.FromResult<object>(new { ok = true });
}

/// <summary>Non-sealed admin for action testing so we can expose protected member.</summary>
public class ActionExtArticleAdmin
    : ModelAdmin<ExtArticleEntity, int, ExtArticleDbContext>
{
    public override string RouterPath => "ext-articles-actions";
    public override string Label      => "Articles (Actions)";
    public ActionExtArticleAdmin(ExtArticleDbContext db) : base(db) { }

    protected override IEnumerable<AdminAction> GetAdminActions()
        => [new ExtCustomAdminAction()];

    /// <summary>Expose protected GetAdminActions for testing.</summary>
    public IReadOnlyList<AdminAction> GetAdminActionsPublic()
        => GetAdminActions().ToList();
}

// ─── Helper factory ──────────────────────────────────────────────────────────

file static class ExtTestHelpers
{
    public static TAdmin CreateAdmin<TAdmin, TDb>(
        Func<DbContextOptions<TDb>, TDb> dbFactory,
        Func<TDb, TAdmin> adminFactory)
        where TAdmin : RouterAdmin
        where TDb : DbContext
    {
        var opts  = new DbContextOptionsBuilder<TDb>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db    = dbFactory(opts);
        var admin = adminFactory(db);
        admin.App = new AdminApp("Main", "/admin",
            new AdminSiteSettings { AdminPath = "/admin" },
            new ServiceCollection().BuildServiceProvider());
        return admin;
    }

    public static HttpContext AuthenticatedContext(string? role = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.NameIdentifier, "1")
        };
        if (role is not null)
            claims.Add(new(ClaimTypes.Role, role));

        var identity  = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        return new DefaultHttpContext { User = principal };
    }

    public static HttpContext UnauthenticatedContext() => new DefaultHttpContext();
}

// ═══════════════════════════════════════════════════════════════════════════
// ReadOnlyModelAdmin Tests
// ═══════════════════════════════════════════════════════════════════════════

public sealed class ReadOnlyModelAdminTests
{
    private static ReadOnlyExtArticleAdmin CreateAdmin() =>
        ExtTestHelpers.CreateAdmin<ReadOnlyExtArticleAdmin, ExtArticleDbContext>(
            o => new ExtArticleDbContext(o),
            db => new ReadOnlyExtArticleAdmin(db));

    [Fact]
    public void HasCreatePermission_ReturnsFalse()
    {
        var admin = CreateAdmin();
        Assert.False(admin.HasCreatePermission(new DefaultHttpContext()));
    }

    [Fact]
    public void HasUpdatePermission_ReturnsFalse()
    {
        var admin = CreateAdmin();
        Assert.False(admin.HasUpdatePermission(new DefaultHttpContext()));
    }

    [Fact]
    public void HasDeletePermission_ReturnsFalse()
    {
        var admin = CreateAdmin();
        Assert.False(admin.HasDeletePermission(new DefaultHttpContext()));
    }

    [Fact]
    public void BuildPageSchema_DoesNotContainCreateButton()
    {
        var admin = CreateAdmin();
        var json  = JsonSerializer.Serialize(admin.BuildPageSchema());

        // Should NOT include a "Create" label button
        Assert.DoesNotContain("Create Articles", json);
    }

    [Fact]
    public void BuildPageSchema_OperationColumn_ContainsViewNotEditOrDelete()
    {
        var admin = CreateAdmin();
        var json  = JsonSerializer.Serialize(admin.BuildPageSchema());

        Assert.Contains("View", json);
        Assert.DoesNotContain("\"Edit\"",   json);
        Assert.DoesNotContain("\"Delete\"", json);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// AutoTimeModelAdmin Tests
// ═══════════════════════════════════════════════════════════════════════════

public sealed class AutoTimeModelAdminTests
{
    private static AutoTimeExtArticleAdmin CreateAdmin() =>
        ExtTestHelpers.CreateAdmin<AutoTimeExtArticleAdmin, ExtArticleDbContext>(
            o => new ExtArticleDbContext(o),
            db => new AutoTimeExtArticleAdmin(db));

    [Fact]
    public void GetCreateFormFields_ExcludesIdAndTimestampFields()
    {
        var admin  = CreateAdmin();
        var json   = JsonSerializer.Serialize(admin.GetCreateAction());

        // "id", "createTime", "updateTime" should NOT appear as field names
        Assert.DoesNotContain("\"name\":\"id\"",          json);
        Assert.DoesNotContain("\"name\":\"createTime\"", json);
        Assert.DoesNotContain("\"name\":\"updateTime\"", json);
        // "title" should be present
        Assert.Contains("\"name\":\"title\"", json);
    }

    [Fact]
    public void GetUpdateFormFields_ExcludesIdAndTimestampFields()
    {
        var admin = CreateAdmin();
        var json  = JsonSerializer.Serialize(admin.GetUpdateAction());

        Assert.DoesNotContain("\"name\":\"id\"",          json);
        Assert.DoesNotContain("\"name\":\"createTime\"", json);
        Assert.DoesNotContain("\"name\":\"updateTime\"", json);
        Assert.Contains("\"name\":\"title\"", json);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// SoftDeleteModelAdmin Tests
// ═══════════════════════════════════════════════════════════════════════════

public sealed class SoftDeleteModelAdminTests
{
    private static (SoftDeleteExtArticleAdmin admin, ExtSoftDbContext db) CreateAdmin()
    {
        var opts = new DbContextOptionsBuilder<ExtSoftDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db    = new ExtSoftDbContext(opts);
        db.Database.EnsureCreated();

        var admin = new SoftDeleteExtArticleAdmin(db);
        admin.App = new AdminApp("Main", "/admin",
            new AdminSiteSettings { AdminPath = "/admin" },
            new ServiceCollection().BuildServiceProvider());
        return (admin, db);
    }

    [Fact]
    public async Task DeleteItemAsync_SetsDeleteTime_NotPhysicalDelete()
    {
        var (admin, db) = CreateAdmin();

        var entity = new ExtSoftEntity { Title = "Test" };
        db.Items.Add(entity);
        await db.SaveChangesAsync();

        var deleted = await admin.DeleteItemAsync(entity.Id);

        Assert.True(deleted);
        var inDb = db.Items.IgnoreQueryFilters().FirstOrDefault(e => e.Id == entity.Id);
        Assert.NotNull(inDb);
        Assert.NotNull(inDb.DeleteTime);
    }

    [Fact]
    public void DeleteItem_SetsDeleteTime_NotPhysicalDelete()
    {
        var (admin, db) = CreateAdmin();

        var entity = new ExtSoftEntity { Title = "Test2" };
        db.Items.Add(entity);
        db.SaveChanges();

        var deleted = admin.DeleteItem(entity.Id);

        Assert.True(deleted);
        var inDb = db.Items.IgnoreQueryFilters().FirstOrDefault(e => e.Id == entity.Id);
        Assert.NotNull(inDb);
        Assert.NotNull(inDb.DeleteTime);
    }

    [Fact]
    public void ApplyFilter_ExcludesSoftDeletedRecords()
    {
        var (admin, db) = CreateAdmin();

        var active  = new ExtSoftEntity { Title = "Active" };
        var deleted = new ExtSoftEntity { Title = "Deleted", DeleteTime = DateTime.UtcNow.AddDays(-1) };
        db.Items.AddRange(active, deleted);
        db.SaveChanges();

        var query    = db.Items.AsQueryable();
        var filtered = admin.ApplyFilter(query, new CrudQueryParams());
        var results  = filtered.ToList();

        Assert.Single(results);
        Assert.Equal("Active", results[0].Title);
    }

    [Fact]
    public void Constructor_ThrowsWhenEntityLacksDeleteTimeProperty()
    {
        var opts = new DbContextOptionsBuilder<ExtArticleDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new ExtArticleDbContext(opts);

        Assert.Throws<InvalidOperationException>(() =>
            new InvalidSoftDeleteExtAdmin(db));
    }
}

/// <summary>Deliberately invalid: uses ExtArticleEntity which has no DeleteTime.</summary>
file sealed class InvalidSoftDeleteExtAdmin
    : SoftDeleteModelAdmin<ExtArticleEntity, int, ExtArticleDbContext>
{
    public override string RouterPath => "invalid-ext";
    public InvalidSoftDeleteExtAdmin(ExtArticleDbContext db) : base(db) { }
}

// ═══════════════════════════════════════════════════════════════════════════
// AuthenticatedModelAdmin Tests
// ═══════════════════════════════════════════════════════════════════════════

public sealed class AuthenticatedModelAdminTests
{
    private static AuthenticatedExtArticleAdmin CreateAdmin() =>
        ExtTestHelpers.CreateAdmin<AuthenticatedExtArticleAdmin, ExtArticleDbContext>(
            o => new ExtArticleDbContext(o),
            db => new AuthenticatedExtArticleAdmin(db));

    [Fact]
    public void HasPagePermission_UnauthenticatedUser_ReturnsFalse()
    {
        var admin = CreateAdmin();
        Assert.False(admin.HasPagePermission(ExtTestHelpers.UnauthenticatedContext()));
    }

    [Fact]
    public void HasPagePermission_AuthenticatedUser_ReturnsTrue()
    {
        var admin = CreateAdmin();
        Assert.True(admin.HasPagePermission(ExtTestHelpers.AuthenticatedContext()));
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// RoleBasedModelAdmin Tests
// ═══════════════════════════════════════════════════════════════════════════

public sealed class RoleBasedModelAdminTests
{
    private static RoleExtArticleAdmin CreateAdmin() =>
        ExtTestHelpers.CreateAdmin<RoleExtArticleAdmin, ExtArticleDbContext>(
            o => new ExtArticleDbContext(o),
            db => new RoleExtArticleAdmin(db));

    [Fact]
    public void HasPagePermission_UnauthenticatedUser_ReturnsFalse()
    {
        var admin = CreateAdmin();
        Assert.False(admin.HasPagePermission(ExtTestHelpers.UnauthenticatedContext()));
    }

    [Fact]
    public void HasPagePermission_AuthenticatedButWrongRole_ReturnsFalse()
    {
        var admin = CreateAdmin();
        Assert.False(admin.HasPagePermission(ExtTestHelpers.AuthenticatedContext("User")));
    }

    [Fact]
    public void HasPagePermission_AuthenticatedWithRequiredRole_ReturnsTrue()
    {
        var admin = CreateAdmin();
        Assert.True(admin.HasPagePermission(ExtTestHelpers.AuthenticatedContext("Admin")));
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// AdminAction Tests
// ═══════════════════════════════════════════════════════════════════════════

public sealed class AdminActionTests
{
    private static ActionExtArticleAdmin CreateAdmin() =>
        ExtTestHelpers.CreateAdmin<ActionExtArticleAdmin, ExtArticleDbContext>(
            o => new ExtArticleDbContext(o),
            db => new ActionExtArticleAdmin(db));

    [Fact]
    public void BuildActionButton_ContainsCorrectApiPath()
    {
        var admin   = CreateAdmin();
        var actions = admin.GetAdminActionsPublic();

        Assert.Single(actions);

        var button = actions[0].BuildActionButton(admin.RouterPrefix);
        var json   = JsonSerializer.Serialize(button);

        Assert.Contains($"/admin/{admin.RouterPath}/actions/export", json);
    }

    [Fact]
    public void BuildPageSchema_HeaderToolbar_ContainsActionButton()
    {
        var admin = CreateAdmin();
        var json  = JsonSerializer.Serialize(admin.BuildPageSchema());

        Assert.Contains("export", json);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// SelectPerm Tests
// ═══════════════════════════════════════════════════════════════════════════

public sealed class SelectPermTests
{
    private static ExtSoftDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<ExtSoftDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new ExtSoftDbContext(opts);
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public void RecentTimeSelectPerm_FiltersOldRecords()
    {
        using var db = CreateDb();

        var old   = new ExtSoftEntity { Title = "Old",   CreateTime = DateTime.UtcNow.AddDays(-30) };
        var fresh = new ExtSoftEntity { Title = "Fresh", CreateTime = DateTime.UtcNow };
        db.Items.AddRange(old, fresh);
        db.SaveChanges();

        var perm = new RecentTimeSelectPerm<ExtSoftEntity>("CreateTime", seconds: 3600);
        var ctx  = new DefaultHttpContext();

        var result = perm.Apply(db.Items.AsQueryable(), ctx).ToList();

        Assert.Single(result);
        Assert.Equal("Fresh", result[0].Title);
    }

    [Fact]
    public void SimpleSelectPerm_FiltersNonMatchingValues()
    {
        using var db = CreateDb();

        db.Items.AddRange(
            new ExtSoftEntity { Title = "A" },
            new ExtSoftEntity { Title = "B" },
            new ExtSoftEntity { Title = "C" });
        db.SaveChanges();

        var perm   = new SimpleSelectPerm<ExtSoftEntity, string>("Title", ["A", "C"]);
        var ctx    = new DefaultHttpContext();
        var result = perm.Apply(db.Items.AsQueryable(), ctx).ToList();

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, r => r.Title == "B");
    }

    [Fact]
    public void SimpleSelectPerm_EmptyValues_ReturnsAll()
    {
        using var db = CreateDb();

        db.Items.AddRange(
            new ExtSoftEntity { Title = "X" },
            new ExtSoftEntity { Title = "Y" });
        db.SaveChanges();

        var perm   = new SimpleSelectPerm<ExtSoftEntity, string>("Title", []);
        var ctx    = new DefaultHttpContext();
        var result = perm.Apply(db.Items.AsQueryable(), ctx).ToList();

        Assert.Equal(2, result.Count);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// ModelParser Enum Tests
// ═══════════════════════════════════════════════════════════════════════════

public sealed class ModelParserEnumTests
{
    [Fact]
    public void ParseColumns_EnumField_HasMappingTypeWithEnumValues()
    {
        var columns   = TableModelParser.ParseColumns(typeof(ExtEnumEntity));
        var statusCol = columns.FirstOrDefault(c => c.Name == "status");

        Assert.NotNull(statusCol);
        Assert.Equal("mapping", statusCol.Type);
        Assert.NotNull(statusCol.Map);
        Assert.Contains("0", statusCol.Map!.Keys);
        Assert.Contains("1", statusCol.Map!.Keys);
        Assert.Contains("Draft",     statusCol.Map!.Values);
        Assert.Contains("Published", statusCol.Map!.Values);
    }

    [Fact]
    public void ParseFormFields_EnumField_ReturnsSelectComponent()
    {
        var fields      = TableModelParser.ParseFormFields(typeof(ExtEnumEntity));
        var statusField = fields.FirstOrDefault(f => f is Select s && s.Name == "status");

        Assert.NotNull(statusField);
        var select = Assert.IsType<Select>(statusField);
        Assert.Equal(3, select.Options?.Count);
    }

    [Fact]
    public void ParseColumns_BoolField_HasQuickEdit()
    {
        var cols      = TableModelParser.ParseColumns(typeof(ExtBoolEntity));
        var activeCol = cols.FirstOrDefault(c => c.Name == "isActive");

        Assert.NotNull(activeCol);
        Assert.NotNull(activeCol.QuickEdit);
        var json = JsonSerializer.Serialize(activeCol.QuickEdit);
        Assert.Contains("switch", json);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// FootableModelAdmin Tests
// ═══════════════════════════════════════════════════════════════════════════

public sealed class FootableModelAdminTests
{
    private static FootableExtArticleAdmin CreateAdmin() =>
        ExtTestHelpers.CreateAdmin<FootableExtArticleAdmin, ExtArticleDbContext>(
            o => new ExtArticleDbContext(o),
            db => new FootableExtArticleAdmin(db));

    [Fact]
    public void BuildPageSchema_CrudComponent_HasFootableTrue()
    {
        var admin = CreateAdmin();
        var json  = JsonSerializer.Serialize(admin.BuildPageSchema());
        Assert.Contains("\"footable\":true", json);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// BulkDelete Tests
// ═══════════════════════════════════════════════════════════════════════════

public sealed class BulkDeleteTests
{
    private static (ProductAdmin admin, ProductDbContext db) CreateAdmin()
    {
        var opts = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db    = new ProductDbContext(opts);
        db.Database.EnsureCreated();
        var admin = new ProductAdmin(db);
        admin.App = new AdminApp("Main", "/admin",
            new AdminSiteSettings { AdminPath = "/admin" },
            new ServiceCollection().BuildServiceProvider());
        return (admin, db);
    }

    [Fact]
    public async Task BulkDeleteItemAsync_DeletesAllMatchingIds()
    {
        var (admin, db) = CreateAdmin();

        db.Products.AddRange(
            new ProductEntity { Title = "P1", Price = 1 },
            new ProductEntity { Title = "P2", Price = 2 },
            new ProductEntity { Title = "P3", Price = 3 });
        await db.SaveChangesAsync();

        var ids   = db.Products.Select(p => p.Id).Take(2).ToList();
        var count = await admin.BulkDeleteItemAsync(ids);

        Assert.Equal(2, count);
        Assert.Equal(1, db.Products.Count());
    }

    [Fact]
    public void GetBulkDeleteAction_ReturnsAjaxButtonWithBulkRoute()
    {
        var (admin, _) = CreateAdmin();
        var btn  = admin.GetBulkDeleteAction();
        var json = JsonSerializer.Serialize(btn);

        Assert.Contains("ajax",  json);
        Assert.Contains("/bulk", json);
    }

    [Fact]
    public void BuildPageSchema_BulkActions_ContainsBulkDeleteButton()
    {
        var (admin, _) = CreateAdmin();
        var json = JsonSerializer.Serialize(admin.BuildPageSchema());

        Assert.Contains("bulk", json);
    }
}
