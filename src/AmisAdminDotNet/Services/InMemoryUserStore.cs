using AmisAdminDotNet.Models;

namespace AmisAdminDotNet.Services;

public sealed class InMemoryUserStore : IUserStore
{
    private readonly object _syncRoot = new();
    private readonly List<UserRecord> _users =
    [
        new(1, "Alice", "alice@example.com", "Administrator", true, DateTimeOffset.UtcNow.AddDays(-15)),
        new(2, "Bob", "bob@example.com", "Editor", true, DateTimeOffset.UtcNow.AddDays(-10)),
        new(3, "Cindy", "cindy@example.com", "Auditor", false, DateTimeOffset.UtcNow.AddDays(-3))
    ];

    private int _nextId = 4;

    public PagedResult<UserRecord> Query(string? keywords, int page, int perPage)
    {
        page = Math.Max(page, 1);
        perPage = Math.Clamp(perPage, 1, 100);

        IEnumerable<UserRecord> query = _users.OrderBy(user => user.Id);

        if (!string.IsNullOrWhiteSpace(keywords))
        {
            query = query.Where(user =>
                user.Name.Contains(keywords, StringComparison.OrdinalIgnoreCase) ||
                user.Email.Contains(keywords, StringComparison.OrdinalIgnoreCase) ||
                user.Role.Contains(keywords, StringComparison.OrdinalIgnoreCase));
        }

        var filtered = query.ToList();
        var items = filtered
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();

        return new PagedResult<UserRecord>(items, filtered.Count);
    }

    public UserRecord Create(SaveUserRequest request)
    {
        lock (_syncRoot)
        {
            var user = new UserRecord(
                _nextId++,
                request.Name!,
                request.Email!,
                request.Role!,
                request.Enabled,
                DateTimeOffset.UtcNow);

            _users.Add(user);
            return user;
        }
    }

    public UserRecord? Update(int id, SaveUserRequest request)
    {
        lock (_syncRoot)
        {
            var index = _users.FindIndex(user => user.Id == id);
            if (index < 0)
            {
                return null;
            }

            var existing = _users[index];
            var updated = existing with
            {
                Name = request.Name!,
                Email = request.Email!,
                Role = request.Role!,
                Enabled = request.Enabled
            };

            _users[index] = updated;
            return updated;
        }
    }

    public bool Delete(int id)
    {
        lock (_syncRoot)
        {
            var removed = _users.RemoveAll(user => user.Id == id);
            return removed > 0;
        }
    }
}
