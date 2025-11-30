using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace MCK9595.APIMocker.Core.Responses;

/// <summary>
/// Custom response configuration.
/// </summary>
public class CustomResponseConfig
{
    public string Method { get; set; } = "";
    public string Path { get; set; } = "";
    public Dictionary<string, object?>? Match { get; set; }
    public int Status { get; set; } = 200;
    public object? Body { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
}

/// <summary>
/// Custom responses configuration file format.
/// </summary>
public class CustomResponseConfigFile
{
    public List<CustomResponseConfig> Responses { get; set; } = new();
}

/// <summary>
/// Provides custom responses for matching requests.
/// </summary>
public class CustomResponseProvider
{
    private readonly List<CustomResponseConfig> _responses = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Gets the count of registered custom responses.
    /// </summary>
    public int ResponseCount => _responses.Count;

    /// <summary>
    /// Registers a custom response configuration.
    /// </summary>
    public void Register(CustomResponseConfig config)
    {
        _responses.Add(config);
    }

    /// <summary>
    /// Loads custom response configurations from a JSON file.
    /// </summary>
    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Custom responses file not found: {filePath}");
        }

        var json = File.ReadAllText(filePath);
        var config = JsonSerializer.Deserialize<CustomResponseConfigFile>(json, _jsonOptions);

        if (config?.Responses != null)
        {
            foreach (var response in config.Responses)
            {
                Register(response);
            }
        }
    }

    /// <summary>
    /// Tries to find a matching custom response for the request.
    /// </summary>
    public CustomResponseConfig? FindMatch(HttpRequest request, Dictionary<string, object?>? requestBody = null)
    {
        var method = request.Method.ToUpperInvariant();
        var path = request.Path.Value ?? "";

        foreach (var response in _responses)
        {
            // Method match
            if (!string.Equals(response.Method, method, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Path match (supports wildcards)
            if (!MatchesPath(response.Path, path))
            {
                continue;
            }

            // Body match (if specified)
            if (response.Match != null && requestBody != null)
            {
                if (!MatchesBody(response.Match, requestBody))
                {
                    continue;
                }
            }
            else if (response.Match != null && requestBody == null)
            {
                // Match specified but no body provided
                continue;
            }

            return response;
        }

        return null;
    }

    private bool MatchesPath(string pattern, string path)
    {
        // Normalize paths
        pattern = pattern.TrimEnd('/');
        path = path.TrimEnd('/');

        if (string.IsNullOrEmpty(pattern)) pattern = "/";
        if (string.IsNullOrEmpty(path)) path = "/";

        // Wildcard at end: /users/* matches /users/1, /users/2, etc.
        if (pattern.EndsWith("/*"))
        {
            var prefix = pattern[..^2];
            return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                   (path.Length == prefix.Length || path[prefix.Length] == '/');
        }

        // Exact match
        return string.Equals(pattern, path, StringComparison.OrdinalIgnoreCase);
    }

    private bool MatchesBody(Dictionary<string, object?> match, Dictionary<string, object?> body)
    {
        foreach (var (key, expectedValue) in match)
        {
            if (!body.TryGetValue(key, out var actualValue))
            {
                return false;
            }

            // Convert JsonElement if needed
            var expected = ConvertValue(expectedValue);
            var actual = ConvertValue(actualValue);

            if (!ValuesEqual(expected, actual))
            {
                return false;
            }
        }

        return true;
    }

    private object? ConvertValue(object? value)
    {
        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => element.ToString()
            };
        }
        return value;
    }

    private bool ValuesEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        // String comparison
        return string.Equals(a.ToString(), b.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates a custom response provider from a config file path.
    /// </summary>
    public static CustomResponseProvider? FromFile(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return null;

        var provider = new CustomResponseProvider();
        provider.LoadFromFile(filePath);
        return provider;
    }
}
