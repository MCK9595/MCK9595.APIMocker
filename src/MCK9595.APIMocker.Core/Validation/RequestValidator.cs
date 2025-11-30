using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;

namespace MCK9595.APIMocker.Core.Validation;

public class RequestValidator : IRequestValidator
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex UuidRegex = new(
        @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ValidationResult Validate(Dictionary<string, object?> data, OpenApiSchema schema)
    {
        var errors = new List<ValidationError>();

        if (schema.Type != "object")
        {
            return ValidationResult.Success();
        }

        // Check required fields
        foreach (var requiredField in schema.Required)
        {
            if (!data.ContainsKey(requiredField) || data[requiredField] == null)
            {
                errors.Add(new ValidationError(requiredField, $"Field '{requiredField}' is required"));
            }
        }

        // Validate each property
        foreach (var (fieldName, value) in data)
        {
            if (!schema.Properties.TryGetValue(fieldName, out var propertySchema))
            {
                continue; // Skip unknown fields
            }

            var fieldErrors = ValidateField(fieldName, value, propertySchema);
            errors.AddRange(fieldErrors);
        }

        return errors.Count > 0
            ? ValidationResult.Failure(errors)
            : ValidationResult.Success();
    }

    private static IEnumerable<ValidationError> ValidateField(string fieldName, object? value, OpenApiSchema schema)
    {
        if (value == null)
        {
            yield break;
        }

        // Handle JsonElement from deserialization
        var actualValue = value is JsonElement jsonElement
            ? ConvertJsonElement(jsonElement)
            : value;

        // Type validation
        var typeError = ValidateType(fieldName, actualValue, schema);
        if (typeError != null)
        {
            yield return typeError;
            yield break; // Skip further validation if type is wrong
        }

        // Format validation
        var formatError = ValidateFormat(fieldName, actualValue, schema);
        if (formatError != null)
        {
            yield return formatError;
        }

        // String validations
        if (actualValue is string stringValue)
        {
            if (schema.MinLength.HasValue && stringValue.Length < schema.MinLength.Value)
            {
                yield return new ValidationError(fieldName,
                    $"Field '{fieldName}' must be at least {schema.MinLength.Value} characters");
            }

            if (schema.MaxLength.HasValue && stringValue.Length > schema.MaxLength.Value)
            {
                yield return new ValidationError(fieldName,
                    $"Field '{fieldName}' must be at most {schema.MaxLength.Value} characters");
            }
        }

        // Numeric validations
        if (TryGetNumericValue(actualValue, out var numericValue))
        {
            if (schema.Minimum.HasValue && numericValue < (decimal)schema.Minimum.Value)
            {
                yield return new ValidationError(fieldName,
                    $"Field '{fieldName}' must be at least {schema.Minimum.Value}");
            }

            if (schema.Maximum.HasValue && numericValue > (decimal)schema.Maximum.Value)
            {
                yield return new ValidationError(fieldName,
                    $"Field '{fieldName}' must be at most {schema.Maximum.Value}");
            }
        }

        // Enum validation
        if (schema.Enum.Count > 0)
        {
            var enumValues = schema.Enum
                .Select(e => e is Microsoft.OpenApi.Any.OpenApiString s ? s.Value : e?.ToString())
                .ToList();

            var stringVal = actualValue?.ToString();
            if (!enumValues.Contains(stringVal))
            {
                yield return new ValidationError(fieldName,
                    $"Field '{fieldName}' must be one of: {string.Join(", ", enumValues)}");
            }
        }
    }

    private static ValidationError? ValidateType(string fieldName, object? value, OpenApiSchema schema)
    {
        if (value == null || string.IsNullOrEmpty(schema.Type))
        {
            return null;
        }

        var isValid = schema.Type switch
        {
            "string" => value is string,
            "integer" => IsInteger(value),
            "number" => IsNumber(value),
            "boolean" => value is bool,
            "array" => value is IEnumerable<object> or Array,
            "object" => value is Dictionary<string, object?> or IDictionary<string, object?>,
            _ => true
        };

        if (!isValid)
        {
            return new ValidationError(fieldName, $"Field '{fieldName}' must be of type '{schema.Type}'");
        }

        return null;
    }

    private static ValidationError? ValidateFormat(string fieldName, object? value, OpenApiSchema schema)
    {
        if (value == null || string.IsNullOrEmpty(schema.Format))
        {
            return null;
        }

        var stringValue = value.ToString() ?? "";

        var isValid = schema.Format switch
        {
            "email" => EmailRegex.IsMatch(stringValue),
            "uuid" => UuidRegex.IsMatch(stringValue),
            "date" => DateTime.TryParse(stringValue, out _),
            "date-time" => DateTime.TryParse(stringValue, out _),
            "uri" => Uri.TryCreate(stringValue, UriKind.Absolute, out _),
            _ => true
        };

        if (!isValid)
        {
            return new ValidationError(fieldName, $"Field '{fieldName}' must be a valid {schema.Format}");
        }

        return null;
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray()
                .Select(ConvertJsonElement)
                .ToList(),
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value)),
            _ => element.ToString()
        };
    }

    private static bool IsInteger(object value)
    {
        if (value is int or long or short or byte or sbyte or uint or ulong or ushort)
        {
            return true;
        }

        // Handle decimal that represents a whole number
        if (value is decimal d)
        {
            return d == Math.Truncate(d);
        }

        // Handle double that represents a whole number
        if (value is double dbl)
        {
            return Math.Abs(dbl - Math.Truncate(dbl)) < 0.0001;
        }

        return false;
    }

    private static bool IsNumber(object value)
    {
        return value is int or long or short or byte or sbyte or uint or ulong or ushort
            or float or double or decimal;
    }

    private static bool TryGetNumericValue(object? value, out decimal numericValue)
    {
        numericValue = 0;
        if (value == null) return false;

        try
        {
            numericValue = Convert.ToDecimal(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
