using System.Text.Json.Serialization;

namespace AmisAdminDotNet.AmisComponents;

/// <summary>
/// Amis <c>crud</c> component — renders a list/table with built-in pagination, filtering,
/// and toolbar actions. Maps to Python <c>class CRUD(AmisNode)</c>.
/// </summary>
public sealed class Crud : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "crud";

    /// <summary>Name used to reference this CRUD from other components (e.g. <c>reload</c>).</summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>
    /// API endpoint for fetching list data. Supports shorthand like <c>"get:/api/items"</c>.
    /// Maps to Python <c>api: Optional[API]</c>.
    /// </summary>
    [JsonPropertyName("api")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Api { get; set; }

    /// <summary>Number of rows per page. Maps to Python <c>perPage: Optional[int]</c>.</summary>
    [JsonPropertyName("perPage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PerPage { get; set; }

    /// <summary>Column definitions rendered in the list/table body.</summary>
    [JsonPropertyName("columns")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<object>? Columns { get; set; }

    /// <summary>Filter form shown above the table. Submitted values are merged into the API query.</summary>
    [JsonPropertyName("filter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Filter { get; set; }

    /// <summary>Toolbar buttons and controls rendered above the table header.</summary>
    [JsonPropertyName("headerToolbar")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<object>? HeaderToolbar { get; set; }

    /// <summary>Toolbar rendered below the table footer (e.g. pagination).</summary>
    [JsonPropertyName("footerToolbar")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<object>? FooterToolbar { get; set; }

    /// <summary>
    /// When <c>true</c> the CRUD does not sync its pagination/filter state to the URL query string.
    /// Maps to Python <c>syncLocation: Optional[bool]</c>.
    /// </summary>
    [JsonPropertyName("syncLocation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SyncLocation { get; set; }

    /// <summary>Whether to show a "bulk actions" checkbox column.</summary>
    [JsonPropertyName("bulkActions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<object>? BulkActions { get; set; }

    /// <summary>
    /// Messages displayed on specific CRUD events (fetchFailed, saveSuccess, etc.).
    /// Maps to Python nested <c>class Messages</c>.
    /// </summary>
    [JsonPropertyName("messages")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CrudMessages? Messages { get; set; }

    /// <summary>Message strings used by <see cref="Crud"/>.</summary>
    public sealed class CrudMessages
    {
        [JsonPropertyName("fetchFailed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FetchFailed { get; set; }

        [JsonPropertyName("saveSuccess")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SaveSuccess { get; set; }

        [JsonPropertyName("saveFailed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SaveFailed { get; set; }
    }

    /// <summary>
    /// When <c>true</c> columns are collapsed into expandable rows on small screens.
    /// Maps to amis <c>footable</c> property.
    /// </summary>
    [JsonPropertyName("footable")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Footable { get; set; }

    /// <summary>
    /// Whether to allow CSV export from the CRUD toolbar.
    /// When <c>true</c>, <c>"export-csv"</c> is added to <see cref="FooterToolbar"/> automatically
    /// during schema generation.
    /// </summary>
    [JsonIgnore]
    public bool EnableExportCsv { get; set; }

    /// <summary>
    /// Whether to allow Excel export from the CRUD toolbar.
    /// When <c>true</c>, <c>"export-excel"</c> is added to <see cref="FooterToolbar"/> automatically.
    /// </summary>
    [JsonIgnore]
    public bool EnableExportExcel { get; set; }
}
