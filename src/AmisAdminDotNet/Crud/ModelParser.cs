using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AmisAdminDotNet.AmisComponents;

namespace AmisAdminDotNet.Crud;

/// <summary>
/// Represents metadata about a single property on an EF Core entity model.
/// Mirrors Python's <c>SqlaField</c> from <c>fastapi_amis_admin/crud/parser.py</c>.
///
/// <para>
/// The Python equivalent maps SQLAlchemy column types to amis form field types
/// (e.g. <c>String → input-text</c>, <c>Integer → input-number</c>).
/// This class performs the equivalent mapping from CLR types.
/// </para>
/// </summary>
public sealed class ModelFieldInfo
{
    /// <summary>C# property name (e.g. <c>"FirstName"</c>).</summary>
    public string Name { get; }

    /// <summary>
    /// camelCase JSON/column name used in amis schema (e.g. <c>"firstName"</c>).
    /// </summary>
    public string ColumnName { get; }

    /// <summary>CLR property type (including nullable wrapper when applicable).</summary>
    public Type ClrType { get; }

    /// <summary>
    /// Whether the field is non-nullable or has a <see cref="RequiredAttribute"/>.
    /// </summary>
    public bool IsRequired { get; }

    /// <summary>Whether this field is part of the entity's primary key.</summary>
    public bool IsPrimaryKey { get; }

    /// <summary>
    /// Amis form input component type for this field.
    /// Examples: <c>"input-text"</c>, <c>"input-number"</c>, <c>"switch"</c>, <c>"input-datetime"</c>.
    /// Maps to Python type mapping in <c>TableModelParser</c>.
    /// </summary>
    public string AmisInputType { get; }

    /// <summary>
    /// Amis column display type used in list views.
    /// An empty string means plain text (no explicit <c>type</c> needed).
    /// Examples: <c>"datetime"</c>, <c>"mapping"</c>, <c>""</c>.
    /// </summary>
    public string AmisColumnType { get; }

    /// <summary>
    /// Constructs a <see cref="ModelFieldInfo"/> from a <see cref="PropertyInfo"/>
    /// discovered via reflection.
    /// </summary>
    public ModelFieldInfo(PropertyInfo property, bool isPrimaryKey)
    {
        Name = property.Name;
        ColumnName = char.ToLowerInvariant(property.Name[0]) + property.Name[1..];
        ClrType = property.PropertyType;
        IsPrimaryKey = isPrimaryKey;

        // Determine nullability via the .NET 6+ NullabilityInfoContext
        var nullCtx = new NullabilityInfoContext();
        var nullInfo = nullCtx.Create(property);
        IsRequired = nullInfo.WriteState == NullabilityState.NotNull
                     || property.GetCustomAttribute<RequiredAttribute>() is not null;

        (AmisInputType, AmisColumnType) = MapTypes(ClrType);
    }

    /// <summary>
    /// Maps a CLR type to the corresponding amis form input type and column display type.
    ///
    /// <para>
    /// Python equivalent type mapping examples from <c>parser.py</c>:
    /// <list type="bullet">
    ///   <item><c>str → String → input-text</c></item>
    ///   <item><c>int → Int32 → input-number</c></item>
    ///   <item><c>bool → Boolean → switch</c></item>
    ///   <item><c>datetime → DateTime → input-datetime</c></item>
    /// </list>
    /// </para>
    /// </summary>
    private static (string inputType, string columnType) MapTypes(Type clrType)
    {
        var underlying = Nullable.GetUnderlyingType(clrType) ?? clrType;

        if (underlying == typeof(bool))
            return ("switch", "mapping");

        if (underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset))
            return ("input-datetime", "datetime");

        if (underlying == typeof(DateOnly))
            return ("input-date", "date");

        if (underlying == typeof(int) || underlying == typeof(long)
            || underlying == typeof(short) || underlying == typeof(byte)
            || underlying == typeof(double) || underlying == typeof(float)
            || underlying == typeof(decimal))
            return ("input-number", "");

        return ("input-text", "");
    }
}

/// <summary>
/// Parses CLR entity types to generate amis schema metadata.
/// Mirrors Python's <c>TableModelParser</c> from <c>fastapi_amis_admin/crud/parser.py</c>.
///
/// <para>
/// In the Python version, <c>TableModelParser</c> introspects SQLAlchemy / SQLModel
/// column metadata. This C# version uses reflection and EF Core conventions
/// (<c>[Key]</c> attribute or the <c>Id</c> / <c>{TypeName}Id</c> naming convention).
/// </para>
/// </summary>
public static class TableModelParser
{
    /// <summary>
    /// Extracts field metadata from a CLR entity type.
    /// Maps to Python's <c>TableModelParser.get_sqlmodel_fields()</c>.
    /// </summary>
    /// <param name="entityType">The CLR entity type to inspect.</param>
    public static IReadOnlyList<ModelFieldInfo> ParseFields(Type entityType)
    {
        var keyNames = DiscoverKeyPropertyNames(entityType);

        return entityType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .Select(p => new ModelFieldInfo(p, keyNames.Contains(p.Name)))
            .ToList();
    }

    /// <summary>
    /// Generates amis <see cref="TableColumn"/> definitions from an entity type for list views.
    /// Maps to Python's <c>TableModelParser.get_table_columns()</c>.
    /// </summary>
    public static IReadOnlyList<TableColumn> ParseColumns(Type entityType)
    {
        return ParseFields(entityType)
            .Select(f => new TableColumn
            {
                Name  = f.ColumnName,
                Label = f.Name,
                Type  = string.IsNullOrEmpty(f.AmisColumnType) ? null : f.AmisColumnType,
                Map   = f.AmisColumnType == "mapping"
                    ? new Dictionary<string, string>
                    {
                        ["true"]  = "<span class='label label-success'>Yes</span>",
                        ["false"] = "<span class='label label-danger'>No</span>"
                    }
                    : null
            })
            .ToList();
    }

    /// <summary>
    /// Generates amis form field components from an entity type's properties,
    /// excluding primary-key fields (which are auto-generated).
    /// Maps to Python's <c>TableModelParser.get_form_fields()</c>.
    /// </summary>
    public static IReadOnlyList<object> ParseFormFields(Type entityType)
    {
        return ParseFields(entityType)
            .Where(f => !f.IsPrimaryKey)
            .Select<ModelFieldInfo, object>(f => f.AmisInputType switch
            {
                "switch" => new Switch
                {
                    Name  = f.ColumnName,
                    Label = f.Name
                },
                "input-datetime" => new InputDatetime
                {
                    Name     = f.ColumnName,
                    Label    = f.Name,
                    Required = f.IsRequired ? true : null
                },
                "input-date" => new InputDate
                {
                    Name     = f.ColumnName,
                    Label    = f.Name,
                    Required = f.IsRequired ? true : null
                },
                "input-number" => new InputNumber
                {
                    Name     = f.ColumnName,
                    Label    = f.Name,
                    Required = f.IsRequired ? true : null
                },
                _ => new InputText
                {
                    Name     = f.ColumnName,
                    Label    = f.Name,
                    Required = f.IsRequired ? true : null
                }
            })
            .ToList();
    }

    /// <summary>
    /// Discovers the primary-key property names for an entity type using:
    /// 1. <see cref="KeyAttribute"/> annotations, then
    /// 2. EF Core conventions (<c>Id</c> or <c>{TypeName}Id</c>).
    /// </summary>
    private static HashSet<string> DiscoverKeyPropertyNames(Type entityType)
    {
        var keyProps = entityType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<KeyAttribute>() is not null)
            .Select(p => p.Name)
            .ToHashSet();

        if (keyProps.Count == 0)
        {
            // EF Core convention: property named "Id" or "{TypeName}Id"
            if (entityType.GetProperty("Id") is not null)
                keyProps.Add("Id");
            else if (entityType.GetProperty(entityType.Name + "Id") is not null)
                keyProps.Add(entityType.Name + "Id");
        }

        return keyProps;
    }
}
