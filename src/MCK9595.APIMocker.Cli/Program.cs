using System.Reflection;
using ConsoleAppFramework;
using MCK9595.APIMocker.Core.OpenApi;
using MCK9595.APIMocker.Core.Server;
using Spectre.Console;

var app = ConsoleApp.Create();
app.Add<Commands>();
app.Run(args);

public class Commands
{
    /// <summary>
    /// Start mock API server from OpenAPI definition
    /// </summary>
    /// <param name="file">Path to OpenAPI definition file (yaml/json)</param>
    /// <param name="port">-p, Port number to listen on</param>
    /// <param name="host">Host address to bind to</param>
    /// <param name="cors">Enable CORS</param>
    /// <param name="verbose">-v, Enable verbose request/response logging</param>
    /// <param name="delay">Add fixed delay in milliseconds to all responses</param>
    /// <param name="delayMin">Minimum delay in milliseconds (used with delay-max for random delay)</param>
    /// <param name="delayMax">Maximum delay in milliseconds (used with delay-min for random delay)</param>
    /// <param name="errorRate">Probability of returning an error (0.0-1.0)</param>
    /// <param name="errorCodes">Comma-separated list of error codes to return randomly</param>
    /// <param name="persist">Persist data to JSON files (survives restarts)</param>
    /// <param name="dataDir">Directory for persisted data files</param>
    /// <param name="seed">Path to JSON file for initial seed data</param>
    /// <param name="auth">Authentication mode: bearer, apikey, basic, none</param>
    /// <param name="authKey">Expected authentication key/credentials</param>
    /// <param name="responses">Path to custom responses JSON file</param>
    /// <param name="webhooks">Path to webhooks configuration JSON file</param>
    [Command("serve")]
    public async Task Serve(
        [Argument] string file,
        int port = 5000,
        string host = "localhost",
        bool cors = true,
        bool verbose = false,
        int? delay = null,
        int? delayMin = null,
        int? delayMax = null,
        double errorRate = 0.0,
        string? errorCodes = null,
        bool persist = false,
        string dataDir = "./.mck-data",
        string? seed = null,
        string? auth = null,
        string? authKey = null,
        string? responses = null,
        string? webhooks = null)
    {
        // Banner
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            ?? "unknown";
        // Remove +commit hash if present
        var plusIndex = version.IndexOf('+');
        if (plusIndex > 0) version = version[..plusIndex];

        AnsiConsole.Write(
            new FigletText("API Mocker")
                .LeftJustified()
                .Color(Color.Blue));
        AnsiConsole.MarkupLine($"[grey]MCK9595.APIMocker v{version}[/]\n");

        try
        {
            // Parse OpenAPI file
            AnsiConsole.MarkupLine($"[blue]Loading:[/] {file}");

            var parser = new OpenApiParser();
            var openApiDoc = parser.Parse(file);

            AnsiConsole.MarkupLine($"[green]✓[/] Title: {openApiDoc.Title}");
            AnsiConsole.MarkupLine($"[green]✓[/] Version: {openApiDoc.Version}");
            AnsiConsole.WriteLine();

            // Endpoints table
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn(new TableColumn("[yellow]Method[/]").Centered());
            table.AddColumn(new TableColumn("[yellow]Path[/]"));
            table.AddColumn(new TableColumn("[yellow]Description[/]"));

            foreach (var endpoint in openApiDoc.Endpoints)
            {
                var methodColor = endpoint.Method switch
                {
                    "GET" => "green",
                    "POST" => "blue",
                    "PUT" => "orange1",
                    "PATCH" => "yellow",
                    "DELETE" => "red",
                    _ => "white"
                };

                table.AddRow(
                    $"[{methodColor}]{endpoint.Method}[/]",
                    endpoint.Path,
                    endpoint.Summary ?? endpoint.Description ?? "-"
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();

            // Build and start server
            var parsedErrorCodes = errorCodes?.Split(',')
                .Select(s => int.TryParse(s.Trim(), out var code) ? code : 500)
                .ToArray() ?? [500];

            var options = new MockServerOptions
            {
                Port = port,
                Host = host,
                EnableCors = cors,
                Verbose = verbose,
                DelayMs = delay,
                DelayMinMs = delayMin,
                DelayMaxMs = delayMax,
                ErrorRate = errorRate,
                ErrorCodes = parsedErrorCodes,
                // Phase 5 options
                PersistData = persist,
                DataDirectory = dataDir,
                SeedFile = seed,
                AuthMode = auth,
                AuthKey = authKey,
                ResponsesFile = responses,
                WebhooksFile = webhooks
            };

            // Show enabled options
            var hasOptions = false;

            if (verbose)
            {
                AnsiConsole.MarkupLine("[yellow]Verbose logging enabled[/]");
                hasOptions = true;
            }
            if (delay.HasValue)
            {
                AnsiConsole.MarkupLine($"[yellow]Fixed delay:[/] {delay}ms");
                hasOptions = true;
            }
            if (delayMin.HasValue && delayMax.HasValue)
            {
                AnsiConsole.MarkupLine($"[yellow]Random delay:[/] {delayMin}-{delayMax}ms");
                hasOptions = true;
            }
            if (errorRate > 0)
            {
                AnsiConsole.MarkupLine($"[yellow]Error rate:[/] {errorRate:P0} (codes: {string.Join(", ", parsedErrorCodes)})");
                hasOptions = true;
            }
            if (persist)
            {
                AnsiConsole.MarkupLine($"[yellow]Data persistence:[/] {dataDir}");
                hasOptions = true;
            }
            if (!string.IsNullOrEmpty(seed))
            {
                AnsiConsole.MarkupLine($"[yellow]Seed data:[/] {seed}");
                hasOptions = true;
            }
            if (!string.IsNullOrEmpty(auth))
            {
                var authInfo = authKey != null ? $"{auth} (key set)" : auth;
                AnsiConsole.MarkupLine($"[yellow]Authentication:[/] {authInfo}");
                hasOptions = true;
            }
            if (!string.IsNullOrEmpty(responses))
            {
                AnsiConsole.MarkupLine($"[yellow]Custom responses:[/] {responses}");
                hasOptions = true;
            }
            if (!string.IsNullOrEmpty(webhooks))
            {
                AnsiConsole.MarkupLine($"[yellow]Webhooks:[/] {webhooks}");
                hasOptions = true;
            }
            if (hasOptions)
            {
                AnsiConsole.WriteLine();
            }

            var builder = new MockServerBuilder(openApiDoc, options);
            var webApp = builder.Build();

            AnsiConsole.MarkupLine($"[green]Server running at:[/]");
            AnsiConsole.MarkupLine($"  [link]http://{host}:{port}[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press Ctrl+C to stop[/]");

            await webApp.RunAsync();
        }
        catch (FileNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            Environment.Exit(1);
        }
        catch (InvalidOperationException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            AnsiConsole.WriteException(ex);
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Validate OpenAPI definition file
    /// </summary>
    /// <param name="file">Path to OpenAPI definition file</param>
    [Command("validate")]
    public void Validate([Argument] string file)
    {
        AnsiConsole.MarkupLine($"[blue]Validating:[/] {file}\n");

        try
        {
            var parser = new OpenApiParser();
            var doc = parser.Parse(file);

            AnsiConsole.MarkupLine("[green]✓ Valid OpenAPI specification[/]");
            AnsiConsole.MarkupLine($"  Title: {doc.Title}");
            AnsiConsole.MarkupLine($"  Version: {doc.Version}");
            AnsiConsole.MarkupLine($"  Endpoints: {doc.Endpoints.Count}");
            AnsiConsole.MarkupLine($"  Schemas: {doc.Schemas.Count}");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]✗ Validation failed[/]");
            AnsiConsole.MarkupLine($"  {ex.Message}");
            Environment.Exit(1);
        }
    }
}
