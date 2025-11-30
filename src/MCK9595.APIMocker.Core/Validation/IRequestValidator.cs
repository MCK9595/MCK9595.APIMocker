using Microsoft.OpenApi.Models;

namespace MCK9595.APIMocker.Core.Validation;

public record ValidationError(string Field, string Message);

public record ValidationResult(bool IsValid, IReadOnlyList<ValidationError> Errors)
{
    public static ValidationResult Success() => new(true, Array.Empty<ValidationError>());
    public static ValidationResult Failure(IEnumerable<ValidationError> errors) => new(false, errors.ToList());
}

public interface IRequestValidator
{
    ValidationResult Validate(Dictionary<string, object?> data, OpenApiSchema schema);
}
