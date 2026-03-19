using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AmisAdminDotNet.Admin.Extensions;

/// <summary>
/// Data-set permission descriptor. Corresponds to Python <c>extensions/schemas.py SelectPerm</c>.
/// Appends additional <c>Where</c> clauses to an <see cref="IQueryable{T}"/> for row-level filtering.
/// </summary>
/// <typeparam name="TEntity">EF Core entity type.</typeparam>
public class SelectPerm<TEntity> where TEntity : class
{
    /// <summary>Machine-readable identifier for this permission filter.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Human-readable display label.</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// Whether to reverse/negate the filter logic.
    /// Interpretation is left to the concrete filter implementation.
    /// </summary>
    public bool Reverse { get; init; } = false;

    /// <summary>
    /// Filter callback: receives the current query and the <see cref="HttpContext"/>,
    /// appends <c>Where</c> predicates, and returns the narrowed query.
    /// Defaults to an identity function (no filtering) when not set.
    /// </summary>
    public Func<IQueryable<TEntity>, HttpContext, IQueryable<TEntity>> Apply { get; init; }
        = static (q, _) => q;
}

/// <summary>
/// Recent-time-range filter permission. Corresponds to Python <c>RecentTimeSelectPerm</c>.
/// Restricts results to records created within the last <paramref name="seconds"/> seconds.
/// </summary>
/// <typeparam name="TEntity">EF Core entity type.</typeparam>
public class RecentTimeSelectPerm<TEntity> : SelectPerm<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Creates a filter that only allows access to records whose
    /// <paramref name="timeColumn"/> is newer than <paramref name="seconds"/> seconds ago.
    /// </summary>
    /// <param name="timeColumn">Name of the <see cref="DateTime"/> property to filter on.</param>
    /// <param name="seconds">Number of seconds defining "recent" (default: 7 days).</param>
    public RecentTimeSelectPerm(string timeColumn = "CreateTime", int seconds = 7 * 24 * 3600)
    {
        Name  = "recent_time";
        Label = $"Recent {seconds}s";
        Apply = (query, _) =>
        {
            var cutoff = DateTime.UtcNow.AddSeconds(-seconds);
            return query.Where(e =>
                EF.Property<DateTime>(e, timeColumn) > cutoff);
        };
    }
}

/// <summary>
/// Simple column-value equality filter. Corresponds to Python <c>SimpleSelectPerm</c>.
/// Only allows access to records where the named column's value is in the provided set.
/// </summary>
/// <typeparam name="TEntity">EF Core entity type.</typeparam>
/// <typeparam name="TValue">Type of the column value.</typeparam>
public class SimpleSelectPerm<TEntity, TValue> : SelectPerm<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Creates a filter that restricts access to rows where <paramref name="column"/>
    /// equals one of the values in <paramref name="values"/>.
    /// </summary>
    /// <param name="column">Entity property name to filter on.</param>
    /// <param name="values">Allowed values for the column.</param>
    public SimpleSelectPerm(string column, IEnumerable<TValue> values)
    {
        Name  = $"simple_{column}";
        Label = $"Filter by {column}";
        var valueList = values.ToList();
        Apply = (query, _) =>
        {
            if (valueList.Count == 0) return query;
            return query.Where(e =>
                valueList.Contains(EF.Property<TValue>(e, column)));
        };
    }
}
