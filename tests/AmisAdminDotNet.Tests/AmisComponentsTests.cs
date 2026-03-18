using System.Text.Json;
using AmisAdminDotNet.AmisComponents;
using CrudComponent = AmisAdminDotNet.AmisComponents.Crud;

namespace AmisAdminDotNet.Tests;

public sealed class AmisComponentsTests
{
    // ── AmisNode / ToJson ──────────────────────────────────────────────────

    [Fact]
    public void AmisNode_ToJson_SerializesConcreteType()
    {
        var page = new Page { Title = "Hello" };

        var json = page.ToJson();

        Assert.Contains("\"type\":\"page\"", json);
        Assert.Contains("\"title\":\"Hello\"", json);
    }

    [Fact]
    public void AmisNode_ToJson_OmitsNullProperties()
    {
        var page = new Page { Title = "T" };

        var json = page.ToJson();

        Assert.DoesNotContain("\"body\"", json);
        Assert.DoesNotContain("\"subTitle\"", json);
        Assert.DoesNotContain("\"className\"", json);
    }

    [Fact]
    public void AmisNode_ToJson_IncludesVisibleAndHidden()
    {
        var page = new Page { Title = "T", Visible = true, Hidden = false };

        var json = page.ToJson();

        Assert.Contains("\"visible\":true", json);
        Assert.Contains("\"hidden\":false", json);
    }

    // ── Page ───────────────────────────────────────────────────────────────

    [Fact]
    public void Page_Type_IsPage()
    {
        var page = new Page();
        Assert.Equal("page", page.Type);
    }

    [Fact]
    public void Page_Serializes_WithInitApi()
    {
        var page = new Page { Title = "T", InitApi = "get:/api/init" };

        var json = page.ToJson();

        Assert.Contains("\"initApi\":\"get:/api/init\"", json);
    }

    // ── Crud ───────────────────────────────────────────────────────────────

    [Fact]
    public void Crud_Type_IsCrud()
    {
        var crud = new CrudComponent();
        Assert.Equal("crud", crud.Type);
    }

    [Fact]
    public void Crud_Serializes_CoreProperties()
    {
        var crud = new CrudComponent
        {
            Name = "myCrud",
            Api = "get:/api/items",
            PerPage = 20,
            SyncLocation = false
        };

        var json = JsonSerializer.Serialize(crud, AmisJsonOptions.Default);

        Assert.Contains("\"type\":\"crud\"", json);
        Assert.Contains("\"name\":\"myCrud\"", json);
        Assert.Contains("\"api\":\"get:/api/items\"", json);
        Assert.Contains("\"perPage\":20", json);
        Assert.Contains("\"syncLocation\":false", json);
    }

    [Fact]
    public void Crud_Messages_SerializesNestedClass()
    {
        var crud = new CrudComponent
        {
            Messages = new CrudComponent.CrudMessages { FetchFailed = "Load error", SaveSuccess = "Saved!" }
        };

        var json = JsonSerializer.Serialize(crud, AmisJsonOptions.Default);

        Assert.Contains("\"fetchFailed\":\"Load error\"", json);
        Assert.Contains("\"saveSuccess\":\"Saved!\"", json);
    }

    // ── Form ───────────────────────────────────────────────────────────────

    [Fact]
    public void Form_Type_IsForm()
    {
        var form = new Form();
        Assert.Equal("form", form.Type);
    }

    [Fact]
    public void Form_Serializes_CoreProperties()
    {
        var form = new Form
        {
            Api = "post:/api/items",
            Mode = "horizontal",
            Reload = "myCrud",
            SubmitText = "Save"
        };

        var json = JsonSerializer.Serialize(form, AmisJsonOptions.Default);

        Assert.Contains("\"type\":\"form\"", json);
        Assert.Contains("\"api\":\"post:/api/items\"", json);
        Assert.Contains("\"mode\":\"horizontal\"", json);
        Assert.Contains("\"reload\":\"myCrud\"", json);
        Assert.Contains("\"submitText\":\"Save\"", json);
    }

    [Fact]
    public void Form_Messages_SerializesNestedClass()
    {
        var form = new Form
        {
            Messages = new Form.FormMessages { SaveSuccess = "Done", ValidateFailed = "Check fields" }
        };

        var json = JsonSerializer.Serialize(form, AmisJsonOptions.Default);

        Assert.Contains("\"saveSuccess\":\"Done\"", json);
        Assert.Contains("\"validateFailed\":\"Check fields\"", json);
    }

    // ── Table / TableColumn ────────────────────────────────────────────────

    [Fact]
    public void Table_Type_IsTable()
    {
        var table = new Table();
        Assert.Equal("table", table.Type);
    }

    [Fact]
    public void Table_Serializes_WithColumns()
    {
        var table = new Table
        {
            Source = "${items}",
            Columns =
            [
                new TableColumn { Name = "id",   Label = "ID" },
                new TableColumn { Name = "name", Label = "Name", Sortable = true }
            ]
        };

        var json = JsonSerializer.Serialize(table, AmisJsonOptions.Default);

        Assert.Contains("\"type\":\"table\"", json);
        Assert.Contains("\"source\":\"${items}\"", json);
        Assert.Contains("\"name\":\"id\"", json);
        Assert.Contains("\"sortable\":true", json);
    }

    [Fact]
    public void TableColumn_MappingType_SerializesMap()
    {
        var col = new TableColumn
        {
            Name = "status",
            Type = "mapping",
            Map  = new Dictionary<string, string> { ["1"] = "Active", ["0"] = "Inactive" }
        };

        var json = JsonSerializer.Serialize(col, AmisJsonOptions.Default);

        Assert.Contains("\"type\":\"mapping\"", json);
        Assert.Contains("\"1\":\"Active\"", json);
    }

    // ── Button / Action ────────────────────────────────────────────────────

    [Fact]
    public void Button_Type_IsButton()
    {
        var btn = new Button();
        Assert.Equal("button", btn.Type);
    }

    [Fact]
    public void Button_DialogAction_SerializesInlineDialog()
    {
        var btn = new Button
        {
            Label      = "Open",
            ActionType = "dialog",
            Dialog     = new Dialog { Title = "My dialog", Body = "Hello" }
        };

        var json = JsonSerializer.Serialize(btn, AmisJsonOptions.Default);

        Assert.Contains("\"actionType\":\"dialog\"", json);
        Assert.Contains("\"type\":\"dialog\"", json);
        Assert.Contains("\"title\":\"My dialog\"", json);
    }

    [Fact]
    public void Button_AjaxAction_SerializesConfirmAndApi()
    {
        var btn = new Button
        {
            Label       = "Delete",
            ActionType  = "ajax",
            ConfirmText = "Sure?",
            Api         = "delete:/api/items/${id}"
        };

        var json = JsonSerializer.Serialize(btn, AmisJsonOptions.Default);

        Assert.Contains("\"confirmText\":\"Sure?\"", json);
        Assert.Contains("\"api\":\"delete:/api/items/${id}\"", json);
    }

    // ── Dialog ─────────────────────────────────────────────────────────────

    [Fact]
    public void Dialog_Type_IsDialog()
    {
        var dlg = new Dialog();
        Assert.Equal("dialog", dlg.Type);
    }

    [Fact]
    public void Dialog_Serializes_SizeAndClose()
    {
        var dlg = new Dialog
        {
            Title          = "Big dialog",
            Size           = "lg",
            CloseOnOutside = true
        };

        var json = JsonSerializer.Serialize(dlg, AmisJsonOptions.Default);

        Assert.Contains("\"size\":\"lg\"", json);
        Assert.Contains("\"closeOnOutside\":true", json);
    }

    // ── Common components ──────────────────────────────────────────────────

    [Fact]
    public void Alert_Type_IsAlert()
    {
        var alert = new Alert { Level = "info", Body = "Heads up!" };
        Assert.Equal("alert", alert.Type);

        var json = alert.ToJson();
        Assert.Contains("\"level\":\"info\"", json);
        Assert.Contains("\"body\":\"Heads up!\"", json);
    }

    [Fact]
    public void Tpl_Type_IsTpl()
    {
        var tpl = new Tpl { Template = "<b>Hello</b>" };
        Assert.Equal("tpl", tpl.Type);

        var json = tpl.ToJson();
        // System.Text.Json safely escapes angle brackets; the amis renderer handles both forms.
        Assert.Contains("\"type\":\"tpl\"", json);
        Assert.Contains("\"tpl\":", json);
        Assert.Contains("Hello", json);
    }

    [Fact]
    public void Tabs_SerializesTabList()
    {
        var tabs = new Tabs
        {
            TabList =
            [
                new Tab { Title = "Tab 1", Body = "Content 1" },
                new Tab { Title = "Tab 2", Body = "Content 2" }
            ]
        };

        var json = tabs.ToJson();

        Assert.Contains("\"type\":\"tabs\"", json);
        Assert.Contains("\"title\":\"Tab 1\"", json);
        Assert.Contains("\"title\":\"Tab 2\"", json);
    }

    [Fact]
    public void InputText_Serializes_RequiredAndPlaceholder()
    {
        var input = new InputText
        {
            Name        = "username",
            Label       = "Username",
            Placeholder = "Enter your username",
            Required    = true
        };

        var json = input.ToJson();

        Assert.Contains("\"type\":\"input-text\"", json);
        Assert.Contains("\"name\":\"username\"", json);
        Assert.Contains("\"required\":true", json);
        Assert.Contains("\"placeholder\":\"Enter your username\"", json);
    }

    [Fact]
    public void InputEmail_Type_IsInputEmail()
    {
        var input = new InputEmail { Name = "email", Label = "Email" };
        Assert.Equal("input-email", input.Type);
    }

    [Fact]
    public void Switch_Serializes_NameAndLabel()
    {
        var sw = new Switch { Name = "enabled", Label = "Enabled" };

        var json = sw.ToJson();

        Assert.Contains("\"type\":\"switch\"", json);
        Assert.Contains("\"name\":\"enabled\"", json);
        Assert.Contains("\"label\":\"Enabled\"", json);
    }

    // ── OperationColumn ────────────────────────────────────────────────────

    [Fact]
    public void OperationColumn_Type_IsOperation()
    {
        var col = new OperationColumn { Label = "Actions" };
        var json = JsonSerializer.Serialize(col, AmisJsonOptions.Default);

        Assert.Contains("\"type\":\"operation\"", json);
        Assert.Contains("\"label\":\"Actions\"", json);
    }

    // ── AdminSchemaService integration ─────────────────────────────────────

    [Fact]
    public void AdminSchemaService_BuildAdminPageSchema_ReturnsTypedPage()
    {
        var service = new AmisAdminDotNet.Services.AdminSchemaService();

        var page = service.BuildAdminPageSchema();

        Assert.IsType<Page>(page);
        Assert.Equal("page", page.Type);
        Assert.NotNull(page.Body);
    }

    [Fact]
    public void AdminSchemaService_BuildAdminPageSchema_JsonContainsAllApiEndpoints()
    {
        var service = new AmisAdminDotNet.Services.AdminSchemaService();

        var json = service.BuildAdminPageSchema().ToJson();

        Assert.Contains("\"type\":\"page\"", json);
        Assert.Contains("\"type\":\"crud\"", json);
        Assert.Contains("get:/api/admin/users", json);
        Assert.Contains("post:/api/admin/users", json);
        Assert.Contains("put:/api/admin/users/${id}", json);
        Assert.Contains("delete:/api/admin/users/${id}", json);
    }
}
