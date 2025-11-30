using Microsoft.OpenApi.Models;

namespace MCK9595.APIMocker.Core.OpenApi;

public record ApiEndpoint(
    string Path,
    string Method,
    string? OperationId,
    string? Summary,
    string? Description,
    OpenApiSchema? RequestBodySchema,
    OpenApiSchema? ResponseSchema,
    IReadOnlyList<ApiParameter> Parameters
);

public record ApiParameter(
    string Name,
    string In,
    bool Required,
    OpenApiSchema? Schema
);

public record ParsedOpenApiDocument(
    string Title,
    string Version,
    string? Description,
    IReadOnlyList<ApiEndpoint> Endpoints,
    IReadOnlyDictionary<string, OpenApiSchema> Schemas
);
