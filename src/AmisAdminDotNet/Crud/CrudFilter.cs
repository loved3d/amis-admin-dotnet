using Microsoft.AspNetCore.Http;

namespace AmisAdminDotNet.Crud;

/// <summary>
/// Query parameters for CRUD list operations, covering pagination, ordering, search,
/// and field-level filtering. Mirrors Python's <c>CrudRequestSchema</c> from
/// <c>fastapi_amis_admin/crud/base.py</c>.
/// </summary>
public class CrudQueryParams
{
    /// <summary>1-based page number. Default is <c>1</c>.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Number of items per page (clamped to 1–100 during processing). Default is <c>10</c>.</summary>
    public int PerPage { get; set; } = 10;

    /// <summary>
    /// Property name used to order results. Case-insensitive.
    /// Leave <c>null</c> to use the default storage order.
    /// </summary>
    public string? OrderBy { get; set; }

    /// <summary>
    /// Sort direction for <see cref="OrderBy"/>: <c>"asc"</c> (default) or <c>"desc"</c>.
    /// </summary>
    public string? OrderDir { get; set; } = "asc";

    /// <summary>
    /// Full-text search keyword. Subclasses should apply this to their designated search
    /// fields inside an overridden <c>ApplyFilter</c>.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Extra field-level filter values parsed from query-string keys with the
    /// <c>filter_</c> prefix (e.g. <c>?filter_name=foo</c> → key <c>"name"</c>,
    /// value <c>"foo"</c>). Matching is case-insensitive.
    /// </summary>
    public Dictionary<string, string?> Filters { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    private const string FilterPrefix = "filter_";

    /// <summary>
    /// Parses a <see cref="CrudQueryParams"/> from the given <see cref="IQueryCollection"/>.
    /// Standard keys (<c>page</c>, <c>perPage</c>, <c>orderBy</c>, <c>orderDir</c>,
    /// <c>search</c>) are mapped directly; any key starting with <c>filter_</c>
    /// is added to <see cref="Filters"/> with its prefix stripped.
    /// </summary>
    public static CrudQueryParams FromQuery(IQueryCollection query)
    {
        var p = new CrudQueryParams
        {
            Page    = int.TryParse(query["page"],    out var pg)  ? pg  : 1,
            PerPage = int.TryParse(query["perPage"], out var pp)  ? pp  : 10,
            OrderBy  = query["orderBy"].FirstOrDefault(),
            OrderDir = query["orderDir"].FirstOrDefault() ?? "asc",
            Search   = query["search"].FirstOrDefault()
        };

        foreach (var key in query.Keys
                     .Where(k => k.StartsWith(FilterPrefix, StringComparison.OrdinalIgnoreCase)))
        {
            var fieldName = key[FilterPrefix.Length..]; // strip "filter_" prefix
            p.Filters[fieldName] = query[key].FirstOrDefault();
        }

        return p;
    }
}
