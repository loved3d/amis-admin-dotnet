using AmisAdminDotNet.AmisComponents;
using Microsoft.Extensions.Caching.Memory;
using CrudComponent = AmisAdminDotNet.AmisComponents.Crud;

namespace AmisAdminDotNet.Services;

/// <summary>
/// Builds the amis JSON schema for the admin UI using typed <see cref="AmisNode"/>
/// component classes rather than anonymous objects, mirroring how
/// fastapi-amis-admin assembles its page schema from Python dataclasses.
/// </summary>
public sealed class AdminSchemaService : IDisposable
{
    private const string PageCacheKey = "admin-schema:page";
    private const string JsonCacheKey = "admin-schema:json";
    // The admin schema is effectively static during the lifetime of the sample app, so
    // a moderate TTL avoids repeated JSON generation while still allowing config changes
    // to show up without a restart in longer-lived hosts.
    private static readonly TimeSpan SchemaCacheLifetime = TimeSpan.FromMinutes(30);
    private readonly IMemoryCache _cache;
    private readonly II18nService _i18n;
    private readonly bool _ownsCache;
    private bool _disposed;

    public AdminSchemaService(II18nService? i18n = null, IMemoryCache? cache = null)
    {
        _i18n = i18n ?? new I18nService();
        _cache = cache ?? new MemoryCache(new MemoryCacheOptions());
        _ownsCache = cache is null;
    }

    /// <summary>
    /// Returns a strongly-typed <see cref="Page"/> amis schema for the admin UI,
    /// composed entirely of typed <see cref="AmisAdminDotNet.AmisComponents.AmisNode"/>
    /// sub-classes rather than anonymous objects.
    /// </summary>
    public Page BuildAdminPageSchema()
    {
        return _cache.GetOrCreate(PageCacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = SchemaCacheLifetime;

            return new Page
            {
                Title = _i18n.Translate("admin.title", "Amis Admin .NET Core"),
                SubTitle = _i18n.Translate(
                    "admin.subtitle",
                    "Backend-generated amis JSON with a minimal .NET admin integration."),
                Body = new Tabs
                {
                    TabList =
                    [
                        new Tab
                        {
                            Title = _i18n.Translate("tabs.dashboard", "Dashboard"),
                            Body = new object[]
                            {
                                new Tpl
                                {
                                    Template = "<div class='text-xl'>Welcome to Amis Admin .NET Core</div><p>This sample mirrors the core fastapi-amis-admin pattern: the backend generates amis schema and CRUD APIs.</p>"
                                },
                                new Alert
                                {
                                    Level = "info",
                                    Body = "This repository started almost empty, so this implementation provides a minimal runnable .NET Core migration skeleton."
                                }
                            }
                        },
                        new Tab
                        {
                            Title = _i18n.Translate("tabs.users", "Users"),
                            Body = new object[] { BuildUserCrud() }
                        }
                    ]
                }
            };
        })!;
    }

    public string BuildAdminPageJson() =>
        _cache.GetOrCreate(JsonCacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = SchemaCacheLifetime;
            return BuildAdminPageSchema().ToJson();
        })!;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
            return;

        if (_ownsCache)
            _cache.Dispose();

        _disposed = true;
    }

    private CrudComponent BuildUserCrud() => new()
    {
        Name = "userCrud",
        SyncLocation = false,
        Api = "get:/api/admin/users",
        PerPage = 10,
        HeaderToolbar =
        [
            new Button
            {
                Label = _i18n.Translate("users.create", "Create user"),
                Level = "primary",
                ActionType = "dialog",
                Dialog = new Dialog
                {
                    Title = _i18n.Translate("users.create", "Create user"),
                    Body = BuildUserForm("post:/api/admin/users")
                }
            },
            "bulkActions"
        ],
        Filter = new Form
        {
            Title = _i18n.Translate("common.search", "Search"),
            SubmitText = _i18n.Translate("common.apply", "Apply"),
            Body =
            [
                new InputText
                {
                    Name = "keywords",
                    Label = _i18n.Translate("common.keywords", "Keywords"),
                    Placeholder = _i18n.Translate("users.searchPlaceholder", "Search by name, email or role")
                }
            ]
        },
        Columns =
        [
            new TableColumn { Name = "id",    Label = "ID" },
            new TableColumn { Name = "name",  Label = "Name" },
            new TableColumn { Name = "email", Label = "Email" },
            new TableColumn { Name = "role",  Label = "Role" },
            new TableColumn
            {
                Name  = "enabled",
                Label = "Enabled",
                Type  = "mapping",
                Map   = new Dictionary<string, string>
                {
                    ["true"]  = "<span class='label label-success'>Enabled</span>",
                    ["false"] = "<span class='label label-danger'>Disabled</span>"
                }
            },
            new TableColumn
            {
                Name   = "createdAt",
                Label  = "Created at",
                Type   = "datetime",
                Format = "YYYY-MM-DD HH:mm:ss"
            },
            new OperationColumn
            {
                Label = "Actions",
                Buttons =
                [
                    new Button
                    {
                        Label      = _i18n.Translate("common.edit", "Edit"),
                        Level      = "link",
                        ActionType = "dialog",
                        Dialog     = new Dialog
                        {
                            Title = _i18n.Translate("users.edit", "Edit user"),
                            Body  = BuildUserForm("put:/api/admin/users/${id}")
                        }
                    },
                    new Button
                    {
                        Label       = _i18n.Translate("common.delete", "Delete"),
                        Level       = "link",
                        ClassName   = "text-danger",
                        ActionType  = "ajax",
                        ConfirmText = _i18n.Translate("users.deleteConfirm", "Delete user ${name}?"),
                        Api         = "delete:/api/admin/users/${id}"
                    }
                ]
            }
        ]
    };

    private static Form BuildUserForm(string api) => new()
    {
        Mode   = "horizontal",
        Api    = api,
        Reload = "userCrud",
        Body   =
        [
            new InputText  { Name = "name",    Label = "Name",    Required = true },
            new InputEmail { Name = "email",   Label = "Email",   Required = true },
            new InputText  { Name = "role",    Label = "Role",    Required = true },
            new Switch     { Name = "enabled", Label = "Enabled" }
        ]
    };
}
