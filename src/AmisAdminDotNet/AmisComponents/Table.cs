using System.Text.Json.Serialization;

namespace AmisAdminDotNet.AmisComponents;

/// <summary>
/// Amis <c>table</c> component — a pure-display data table without built-in pagination or CRUD.
/// For a full CRUD table use <see cref="Crud"/> instead.
/// Maps to Python <c>class Table(AmisNode)</c>.
/// </summary>
public sealed class Table : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "table";

    /// <summary>
    /// Data source expression (e.g. <c>"${items}"</c>) or an API descriptor.
    /// Maps to Python <c>source: Optional[str]</c>.
    /// </summary>
    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Source { get; set; }

    /// <summary>Column definitions.</summary>
    [JsonPropertyName("columns")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<TableColumn>? Columns { get; set; }

    /// <summary>Whether to show a summary row at the bottom of the table.</summary>
    [JsonPropertyName("showFooter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ShowFooter { get; set; }

    /// <summary>Whether to show a column header row.</summary>
    [JsonPropertyName("showHeader")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ShowHeader { get; set; }

    /// <summary>Message shown when the table is empty.</summary>
    [JsonPropertyName("placeholder")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Whether the table layout uses fixed column widths.
    /// Maps to Python <c>tableLayout: Optional[str]</c>.
    /// </summary>
    [JsonPropertyName("tableLayout")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TableLayout { get; set; }
}

/// <summary>
/// A single column definition used inside <see cref="Table"/> and <see cref="Crud"/>.
/// Maps to Python <c>class TableColumn(AmisNode)</c>.
/// </summary>
public sealed class TableColumn
{
    /// <summary>Field name in the row data object.</summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>Column header label displayed in the UI.</summary>
    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    /// <summary>
    /// Column renderer type, e.g. <c>"text"</c>, <c>"datetime"</c>, <c>"mapping"</c>, <c>"operation"</c>.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }

    /// <summary>Whether the column is sortable.</summary>
    [JsonPropertyName("sortable")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Sortable { get; set; }

    /// <summary>
    /// Mapping dictionary for the <c>"mapping"</c> type, where keys are field values
    /// and values are display strings or HTML.
    /// </summary>
    [JsonPropertyName("map")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Map { get; set; }

    /// <summary>
    /// Date/time format string used when <see cref="Type"/> is <c>"datetime"</c>.
    /// E.g. <c>"YYYY-MM-DD HH:mm:ss"</c>.
    /// </summary>
    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Format { get; set; }

    /// <summary>Action buttons rendered inside the <c>"operation"</c> column type.</summary>
    [JsonPropertyName("buttons")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<object>? Buttons { get; set; }

    /// <summary>Minimum column width in pixels.</summary>
    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Width { get; set; }

    /// <summary>
    /// Quick-edit configuration. When set, the cell becomes editable inline.
    /// Set to <c>new { type = "switch" }</c> for bool columns, or any other amis form component.
    /// Maps to amis <c>quickEdit</c> property.
    /// </summary>
    [JsonPropertyName("quickEdit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? QuickEdit { get; set; }
}
