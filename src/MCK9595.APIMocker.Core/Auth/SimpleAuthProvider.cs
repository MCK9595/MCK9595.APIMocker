using System.Text;

namespace MCK9595.APIMocker.Core.Auth;

/// <summary>
/// Simple authentication provider supporting Bearer, API Key, and Basic auth.
/// </summary>
public class SimpleAuthProvider : IAuthProvider
{
    private readonly AuthMode _mode;
    private readonly string? _expectedKey;

    public SimpleAuthProvider(AuthMode mode, string? expectedKey = null)
    {
        _mode = mode;
        _expectedKey = expectedKey;
    }

    public string HeaderName => _mode switch
    {
        AuthMode.ApiKey => "X-API-Key",
        _ => "Authorization"
    };

    public AuthResult Validate(string? headerValue)
    {
        if (_mode == AuthMode.None)
        {
            return new AuthResult(true);
        }

        if (string.IsNullOrWhiteSpace(headerValue))
        {
            return new AuthResult(false, $"Missing {HeaderName} header");
        }

        return _mode switch
        {
            AuthMode.Bearer => ValidateBearer(headerValue),
            AuthMode.ApiKey => ValidateApiKey(headerValue),
            AuthMode.Basic => ValidateBasic(headerValue),
            _ => new AuthResult(true)
        };
    }

    private AuthResult ValidateBearer(string headerValue)
    {
        // Accept "Bearer <token>" format
        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return new AuthResult(false, "Invalid Authorization header format. Expected: Bearer <token>");
        }

        var token = headerValue["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(token))
        {
            return new AuthResult(false, "Empty Bearer token");
        }

        // If a specific key is set, validate against it
        if (!string.IsNullOrEmpty(_expectedKey) && token != _expectedKey)
        {
            return new AuthResult(false, "Invalid Bearer token");
        }

        return new AuthResult(true);
    }

    private AuthResult ValidateApiKey(string headerValue)
    {
        if (string.IsNullOrEmpty(_expectedKey))
        {
            // No specific key required, any non-empty key is valid
            return new AuthResult(true);
        }

        if (headerValue != _expectedKey)
        {
            return new AuthResult(false, "Invalid API key");
        }

        return new AuthResult(true);
    }

    private AuthResult ValidateBasic(string headerValue)
    {
        // Accept "Basic <base64>" format
        if (!headerValue.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return new AuthResult(false, "Invalid Authorization header format. Expected: Basic <base64>");
        }

        var base64 = headerValue["Basic ".Length..].Trim();
        if (string.IsNullOrEmpty(base64))
        {
            return new AuthResult(false, "Empty Basic credentials");
        }

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            var parts = decoded.Split(':', 2);

            if (parts.Length != 2)
            {
                return new AuthResult(false, "Invalid Basic credentials format");
            }

            // If a specific key is set (format: username:password), validate against it
            if (!string.IsNullOrEmpty(_expectedKey))
            {
                if (decoded != _expectedKey)
                {
                    return new AuthResult(false, "Invalid username or password");
                }
            }

            return new AuthResult(true);
        }
        catch (FormatException)
        {
            return new AuthResult(false, "Invalid Base64 encoding in Basic credentials");
        }
    }

    /// <summary>
    /// Creates an auth provider from CLI options.
    /// </summary>
    public static IAuthProvider? FromOptions(string? authMode, string? authKey)
    {
        if (string.IsNullOrEmpty(authMode))
        {
            return null;
        }

        var mode = authMode.ToLowerInvariant() switch
        {
            "bearer" => AuthMode.Bearer,
            "apikey" => AuthMode.ApiKey,
            "basic" => AuthMode.Basic,
            "none" => AuthMode.None,
            _ => throw new ArgumentException($"Unknown auth mode: {authMode}. Valid values: bearer, apikey, basic, none")
        };

        return new SimpleAuthProvider(mode, authKey);
    }
}
