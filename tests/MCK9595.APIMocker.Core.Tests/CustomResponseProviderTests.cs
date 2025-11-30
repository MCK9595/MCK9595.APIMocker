using MCK9595.APIMocker.Core.Responses;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace MCK9595.APIMocker.Core.Tests;

public class CustomResponseProviderTests
{
    [Fact]
    public void FindMatch_ExactPathMatch_ReturnsResponse()
    {
        var provider = new CustomResponseProvider();
        provider.Register(new CustomResponseConfig
        {
            Method = "GET",
            Path = "/health",
            Status = 200,
            Body = new { status = "ok" }
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/health";

        var match = provider.FindMatch(context.Request);

        Assert.NotNull(match);
        Assert.Equal(200, match.Status);
    }

    [Fact]
    public void FindMatch_WildcardPath_MatchesSubPaths()
    {
        var provider = new CustomResponseProvider();
        provider.Register(new CustomResponseConfig
        {
            Method = "GET",
            Path = "/users/*",
            Status = 200
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/users/123";

        var match = provider.FindMatch(context.Request);

        Assert.NotNull(match);
    }

    [Fact]
    public void FindMatch_WrongMethod_ReturnsNull()
    {
        var provider = new CustomResponseProvider();
        provider.Register(new CustomResponseConfig
        {
            Method = "GET",
            Path = "/health"
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/health";

        var match = provider.FindMatch(context.Request);

        Assert.Null(match);
    }

    [Fact]
    public void FindMatch_WrongPath_ReturnsNull()
    {
        var provider = new CustomResponseProvider();
        provider.Register(new CustomResponseConfig
        {
            Method = "GET",
            Path = "/health"
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/other";

        var match = provider.FindMatch(context.Request);

        Assert.Null(match);
    }

    [Fact]
    public void FindMatch_BodyMatch_ReturnsResponse()
    {
        var provider = new CustomResponseProvider();
        provider.Register(new CustomResponseConfig
        {
            Method = "POST",
            Path = "/users",
            Match = new Dictionary<string, object?> { ["email"] = "error@test.com" },
            Status = 409,
            Body = new { error = "Email exists" }
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/users";

        var requestBody = new Dictionary<string, object?>
        {
            ["name"] = "Test",
            ["email"] = "error@test.com"
        };

        var match = provider.FindMatch(context.Request, requestBody);

        Assert.NotNull(match);
        Assert.Equal(409, match.Status);
    }

    [Fact]
    public void FindMatch_BodyNoMatch_ReturnsNull()
    {
        var provider = new CustomResponseProvider();
        provider.Register(new CustomResponseConfig
        {
            Method = "POST",
            Path = "/users",
            Match = new Dictionary<string, object?> { ["email"] = "error@test.com" },
            Status = 409
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/users";

        var requestBody = new Dictionary<string, object?>
        {
            ["name"] = "Test",
            ["email"] = "valid@test.com"
        };

        var match = provider.FindMatch(context.Request, requestBody);

        Assert.Null(match);
    }

    [Fact]
    public void FindMatch_MatchRequiredButNoBody_ReturnsNull()
    {
        var provider = new CustomResponseProvider();
        provider.Register(new CustomResponseConfig
        {
            Method = "POST",
            Path = "/users",
            Match = new Dictionary<string, object?> { ["email"] = "error@test.com" }
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/users";

        var match = provider.FindMatch(context.Request); // No body provided

        Assert.Null(match);
    }

    [Fact]
    public void ResponseCount_ReturnsCorrectCount()
    {
        var provider = new CustomResponseProvider();
        provider.Register(new CustomResponseConfig { Method = "GET", Path = "/a" });
        provider.Register(new CustomResponseConfig { Method = "GET", Path = "/b" });

        Assert.Equal(2, provider.ResponseCount);
    }

    [Fact]
    public void FindMatch_PathCaseInsensitive()
    {
        var provider = new CustomResponseProvider();
        provider.Register(new CustomResponseConfig
        {
            Method = "GET",
            Path = "/HEALTH"
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/health";

        var match = provider.FindMatch(context.Request);

        Assert.NotNull(match);
    }

    [Fact]
    public void FindMatch_FirstMatchWins()
    {
        var provider = new CustomResponseProvider();
        provider.Register(new CustomResponseConfig { Method = "GET", Path = "/health", Status = 200 });
        provider.Register(new CustomResponseConfig { Method = "GET", Path = "/health", Status = 500 });

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/health";

        var match = provider.FindMatch(context.Request);

        Assert.Equal(200, match!.Status);
    }

    [Fact]
    public void FromFile_NullPath_ReturnsNull()
    {
        var result = CustomResponseProvider.FromFile(null);

        Assert.Null(result);
    }

    [Fact]
    public void FromFile_EmptyPath_ReturnsNull()
    {
        var result = CustomResponseProvider.FromFile("");

        Assert.Null(result);
    }
}
