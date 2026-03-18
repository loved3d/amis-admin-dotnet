using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Crud;
using AmisAdminDotNet.Models;
using AmisAdminDotNet.Services;
using Microsoft.EntityFrameworkCore;
using CrudComponent = AmisAdminDotNet.AmisComponents.Crud;

namespace AmisAdminDotNet.Admin;

/// <summary>
/// Model-backed admin panel that combines EF Core CRUD operations with amis schema
/// generation. Mirrors Python's <c>ModelAdmin</c> from
/// <c>fastapi_amis_admin/admin/admin.py</c>.
///
/// <para>
/// In Python, <c>ModelAdmin</c> inherits from both <c>CrudAdmin</c> and
/// <c>RouterAdmin</c> via multiple inheritance. In C# — which allows only single
/// class inheritance — <see cref="ModelAdmin{TEntity,TKey,TDbContext}"/> extends
/// <see cref="RouterAdmin"/> (which extends <see cref="BaseAdmin"/>) and implements
/// the same CRUD methods as <see cref="CrudController{TEntity,TKey,TDbContext}"/>
/// directly, keeping the same public API contract.
/// </para>
///
/// <para>
/// Schema-generation methods correspond to Python helpers:
/// <list type="bullet">
///   <item><see cref="GetCreateAction"/> → Python <c>get_create_action()</c></item>
///   <item><see cref="GetUpdateAction"/> → Python <c>get_update_action()</c></item>
///   <item><see cref="GetDeleteAction"/> → Python <c>get_delete_action()</c></item>
///   <item><see cref="BuildPageSchema"/>  → Python <c>ModelAdmin.get_page()</c></item>
/// </list>
/// </para>
/// </summary>
/// <typeparam name="TEntity">EF Core entity type.</typeparam>
/// <typeparam name="TKey">Primary-key type (e.g. <see cref="int"/>, <see cref="Guid"/>).</typeparam>
/// <typeparam name="TDbContext">Concrete <see cref="DbContext"/> type.</typeparam>
public abstract class ModelAdmin<TEntity, TKey, TDbContext> : RouterAdmin
    where TEntity : class
    where TDbContext : DbContext
{
    /// <summary>The EF Core database context supplied via constructor injection.</summary>
    protected TDbContext Db { get; }

    protected ModelAdmin(TDbContext db)
    {
        Db = db;
    }

    // ── CRUD — mirrors CrudController<TEntity, TKey, TDbContext> API ──────────

    /// <summary>
    /// Returns a paged list of all entities. Maps to Python <c>SqlalchemyCrud.select()</c>.
    /// <c>GET {RouterPrefix}</c>
    /// </summary>
    public virtual PagedResult<TEntity> GetItems(int page = 1, int perPage = 10)
    {
        page    = Math.Max(page, 1);
        perPage = Math.Clamp(perPage, 1, 100);

        var set   = Db.Set<TEntity>();
        var total = set.Count();
        var items = set.Skip((page - 1) * perPage).Take(perPage).ToList();
        return new PagedResult<TEntity>(items, total);
    }

    /// <summary>
    /// Persists a new entity. Maps to Python <c>SqlalchemyCrud.create()</c>.
    /// <c>POST {RouterPrefix}</c>
    /// </summary>
    public virtual TEntity CreateItem(TEntity entity)
    {
        Db.Set<TEntity>().Add(entity);
        Db.SaveChanges();
        return entity;
    }

    /// <summary>
    /// Overwrites an entity by id. Maps to Python <c>SqlalchemyCrud.update()</c>.
    /// <c>PUT {RouterPrefix}/{id}</c>
    /// </summary>
    public virtual TEntity? UpdateItem(TKey id, TEntity entity)
    {
        var existing = Db.Set<TEntity>().Find(id);
        if (existing is null) return null;

        Db.Entry(existing).CurrentValues.SetValues(entity);
        Db.SaveChanges();
        return existing;
    }

    /// <summary>
    /// Removes an entity by id. Maps to Python <c>SqlalchemyCrud.delete()</c>.
    /// <c>DELETE {RouterPrefix}/{id}</c>
    /// </summary>
    public virtual bool DeleteItem(TKey id)
    {
        var existing = Db.Set<TEntity>().Find(id);
        if (existing is null) return false;

        Db.Set<TEntity>().Remove(existing);
        Db.SaveChanges();
        return true;
    }

    // ── Schema helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the amis column definitions for the list table, derived from the
    /// entity model via <see cref="TableModelParser.ParseColumns"/>.
    /// </summary>
    protected virtual IReadOnlyList<TableColumn> GetColumns() =>
        TableModelParser.ParseColumns(typeof(TEntity));

    /// <summary>
    /// Returns the amis form fields for create/update dialogs, derived from the
    /// entity model via <see cref="TableModelParser.ParseFormFields"/>.
    /// </summary>
    protected virtual IReadOnlyList<object> GetFormFields() =>
        TableModelParser.ParseFormFields(typeof(TEntity));

    /// <summary>
    /// Builds the create-button <see cref="Button"/> that opens a Dialog with a
    /// creation form. Maps to Python <c>ModelAdmin.get_create_action()</c>.
    /// </summary>
    public virtual Button GetCreateAction() => new()
    {
        Label      = $"Create {Label}",
        Level      = "primary",
        ActionType = "dialog",
        Dialog     = new Dialog
        {
            Title = $"Create {Label}",
            Body  = new Form
            {
                Mode   = "horizontal",
                Api    = $"post:{RouterPrefix}",
                Reload = RouterPath + "Crud",
                Body   = GetFormFields().ToList()
            }
        }
    };

    /// <summary>
    /// Builds the edit-button <see cref="Button"/> for each table row.
    /// Maps to Python <c>ModelAdmin.get_update_action()</c>.
    /// </summary>
    public virtual Button GetUpdateAction() => new()
    {
        Label      = "Edit",
        Level      = "link",
        ActionType = "dialog",
        Dialog     = new Dialog
        {
            Title = $"Edit {Label}",
            Body  = new Form
            {
                Mode   = "horizontal",
                Api    = $"put:{RouterPrefix}/${{id}}",
                Reload = RouterPath + "Crud",
                Body   = GetFormFields().ToList()
            }
        }
    };

    /// <summary>
    /// Builds the delete-button <see cref="Button"/> for each table row.
    /// Maps to Python <c>ModelAdmin.get_delete_action()</c>.
    /// </summary>
    public virtual Button GetDeleteAction() => new()
    {
        Label       = "Delete",
        Level       = "link",
        ClassName   = "text-danger",
        ActionType  = "ajax",
        ConfirmText = $"Delete this {Label}?",
        Api         = $"delete:{RouterPrefix}/${{id}}"
    };

    /// <summary>
    /// Builds the full amis <see cref="Page"/> schema for this model admin, including
    /// the CRUD component with list columns, create/update/delete actions.
    /// Maps to Python <c>ModelAdmin.get_page()</c>.
    /// </summary>
    public override Page BuildPageSchema()
    {
        var columns = GetColumns().Cast<object>().ToList();
        columns.Add(new OperationColumn
        {
            Label   = "Actions",
            Buttons = [GetUpdateAction(), GetDeleteAction()]
        });

        return new Page
        {
            Title = Label,
            Body  = new CrudComponent
            {
                Name          = RouterPath + "Crud",
                SyncLocation  = false,
                Api           = $"get:{RouterPrefix}",
                PerPage       = 10,
                HeaderToolbar = [GetCreateAction(), "bulkActions"],
                Columns       = columns
            }
        };
    }

    // ── Route registration ────────────────────────────────────────────────────

    /// <summary>
    /// Registers the four standard CRUD endpoints on the given
    /// <see cref="WebApplication"/>. Mirrors Python's
    /// <c>@router.add_api_route</c> decorators.
    /// </summary>
    public override void RegisterRoutes(WebApplication app)
    {
        var prefix = RouterPrefix;

        app.MapGet(prefix, (int page, int perPage) =>
        {
            var result = GetItems(page, perPage);
            return Results.Json(AdminApiResponse.Ok(new { items = result.Items, total = result.Total }));
        });

        app.MapPost(prefix, (TEntity entity) =>
        {
            var created = CreateItem(entity);
            return Results.Json(AdminApiResponse.Ok(new { item = created }, $"{Label} created."));
        });

        app.MapPut(prefix + "/{id}", (TKey id, TEntity entity) =>
        {
            var updated = UpdateItem(id, entity);
            return updated is null
                ? Results.Json(AdminApiResponse.Fail($"{Label} not found."))
                : Results.Json(AdminApiResponse.Ok(new { item = updated }, $"{Label} updated."));
        });

        app.MapDelete(prefix + "/{id}", (TKey id) =>
        {
            return DeleteItem(id)
                ? Results.Json(AdminApiResponse.Ok(msg: $"{Label} deleted."))
                : Results.Json(AdminApiResponse.Fail($"{Label} not found."));
        });
    }
}

