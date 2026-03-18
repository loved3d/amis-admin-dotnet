namespace AmisAdminDotNet.Models;

public sealed record UserRecord(
    int Id,
    string Name,
    string Email,
    string Role,
    bool Enabled,
    DateTimeOffset CreatedAt);
