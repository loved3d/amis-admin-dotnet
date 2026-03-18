using AmisAdminDotNet.Admin;
using Microsoft.EntityFrameworkCore;

namespace AmisAdminDotNet.Demo;

/// <summary>
/// Minimal entity used by the demo admin to show the full
/// <see cref="ModelAdmin{TEntity,TKey,TDbContext}"/> pipeline in action.
/// </summary>
public sealed class DemoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Done { get; set; }
}

/// <summary>
/// EF Core DbContext for <see cref="DemoItem"/>. Uses an in-memory database so
/// no external database is required for the demo.
/// </summary>
public sealed class DemoDbContext : DbContext
{
    public DbSet<DemoItem> Items => Set<DemoItem>();

    public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options) { }
}

/// <summary>
/// Concrete <see cref="ModelAdmin{TEntity,TKey,TDbContext}"/> for <see cref="DemoItem"/>.
/// Registering this admin with an <see cref="AmisAdminDotNet.Admin.AdminSite"/> causes
/// four CRUD endpoints (<c>GET/POST/PUT/DELETE /admin/demo-items</c>) plus an amis schema
/// to be generated automatically from the entity model — demonstrating the full
/// <c>ModelAdmin → AdminApp → AdminSite</c> pattern end-to-end.
/// </summary>
public sealed class DemoAdmin : ModelAdmin<DemoItem, int, DemoDbContext>
{
    public override string RouterPath => "demo-items";
    public override string Label => "Demo Items";

    public DemoAdmin(DemoDbContext db) : base(db) { }
}
