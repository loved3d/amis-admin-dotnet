using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmisAdminDotNet.AmisComponents;

/// <summary>
/// Base class for all amis schema nodes, mirroring the Python AmisNode Pydantic model
/// from fastapi_amis_admin/amis/components.py.
/// </summary>
public abstract class AmisNode
{
    /// <summary>Component type discriminator used by the amis renderer.</summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    /// <summary>Additional CSS class names appended to the component root element.</summary>
    [JsonPropertyName("className")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ClassName { get; set; }

    /// <summary>Whether the component is visible. Maps to Python <c>visible: Optional[bool]</c>.</summary>
    [JsonPropertyName("visible")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Visible { get; set; }

    /// <summary>Hides the component when <c>true</c>. Maps to Python <c>hidden: Optional[bool]</c>.</summary>
    [JsonPropertyName("hidden")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Hidden { get; set; }

    /// <summary>Inline style string applied to the component root element.</summary>
    [JsonPropertyName("style")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Style { get; set; }

    /// <summary>
    /// Serializes this node to a JSON string using the concrete runtime type so that
    /// all derived properties are included. Maps to Python <c>amis_json()</c> / <c>dict()</c>.
    /// </summary>
    public string ToJson(JsonSerializerOptions? options = null) =>
        JsonSerializer.Serialize(this, GetType(), options ?? AmisJsonOptions.Default);
}

/// <summary>Shared <see cref="JsonSerializerOptions"/> used across all amis schema serialization.</summary>
public static class AmisJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };
}
