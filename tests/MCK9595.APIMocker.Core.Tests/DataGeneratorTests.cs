using MCK9595.APIMocker.Core.Generator;
using Microsoft.OpenApi.Models;
using Xunit;

namespace MCK9595.APIMocker.Core.Tests;

public class DataGeneratorTests
{
    private readonly DataGenerator _generator = new();

    [Fact]
    public void GenerateFromSchema_GeneratesIdField()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["id"] = new() { Type = "integer" }
            }
        };

        var data = _generator.GenerateFromSchema(schema, id: 42);

        Assert.Equal(42, data["id"]);
    }

    [Fact]
    public void GenerateFromSchema_GeneratesStringField()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["name"] = new() { Type = "string" }
            }
        };

        var data = _generator.GenerateFromSchema(schema);

        Assert.NotNull(data["name"]);
        Assert.IsType<string>(data["name"]);
    }

    [Fact]
    public void GenerateFromSchema_GeneratesEmailFormat()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["email"] = new() { Type = "string", Format = "email" }
            }
        };

        var data = _generator.GenerateFromSchema(schema);

        var email = data["email"] as string;
        Assert.NotNull(email);
        Assert.Contains("@", email);
    }

    [Fact]
    public void GenerateFromSchema_GeneratesDateTimeFormat()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["createdAt"] = new() { Type = "string", Format = "date-time" }
            }
        };

        var data = _generator.GenerateFromSchema(schema);

        var dateTime = data["createdAt"] as string;
        Assert.NotNull(dateTime);
        Assert.Contains("T", dateTime);
    }

    [Fact]
    public void GenerateFromSchema_GeneratesUuidFormat()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["uuid"] = new() { Type = "string", Format = "uuid" }
            }
        };

        var data = _generator.GenerateFromSchema(schema);

        var uuid = data["uuid"] as string;
        Assert.NotNull(uuid);
        Assert.True(Guid.TryParse(uuid, out _));
    }

    [Fact]
    public void GenerateFromSchema_GeneratesBooleanField()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["active"] = new() { Type = "boolean" }
            }
        };

        var data = _generator.GenerateFromSchema(schema);

        Assert.IsType<bool>(data["active"]);
    }

    [Fact]
    public void GenerateFromSchema_RespectsEnumValues()
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
                        new Microsoft.OpenApi.Any.OpenApiString("inactive"),
                        new Microsoft.OpenApi.Any.OpenApiString("pending")
                    }
                }
            }
        };

        var data = _generator.GenerateFromSchema(schema);

        var status = data["status"] as string;
        Assert.Contains(status, new[] { "active", "inactive", "pending" });
    }

    [Fact]
    public void GenerateFromSchema_GeneratesAgeWithinBounds()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["age"] = new() { Type = "integer", Minimum = 18, Maximum = 65 }
            }
        };

        // Generate multiple times to test bounds
        for (int i = 0; i < 10; i++)
        {
            var data = _generator.GenerateFromSchema(schema);
            var age = Convert.ToInt32(data["age"]);
            Assert.InRange(age, 18, 65);
        }
    }

    [Fact]
    public void GenerateMany_GeneratesCorrectCount()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["name"] = new() { Type = "string" }
            }
        };

        var items = _generator.GenerateMany(schema, count: 5);

        Assert.Equal(5, items.Count);
    }

    [Fact]
    public void GenerateMany_GeneratesSequentialIds()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["id"] = new() { Type = "integer" }
            }
        };

        var items = _generator.GenerateMany(schema, count: 3, startId: 10);

        Assert.Equal(10, items[0]["id"]);
        Assert.Equal(11, items[1]["id"]);
        Assert.Equal(12, items[2]["id"]);
    }

    [Fact]
    public void GenerateFromSchema_GeneratesPhoneNumber()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["phoneNumber"] = new() { Type = "string" }
            }
        };

        var data = _generator.GenerateFromSchema(schema);

        var phone = data["phoneNumber"] as string;
        Assert.NotNull(phone);
        Assert.Contains("-", phone);
    }

    [Fact]
    public void GenerateFromSchema_GeneratesUrl()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["website"] = new() { Type = "string", Format = "uri" }
            }
        };

        var data = _generator.GenerateFromSchema(schema);

        var url = data["website"] as string;
        Assert.NotNull(url);
        Assert.StartsWith("http", url);
    }

    [Fact]
    public void GenerateFromSchema_EmptySchema_ReturnsEmptyDictionary()
    {
        var schema = new OpenApiSchema
        {
            Type = "object"
        };

        var data = _generator.GenerateFromSchema(schema);

        Assert.Empty(data);
    }

    [Fact]
    public void GenerateFromSchema_GeneratesArray()
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["tags"] = new()
                {
                    Type = "array",
                    Items = new OpenApiSchema { Type = "string" }
                }
            }
        };

        var data = _generator.GenerateFromSchema(schema);

        Assert.NotNull(data["tags"]);
        var tags = data["tags"] as List<object?>;
        Assert.NotNull(tags);
        Assert.True(tags.Count >= 1);
    }
}
