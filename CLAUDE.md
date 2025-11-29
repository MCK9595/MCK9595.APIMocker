# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MCK9595.APIMocker is a .NET 10 CLI tool that generates mock API servers from OpenAPI definitions. Install-free execution via `dnx mck-api-mocker` is the primary distribution method.

## Build & Test Commands

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run CLI locally
dotnet run --project src/MCK9595.APIMocker.Cli -- serve openapi.yaml

# Run CLI with options
dotnet run --project src/MCK9595.APIMocker.Cli -- serve openapi.yaml --port 3000 --cors

# Pack as global tool
dotnet pack -c Release
```

## Architecture

```
MCK9595.APIMocker.Cli (CLI entry point)
    └── MCK9595.APIMocker.Core (Core logic)
            ├── OpenAPI parsing (Microsoft.OpenApi.Readers)
            ├── Dummy data generation (Bogus)
            └── Mock server (ASP.NET Minimal API)
```

### Key Libraries

| Library | Purpose |
|---------|---------|
| ConsoleAppFramework | CLI framework (Source Generator based, zero allocation) |
| Microsoft.OpenApi.Readers | OpenAPI 3.0/3.1 parsing |
| Bogus | Fake data generation with Japanese locale support |
| Spectre.Console | Rich CLI output |
| ZLogger | High-performance logging |
| YamlDotNet | YAML parsing |

## Code Conventions

- CLI commands are implemented as static methods with `[Command]` attribute (ConsoleAppFramework pattern)
- Use `[Argument]` for positional arguments, default parameter values for options
- Japanese dummy data is supported via Bogus `ja` locale
- All packages use MIT license

## CLI Command Structure

```csharp
[Command("serve")]
public static async Task Execute(
    [Argument] string file,      // Positional argument
    int port = 5000,             // -p, --port
    string host = "localhost",   // -h, --host
    bool cors = true,            // --cors
    CancellationToken ct = default)
```
