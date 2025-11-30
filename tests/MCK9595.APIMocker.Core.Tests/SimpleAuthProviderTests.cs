using MCK9595.APIMocker.Core.Auth;
using Xunit;

namespace MCK9595.APIMocker.Core.Tests;

public class SimpleAuthProviderTests
{
    [Fact]
    public void Bearer_ValidToken_ReturnsValid()
    {
        var provider = new SimpleAuthProvider(AuthMode.Bearer);

        var result = provider.Validate("Bearer my-token");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Bearer_MissingHeader_ReturnsInvalid()
    {
        var provider = new SimpleAuthProvider(AuthMode.Bearer);

        var result = provider.Validate(null);

        Assert.False(result.IsValid);
        Assert.Contains("Missing", result.ErrorMessage);
    }

    [Fact]
    public void Bearer_WrongFormat_ReturnsInvalid()
    {
        var provider = new SimpleAuthProvider(AuthMode.Bearer);

        var result = provider.Validate("Basic token");

        Assert.False(result.IsValid);
        Assert.Contains("format", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Bearer_EmptyToken_ReturnsInvalid()
    {
        var provider = new SimpleAuthProvider(AuthMode.Bearer);

        var result = provider.Validate("Bearer ");

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Bearer_SpecificKey_MatchingToken_ReturnsValid()
    {
        var provider = new SimpleAuthProvider(AuthMode.Bearer, "secret-token");

        var result = provider.Validate("Bearer secret-token");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Bearer_SpecificKey_NonMatchingToken_ReturnsInvalid()
    {
        var provider = new SimpleAuthProvider(AuthMode.Bearer, "secret-token");

        var result = provider.Validate("Bearer wrong-token");

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ApiKey_ValidKey_ReturnsValid()
    {
        var provider = new SimpleAuthProvider(AuthMode.ApiKey, "my-api-key");

        var result = provider.Validate("my-api-key");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ApiKey_WrongKey_ReturnsInvalid()
    {
        var provider = new SimpleAuthProvider(AuthMode.ApiKey, "my-api-key");

        var result = provider.Validate("wrong-key");

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ApiKey_NoKeyRequired_AnyKeyValid()
    {
        var provider = new SimpleAuthProvider(AuthMode.ApiKey);

        var result = provider.Validate("any-key");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Basic_ValidCredentials_ReturnsValid()
    {
        var provider = new SimpleAuthProvider(AuthMode.Basic, "user:pass");
        var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("user:pass"));

        var result = provider.Validate($"Basic {base64}");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Basic_WrongCredentials_ReturnsInvalid()
    {
        var provider = new SimpleAuthProvider(AuthMode.Basic, "user:pass");
        var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("wrong:creds"));

        var result = provider.Validate($"Basic {base64}");

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Basic_InvalidBase64_ReturnsInvalid()
    {
        var provider = new SimpleAuthProvider(AuthMode.Basic);

        var result = provider.Validate("Basic not-valid-base64!!!");

        Assert.False(result.IsValid);
    }

    [Fact]
    public void None_Always_ReturnsValid()
    {
        var provider = new SimpleAuthProvider(AuthMode.None);

        var result = provider.Validate(null);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void FromOptions_Bearer_ReturnsProvider()
    {
        var provider = SimpleAuthProvider.FromOptions("bearer", null);

        Assert.NotNull(provider);
    }

    [Fact]
    public void FromOptions_Null_ReturnsNull()
    {
        var provider = SimpleAuthProvider.FromOptions(null, null);

        Assert.Null(provider);
    }

    [Fact]
    public void FromOptions_Invalid_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => SimpleAuthProvider.FromOptions("invalid", null));
    }

    [Fact]
    public void HeaderName_ApiKey_ReturnsXApiKey()
    {
        var provider = new SimpleAuthProvider(AuthMode.ApiKey);

        Assert.Equal("X-API-Key", provider.HeaderName);
    }

    [Fact]
    public void HeaderName_Bearer_ReturnsAuthorization()
    {
        var provider = new SimpleAuthProvider(AuthMode.Bearer);

        Assert.Equal("Authorization", provider.HeaderName);
    }
}
