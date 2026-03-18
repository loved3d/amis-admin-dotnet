using AmisAdminDotNet.Models;

namespace AmisAdminDotNet.Services;

public static class UserRequestValidator
{
    public static bool TryNormalize(SaveUserRequest request, out SaveUserRequest normalized, out string? error)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            normalized = request;
            error = "Name is required.";
            return false;
        }

        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            normalized = request;
            error = "Email is required.";
            return false;
        }

        if (!email.Contains('@', StringComparison.Ordinal))
        {
            normalized = request;
            error = "Email must be a valid address.";
            return false;
        }

        var role = request.Role?.Trim();
        if (string.IsNullOrWhiteSpace(role))
        {
            normalized = request;
            error = "Role is required.";
            return false;
        }

        normalized = request with
        {
            Name = name,
            Email = email,
            Role = role
        };

        error = null;
        return true;
    }
}
