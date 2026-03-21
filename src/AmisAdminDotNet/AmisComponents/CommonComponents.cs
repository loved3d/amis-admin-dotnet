using System.Text.Json.Serialization;

namespace AmisAdminDotNet.AmisComponents;

/// <summary>
/// Amis <c>alert</c> component — displays a coloured notification banner.
/// Maps to Python <c>class Alert(AmisNode)</c>.
/// </summary>
public sealed class Alert : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "alert";

    /// <summary>
    /// Alert level/colour: <c>"info"</c>, <c>"success"</c>, <c>"warning"</c>, <c>"danger"</c>.
    /// Maps to Python <c>level: Optional[str]</c>.
    /// </summary>
    [JsonPropertyName("level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Level { get; set; }

    /// <summary>Alert content. Accepts a string or a nested <see cref="AmisNode"/>.</summary>
    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Body { get; set; }

    /// <summary>Whether the alert can be dismissed by the user.</summary>
    [JsonPropertyName("showCloseButton")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ShowCloseButton { get; set; }
}

/// <summary>
/// Amis <c>tpl</c> component — renders a Lodash template string.
/// Maps to Python <c>class Tpl(AmisNode)</c>.
/// </summary>
public sealed class Tpl : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "tpl";

    /// <summary>
    /// Lodash template or plain HTML string to render.
    /// Maps to Python <c>tpl: Optional[str]</c>.
    /// </summary>
    [JsonPropertyName("tpl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Template { get; set; }
}

/// <summary>
/// Amis <c>tabs</c> component — a tab-strip container.
/// Maps to Python <c>class Tabs(AmisNode)</c>.
/// </summary>
public sealed class Tabs : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "tabs";

    /// <summary>List of <see cref="Tab"/> definitions.</summary>
    [JsonPropertyName("tabs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Tab>? TabList { get; set; }

    /// <summary>
    /// Tab style: <c>"line"</c>, <c>"card"</c>, <c>"radio"</c>, <c>"vertical"</c>, etc.
    /// Maps to Python <c>tabsMode: Optional[str]</c>.
    /// </summary>
    [JsonPropertyName("tabsMode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TabsMode { get; set; }
}

/// <summary>
/// A single tab within a <see cref="Tabs"/> container.
/// Maps to Python <c>class Tab(AmisNode)</c>.
/// </summary>
public sealed class Tab : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "tab";

    /// <summary>Tab header label.</summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    /// <summary>Tab body content.</summary>
    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Body { get; set; }

    /// <summary>Whether the tab is disabled.</summary>
    [JsonPropertyName("disabled")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Disabled { get; set; }
}

/// <summary>
/// Amis <c>input-text</c> component — a single-line text field.
/// Maps to Python <c>class InputText(AmisNode)</c>.
/// </summary>
public sealed class InputText : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "input-text";

    /// <summary>Field name bound to the form data object.</summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>Field label displayed to the user.</summary>
    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    /// <summary>Placeholder text shown when the field is empty.</summary>
    [JsonPropertyName("placeholder")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Placeholder { get; set; }

    /// <summary>Whether the field is required. Maps to Python <c>required: Optional[bool]</c>.</summary>
    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }

    /// <summary>Whether the field is read-only.</summary>
    [JsonPropertyName("readOnly")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ReadOnly { get; set; }

    /// <summary>Whether the field is disabled (greyed out, non-interactive).</summary>
    [JsonPropertyName("disabled")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Disabled { get; set; }
}

/// <summary>
/// Amis <c>input-email</c> component — an email address input field.
/// </summary>
public sealed class InputEmail : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "input-email";

    /// <summary>Field name bound to the form data object.</summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>Field label displayed to the user.</summary>
    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    /// <summary>Placeholder text.</summary>
    [JsonPropertyName("placeholder")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Placeholder { get; set; }

    /// <summary>Whether the field is required.</summary>
    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }
}

/// <summary>
/// Amis <c>input-number</c> component — a numeric entry field.
/// Maps to Python <c>class InputNumber(AmisNode)</c>.
/// </summary>
public sealed class InputNumber : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "input-number";

    /// <summary>Field name bound to the form data object.</summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>Field label displayed to the user.</summary>
    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    /// <summary>Whether the field is required.</summary>
    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }

    /// <summary>Minimum allowed value.</summary>
    [JsonPropertyName("min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Min { get; set; }

    /// <summary>Maximum allowed value.</summary>
    [JsonPropertyName("max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Max { get; set; }

    /// <summary>Increment step for each click. Maps to Python <c>step: Optional[float]</c>.</summary>
    [JsonPropertyName("step")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Step { get; set; }

    /// <summary>Whether the field is disabled (greyed out, non-interactive).</summary>
    [JsonPropertyName("disabled")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Disabled { get; set; }
}

/// <summary>
/// Amis <c>input-datetime</c> component — a combined date-and-time picker.
/// Maps to Python <c>class InputDatetime(AmisNode)</c>.
/// </summary>
public sealed class InputDatetime : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "input-datetime";

    /// <summary>Field name bound to the form data object.</summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>Field label displayed to the user.</summary>
    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    /// <summary>Whether the field is required.</summary>
    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }

    /// <summary>
    /// Display format string (e.g. <c>"YYYY-MM-DD HH:mm:ss"</c>).
    /// Maps to Python <c>format: Optional[str]</c>.
    /// </summary>
    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Format { get; set; }

    /// <summary>Whether the field is disabled (greyed out, non-interactive).</summary>
    [JsonPropertyName("disabled")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Disabled { get; set; }
}

/// <summary>
/// Amis <c>switch</c> component — a boolean toggle control.
/// Maps to Python <c>class Switch(AmisNode)</c>.
/// </summary>
public sealed class Switch : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "switch";

    /// <summary>Field name bound to the form data object.</summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>Field label displayed to the user.</summary>
    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    /// <summary>Value emitted when the switch is ON. Defaults to <c>true</c>.</summary>
    [JsonPropertyName("trueValue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? TrueValue { get; set; }

    /// <summary>Value emitted when the switch is OFF. Defaults to <c>false</c>.</summary>
    [JsonPropertyName("falseValue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? FalseValue { get; set; }

    /// <summary>Whether the field is disabled (greyed out, non-interactive).</summary>
    [JsonPropertyName("disabled")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Disabled { get; set; }
}

/// <summary>
/// Amis <c>iframe</c> component — embeds an external URL.
/// Maps to Python <c>class Iframe(AmisNode)</c>.
/// </summary>
public sealed class Iframe : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "iframe";

    /// <summary>The URL to embed in the iframe.</summary>
    [JsonPropertyName("src")]
    public string Src { get; set; } = string.Empty;

    /// <summary>Width of the iframe (e.g. <c>"100%"</c> or <c>"800px"</c>).</summary>
    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Width { get; set; }

    /// <summary>Height of the iframe (e.g. <c>"600px"</c>).</summary>
    [JsonPropertyName("height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Height { get; set; }
}

/// <summary>
/// Amis <c>drawer</c> component — slide-in panel.
/// Maps to Python <c>class Drawer(AmisNode)</c>.
/// </summary>
public sealed class Drawer : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "drawer";

    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Body { get; set; }

    [JsonPropertyName("size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Size { get; set; }

    [JsonPropertyName("position")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Position { get; set; }
}

/// <summary>
/// Amis <c>panel</c> component — bordered card.
/// Maps to Python <c>class Panel(AmisNode)</c>.
/// </summary>
public sealed class Panel : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "panel";

    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Body { get; set; }

    [JsonPropertyName("footer")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Footer { get; set; }

    [JsonPropertyName("headerClassName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HeaderClassName { get; set; }
}

/// <summary>
/// Amis <c>grid</c> component — multi-column grid layout.
/// Maps to Python <c>class Grid(AmisNode)</c>.
/// </summary>
public sealed class Grid : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "grid";

    [JsonPropertyName("columns")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<object>? Columns { get; set; }

    [JsonPropertyName("gap")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Gap { get; set; }
}

/// <summary>
/// Amis <c>service</c> component — loads remote schema/data.
/// Maps to Python <c>class Service(AmisNode)</c>.
/// </summary>
public sealed class Service : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "service";

    [JsonPropertyName("api")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Api { get; set; }

    [JsonPropertyName("schemaApi")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SchemaApi { get; set; }

    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Body { get; set; }

    [JsonPropertyName("interval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Interval { get; set; }
}

/// <summary>
/// Amis <c>select</c> form item.
/// Maps to Python <c>class Select(FormItem)</c>.
/// </summary>
public sealed class Select : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "select";

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    [JsonPropertyName("options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SelectOption>? Options { get; set; }

    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Source { get; set; }

    [JsonPropertyName("multiple")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Multiple { get; set; }

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }

    [JsonPropertyName("clearable")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Clearable { get; set; }
}

/// <summary>A static option entry for <see cref="Select"/> and similar components.</summary>
public sealed class SelectOption
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Value { get; set; }
}

/// <summary>
/// Amis <c>input-date</c> form item.
/// Maps to Python <c>class InputDate(FormItem)</c>.
/// </summary>
public sealed class InputDate : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "input-date";

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Format { get; set; }

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }

    [JsonPropertyName("placeholder")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Placeholder { get; set; }

    /// <summary>Whether the field is disabled (greyed out, non-interactive).</summary>
    [JsonPropertyName("disabled")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Disabled { get; set; }
}

/// <summary>
/// Amis <c>input-datetime</c> form item — alias for <see cref="InputDatetime"/> using consistent C# naming.
/// Maps to Python <c>class InputDatetime(FormItem)</c>.
/// </summary>
public sealed class InputDateTime : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "input-datetime";

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Format { get; set; }

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }
}

/// <summary>
/// Amis <c>textarea</c> form item.
/// Maps to Python <c>class Textarea(FormItem)</c>.
/// </summary>
public sealed class Textarea : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "textarea";

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }

    [JsonPropertyName("placeholder")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Placeholder { get; set; }

    [JsonPropertyName("minRows")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MinRows { get; set; }

    [JsonPropertyName("maxRows")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxRows { get; set; }
}

/// <summary>
/// Amis <c>input-datetime-range</c> form item — a date-time range picker.
/// Used in filter forms to select a start and end date-time.
/// </summary>
public sealed class InputDatetimeRange : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "input-datetime-range";

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    /// <summary>
    /// Date-time format string, e.g. <c>"YYYY-MM-DD HH:mm:ss"</c>.
    /// </summary>
    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Format { get; set; }

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }
}

/// <summary>
/// Amis <c>input-file</c> form item — a file-upload field.
/// Maps to Python <c>class InputFile(FormItem)</c>.
/// </summary>
public sealed class InputFile : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "input-file";

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    /// <summary>Upload receiver URL (e.g. <c>"/admin/file/upload"</c>).</summary>
    [JsonPropertyName("receiver")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Receiver { get; set; }

    /// <summary>Accepted MIME types or extensions (e.g. <c>".pdf,.docx"</c>).</summary>
    [JsonPropertyName("accept")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Accept { get; set; }

    /// <summary>Maximum upload size in bytes.</summary>
    [JsonPropertyName("maxSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? MaxSize { get; set; }

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }

    /// <summary>Whether multiple files may be selected at once.</summary>
    [JsonPropertyName("multiple")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Multiple { get; set; }
}

/// <summary>
/// Amis <c>input-image</c> form item — an image-upload field with inline preview.
/// Maps to Python <c>class InputImage(FormItem)</c>.
/// </summary>
public sealed class InputImage : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "input-image";

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    /// <summary>Upload receiver URL (e.g. <c>"/admin/file/upload"</c>).</summary>
    [JsonPropertyName("receiver")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Receiver { get; set; }

    /// <summary>Accepted image MIME types (e.g. <c>".jpg,.png,.gif"</c>).</summary>
    [JsonPropertyName("accept")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Accept { get; set; }

    /// <summary>Maximum upload size in bytes.</summary>
    [JsonPropertyName("maxSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? MaxSize { get; set; }

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }

    /// <summary>Whether multiple images may be selected.</summary>
    [JsonPropertyName("multiple")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Multiple { get; set; }

    /// <summary>Crop settings object. Set to enable built-in image crop dialog.</summary>
    [JsonPropertyName("crop")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Crop { get; set; }
}

/// <summary>
/// Amis <c>input-color</c> form item — a color picker.
/// Maps to Python <c>class InputColor(FormItem)</c>.
/// </summary>
public sealed class InputColor : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "input-color";

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    /// <summary>
    /// Color format: <c>"hex"</c> (default), <c>"rgb"</c>, <c>"rgba"</c>, <c>"hsl"</c>.
    /// </summary>
    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Format { get; set; }

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Required { get; set; }
}

/// <summary>
/// Amis <c>collapse</c> component — an expandable/collapsible container.
/// Maps to Python <c>class Collapse(AmisNode)</c>.
/// </summary>
public sealed class Collapse : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "collapse";

    /// <summary>Header label shown on the collapse toggle.</summary>
    [JsonPropertyName("header")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Header { get; set; }

    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Body { get; set; }

    /// <summary>Whether the panel is collapsed by default.</summary>
    [JsonPropertyName("collapsed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Collapsed { get; set; }
}

/// <summary>
/// Amis <c>collapse-group</c> component — groups multiple <see cref="Collapse"/> panels.
/// </summary>
public sealed class CollapseGroup : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "collapse-group";

    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Collapse>? Body { get; set; }

    /// <summary>
    /// When <c>true</c>, only one panel can be open at a time (accordion mode).
    /// </summary>
    [JsonPropertyName("accordion")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Accordion { get; set; }
}

/// <summary>
/// Amis <c>divider</c> component — a horizontal visual separator.
/// Maps to Python <c>class Divider(AmisNode)</c>.
/// </summary>
public sealed class Divider : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "divider";

    /// <summary>
    /// Line style: <c>"solid"</c> (default), <c>"dashed"</c>, <c>"dotted"</c>.
    /// </summary>
    [JsonPropertyName("lineStyle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LineStyle { get; set; }

    /// <summary>Direction: <c>"horizontal"</c> (default) or <c>"vertical"</c>.</summary>
    [JsonPropertyName("direction")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Direction { get; set; }
}

/// <summary>
/// Amis <c>card</c> component — a bordered card with header and body.
/// Maps to Python <c>class Card(AmisNode)</c>.
/// </summary>
public sealed class Card : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "card";

    [JsonPropertyName("header")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Header { get; set; }

    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Body { get; set; }

    [JsonPropertyName("footer")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Footer { get; set; }

    /// <summary>
    /// Card actions displayed in the card footer area (list of amis action schemas).
    /// </summary>
    [JsonPropertyName("actions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<object>? Actions { get; set; }
}

/// <summary>
/// Amis <c>cards</c> component — a grid of <see cref="Card"/> items backed by a data source.
/// Maps to Python <c>class Cards(AmisNode)</c>.
/// </summary>
public sealed class Cards : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "cards";

    /// <summary>Data source API URL.</summary>
    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Source { get; set; }

    /// <summary>Card template used to render each item.</summary>
    [JsonPropertyName("card")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Card? CardTemplate { get; set; }

    /// <summary>Number of cards per row.</summary>
    [JsonPropertyName("perRow")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PerRow { get; set; }

    [JsonPropertyName("placeholder")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Placeholder { get; set; }
}

/// <summary>
/// Amis <c>steps</c> component — a step-by-step wizard indicator.
/// Maps to Python <c>class Steps(AmisNode)</c>.
/// </summary>
public sealed class Steps : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "steps";

    [JsonPropertyName("steps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Step>? StepList { get; set; }

    /// <summary>Current active step index (0-based).</summary>
    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Value { get; set; }

    /// <summary>Display mode: <c>"horizontal"</c> (default) or <c>"vertical"</c>.</summary>
    [JsonPropertyName("mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Mode { get; set; }
}

/// <summary>A single step within a <see cref="Steps"/> component.</summary>
public sealed class Step
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("subTitle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SubTitle { get; set; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    /// <summary>Step icon (amis icon class).</summary>
    [JsonPropertyName("icon")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Icon { get; set; }
}

/// <summary>
/// Amis <c>image</c> display component — renders an image from a URL.
/// Maps to Python <c>class Image(AmisNode)</c>.
/// Use <see cref="InputImage"/> for an image-upload <em>form field</em>.
/// </summary>
public sealed class Image : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "image";

    /// <summary>Image URL.</summary>
    [JsonPropertyName("src")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Src { get; set; }

    [JsonPropertyName("alt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Alt { get; set; }

    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    /// <summary>Width in CSS units (e.g. <c>"100px"</c> or <c>"50%"</c>).</summary>
    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Width { get; set; }

    /// <summary>Height in CSS units.</summary>
    [JsonPropertyName("height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Height { get; set; }
}
