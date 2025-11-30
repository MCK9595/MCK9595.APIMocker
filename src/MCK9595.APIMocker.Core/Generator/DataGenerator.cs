using Bogus;
using Microsoft.OpenApi.Models;

namespace MCK9595.APIMocker.Core.Generator;

public class DataGenerator : IDataGenerator
{
    private readonly Faker _faker;

    public DataGenerator()
    {
        _faker = new Faker("ja");
    }

    public List<Dictionary<string, object?>> GenerateMany(OpenApiSchema schema, int count, int startId = 1)
    {
        var items = new List<Dictionary<string, object?>>();
        for (var i = 0; i < count; i++)
        {
            items.Add(GenerateFromSchema(schema, startId + i));
        }
        return items;
    }

    public Dictionary<string, object?> GenerateFromSchema(OpenApiSchema schema, int id = 1)
    {
        var data = new Dictionary<string, object?>();

        if (schema.Properties == null || schema.Properties.Count == 0)
        {
            return data;
        }

        foreach (var (name, propSchema) in schema.Properties)
        {
            data[name] = GenerateValue(name, propSchema, id);
        }

        return data;
    }

    private object? GenerateValue(string propertyName, OpenApiSchema schema, int id)
    {
        // Handle $ref - if the schema has a reference, use its properties
        if (schema.Reference != null && schema.Properties == null)
        {
            return null;
        }

        // Handle enum
        if (schema.Enum?.Count > 0)
        {
            var enumValues = schema.Enum
                .Select(e => e is Microsoft.OpenApi.Any.OpenApiString s ? s.Value : e.ToString())
                .ToList();
            return _faker.PickRandom(enumValues);
        }

        // Handle by type
        return schema.Type?.ToLower() switch
        {
            "integer" or "number" => GenerateNumber(propertyName, schema, id),
            "string" => GenerateString(propertyName, schema),
            "boolean" => _faker.Random.Bool(),
            "array" => GenerateArray(propertyName, schema, id),
            "object" => GenerateFromSchema(schema, id),
            _ => null
        };
    }

    private object GenerateNumber(string propertyName, OpenApiSchema schema, int id)
    {
        var nameLower = propertyName.ToLower();

        // ID field
        if (nameLower == "id" || nameLower.EndsWith("id"))
        {
            return id;
        }

        // Age field
        if (nameLower.Contains("age"))
        {
            var min = (int)(schema.Minimum ?? 18);
            var max = (int)(schema.Maximum ?? 80);
            return _faker.Random.Int(min, max);
        }

        // General number
        var minimum = (int)(schema.Minimum ?? 0);
        var maximum = (int)(schema.Maximum ?? 1000);
        return schema.Type == "integer"
            ? _faker.Random.Int(minimum, maximum)
            : _faker.Random.Double(minimum, maximum);
    }

    private object GenerateString(string propertyName, OpenApiSchema schema)
    {
        var nameLower = propertyName.ToLower();
        var format = schema.Format?.ToLower();

        // Handle format first
        if (!string.IsNullOrEmpty(format))
        {
            return format switch
            {
                "email" => _faker.Internet.Email(),
                "date" => _faker.Date.Past(2).ToString("yyyy-MM-dd"),
                "date-time" => _faker.Date.Past(2).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                "uri" or "url" => _faker.Internet.Url(),
                "uuid" => Guid.NewGuid().ToString(),
                "phone" => _faker.Phone.PhoneNumber("0#0-####-####"),
                _ => _faker.Lorem.Word()
            };
        }

        // Handle by field name
        if (nameLower.Contains("name") && !nameLower.Contains("file"))
        {
            return _faker.Name.FullName();
        }

        if (nameLower.Contains("email"))
        {
            return _faker.Internet.Email();
        }

        if (nameLower.Contains("phone") || nameLower.Contains("tel"))
        {
            return _faker.Phone.PhoneNumber("0#0-####-####");
        }

        if (nameLower.Contains("address") || nameLower.Contains("住所"))
        {
            return _faker.Address.FullAddress();
        }

        if (nameLower.Contains("city") || nameLower.Contains("市"))
        {
            return _faker.Address.City();
        }

        if (nameLower.Contains("title") || nameLower.Contains("タイトル"))
        {
            return _faker.Lorem.Sentence(3);
        }

        if (nameLower.Contains("description") || nameLower.Contains("説明"))
        {
            return _faker.Lorem.Paragraph();
        }

        if (nameLower.Contains("url") || nameLower.Contains("link"))
        {
            return _faker.Internet.Url();
        }

        if (nameLower.Contains("image") || nameLower.Contains("avatar") || nameLower.Contains("photo"))
        {
            return _faker.Image.PicsumUrl();
        }

        if (nameLower.Contains("status") || nameLower.Contains("state"))
        {
            return _faker.PickRandom("active", "inactive", "pending");
        }

        if (nameLower.Contains("created") || nameLower.Contains("updated") || nameLower.Contains("date"))
        {
            return _faker.Date.Past(2).ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        // Default
        var maxLength = schema.MaxLength ?? 50;
        var text = _faker.Lorem.Text();
        return text[..Math.Min((int)maxLength, text.Length)];
    }

    private object GenerateArray(string propertyName, OpenApiSchema schema, int id)
    {
        var items = new List<object?>();
        var count = _faker.Random.Int(1, 5);

        if (schema.Items != null)
        {
            for (var i = 0; i < count; i++)
            {
                items.Add(GenerateValue(propertyName, schema.Items, id + i));
            }
        }

        return items;
    }
}
