using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Crud;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using CrudComponent = AmisAdminDotNet.AmisComponents.Crud;

namespace AmisAdminDotNet.Admin.Extensions;

/// <summary>
/// Read-only model admin that disables create, update, and delete operations.
/// Corresponds to Python <c>ReadOnlyModelAdmin</c> from
/// <c>fastapi_amis_admin/admin/extensions/admin.py</c>.
/// </summary>
/// <typeparam name="TEntity">EF Core entity type.</typeparam>
/// <typeparam name="TKey">Primary-key type.</typeparam>
/// <typeparam name="TDbContext">Concrete <see cref="DbContext"/> type.</typeparam>
public abstract class ReadOnlyModelAdmin<TEntity, TKey, TDbContext>
    : ModelAdmin<TEntity, TKey, TDbContext>
    where TEntity : class
    where TDbContext : DbContext
{
    protected ReadOnlyModelAdmin(TDbContext db) : base(db) { }

    /// <inheritdoc/>
    public override bool HasCreatePermission(HttpContext context) => false;

    /// <inheritdoc/>
    public override bool HasUpdatePermission(HttpContext context) => false;

    /// <inheritdoc/>
    public override bool HasDeletePermission(HttpContext context) => false;

    /// <summary>
    /// Builds a read-only page schema: no Create button, and the operation column
    /// contains only a View (read-only dialog) button — no Edit or Delete buttons.
    /// </summary>
    public override Page BuildPageSchema()
    {
        var columns = GetColumns().Cast<object>().ToList();
        columns.Add(new OperationColumn
        {
            Label   = "Actions",
            Buttons = [GetViewAction()]
        });

        var footerToolbar = new List<object> { "statistics", "switch-per-page", "pagination" };
        if (EnableExportCsv)   footerToolbar.Add("export-csv");
        if (EnableExportExcel) footerToolbar.Add("export-excel");

        var headerToolbar = new List<object>();
        foreach (var action in GetAdminActions())
            headerToolbar.Add(action.BuildActionButton(RouterPrefix));

        return new Page
        {
            Title = Label,
            Body  = new CrudComponent
            {
                Name          = RouterPath + "Crud",
                SyncLocation  = false,
                Api           = $"get:{RouterPrefix}",
                PerPage       = ListPerPage,
                HeaderToolbar = headerToolbar.Count > 0 ? headerToolbar : null,
                FooterToolbar = footerToolbar,
                Columns       = columns
            }
        };
    }

    /// <summary>
    /// Builds a read-only view button that opens a dialog showing the entity's fields
    /// (all disabled, no submit action).
    /// </summary>
    protected virtual Button GetViewAction() => new()
    {
        Label      = "View",
        Level      = "link",
        ActionType = "dialog",
        Dialog     = new Dialog
        {
            Title = $"View {Label}",
            Body  = new Form
            {
                Mode   = "horizontal",
                Body   = GetFormFields()
                    .Select(f => { SetFieldDisabled(f); return f; })
                    .ToList()
            }
        }
    };

    private static void SetFieldDisabled(object field)
    {
        switch (field)
        {
            case InputText f:     f.Disabled = true; break;
            case InputNumber f:   f.Disabled = true; break;
            case Switch f:        f.Disabled = true; break;
            case InputDatetime f: f.Disabled = true; break;
            case InputDate f:     f.Disabled = true; break;
            case Select f:        f.Clearable = false; break;
        }
    }
}

/// <summary>
/// Model admin that automatically excludes auto-managed time fields from create/update forms.
/// Corresponds to Python <c>AutoTimeModelAdmin</c>.
/// </summary>
/// <typeparam name="TEntity">EF Core entity type.</typeparam>
/// <typeparam name="TKey">Primary-key type.</typeparam>
/// <typeparam name="TDbContext">Concrete <see cref="DbContext"/> type.</typeparam>
public abstract class AutoTimeModelAdmin<TEntity, TKey, TDbContext>
    : ModelAdmin<TEntity, TKey, TDbContext>
    where TEntity : class
    where TDbContext : DbContext
{
    protected AutoTimeModelAdmin(TDbContext db) : base(db) { }

    /// <summary>
    /// Fields excluded from the Create form.
    /// Defaults to the common auto-managed fields.
    /// </summary>
    public virtual string[] CreateExclude { get; } =
        ["Id", "CreateTime", "UpdateTime", "DeleteTime"];

    /// <summary>
    /// Fields excluded from the Update/Edit form.
    /// Defaults to the common auto-managed fields.
    /// </summary>
    public virtual string[] UpdateExclude { get; } =
        ["Id", "CreateTime", "UpdateTime", "DeleteTime"];

    /// <summary>
    /// Returns form fields for the Create dialog, excluding <see cref="CreateExclude"/> fields.
    /// </summary>
    protected override IReadOnlyList<object> GetCreateFormFields()
    {
        var excludeSet = new HashSet<string>(CreateExclude, StringComparer.OrdinalIgnoreCase);
        return GetFormFields()
            .Where(f => !IsFieldExcluded(f, excludeSet))
            .ToList();
    }

    /// <summary>
    /// Returns form fields for the Update dialog, excluding <see cref="UpdateExclude"/> fields.
    /// </summary>
    protected override IReadOnlyList<object> GetUpdateFormFields()
    {
        var excludeSet = new HashSet<string>(UpdateExclude, StringComparer.OrdinalIgnoreCase);
        return GetFormFields()
            .Where(f => !IsFieldExcluded(f, excludeSet))
            .ToList();
    }

    private static bool IsFieldExcluded(object field, HashSet<string> excludeSet)
    {
        var name = field switch
        {
            InputText f     => f.Name,
            InputNumber f   => f.Name,
            Switch f        => f.Name,
            InputDatetime f => f.Name,
            InputDate f     => f.Name,
            Select f        => f.Name,
            _               => null
        };

        if (name is null) return false;

        // The field Name is in camelCase (e.g. "createTime"); compare case-insensitively.
        // Also compare with PascalCase version so "CreateTime" matches "createTime".
        return excludeSet.Contains(name) ||
               (name.Length > 1 && excludeSet.Contains(
                   char.ToUpperInvariant(name[0]) + name[1..])) ||
               (name.Length == 1 && excludeSet.Contains(
                   char.ToUpperInvariant(name[0]).ToString()));
    }
}

/// <summary>
/// Model admin with soft-delete support. Instead of physically removing records,
/// sets the <c>DeleteTime</c> property to the current UTC time.
/// Automatically filters out soft-deleted records in all list queries.
/// Corresponds to Python <c>SoftDeleteModelAdmin</c>.
/// </summary>
/// <typeparam name="TEntity">EF Core entity type that must have a <c>DateTime? DeleteTime</c> property.</typeparam>
/// <typeparam name="TKey">Primary-key type.</typeparam>
/// <typeparam name="TDbContext">Concrete <see cref="DbContext"/> type.</typeparam>
public abstract class SoftDeleteModelAdmin<TEntity, TKey, TDbContext>
    : AutoTimeModelAdmin<TEntity, TKey, TDbContext>
    where TEntity : class
    where TDbContext : DbContext
{
    protected SoftDeleteModelAdmin(TDbContext db) : base(db)
    {
        var prop = typeof(TEntity).GetProperty("DeleteTime");
        if (prop == null || prop.PropertyType != typeof(DateTime?))
            throw new InvalidOperationException(
                $"{typeof(TEntity).Name} must have a 'DateTime? DeleteTime' property " +
                $"to use {nameof(SoftDeleteModelAdmin<TEntity, TKey, TDbContext>)}.");
    }

    /// <summary>
    /// Automatically excludes soft-deleted (where <c>DeleteTime IS NOT NULL</c>) records.
    /// </summary>
    public override IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query, CrudQueryParams p)
    {
        query = base.ApplyFilter(query, p);
        return query.Where(e => EF.Property<DateTime?>(e, "DeleteTime") == null);
    }

    /// <summary>
    /// Soft-deletes an entity by setting its <c>DeleteTime</c> property to <see cref="DateTime.UtcNow"/>.
    /// Does not physically remove the record from the database.
    /// </summary>
    public override async Task<bool> DeleteItemAsync(TKey id)
    {
        var existing = await Db.Set<TEntity>().FindAsync(new object?[] { id });
        if (existing is null) return false;

        var prop = typeof(TEntity).GetProperty("DeleteTime")!;
        prop.SetValue(existing, DateTime.UtcNow);
        await Db.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc cref="DeleteItemAsync"/>
    public override bool DeleteItem(TKey id)
    {
        var existing = Db.Set<TEntity>().Find(id);
        if (existing is null) return false;

        var prop = typeof(TEntity).GetProperty("DeleteTime")!;
        prop.SetValue(existing, DateTime.UtcNow);
        Db.SaveChanges();
        return true;
    }
}

/// <summary>
/// Model admin that enables the amis <c>footable</c> feature on the CRUD component,
/// collapsing columns into expandable rows on smaller screens.
/// Corresponds to Python <c>FootableModelAdmin</c>.
/// </summary>
/// <typeparam name="TEntity">EF Core entity type.</typeparam>
/// <typeparam name="TKey">Primary-key type.</typeparam>
/// <typeparam name="TDbContext">Concrete <see cref="DbContext"/> type.</typeparam>
public abstract class FootableModelAdmin<TEntity, TKey, TDbContext>
    : ModelAdmin<TEntity, TKey, TDbContext>
    where TEntity : class
    where TDbContext : DbContext
{
    protected FootableModelAdmin(TDbContext db) : base(db) { }

    /// <summary>
    /// Builds the page schema with <c>Footable = true</c> on the CRUD component.
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

        var headerToolbar = new List<object> { GetCreateAction() };
        foreach (var action in GetAdminActions())
            headerToolbar.Add(action.BuildActionButton(RouterPrefix));
        headerToolbar.Add("bulkActions");

        return new Page
        {
            Title = Label,
            Body  = new CrudComponent
            {
                Name          = RouterPath + "Crud",
                SyncLocation  = false,
                Api           = $"get:{RouterPrefix}",
                PerPage       = ListPerPage,
                Footable      = true,
                HeaderToolbar = headerToolbar,
                FooterToolbar = footerToolbar,
                BulkActions   = [GetBulkDeleteAction()],
                Columns       = columns
            }
        };
    }
}
