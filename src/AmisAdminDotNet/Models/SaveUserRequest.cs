namespace AmisAdminDotNet.Models;

public sealed record SaveUserRequest(
    string? Name,
    string? Email,
    string? Role,
    bool Enabled = true);
