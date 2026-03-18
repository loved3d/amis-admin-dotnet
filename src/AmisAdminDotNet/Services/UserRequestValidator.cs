using AmisAdminDotNet.Models;

namespace AmisAdminDotNet.Services;

/// <summary>
/// Normalises (trims whitespace) and validates a <see cref="SaveUserRequest"/>.
/// Validation rules are defined in <see cref="SaveUserRequestValidator"/> (FluentValidation).
/// </summary>
public static class UserRequestValidator
{
    private static readonly SaveUserRequestValidator _validator = new();

    public static bool TryNormalize(SaveUserRequest request, out SaveUserRequest normalized, out string? error)
    {
        // Trim whitespace first so validation messages are accurate.
        normalized = request with
        {
            Name = request.Name?.Trim(),
            Email = request.Email?.Trim(),
            Role = request.Role?.Trim()
        };

        var result = _validator.Validate(normalized);
        if (!result.IsValid)
        {
            error = result.Errors[0].ErrorMessage;
            return false;
        }

        error = null;
        return true;
    }
}
