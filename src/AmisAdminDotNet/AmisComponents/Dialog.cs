using System.Text.Json.Serialization;

namespace AmisAdminDotNet.AmisComponents;

/// <summary>
/// Amis <c>dialog</c> component — a modal pop-up container.
/// Maps to Python <c>class Dialog(AmisNode)</c>.
/// </summary>
public sealed class Dialog : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "dialog";

    /// <summary>Dialog title shown in the header bar.</summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Title { get; set; }

    /// <summary>
    /// Dialog body. Accepts a single <see cref="AmisNode"/>, a list of nodes, or a string.
    /// Maps to Python <c>body: Optional[SchemaNode]</c>.
    /// </summary>
    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Body { get; set; }

    /// <summary>Footer action buttons (defaults to Close + Confirm).</summary>
    [JsonPropertyName("actions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<object>? Actions { get; set; }

    /// <summary>
    /// Dialog size: <c>"xs"</c>, <c>"sm"</c>, <c>"md"</c>, <c>"lg"</c>, <c>"xl"</c>, <c>"full"</c>.
    /// Maps to Python <c>size: Optional[str]</c>.
    /// </summary>
    [JsonPropertyName("size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Size { get; set; }

    /// <summary>Whether clicking the backdrop closes the dialog.</summary>
    [JsonPropertyName("closeOnOutside")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? CloseOnOutside { get; set; }

    /// <summary>Whether to show the close (×) button in the header.</summary>
    [JsonPropertyName("showCloseButton")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ShowCloseButton { get; set; }
}
