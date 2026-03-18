namespace AmisAdminDotNet.Models;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total);
