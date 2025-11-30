namespace MCK9595.APIMocker.Core.Auth;

/// <summary>
/// Authentication mode for the mock server.
/// </summary>
public enum AuthMode
{
    /// <summary>No authentication required</summary>
    None,
    /// <summary>Bearer token authentication (any non-empty token accepted)</summary>
    Bearer,
    /// <summary>API Key authentication via X-API-Key header</summary>
    ApiKey,
    /// <summary>Basic authentication (username:password)</summary>
    Basic
}

/// <summary>
/// Authentication result with validation status and optional error message.
/// </summary>
public record AuthResult(bool IsValid, string? ErrorMessage = null);

/// <summary>
/// Interface for authentication providers.
/// </summary>
public interface IAuthProvider
{
    /// <summary>
    /// Validates the authentication header value.
    /// </summary>
    /// <param name="headerValue">The value of the Authorization or X-API-Key header</param>
    /// <returns>Authentication result</returns>
    AuthResult Validate(string? headerValue);

    /// <summary>
    /// Gets the expected header name for this authentication method.
    /// </summary>
    string HeaderName { get; }
}
