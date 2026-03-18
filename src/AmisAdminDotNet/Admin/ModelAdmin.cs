using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Crud;
using AmisAdminDotNet.Models;
using AmisAdminDotNet.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
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

    // ── Declarative configuration ────────────────────────────────────────────

    /// <summary>
    /// Properties to display as table columns (by name, case-insensitive).
    /// An empty array means "show all public properties" (current behaviour).
    /// Maps to Python <c>ModelAdmin.list_display</c>.
    /// </summary>
    public virtual string[] ListDisplay { get; } = [];

    /// <summary>
    /// Properties used for keyword search when <see cref="CrudQueryParams.Search"/> is non-null.
    /// Only <see cref="string"/> type properties are matched.
    /// Maps to Python <c>ModelAdmin.search_fields</c>.
    /// </summary>
    public virtual string[] SearchFields { get; } = [];

    /// <summary>
    /// Default number of rows per page. Maps to Python <c>ModelAdmin.list_per_page</c>.
    /// </summary>
    public virtual int ListPerPage { get; } = 10;

    /// <summary>
    /// Default ordering column and direction, e.g. <c>"Title"</c> or <c>"-CreatedAt"</c>.
    /// A leading <c>"-"</c> means descending. Maps to Python <c>ModelAdmin.ordering</c>.
    /// </summary>
    public virtual string? DefaultOrdering { get; } = null;

    /// <summary>
    /// Properties that are read-only in create/edit forms (rendered with disabled).
    /// Maps to Python <c>ModelAdmin.readonly_fields</c>.
    /// </summary>
    public virtual string[] ReadonlyFields { get; } = [];

    /// <summary>Whether to add export-csv to the list table footer. Maps to Python footerToolbar.</summary>
    public virtual bool EnableExportCsv { get; } = false;

    /// <summary>Whether to add export-excel to the list table footer.</summary>
    public virtual bool EnableExportExcel { get; } = false;

    // ── Permission hooks ──────────────────────────────────────────────────────

    /// <summary>
    /// Checks whether the context may perform READ (list/get) operations on this model.
    /// Defaults to <see cref="BaseAdmin.HasPagePermission"/>.
    /// </summary>
    public virtual bool HasReadPermission(HttpContext context) => HasPagePermission(context);

    /// <summary>
    /// Checks whether the context may perform CREATE operations on this model.
    /// Defaults to <see cref="BaseAdmin.HasPagePermission"/>.
    /// </summary>
    public virtual bool HasCreatePermission(HttpContext context) => HasPagePermission(context);

    /// <summary>
    /// Checks whether the context may perform UPDATE operations on this model.
    /// Defaults to <see cref="BaseAdmin.HasPagePermission"/>.
    /// </summary>
    public virtual bool HasUpdatePermission(HttpContext context) => HasPagePermission(context);

    /// <summary>
    /// Checks whether the context may perform DELETE operations on this model.
    /// Defaults to <see cref="BaseAdmin.HasPagePermission"/>.
    /// </summary>
    public virtual bool HasDeletePermission(HttpContext context) => HasPagePermission(context);

    /// <summary>
    /// Async version of <see cref="HasReadPermission"/>.
    /// Override for async authorization (e.g. database role lookup).
    /// Default implementation calls the synchronous <see cref="HasReadPermission"/>.
    /// </summary>
    public virtual Task<bool> HasReadPermissionAsync(HttpContext context)
        => Task.FromResult(HasReadPermission(context));

    /// <summary>
    /// Async version of <see cref="HasCreatePermission"/>.
    /// Default implementation calls the synchronous <see cref="HasCreatePermission"/>.
    /// </summary>
    public virtual Task<bool> HasCreatePermissionAsync(HttpContext context)
        => Task.FromResult(HasCreatePermission(context));

    /// <summary>
    /// Async version of <see cref="HasUpdatePermission"/>.
    /// Default implementation calls the synchronous <see cref="HasUpdatePermission"/>.
    /// </summary>
    public virtual Task<bool> HasUpdatePermissionAsync(HttpContext context)
        => Task.FromResult(HasUpdatePermission(context));

    /// <summary>
    /// Async version of <see cref="HasDeletePermission"/>.
    /// Default implementation calls the synchronous <see cref="HasDeletePermission"/>.
    /// </summary>
    public virtual Task<bool> HasDeletePermissionAsync(HttpContext context)
        => Task.FromResult(HasDeletePermission(context));

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

    public virtual async Task<PagedResult<TEntity>> GetItemsAsync(int page = 1, int perPage = 10)
    {
        page = Math.Max(page, 1);
        perPage = Math.Clamp(perPage, 1, 100);

        var set = Db.Set<TEntity>().AsNoTracking();
        var total = await set.CountAsync();
        var items = await set.Skip((page - 1) * perPage).Take(perPage).ToListAsync();
        return new PagedResult<TEntity>(items, total);
    }

    // ── Filter / ordering helpers ─────────────────────────────────────────────

    /// <summary>
    /// Applies additional filtering to <paramref name="query"/> based on
    /// <paramref name="p"/>. When <see cref="SearchFields"/> is non-empty and
    /// <see cref="CrudQueryParams.Search"/> is set, automatically filters to rows where
    /// any of the designated string properties contain the search term (case-insensitive).
    /// The filter is applied in-memory (via <c>AsEnumerable()</c>) which is appropriate
    /// for the EF Core InMemory provider; for production SQL databases, consider overriding
    /// to use EF.Functions.Like() or an expression tree for server-side evaluation.
    /// Override to add extra WHERE clauses.
    /// Maps to Python <c>SqlalchemyCrud.get_select()</c> filter composition.
    /// </summary>
    public virtual IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query, CrudQueryParams p)
    {
        if (SearchFields.Length > 0 && !string.IsNullOrEmpty(p.Search))
        {
            var search = p.Search;
            query = query
                .AsEnumerable()
                .Where(entity =>
                {
                    foreach (var fieldName in SearchFields)
                    {
                        var prop = typeof(TEntity).GetProperty(
                            fieldName,
                            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (prop?.PropertyType == typeof(string))
                        {
                            var value = (string?)prop.GetValue(entity);
                            if (value != null &&
                                value.Contains(search, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }
                    }
                    return false;
                })
                .AsQueryable();
        }
        return query;
    }

    /// <summary>
    /// Applies ORDER BY to <paramref name="query"/> using <see cref="CrudQueryParams.OrderBy"/>
    /// and <see cref="CrudQueryParams.OrderDir"/>. When <see cref="CrudQueryParams.OrderBy"/> is
    /// null and <see cref="DefaultOrdering"/> is set, the default ordering is used instead.
    /// A leading <c>"-"</c> in <see cref="DefaultOrdering"/> means descending.
    /// Property lookup is case-insensitive.
    /// Returns the query unchanged when no ordering column can be resolved.
    /// </summary>
    public virtual IQueryable<TEntity> ApplyOrdering(IQueryable<TEntity> query, CrudQueryParams p)
    {
        var orderBy  = p.OrderBy;
        var orderDir = p.OrderDir;

        if (string.IsNullOrEmpty(orderBy) && DefaultOrdering is not null)
        {
            if (DefaultOrdering.StartsWith('-'))
            {
                orderBy  = DefaultOrdering[1..];
                orderDir = "desc";
            }
            else
            {
                orderBy  = DefaultOrdering;
                orderDir = "asc";
            }
        }

        if (string.IsNullOrEmpty(orderBy))
            return query;

        var prop = typeof(TEntity).GetProperty(
            orderBy,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop is null)
            return query;

        var param       = Expression.Parameter(typeof(TEntity), "x");
        var member      = Expression.Property(param, prop);
        var keySelector = Expression.Lambda(member, param);

        var methodName = string.Equals(orderDir, "desc", StringComparison.OrdinalIgnoreCase)
            ? "OrderByDescending"
            : "OrderBy";

        var method = typeof(Queryable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TEntity), prop.PropertyType);

        return (IQueryable<TEntity>)method.Invoke(null, [query, keySelector])!;
    }

    /// <summary>
    /// Returns a paged list using the richer <see cref="CrudQueryParams"/> (filter + ordering).
    /// <c>GET {RouterPrefix}</c>
    /// </summary>
    public virtual PagedResult<TEntity> GetItems(CrudQueryParams p)
    {
        var page    = Math.Max(p.Page, 1);
        var perPage = Math.Clamp(p.PerPage, 1, 100);

        IQueryable<TEntity> query = Db.Set<TEntity>();
        query = ApplyFilter(query, p);

        var total   = query.Count();
        var ordered = ApplyOrdering(query, p);
        var items   = ordered.Skip((page - 1) * perPage).Take(perPage).ToList();
        return new PagedResult<TEntity>(items, total);
    }

    /// <summary>Async version of <see cref="GetItems(CrudQueryParams)"/>.</summary>
    public virtual async Task<PagedResult<TEntity>> GetItemsAsync(CrudQueryParams p)
    {
        var page    = Math.Max(p.Page, 1);
        var perPage = Math.Clamp(p.PerPage, 1, 100);

        IQueryable<TEntity> query = Db.Set<TEntity>().AsNoTracking();
        query = ApplyFilter(query, p);

        var total   = await query.CountAsync();
        var ordered = ApplyOrdering(query, p);
        var items   = await ordered.Skip((page - 1) * perPage).Take(perPage).ToListAsync();
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

    public virtual async Task<TEntity> CreateItemAsync(TEntity entity)
    {
        Db.Set<TEntity>().Add(entity);
        await Db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Overwrites an entity by id. Maps to Python <c>SqlalchemyCrud.update()</c>.
    /// <c>PUT {RouterPrefix}/{id}</c>
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
    /// Removes an entity by id. Maps to Python <c>SqlalchemyCrud.delete()</c>.
    /// <c>DELETE {RouterPrefix}/{id}</c>
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

    // ── Schema helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the amis column definitions for the list table, derived from the
    /// entity model via <see cref="TableModelParser.ParseColumns"/>.
    /// When <see cref="ListDisplay"/> is non-empty, only the specified columns are
    /// returned, in the declared order.
    /// </summary>
    protected virtual IReadOnlyList<TableColumn> GetColumns()
    {
        var allColumns = TableModelParser.ParseColumns(typeof(TEntity));
        if (ListDisplay.Length == 0)
            return allColumns;

        return ListDisplay
            .Select(displayName => allColumns.FirstOrDefault(c =>
                string.Equals(c.Name,  displayName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Label, displayName, StringComparison.OrdinalIgnoreCase)))
            .Where(c => c is not null)
            .Cast<TableColumn>()
            .ToList();
    }

    /// <summary>
    /// Returns the amis form fields for create/update dialogs, derived from the
    /// entity model via <see cref="TableModelParser.ParseFormFields"/>.
    /// Fields listed in <see cref="ReadonlyFields"/> are marked with
    /// <c>Disabled = true</c>.
    /// </summary>
    protected virtual IReadOnlyList<object> GetFormFields()
    {
        var fields = TableModelParser.ParseFormFields(typeof(TEntity)).ToList();
        if (ReadonlyFields.Length == 0)
            return fields;

        var readonlySet = new HashSet<string>(ReadonlyFields, StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < fields.Count; i++)
        {
            var name = GetFormFieldName(fields[i]);
            if (name is not null && readonlySet.Contains(name))
                SetFormFieldDisabled(fields[i]);
        }
        return fields;
    }

    private static string? GetFormFieldName(object field) => field switch
    {
        InputText f      => f.Name,
        InputNumber f    => f.Name,
        Switch f         => f.Name,
        InputDatetime f  => f.Name,
        InputDate f      => f.Name,
        _                => null
    };

    private static void SetFormFieldDisabled(object field)
    {
        switch (field)
        {
            case InputText f:     f.Disabled = true; break;
            case InputNumber f:   f.Disabled = true; break;
            case Switch f:        f.Disabled = true; break;
            case InputDatetime f: f.Disabled = true; break;
            case InputDate f:     f.Disabled = true; break;
        }
    }

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
    /// Uses <see cref="ListPerPage"/> for pagination and honours
    /// <see cref="EnableExportCsv"/>/<see cref="EnableExportExcel"/> for the footer toolbar.
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

        var footerToolbar = new List<object> { "statistics", "switch-per-page", "pagination" };
        if (EnableExportCsv)   footerToolbar.Add("export-csv");
        if (EnableExportExcel) footerToolbar.Add("export-excel");

        return new Page
        {
            Title = Label,
            Body  = new CrudComponent
            {
                Name          = RouterPath + "Crud",
                SyncLocation  = false,
                Api           = $"get:{RouterPrefix}",
                PerPage       = ListPerPage,
                HeaderToolbar = [GetCreateAction(), "bulkActions"],
                FooterToolbar = footerToolbar,
                Columns       = columns
            }
        };
    }

    // ── Route registration ────────────────────────────────────────────────────

    /// <summary>
    /// Registers the four standard CRUD endpoints on the given
    /// <see cref="WebApplication"/>. Each endpoint checks the corresponding
    /// async permission hook before executing; a denied request returns HTTP 401.
    /// Mirrors Python's <c>@router.add_api_route</c> decorators.
    /// </summary>
    public override void RegisterRoutes(WebApplication app)
    {
        var prefix = RouterPrefix;

        app.MapGet(prefix, async (HttpContext ctx) =>
        {
            if (!await HasReadPermissionAsync(ctx))
                return Results.Json(AdminApiResponse.Fail("Unauthorized"), statusCode: 401);

            var queryParams = CrudQueryParams.FromQuery(ctx.Request.Query);
            var result = await GetItemsAsync(queryParams);
            return Results.Json(AdminApiResponse.Ok(new { items = result.Items, total = result.Total }));
        });

        app.MapPost(prefix, async (TEntity entity, HttpContext ctx) =>
        {
            if (!await HasCreatePermissionAsync(ctx))
                return Results.Json(AdminApiResponse.Fail("Unauthorized"), statusCode: 401);

            if (!TryValidateEntity(entity, out var errorMessage))
                return Results.Json(AdminApiResponse.Fail(errorMessage!));

            var created = await CreateItemAsync(entity);
            return Results.Json(AdminApiResponse.Ok(new { item = created }, $"{Label} created."));
        });

        app.MapPut(prefix + "/{id}", async (TKey id, TEntity entity, HttpContext ctx) =>
        {
            if (!await HasUpdatePermissionAsync(ctx))
                return Results.Json(AdminApiResponse.Fail("Unauthorized"), statusCode: 401);

            if (!TryValidateEntity(entity, out var errorMessage))
                return Results.Json(AdminApiResponse.Fail(errorMessage!));

            var updated = await UpdateItemAsync(id, entity);
            return updated is null
                ? Results.Json(AdminApiResponse.Fail($"{Label} not found."))
                : Results.Json(AdminApiResponse.Ok(new { item = updated }, $"{Label} updated."));
        });

        app.MapDelete(prefix + "/{id}", async (TKey id, HttpContext ctx) =>
        {
            if (!await HasDeletePermissionAsync(ctx))
                return Results.Json(AdminApiResponse.Fail("Unauthorized"), statusCode: 401);

            return await DeleteItemAsync(id)
                ? Results.Json(AdminApiResponse.Ok(msg: $"{Label} deleted."))
                : Results.Json(AdminApiResponse.Fail($"{Label} not found."));
        });
    }

    protected virtual bool TryValidateEntity(TEntity entity, out string? errorMessage)
    {
        if (DataAnnotationsModelValidator.TryValidate(entity, out var errors))
        {
            errorMessage = null;
            return true;
        }

        errorMessage = DataAnnotationsModelValidator.FormatErrors(errors);
        return false;
    }
}
