namespace AmisAdminDotNet.Services;

public sealed class AdminSchemaService
{
    public object BuildAdminPageSchema()
    {
        return new
        {
            type = "page",
            title = "Amis Admin .NET Core",
            subTitle = "Backend-generated amis JSON with a minimal .NET admin integration.",
            body = new object[]
            {
                new
                {
                    type = "tabs",
                    tabs = new object[]
                    {
                        new
                        {
                            title = "Dashboard",
                            body = new object[]
                            {
                                new
                                {
                                    type = "tpl",
                                    tpl = "<div class='text-xl'>Welcome to Amis Admin .NET Core</div><p>This sample mirrors the core fastapi-amis-admin pattern: the backend generates amis schema and CRUD APIs.</p>"
                                },
                                new
                                {
                                    type = "alert",
                                    level = "info",
                                    body = "This repository started almost empty, so this implementation provides a minimal runnable .NET Core migration skeleton."
                                }
                            }
                        },
                        new
                        {
                            title = "Users",
                            body = new object[]
                            {
                                new
                                {
                                    type = "crud",
                                    name = "userCrud",
                                    syncLocation = false,
                                    api = "get:/api/admin/users",
                                    perPage = 10,
                                    headerToolbar = new object[]
                                    {
                                        new
                                        {
                                            type = "button",
                                            label = "Create user",
                                            level = "primary",
                                            actionType = "dialog",
                                            dialog = new
                                            {
                                                title = "Create user",
                                                body = BuildUserForm("post:/api/admin/users")
                                            }
                                        },
                                        "bulkActions"
                                    },
                                    filter = new
                                    {
                                        title = "Search",
                                        submitText = "Apply",
                                        body = new object[]
                                        {
                                            new
                                            {
                                                type = "input-text",
                                                name = "keywords",
                                                label = "Keywords",
                                                placeholder = "Search by name, email or role"
                                            }
                                        }
                                    },
                                    columns = new object[]
                                    {
                                        new { name = "id", label = "ID" },
                                        new { name = "name", label = "Name" },
                                        new { name = "email", label = "Email" },
                                        new { name = "role", label = "Role" },
                                        new
                                        {
                                            name = "enabled",
                                            label = "Enabled",
                                            type = "mapping",
                                            map = new Dictionary<string, string>
                                            {
                                                ["true"] = "<span class='label label-success'>Enabled</span>",
                                                ["false"] = "<span class='label label-danger'>Disabled</span>"
                                            }
                                        },
                                        new
                                        {
                                            name = "createdAt",
                                            label = "Created at",
                                            type = "datetime",
                                            format = "YYYY-MM-DD HH:mm:ss"
                                        },
                                        new
                                        {
                                            type = "operation",
                                            label = "Actions",
                                            buttons = new object[]
                                            {
                                                new
                                                {
                                                    type = "button",
                                                    label = "Edit",
                                                    level = "link",
                                                    actionType = "dialog",
                                                    dialog = new
                                                    {
                                                        title = "Edit user",
                                                        body = BuildUserForm("put:/api/admin/users/${id}")
                                                    }
                                                },
                                                new
                                                {
                                                    type = "button",
                                                    label = "Delete",
                                                    level = "link",
                                                    className = "text-danger",
                                                    actionType = "ajax",
                                                    confirmText = "Delete user ${name}?",
                                                    api = "delete:/api/admin/users/${id}"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static object BuildUserForm(string api) => new
    {
        type = "form",
        mode = "horizontal",
        api,
        reload = "userCrud",
        body = new object[]
        {
            new
            {
                type = "input-text",
                name = "name",
                label = "Name",
                required = true
            },
            new
            {
                type = "input-email",
                name = "email",
                label = "Email",
                required = true
            },
            new
            {
                type = "input-text",
                name = "role",
                label = "Role",
                required = true
            },
            new
            {
                type = "switch",
                name = "enabled",
                label = "Enabled"
            }
        }
    };
}
