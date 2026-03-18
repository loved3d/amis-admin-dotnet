using AmisAdminDotNet.AmisComponents;

namespace AmisAdminDotNet.Services;

/// <summary>
/// Builds the amis JSON schema for the admin UI using typed <see cref="AmisNode"/>
/// component classes rather than anonymous objects, mirroring how
/// fastapi-amis-admin assembles its page schema from Python dataclasses.
/// </summary>
public sealed class AdminSchemaService
{
    /// <summary>
    /// Returns a strongly-typed <see cref="Page"/> amis schema for the admin UI,
    /// composed entirely of typed <see cref="AmisAdminDotNet.AmisComponents.AmisNode"/>
    /// sub-classes rather than anonymous objects.
    /// </summary>
    public Page BuildAdminPageSchema()
    {
        return new Page
        {
            Title = "Amis Admin .NET Core",
            SubTitle = "Backend-generated amis JSON with a minimal .NET admin integration.",
            Body = new Tabs
            {
                TabList =
                [
                    new Tab
                    {
                        Title = "Dashboard",
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
                        Title = "Users",
                        Body = new object[] { BuildUserCrud() }
                    }
                ]
            }
        };
    }

    private static Crud BuildUserCrud() => new()
    {
        Name = "userCrud",
        SyncLocation = false,
        Api = "get:/api/admin/users",
        PerPage = 10,
        HeaderToolbar =
        [
            new Button
            {
                Label = "Create user",
                Level = "primary",
                ActionType = "dialog",
                Dialog = new Dialog
                {
                    Title = "Create user",
                    Body = BuildUserForm("post:/api/admin/users")
                }
            },
            "bulkActions"
        ],
        Filter = new Form
        {
            Title = "Search",
            SubmitText = "Apply",
            Body =
            [
                new InputText
                {
                    Name = "keywords",
                    Label = "Keywords",
                    Placeholder = "Search by name, email or role"
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
                        Label      = "Edit",
                        Level      = "link",
                        ActionType = "dialog",
                        Dialog     = new Dialog
                        {
                            Title = "Edit user",
                            Body  = BuildUserForm("put:/api/admin/users/${id}")
                        }
                    },
                    new Button
                    {
                        Label       = "Delete",
                        Level       = "link",
                        ClassName   = "text-danger",
                        ActionType  = "ajax",
                        ConfirmText = "Delete user ${name}?",
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

