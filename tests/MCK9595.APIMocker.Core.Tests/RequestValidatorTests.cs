using MCK9595.APIMocker.Core.Validation;
using Microsoft.OpenApi.Models;
using Xunit;

namespace MCK9595.APIMocker.Core.Tests;

public class RequestValidatorTests
{
    private readonly RequestValidator _validator = new();

    [Fact]
    public void Validate_RequiredFieldMissing_ReturnsError()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Required = new HashSet<string> { "name", "email" },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["name"] = new() { Type = "string" },
                ["email"] = new() { Type = "string" }
            }
        };
        var data = new Dictionary<string, object?> { ["name"] = "Test" };

        var result = _validator.Validate(data, schema);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == "email");
    }

    [Fact]
    public void Validate_AllRequiredFieldsPresent_ReturnsSuccess()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Required = new HashSet<string> { "name" },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["name"] = new() { Type = "string" }
            }
        };
        var data = new Dictionary<string, object?> { ["name"] = "Test" };

        var result = _validator.Validate(data, schema);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WrongType_ReturnsError()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["age"] = new() { Type = "integer" }
            }
        };
        var data = new Dictionary<string, object?> { ["age"] = "not a number" };

        var result = _validator.Validate(data, schema);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == "age");
    }

    [Fact]
    public void Validate_IntegerAsWholeNumber_ReturnsSuccess()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["age"] = new() { Type = "integer" }
            }
        };
        // JSON deserializes numbers as long
        var data = new Dictionary<string, object?> { ["age"] = 30L };

        var result = _validator.Validate(data, schema);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_InvalidEmail_ReturnsError()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["email"] = new() { Type = "string", Format = "email" }
            }
        };
        var data = new Dictionary<string, object?> { ["email"] = "invalid-email" };

        var result = _validator.Validate(data, schema);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == "email");
    }

    [Fact]
    public void Validate_ValidEmail_ReturnsSuccess()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["email"] = new() { Type = "string", Format = "email" }
            }
        };
        var data = new Dictionary<string, object?> { ["email"] = "test@example.com" };

        var result = _validator.Validate(data, schema);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_StringTooShort_ReturnsError()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["name"] = new() { Type = "string", MinLength = 3 }
            }
        };
        var data = new Dictionary<string, object?> { ["name"] = "ab" };

        var result = _validator.Validate(data, schema);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == "name");
    }

    [Fact]
    public void Validate_StringTooLong_ReturnsError()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["name"] = new() { Type = "string", MaxLength = 5 }
            }
        };
        var data = new Dictionary<string, object?> { ["name"] = "too long name" };

        var result = _validator.Validate(data, schema);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_NumberBelowMinimum_ReturnsError()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["age"] = new() { Type = "integer", Minimum = 18 }
            }
        };
        var data = new Dictionary<string, object?> { ["age"] = 15 };

        var result = _validator.Validate(data, schema);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == "age");
    }

    [Fact]
    public void Validate_NumberAboveMaximum_ReturnsError()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["age"] = new() { Type = "integer", Maximum = 120 }
            }
        };
        var data = new Dictionary<string, object?> { ["age"] = 150 };

        var result = _validator.Validate(data, schema);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_InvalidEnumValue_ReturnsError()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["status"] = new()
                {
                    Type = "string",
                    Enum = new List<Microsoft.OpenApi.Any.IOpenApiAny>
                    {
                        new Microsoft.OpenApi.Any.OpenApiString("active"),
                        new Microsoft.OpenApi.Any.OpenApiString("inactive")
                    }
                }
            }
        };
        var data = new Dictionary<string, object?> { ["status"] = "unknown" };

        var result = _validator.Validate(data, schema);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == "status");
    }

    [Fact]
    public void Validate_ValidEnumValue_ReturnsSuccess()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["status"] = new()
                {
                    Type = "string",
                    Enum = new List<Microsoft.OpenApi.Any.IOpenApiAny>
                    {
                        new Microsoft.OpenApi.Any.OpenApiString("active"),
                        new Microsoft.OpenApi.Any.OpenApiString("inactive")
                    }
                }
            }
        };
        var data = new Dictionary<string, object?> { ["status"] = "active" };

        var result = _validator.Validate(data, schema);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_UnknownFieldIgnored_ReturnsSuccess()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Required = new HashSet<string> { "name" },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["name"] = new() { Type = "string" }
            }
        };
        var data = new Dictionary<string, object?>
        {
            ["name"] = "Test",
            ["unknownField"] = "should be ignored"
        };

        var result = _validator.Validate(data, schema);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_InvalidUuid_ReturnsError()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["id"] = new() { Type = "string", Format = "uuid" }
            }
        };
        var data = new Dictionary<string, object?> { ["id"] = "not-a-uuid" };

        var result = _validator.Validate(data, schema);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ValidUuid_ReturnsSuccess()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["id"] = new() { Type = "string", Format = "uuid" }
            }
        };
        var data = new Dictionary<string, object?> { ["id"] = "550e8400-e29b-41d4-a716-446655440000" };

        var result = _validator.Validate(data, schema);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NullValue_SkipsValidation()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["age"] = new() { Type = "integer" }
            }
        };
        var data = new Dictionary<string, object?> { ["age"] = null };

        var result = _validator.Validate(data, schema);

        Assert.True(result.IsValid);
    }
}
