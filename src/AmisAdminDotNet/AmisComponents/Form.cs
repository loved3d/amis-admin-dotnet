using System.Text.Json.Serialization;

namespace AmisAdminDotNet.AmisComponents;

/// <summary>
/// Amis <c>form</c> component — renders a data-entry form that can submit to an API.
/// Maps to Python <c>class Form(AmisNode)</c>.
/// </summary>
public sealed class Form : AmisNode
{
    [JsonPropertyName("type")]
    public override string Type => "form";

    /// <summary>API endpoint for form submission. Supports shorthand like <c>"post:/api/items"</c>.</summary>
    [JsonPropertyName("api")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Api { get; set; }

    /// <summary>Form field components (inputs, selects, switches, etc.).</summary>
    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<object>? Body { get; set; }

    /// <summary>
    /// Layout mode: <c>"normal"</c>, <c>"horizontal"</c>, or <c>"inline"</c>.
    /// Maps to Python <c>mode: Optional[str]</c>.
    /// </summary>
    [JsonPropertyName("mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Mode { get; set; }

    /// <summary>
    /// Name or names of CRUD/Form components to reload after a successful submission.
    /// Maps to Python <c>reload: Optional[str]</c>.
    /// </summary>
    [JsonPropertyName("reload")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Reload { get; set; }

    /// <summary>Form title shown inside a dialog.</summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    /// <summary>Label for the submit button.</summary>
    [JsonPropertyName("submitText")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SubmitText { get; set; }

    /// <summary>Whether to reset the form after a successful submission.</summary>
    [JsonPropertyName("resetAfterSubmit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ResetAfterSubmit { get; set; }

    /// <summary>
    /// Messages displayed on form events (saveSuccess, saveFailed, etc.).
    /// Maps to Python nested <c>class Messages</c>.
    /// </summary>
    [JsonPropertyName("messages")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FormMessages? Messages { get; set; }

    /// <summary>Message strings used by <see cref="Form"/>.</summary>
    public sealed class FormMessages
    {
        [JsonPropertyName("saveSuccess")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SaveSuccess { get; set; }

        [JsonPropertyName("saveFailed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SaveFailed { get; set; }

        [JsonPropertyName("validateFailed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ValidateFailed { get; set; }
    }
}
