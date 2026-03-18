using System.Text.Json.Serialization;

namespace AmisAdminDotNet.AmisComponents;

/// <summary>
/// Amis <c>button</c> component. In the Python SDK this is part of the Action hierarchy.
/// Maps to Python <c>class Action(AmisNode)</c> / <c>class ActionType.Dialog</c> etc.
/// </summary>
public sealed class Button : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "button";

    /// <summary>Button label text.</summary>
    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    /// <summary>
    /// Visual level: <c>"primary"</c>, <c>"secondary"</c>, <c>"success"</c>,
    /// <c>"danger"</c>, <c>"warning"</c>, <c>"link"</c>, etc.
    /// </summary>
    [JsonPropertyName("level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Level { get; set; }

    /// <summary>
    /// Action type: <c>"dialog"</c>, <c>"drawer"</c>, <c>"ajax"</c>, <c>"link"</c>,
    /// <c>"url"</c>, <c>"copy"</c>, <c>"submit"</c>, <c>"reset"</c>, etc.
    /// Maps to Python <c>actionType: Optional[str]</c>.
    /// </summary>
    [JsonPropertyName("actionType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ActionType { get; set; }

    /// <summary>
    /// Inline dialog schema opened when <see cref="ActionType"/> is <c>"dialog"</c>.
    /// Maps to Python <c>dialog: Optional[Dialog]</c>.
    /// </summary>
    [JsonPropertyName("dialog")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Dialog { get; set; }

    /// <summary>
    /// API endpoint called when <see cref="ActionType"/> is <c>"ajax"</c>.
    /// Supports shorthand like <c>"delete:/api/items/${id}"</c>.
    /// </summary>
    [JsonPropertyName("api")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Api { get; set; }

    /// <summary>Confirmation prompt displayed before the action is executed.</summary>
    [JsonPropertyName("confirmText")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ConfirmText { get; set; }

    /// <summary>Whether the button is disabled.</summary>
    [JsonPropertyName("disabled")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Disabled { get; set; }

    /// <summary>Tooltip shown on hover.</summary>
    [JsonPropertyName("tooltip")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Tooltip { get; set; }

    /// <summary>Icon class name (e.g. <c>"fa fa-edit"</c>).</summary>
    [JsonPropertyName("icon")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Icon { get; set; }
}

/// <summary>
/// The <c>"operation"</c> column type used inside <see cref="Crud"/> to render per-row action buttons.
/// Maps to Python <c>class TableColumn</c> with <c>type="operation"</c>.
/// </summary>
public sealed class OperationColumn
{
    [JsonPropertyName("type")]
    public string Type => "operation";

    /// <summary>Column header label.</summary>
    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    /// <summary>Row-level action buttons.</summary>
    [JsonPropertyName("buttons")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<object>? Buttons { get; set; }
}
