using FluentValidation;
using AmisAdminDotNet.Models;

namespace AmisAdminDotNet.Services;

/// <summary>
/// FluentValidation validator for <see cref="SaveUserRequest"/>.
/// Provides the same rules that were previously implemented inline in
/// <see cref="UserRequestValidator"/>, expressed via the FluentValidation DSL.
/// </summary>
public sealed class SaveUserRequestValidator : AbstractValidator<SaveUserRequest>
{
    public SaveUserRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid address.");

        RuleFor(r => r.Role)
            .NotEmpty().WithMessage("Role is required.");
    }
}
