using AmisAdminDotNet.Models;
using Microsoft.EntityFrameworkCore;

namespace AmisAdminDotNet.Crud;

/// <summary>
/// EF Core–backed generic CRUD base class. Mirrors Python's <c>SqlalchemyCrud</c> from
/// <c>fastapi_amis_admin/crud/base.py</c>.
///
/// <para>
/// Provides four standard operations:
/// <list type="bullet">
///   <item><term>GetItems</term>  <description>GET  /items  — paged list</description></item>
///   <item><term>CreateItem</term><description>POST /item   — insert new entity</description></item>
///   <item><term>UpdateItem</term><description>PUT  /item/{id} — update existing entity</description></item>
///   <item><term>DeleteItem</term><description>DELETE /item/{id} — remove entity</description></item>
/// </list>
/// </para>
///
/// <para>
/// <typeparamref name="TDbContext"/> is supplied through constructor injection, replacing
/// SQLAlchemy's <c>async_session</c> dependency injection used in the Python version.
/// </para>
/// </summary>
/// <typeparam name="TEntity">EF Core entity type.</typeparam>
/// <typeparam name="TKey">Primary-key type (e.g. <see cref="int"/>, <see cref="Guid"/>).</typeparam>
/// <typeparam name="TDbContext">Concrete <see cref="DbContext"/> type.</typeparam>
public abstract class CrudController<TEntity, TKey, TDbContext>
    where TEntity : class
    where TDbContext : DbContext
{
    /// <summary>The EF Core database context supplied via constructor injection.</summary>
    protected TDbContext Db { get; }

    protected CrudController(TDbContext db)
    {
        Db = db;
    }

    /// <summary>
    /// Returns a paged list of all entities. Maps to Python <c>SqlalchemyCrud.select()</c>.
    /// <c>GET /items</c>
    /// </summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="perPage">Page size (clamped to 1–100).</param>
    public virtual PagedResult<TEntity> GetItems(int page = 1, int perPage = 10)
    {
        page = Math.Max(page, 1);
        perPage = Math.Clamp(perPage, 1, 100);

        var set = Db.Set<TEntity>();
        var total = set.Count();
        var items = set
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();

        return new PagedResult<TEntity>(items, total);
    }

    /// <summary>
    /// Async equivalent of <see cref="GetItems"/> using EF Core async APIs.
    /// </summary>
    public virtual async Task<PagedResult<TEntity>> GetItemsAsync(int page = 1, int perPage = 10)
    {
        page = Math.Max(page, 1);
        perPage = Math.Clamp(perPage, 1, 100);

        var set = Db.Set<TEntity>().AsNoTracking();
        var total = await set.CountAsync();
        var items = await set
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync();

        return new PagedResult<TEntity>(items, total);
    }

    /// <summary>
    /// Persists a new entity and returns it with its generated key populated.
    /// Maps to Python <c>SqlalchemyCrud.create()</c>.
    /// <c>POST /item</c>
    /// </summary>
    public virtual TEntity CreateItem(TEntity entity)
    {
        Db.Set<TEntity>().Add(entity);
        Db.SaveChanges();
        return entity;
    }

    public virtual async Task<TEntity> CreateItemAsync(TEntity entity)
    {
        Db.Set<TEntity>().Add(entity);
        await Db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Overwrites the entity identified by <paramref name="id"/> with values from
    /// <paramref name="entity"/>. Returns <c>null</c> when the entity is not found.
    /// Maps to Python <c>SqlalchemyCrud.update()</c>.
    /// <c>PUT /item/{id}</c>
    /// </summary>
    public virtual TEntity? UpdateItem(TKey id, TEntity entity)
    {
        var existing = Db.Set<TEntity>().Find(id);
        if (existing is null)
            return null;

        Db.Entry(existing).CurrentValues.SetValues(entity);
        Db.SaveChanges();
        return existing;
    }

    public virtual async Task<TEntity?> UpdateItemAsync(TKey id, TEntity entity)
    {
        var existing = await Db.Set<TEntity>().FindAsync(new object?[] { id });
        if (existing is null)
            return null;

        Db.Entry(existing).CurrentValues.SetValues(entity);
        await Db.SaveChangesAsync();
        return existing;
    }

    /// <summary>
    /// Removes the entity identified by <paramref name="id"/>.
    /// Returns <c>true</c> when found and removed, <c>false</c> otherwise.
    /// Maps to Python <c>SqlalchemyCrud.delete()</c>.
    /// <c>DELETE /item/{id}</c>
    /// </summary>
    public virtual bool DeleteItem(TKey id)
    {
        var existing = Db.Set<TEntity>().Find(id);
        if (existing is null)
            return false;

        Db.Set<TEntity>().Remove(existing);
        Db.SaveChanges();
        return true;
    }

    public virtual async Task<bool> DeleteItemAsync(TKey id)
    {
        var existing = await Db.Set<TEntity>().FindAsync(new object?[] { id });
        if (existing is null)
            return false;

        Db.Set<TEntity>().Remove(existing);
        await Db.SaveChangesAsync();
        return true;
    }
}
