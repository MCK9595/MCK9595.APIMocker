using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace MCK9595.APIMocker.Core.OpenApi;

public class OpenApiParser : IOpenApiParser
{
    public ParsedOpenApiDocument Parse(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"OpenAPI file not found: {filePath}");
        }

        using var stream = File.OpenRead(filePath);
        var reader = new OpenApiStreamReader();
        var openApiDoc = reader.Read(stream, out var diagnostic);

        if (diagnostic.Errors.Any())
        {
            var errors = string.Join("\n", diagnostic.Errors.Select(e => e.Message));
            throw new InvalidOperationException($"OpenAPI parsing failed:\n{errors}");
        }

        var endpoints = ExtractEndpoints(openApiDoc);
        var schemas = openApiDoc.Components?.Schemas?.ToDictionary(x => x.Key, x => x.Value)
            ?? new Dictionary<string, OpenApiSchema>();

        return new ParsedOpenApiDocument(
            Title: openApiDoc.Info.Title,
            Version: openApiDoc.Info.Version,
            Description: openApiDoc.Info.Description,
            Endpoints: endpoints,
            Schemas: schemas
        );
    }

    private static List<ApiEndpoint> ExtractEndpoints(OpenApiDocument doc)
    {
        var endpoints = new List<ApiEndpoint>();

        foreach (var (path, pathItem) in doc.Paths)
        {
            foreach (var (method, operation) in pathItem.Operations)
            {
                var responseSchema = GetResponseSchema(operation);
                var requestBodySchema = GetRequestBodySchema(operation);
                var parameters = GetParameters(pathItem, operation);

                endpoints.Add(new ApiEndpoint(
                    Path: path,
                    Method: method.ToString().ToUpper(),
                    OperationId: operation.OperationId,
                    Summary: operation.Summary,
                    Description: operation.Description,
                    RequestBodySchema: requestBodySchema,
                    ResponseSchema: responseSchema,
                    Parameters: parameters
                ));
            }
        }

        return endpoints;
    }

    private static OpenApiSchema? GetResponseSchema(OpenApiOperation operation)
    {
        var successResponse = operation.Responses
            .FirstOrDefault(r => r.Key.StartsWith("2"));

        if (successResponse.Value?.Content == null)
            return null;

        var jsonContent = successResponse.Value.Content
            .FirstOrDefault(c => c.Key.Contains("json", StringComparison.OrdinalIgnoreCase));

        return jsonContent.Value?.Schema;
    }

    private static OpenApiSchema? GetRequestBodySchema(OpenApiOperation operation)
    {
        if (operation.RequestBody?.Content == null)
            return null;

        var jsonContent = operation.RequestBody.Content
            .FirstOrDefault(c => c.Key.Contains("json", StringComparison.OrdinalIgnoreCase));

        return jsonContent.Value?.Schema;
    }

    private static List<ApiParameter> GetParameters(OpenApiPathItem pathItem, OpenApiOperation operation)
    {
        var parameters = new List<ApiParameter>();

        // Path-level parameters
        foreach (var param in pathItem.Parameters)
        {
            parameters.Add(new ApiParameter(
                Name: param.Name,
                In: param.In?.ToString() ?? "query",
                Required: param.Required,
                Schema: param.Schema
            ));
        }

        // Operation-level parameters
        foreach (var param in operation.Parameters)
        {
            parameters.Add(new ApiParameter(
                Name: param.Name,
                In: param.In?.ToString() ?? "query",
                Required: param.Required,
                Schema: param.Schema
            ));
        }

        return parameters;
    }
}
