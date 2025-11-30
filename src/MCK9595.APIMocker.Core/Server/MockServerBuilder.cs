using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using MCK9595.APIMocker.Core.Generator;
using MCK9595.APIMocker.Core.OpenApi;
using MCK9595.APIMocker.Core.Storage;
using MCK9595.APIMocker.Core.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace MCK9595.APIMocker.Core.Server;

public class MockServerOptions
{
    public int Port { get; set; } = 5000;
    public string Host { get; set; } = "localhost";
    public bool EnableCors { get; set; } = true;
    public int InitialDataCount { get; set; } = 10;

    // Phase 3: Simulation options
    public bool Verbose { get; set; } = false;
    public int? DelayMs { get; set; }
    public int? DelayMinMs { get; set; }
    public int? DelayMaxMs { get; set; }
    public double ErrorRate { get; set; } = 0.0;
    public int[] ErrorCodes { get; set; } = [500];
}

public class MockServerBuilder
{
    private readonly ParsedOpenApiDocument _openApiDoc;
    private readonly MockServerOptions _options;
    private readonly IDataStore _dataStore;
    private readonly IDataGenerator _dataGenerator;
    private readonly IRequestValidator _validator;
    private readonly Random _random = new();

    public MockServerBuilder(
        ParsedOpenApiDocument openApiDoc,
        MockServerOptions options,
        IDataStore? dataStore = null,
        IDataGenerator? dataGenerator = null,
        IRequestValidator? validator = null)
    {
        _openApiDoc = openApiDoc;
        _options = options;
        _dataStore = dataStore ?? new InMemoryDataStore();
        _dataGenerator = dataGenerator ?? new DataGenerator();
        _validator = validator ?? new RequestValidator();
    }

    public WebApplication Build()
    {
        var builder = WebApplication.CreateSlimBuilder();

        builder.WebHost.UseUrls($"http://{_options.Host}:{_options.Port}");

        // Configure JSON serialization
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.WriteIndented = true;
        });

        var app = builder.Build();

        // CORS
        if (_options.EnableCors)
        {
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, PATCH, DELETE, OPTIONS");
                context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");

                if (context.Request.Method == "OPTIONS")
                {
                    context.Response.StatusCode = 204;
                    return;
                }

                await next();
            });
        }

        // Simulation middleware (delay, error, logging)
        app.Use(async (context, next) =>
        {
            var stopwatch = Stopwatch.StartNew();
            var requestPath = context.Request.Path + context.Request.QueryString;
            var method = context.Request.Method;

            // Check for custom _status query parameter
            if (context.Request.Query.TryGetValue("_status", out var statusParam) &&
                int.TryParse(statusParam, out var customStatus))
            {
                if (_options.Verbose)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] → {method} {requestPath}");
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ← {customStatus} (custom status)");
                }
                context.Response.StatusCode = customStatus;
                await context.Response.WriteAsJsonAsync(new { error = $"Simulated {customStatus} error" });
                return;
            }

            // Check for custom _delay query parameter
            if (context.Request.Query.TryGetValue("_delay", out var delayParam) &&
                int.TryParse(delayParam, out var customDelay))
            {
                await Task.Delay(customDelay);
            }

            // Apply configured delay
            if (_options.DelayMs.HasValue)
            {
                await Task.Delay(_options.DelayMs.Value);
            }
            else if (_options.DelayMinMs.HasValue && _options.DelayMaxMs.HasValue)
            {
                var delay = _random.Next(_options.DelayMinMs.Value, _options.DelayMaxMs.Value + 1);
                await Task.Delay(delay);
            }

            // Apply error rate simulation
            if (_options.ErrorRate > 0 && _random.NextDouble() < _options.ErrorRate)
            {
                var errorCode = _options.ErrorCodes[_random.Next(_options.ErrorCodes.Length)];
                stopwatch.Stop();
                if (_options.Verbose)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] → {method} {requestPath}");
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ← {errorCode} (simulated error) ({stopwatch.ElapsedMilliseconds}ms)");
                }
                context.Response.StatusCode = errorCode;
                await context.Response.WriteAsJsonAsync(new { error = $"Simulated {errorCode} error" });
                return;
            }

            // Log request
            if (_options.Verbose)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] → {method} {requestPath}");
            }

            await next();

            stopwatch.Stop();

            // Log response
            if (_options.Verbose)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ← {context.Response.StatusCode} ({stopwatch.ElapsedMilliseconds}ms)");
            }
        });

        // Register endpoints
        foreach (var endpoint in _openApiDoc.Endpoints)
        {
            RegisterEndpoint(app, endpoint);
        }

        return app;
    }

    private void RegisterEndpoint(WebApplication app, ApiEndpoint endpoint)
    {
        var aspNetPath = ConvertToAspNetPath(endpoint.Path);
        var collection = ExtractCollectionName(endpoint.Path);

        switch (endpoint.Method)
        {
            case "GET":
                RegisterGetEndpoint(app, aspNetPath, collection, endpoint);
                break;
            case "POST":
                RegisterPostEndpoint(app, aspNetPath, collection, endpoint);
                break;
            case "PUT":
                RegisterPutEndpoint(app, aspNetPath, collection, endpoint);
                break;
            case "PATCH":
                RegisterPatchEndpoint(app, aspNetPath, collection, endpoint);
                break;
            case "DELETE":
                RegisterDeleteEndpoint(app, aspNetPath, collection, endpoint);
                break;
        }
    }

    private void RegisterGetEndpoint(WebApplication app, string path, string collection, ApiEndpoint endpoint)
    {
        if (path.Contains("{id}"))
        {
            // GET /collection/{id}
            app.MapGet(path, (string id) =>
            {
                var item = _dataStore.GetById(collection, id);
                return item != null ? Results.Ok(item) : Results.NotFound(new { error = "Not found" });
            });
        }
        else
        {
            // GET /collection with query parameters
            app.MapGet(path, (HttpRequest request) =>
            {
                var items = _dataStore.GetAll(collection);

                // Seed initial data if empty
                if (items.Count == 0)
                {
                    SeedCollection(collection, endpoint);
                }

                // Parse query parameters
                var queryOptions = ParseQueryOptions(request, endpoint);
                var result = _dataStore.Query(collection, queryOptions);

                // Return with pagination metadata
                return Results.Ok(new
                {
                    items = result.Items,
                    total = result.TotalCount,
                    skip = queryOptions.Skip ?? 0,
                    take = queryOptions.Take,
                    hasMore = queryOptions.Take.HasValue &&
                              (queryOptions.Skip ?? 0) + result.Items.Count < result.TotalCount
                });
            });
        }
    }

    private static QueryOptions ParseQueryOptions(HttpRequest request, ApiEndpoint endpoint)
    {
        var query = request.Query;
        var filters = new Dictionary<string, string>();

        // Reserved query parameters
        var reservedParams = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "_sort", "_order", "_skip", "_take", "_limit", "_offset"
        };

        // Extract filter parameters
        foreach (var param in query)
        {
            if (!reservedParams.Contains(param.Key) && !string.IsNullOrEmpty(param.Value))
            {
                filters[param.Key] = param.Value.ToString();
            }
        }

        // Parse sorting
        string? sortBy = query["_sort"].FirstOrDefault();
        bool sortDescending = query["_order"].FirstOrDefault()?.Equals("desc", StringComparison.OrdinalIgnoreCase) ?? false;

        // Parse pagination
        int? skip = null;
        int? take = null;

        if (int.TryParse(query["_skip"].FirstOrDefault() ?? query["_offset"].FirstOrDefault(), out var skipValue))
        {
            skip = skipValue;
        }

        if (int.TryParse(query["_take"].FirstOrDefault() ?? query["_limit"].FirstOrDefault(), out var takeValue))
        {
            take = takeValue;
        }

        return new QueryOptions(
            Filters: filters.Count > 0 ? filters : null,
            SortBy: sortBy,
            SortDescending: sortDescending,
            Skip: skip,
            Take: take
        );
    }

    private void RegisterPostEndpoint(WebApplication app, string path, string collection, ApiEndpoint endpoint)
    {
        app.MapPost(path, async (HttpRequest request) =>
        {
            var body = await ReadJsonBody(request);
            if (body == null)
            {
                return Results.BadRequest(new { error = "Invalid JSON body" });
            }

            // Validate request body
            if (endpoint.RequestBodySchema != null)
            {
                var schema = ResolveSchema(endpoint.RequestBodySchema);
                if (schema != null)
                {
                    var validationResult = _validator.Validate(body, schema);
                    if (!validationResult.IsValid)
                    {
                        return Results.BadRequest(new
                        {
                            error = "Validation failed",
                            details = validationResult.Errors.Select(e => new { field = e.Field, message = e.Message })
                        });
                    }
                }
            }

            var created = _dataStore.Create(collection, body);
            return Results.Created($"{path}/{created["id"]}", created);
        });
    }

    private void RegisterPutEndpoint(WebApplication app, string path, string collection, ApiEndpoint endpoint)
    {
        app.MapPut(path, async (string id, HttpRequest request) =>
        {
            var body = await ReadJsonBody(request);
            if (body == null)
            {
                return Results.BadRequest(new { error = "Invalid JSON body" });
            }

            // Validate request body
            if (endpoint.RequestBodySchema != null)
            {
                var schema = ResolveSchema(endpoint.RequestBodySchema);
                if (schema != null)
                {
                    var validationResult = _validator.Validate(body, schema);
                    if (!validationResult.IsValid)
                    {
                        return Results.BadRequest(new
                        {
                            error = "Validation failed",
                            details = validationResult.Errors.Select(e => new { field = e.Field, message = e.Message })
                        });
                    }
                }
            }

            var updated = _dataStore.Update(collection, id, body);
            return updated != null ? Results.Ok(updated) : Results.NotFound(new { error = "Not found" });
        });
    }

    private void RegisterPatchEndpoint(WebApplication app, string path, string collection, ApiEndpoint endpoint)
    {
        app.MapPatch(path, async (string id, HttpRequest request) =>
        {
            var existing = _dataStore.GetById(collection, id);
            if (existing == null)
            {
                return Results.NotFound(new { error = "Not found" });
            }

            var body = await ReadJsonBody(request);
            if (body == null)
            {
                return Results.BadRequest(new { error = "Invalid JSON body" });
            }

            // Validate provided fields (PATCH doesn't require all fields)
            if (endpoint.RequestBodySchema != null)
            {
                var schema = ResolveSchema(endpoint.RequestBodySchema);
                if (schema != null)
                {
                    // Create a partial schema without required fields for PATCH validation
                    var validationResult = _validator.Validate(body, schema);
                    // Only report errors for fields that were actually provided
                    var relevantErrors = validationResult.Errors
                        .Where(e => body.ContainsKey(e.Field) || !e.Message.Contains("required"))
                        .ToList();
                    if (relevantErrors.Count > 0)
                    {
                        return Results.BadRequest(new
                        {
                            error = "Validation failed",
                            details = relevantErrors.Select(e => new { field = e.Field, message = e.Message })
                        });
                    }
                }
            }

            // Merge existing with new values
            foreach (var (key, value) in body)
            {
                existing[key] = value;
            }

            var updated = _dataStore.Update(collection, id, existing);
            return Results.Ok(updated);
        });
    }

    private void RegisterDeleteEndpoint(WebApplication app, string path, string collection, ApiEndpoint endpoint)
    {
        app.MapDelete(path, (string id) =>
        {
            var deleted = _dataStore.Delete(collection, id);
            return deleted ? Results.NoContent() : Results.NotFound(new { error = "Not found" });
        });
    }

    private void SeedCollection(string collection, ApiEndpoint endpoint)
    {
        var schema = GetSchemaForEndpoint(endpoint);
        if (schema == null) return;

        var items = _dataGenerator.GenerateMany(schema, _options.InitialDataCount);
        _dataStore.Seed(collection, items);
    }

    private OpenApiSchema? GetSchemaForEndpoint(ApiEndpoint endpoint)
    {
        // For array response, get the items schema
        if (endpoint.ResponseSchema?.Type == "array" && endpoint.ResponseSchema.Items != null)
        {
            return ResolveSchema(endpoint.ResponseSchema.Items);
        }

        return ResolveSchema(endpoint.ResponseSchema);
    }

    private OpenApiSchema? ResolveSchema(OpenApiSchema? schema)
    {
        if (schema == null) return null;

        // If it's a reference, try to find it in schemas
        if (schema.Reference != null)
        {
            var refId = schema.Reference.Id;
            if (_openApiDoc.Schemas.TryGetValue(refId, out var resolved))
            {
                return resolved;
            }
        }

        return schema;
    }

    private static async Task<Dictionary<string, object?>?> ReadJsonBody(HttpRequest request)
    {
        try
        {
            using var reader = new StreamReader(request.Body);
            var json = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    private static string ConvertToAspNetPath(string openApiPath)
    {
        // Convert OpenAPI path params {param} to ASP.NET {param}
        // OpenAPI uses {id}, ASP.NET also uses {id}, so mostly compatible
        return openApiPath;
    }

    private static string ExtractCollectionName(string path)
    {
        // /users/{id} -> users
        // /api/v1/users -> users
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in segments)
        {
            if (!segment.StartsWith('{') && segment != "api" && !Regex.IsMatch(segment, @"^v\d+$"))
            {
                return segment;
            }
        }

        return segments.FirstOrDefault() ?? "items";
    }
}
