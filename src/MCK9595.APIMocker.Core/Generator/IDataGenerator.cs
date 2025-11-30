using Microsoft.OpenApi.Models;

namespace MCK9595.APIMocker.Core.Generator;

public interface IDataGenerator
{
    Dictionary<string, object?> GenerateFromSchema(OpenApiSchema schema, int id = 1);
    List<Dictionary<string, object?>> GenerateMany(OpenApiSchema schema, int count, int startId = 1);
}
