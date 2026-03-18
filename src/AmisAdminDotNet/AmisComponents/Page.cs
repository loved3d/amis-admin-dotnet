using System.Text.Json.Serialization;

namespace AmisAdminDotNet.AmisComponents;

/// <summary>
/// Top-level amis <c>Page</c> component.
/// Maps to Python <c>class Page(AmisNode)</c> in fastapi_amis_admin/amis/components.py.
/// </summary>
public sealed class Page : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "page";

    /// <summary>Page title. Maps to Python <c>title: Optional[SchemaNode]</c>.</summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Title { get; set; }

    /// <summary>Page subtitle below the title.</summary>
    [JsonPropertyName("subTitle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? SubTitle { get; set; }

    /// <summary>
    /// Main content area. Accepts a single <see cref="AmisNode"/>, a list of nodes,
    /// or a raw string. Maps to Python <c>body: Optional[SchemaNode]</c>.
    /// </summary>
    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Body { get; set; }

    /// <summary>
    /// API called on page initialisation to fetch initial data.
    /// Maps to Python <c>initApi: Optional[API]</c>.
    /// </summary>
    [JsonPropertyName("initApi")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? InitApi { get; set; }

    /// <summary>Polling interval in milliseconds for <see cref="InitApi"/>.</summary>
    [JsonPropertyName("interval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Interval { get; set; }

    /// <summary>Show/hide the aside (left sidebar) panel.</summary>
    [JsonPropertyName("aside")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Aside { get; set; }

    /// <summary>Content rendered in the page toolbar area.</summary>
    [JsonPropertyName("toolbar")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Toolbar { get; set; }

    /// <summary>Whether to show a reload button in the header.</summary>
    [JsonPropertyName("pullRefresh")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? PullRefresh { get; set; }
}
