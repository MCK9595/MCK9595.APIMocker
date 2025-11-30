namespace MCK9595.APIMocker.Core.OpenApi;

public interface IOpenApiParser
{
    ParsedOpenApiDocument Parse(string filePath);
}
