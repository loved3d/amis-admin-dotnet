using System.ComponentModel.DataAnnotations;

namespace AmisAdminDotNet.Services;

/// <summary>
/// Pydantic-like model validation helper backed by <see cref="Validator"/> and
/// DataAnnotations.
/// </summary>
public static class DataAnnotationsModelValidator
{
    public static bool TryValidate(object instance, out IReadOnlyDictionary<string, string[]> errors)
    {
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            instance,
            new ValidationContext(instance),
            validationResults,
            validateAllProperties: true);

        if (isValid)
        {
            errors = new Dictionary<string, string[]>();
            return true;
        }

        errors = validationResults
            .SelectMany(result =>
            {
                var members = result.MemberNames.Any()
                    ? result.MemberNames
                    : ["$"];

                return members.Select(member => new
                {
                    Member = member,
                    Error = result.ErrorMessage ?? "Validation failed."
                });
            })
            .GroupBy(item => item.Member, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => item.Error).Distinct().ToArray(),
                StringComparer.Ordinal);

        return false;
    }
}
