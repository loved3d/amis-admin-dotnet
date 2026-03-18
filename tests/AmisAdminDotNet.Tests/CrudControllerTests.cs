using System.Text.Json;
using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Crud;
using AmisAdminDotNet.Models;
using Microsoft.EntityFrameworkCore;

namespace AmisAdminDotNet.Tests;

// ── Fixtures ──────────────────────────────────────────────────────────────────

/// <summary>Sample entity used across CrudController and ModelParser tests.</summary>
public sealed class SampleEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class SampleDbContext : DbContext
{
    public DbSet<SampleEntity> Samples => Set<SampleEntity>();

    public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }
}

/// <summary>Concrete CrudController over SampleEntity for testing.</summary>
public sealed class SampleCrudController : CrudController<SampleEntity, int, SampleDbContext>
{
    public SampleCrudController(SampleDbContext db) : base(db) { }
}

// ── CrudController tests ──────────────────────────────────────────────────────

public sealed class CrudControllerTests
{
    private static SampleDbContext CreateContext()
    {
        var opts = new DbContextOptionsBuilder<SampleDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SampleDbContext(opts);
    }

    [Fact]
    public void GetItems_ReturnsPagedResult()
    {
        using var db = CreateContext();
        db.Samples.AddRange(
            new SampleEntity { Id = 1, Name = "Alpha", IsActive = true,  CreatedAt = DateTime.UtcNow },
            new SampleEntity { Id = 2, Name = "Beta",  IsActive = false, CreatedAt = DateTime.UtcNow },
            new SampleEntity { Id = 3, Name = "Gamma", IsActive = true,  CreatedAt = DateTime.UtcNow });
        db.SaveChanges();

        var ctrl = new SampleCrudController(db);
        var result = ctrl.GetItems(page: 1, perPage: 2);

        Assert.Equal(3, result.Total);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public void GetItems_SecondPage_ReturnsRemainingItems()
    {
        using var db = CreateContext();
        db.Samples.AddRange(
            new SampleEntity { Id = 1, Name = "A" },
            new SampleEntity { Id = 2, Name = "B" },
            new SampleEntity { Id = 3, Name = "C" });
        db.SaveChanges();

        var ctrl = new SampleCrudController(db);
        var result = ctrl.GetItems(page: 2, perPage: 2);

        Assert.Equal(3, result.Total);
        Assert.Single(result.Items);
    }

    [Fact]
    public void CreateItem_PersistsEntityAndReturnsIt()
    {
        using var db = CreateContext();
        var ctrl = new SampleCrudController(db);

        var entity = new SampleEntity { Id = 10, Name = "New", IsActive = true };
        var created = ctrl.CreateItem(entity);

        Assert.Equal("New", created.Name);
        Assert.Equal(1, db.Samples.Count());
    }

    [Fact]
    public void UpdateItem_ExistingId_UpdatesAndReturnsEntity()
    {
        using var db = CreateContext();
        db.Samples.Add(new SampleEntity { Id = 1, Name = "Old" });
        db.SaveChanges();

        var ctrl = new SampleCrudController(db);
        var updated = ctrl.UpdateItem(1, new SampleEntity { Id = 1, Name = "Updated" });

        Assert.NotNull(updated);
        Assert.Equal("Updated", updated!.Name);
        Assert.Equal("Updated", db.Samples.Find(1)!.Name);
    }

    [Fact]
    public void UpdateItem_MissingId_ReturnsNull()
    {
        using var db = CreateContext();
        var ctrl = new SampleCrudController(db);

        var result = ctrl.UpdateItem(999, new SampleEntity { Id = 999, Name = "Ghost" });

        Assert.Null(result);
    }

    [Fact]
    public void DeleteItem_ExistingId_RemovesAndReturnsTrue()
    {
        using var db = CreateContext();
        db.Samples.Add(new SampleEntity { Id = 1, Name = "ToDelete" });
        db.SaveChanges();

        var ctrl = new SampleCrudController(db);
        var deleted = ctrl.DeleteItem(1);

        Assert.True(deleted);
        Assert.Equal(0, db.Samples.Count());
    }

    [Fact]
    public void DeleteItem_MissingId_ReturnsFalse()
    {
        using var db = CreateContext();
        var ctrl = new SampleCrudController(db);

        Assert.False(ctrl.DeleteItem(42));
    }
}

// ── ModelParser tests ─────────────────────────────────────────────────────────

public sealed class ModelParserTests
{
    [Fact]
    public void ParseFields_DetectsIdAsPrimaryKey()
    {
        var fields = TableModelParser.ParseFields(typeof(SampleEntity));
        var idField = fields.Single(f => f.Name == "Id");

        Assert.True(idField.IsPrimaryKey);
    }

    [Fact]
    public void ParseFields_MapsBoolToSwitch()
    {
        var fields = TableModelParser.ParseFields(typeof(SampleEntity));
        var activeField = fields.Single(f => f.Name == "IsActive");

        Assert.Equal("switch", activeField.AmisInputType);
        Assert.Equal("mapping", activeField.AmisColumnType);
    }

    [Fact]
    public void ParseFields_MapsDateTimeToInputDatetime()
    {
        var fields = TableModelParser.ParseFields(typeof(SampleEntity));
        var dateField = fields.Single(f => f.Name == "CreatedAt");

        Assert.Equal("input-datetime", dateField.AmisInputType);
        Assert.Equal("datetime", dateField.AmisColumnType);
    }

    [Fact]
    public void ParseFields_MapsStringToInputText()
    {
        var fields = TableModelParser.ParseFields(typeof(SampleEntity));
        var nameField = fields.Single(f => f.Name == "Name");

        Assert.Equal("input-text", nameField.AmisInputType);
        Assert.Equal("", nameField.AmisColumnType);
    }

    [Fact]
    public void ParseFields_CamelCasesColumnName()
    {
        var fields = TableModelParser.ParseFields(typeof(SampleEntity));
        var active = fields.Single(f => f.Name == "IsActive");

        Assert.Equal("isActive", active.ColumnName);
    }

    [Fact]
    public void ParseColumns_ReturnsTableColumnForEveryProperty()
    {
        var cols = TableModelParser.ParseColumns(typeof(SampleEntity));

        Assert.Equal(4, cols.Count); // Id, Name, IsActive, CreatedAt
        Assert.Contains(cols, c => c.Name == "id");
        Assert.Contains(cols, c => c.Name == "name");
        Assert.Contains(cols, c => c.Name == "isActive");
        Assert.Contains(cols, c => c.Name == "createdAt");
    }

    [Fact]
    public void ParseColumns_BoolField_HasMappingType()
    {
        var cols = TableModelParser.ParseColumns(typeof(SampleEntity));
        var active = cols.Single(c => c.Name == "isActive");

        Assert.Equal("mapping", active.Type);
        Assert.NotNull(active.Map);
        Assert.True(active.Map!.ContainsKey("true"));
        Assert.True(active.Map!.ContainsKey("false"));
    }

    [Fact]
    public void ParseColumns_DateTimeField_HasDatetimeType()
    {
        var cols = TableModelParser.ParseColumns(typeof(SampleEntity));
        var dateCol = cols.Single(c => c.Name == "createdAt");

        Assert.Equal("datetime", dateCol.Type);
    }

    [Fact]
    public void ParseFormFields_ExcludesPrimaryKey()
    {
        var fields = TableModelParser.ParseFormFields(typeof(SampleEntity)).ToList();

        // Id should not appear
        Assert.DoesNotContain(fields, f =>
            f is InputText it && it.Name == "id"
            || f is InputNumber num && num.Name == "id");

        // Name, IsActive and CreatedAt should appear
        Assert.Equal(3, fields.Count);
    }

    [Fact]
    public void ParseFormFields_BoolField_ProducesSwitch()
    {
        var fields = TableModelParser.ParseFormFields(typeof(SampleEntity));
        Assert.Contains(fields, f => f is Switch sw && sw.Name == "isActive");
    }

    [Fact]
    public void ParseFormFields_StringField_ProducesInputText()
    {
        var fields = TableModelParser.ParseFormFields(typeof(SampleEntity));
        Assert.Contains(fields, f => f is InputText it && it.Name == "name");
    }
}
