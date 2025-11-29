# API Mocker - 技術設計書 (ConsoleAppFramework版)

## 📐 システムアーキテクチャ

### 全体構成

```
┌─────────────────────────────────────────────────────────┐
│                     User (CLI)                          │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│              api-mocker CLI Tool                        │
│              (ConsoleAppFramework)                      │
│                                                          │
│  ┌────────────────────────────────────────────────┐    │
│  │         Command Handler                        │    │
│  │  • ServeCommand (メインコマンド)                │    │
│  │  • ValidateCommand (検証のみ)                   │    │
│  └────────────────────────────────────────────────┘    │
│                          │                               │
│                          ▼                               │
│  ┌────────────────────────────────────────────────┐    │
│  │      OpenAPI Parser                            │    │
│  │  • YAML/JSON読み込み                            │    │
│  │  • スキーマ検証                                  │    │
│  │  • エンドポイント抽出                            │    │
│  └────────────────────────────────────────────────┘    │
│                          │                               │
│                          ▼                               │
│  ┌────────────────────────────────────────────────┐    │
│  │      Mock Server Generator                     │    │
│  │  • ASP.NET Minimal API生成                     │    │
│  │  • ルート動的生成                               │    │
│  │  • ミドルウェア設定                             │    │
│  └────────────────────────────────────────────────┘    │
│                          │                               │
│                          ▼                               │
│  ┌────────────────────────────────────────────────┐    │
│  │      Data Generator                            │    │
│  │  • スキーマベースのダミーデータ生成              │    │
│  │  • Bogus統合                                    │    │
│  │  • 日本語データ対応                             │    │
│  └────────────────────────────────────────────────┘    │
│                          │                               │
│                          ▼                               │
│  ┌────────────────────────────────────────────────┐    │
│  │      In-Memory Database                        │    │
│  │  • Dictionary<string, List<object>>            │    │
│  │  • CRUD操作                                     │    │
│  │  • クエリ対応                                    │    │
│  └────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│              ASP.NET Core Server                        │
│              (Minimal API)                              │
└─────────────────────────────────────────────────────────┘
```

---

## 🏗️ プロジェクト構造

```
api-mocker/
├── src/
│   ├── ApiMocker.Cli/                      # CLIエントリポイント
│   │   ├── Commands/
│   │   │   ├── ServeCommand.cs            # serve コマンド
│   │   │   └── ValidateCommand.cs         # validate コマンド
│   │   ├── Program.cs                      # ConsoleApp.Create()
│   │   └── ApiMocker.Cli.csproj
│   │
│   ├── ApiMocker.Core/                     # コアロジック
│   │   ├── OpenApi/
│   │   │   ├── IOpenApiParser.cs
│   │   │   ├── OpenApiParser.cs
│   │   │   └── OpenApiDocument.cs
│   │   ├── Generator/
│   │   │   ├── IDataGenerator.cs
│   │   │   ├── DataGenerator.cs
│   │   │   └── SchemaMapper.cs
│   │   ├── Storage/
│   │   │   ├── IDataStore.cs
│   │   │   ├── InMemoryDataStore.cs
│   │   │   └── FileDataStore.cs
│   │   ├── Server/
│   │   │   ├── MockServerBuilder.cs
│   │   │   └── EndpointGenerator.cs
│   │   ├── Models/
│   │   │   ├── ApiEndpoint.cs
│   │   │   ├── ApiSchema.cs
│   │   │   └── MockOptions.cs
│   │   └── ApiMocker.Core.csproj
│   │
│   └── ApiMocker.Server/                   # 実行時サーバー
│       ├── Middleware/
│       │   ├── CorsMiddleware.cs
│       │   ├── DelayMiddleware.cs
│       │   └── LoggingMiddleware.cs
│       └── ApiMocker.Server.csproj
│
├── tests/
│   ├── ApiMocker.Core.Tests/
│   └── ApiMocker.Integration.Tests/
│
├── samples/
│   ├── petstore.yaml
│   ├── user-api.yaml
│   └── blog-api.yaml
│
├── docs/
│   └── getting-started.md
│
├── LICENSE                                  # MIT License
├── THIRD-PARTY-LICENSES.md                 # 外部ライブラリライセンス
└── README.md
```

---

## 📦 技術スタック

### 必須ライブラリ

```xml
<!-- ApiMocker.Cli.csproj -->
<PackageReference Include="ConsoleAppFramework" Version="5.7.13" />
<PackageReference Include="Spectre.Console" Version="0.49.1" />
<PackageReference Include="ZLogger" Version="2.5.10" />

<!-- ApiMocker.Core.csproj -->
<PackageReference Include="Microsoft.OpenApi.Readers" Version="1.6.22" />
<PackageReference Include="Bogus" Version="35.6.1" />
<PackageReference Include="YamlDotNet" Version="16.2.1" />
<PackageReference Include="ZLogger" Version="2.5.10" />
```

### ライブラリ選定理由

| ライブラリ | 用途 | ライセンス | 選定理由 |
|-----------|------|-----------|---------|
| ConsoleAppFramework | CLI構築 | MIT | Zero Overhead、AOT対応、Source Generator |
| Microsoft.OpenApi.Readers | OpenAPI解析 | MIT | Microsoft公式、最も信頼性が高い |
| Bogus | ダミーデータ | MIT | 多言語対応、柔軟性が高い |
| Spectre.Console | リッチ出力 | MIT | 美しいCLI UI |
| YamlDotNet | YAML解析 | MIT | デファクトスタンダード |
| ZLogger | ロギング | MIT | 超高速、Zero Allocation、Cysharp製 |

**全てMITライセンス** ✅

---

## 📝 ロギング設計

### ZLogger による高性能ロギング

api-mockerは**ZLogger**を使用して、高性能かつ柔軟なロギングを実現します。

#### ZLoggerの特徴
- ⚡️ **Zero Allocation**: メモリアロケーションなし
- 🚀 **超高速**: 標準ロガーの8倍速
- 📦 **UTF8直接出力**: 文字列変換のオーバーヘッドなし
- 🔧 **非同期バッファリング**: デフォルトで非同期処理
- 📊 **構造化ログ対応**: JSON形式でも出力可能

### ログレベル

```csharp
public enum LogLevel
{
    Trace = 0,       // 極めて詳細 (ダミーデータ内容等)
    Debug = 1,       // デバッグ情報 (エンドポイント登録等)
    Information = 2, // 一般情報 (起動、リクエスト処理) [デフォルト]
    Warning = 3,     // 警告 (バリデーションエラー等)
    Error = 4,       // エラー (パース失敗等)
    Critical = 5,    // 致命的エラー (起動失敗等)
    None = 6         // ログ出力なし
}
```

### ログ出力先

#### 1. コンソール (デフォルト)
```bash
[INFO][2025-11-30 14:30:15] Starting API Mocker v1.0.0
[INFO][2025-11-30 14:30:15] Server running at http://localhost:5000
[INFO][2025-11-30 14:30:20] GET /users → 200 OK (12ms)
```

#### 2. ファイル (--log-dir 指定時)
```bash
# ログディレクトリ指定
api-mocker serve openapi.yaml --log-dir ./logs

# 自動生成されるファイル名
logs/api-mocker-2025-11-30_14-30-15.log
```

### CLIパラメータ

```csharp
[Command("serve")]
public static async Task Execute(
    [Argument] string file,
    int port = 5000,
    string host = "localhost",
    bool cors = true,
    bool verbose = false,
    string? delay = null,
    string? persist = null,
    
    // ログ設定
    LogLevel logLevel = LogLevel.Information,  // コンソールログレベル
    string? logDir = null,                     // ファイル出力ディレクトリ
    
    CancellationToken cancellationToken = default)
{
    // ロガー設定
    var loggerFactory = SetupLogger(logLevel, logDir);
    var logger = loggerFactory.CreateLogger<ServeCommand>();
    
    logger.ZLogInformation($"Starting API Mocker");
    // ...
}
```

### ロガー設定実装

```csharp
private static ILoggerFactory SetupLogger(LogLevel logLevel, string? logDir)
{
    var loggerFactory = LoggerFactory.Create(logging =>
    {
        // 1. コンソールログ (常に有効)
        logging.AddZLoggerConsole(options =>
        {
            options.PrefixFormatter = (writer, info) =>
            {
                ZString.Utf8Format(writer, "[{0}][{1:yyyy-MM-dd HH:mm:ss}] ",
                    info.LogLevel.ToString().ToUpper(),
                    info.Timestamp.ToLocalTime().DateTime);
            };
        });
        logging.AddFilter<ZLoggerConsoleLoggerProvider>(null, logLevel);
        
        // 2. ファイルログ (--log-dir 指定時)
        if (!string.IsNullOrEmpty(logDir))
        {
            Directory.CreateDirectory(logDir);
            var fileName = $"api-mocker-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
            var filePath = Path.Combine(logDir, fileName);
            
            logging.AddZLoggerFile(filePath);
            // ファイルは全レベル記録
            logging.AddFilter<ZLoggerFileLoggerProvider>(null, LogLevel.Trace);
        }
    });
    
    return loggerFactory;
}
```

### 使用例

```csharp
// 情報ログ
logger.ZLogInformation($"Loaded {endpoints.Count} endpoints");

// デバッグログ
logger.ZLogDebug($"Registering endpoint: {method} {path}");

// 警告ログ
logger.ZLogWarning($"Validation failed: {error}");

// エラーログ
logger.ZLogError($"Failed to parse OpenAPI: {ex.Message}");

// 詳細ログ (Trace)
logger.ZLogTrace($"Generated dummy data: {json}");
```

詳細は **[logging-design.md](computer:///mnt/user-data/outputs/logging-design.md)** を参照。

---

## 🔧 ConsoleAppFramework実装

### Program.cs - メインエントリポイント

```csharp
using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;

// ConsoleAppFrameworkのアプリケーション作成
var app = ConsoleApp.Create();

// DI設定 (必要に応じて)
app.ConfigureServices(services =>
{
    services.AddSingleton<IOpenApiParser, OpenApiParser>();
    services.AddSingleton<IDataGenerator, DataGenerator>();
    services.AddSingleton<IDataStore, InMemoryDataStore>();
});

// コマンド登録
app.Add("serve", ServeCommand.Execute);
app.Add("validate", ValidateCommand.Execute);

// 実行
await app.RunAsync(args);
```

### ServeCommand.cs - serveコマンド実装

```csharp
using ConsoleAppFramework;
using Spectre.Console;

public static class ServeCommand
{
    /// <summary>
    /// OpenAPI定義からモックサーバーを起動
    /// </summary>
    /// <param name="file">OpenAPI definition file (YAML or JSON)</param>
    /// <param name="port">-p, Port number</param>
    /// <param name="host">-h, Host address</param>
    /// <param name="cors">--cors, Enable CORS</param>
    /// <param name="verbose">--verbose, Show verbose logs</param>
    /// <param name="delay">--delay, Response delay in milliseconds (e.g., "500" or "500-1000")</param>
    /// <param name="persist">--persist, Persist data to file</param>
    [Command("serve")]
    public static async Task Execute(
        [Argument] string file,
        int port = 5000,
        string host = "localhost",
        bool cors = true,
        bool verbose = false,
        string? delay = null,
        string? persist = null,
        CancellationToken cancellationToken = default)
    {
        // バナー表示
        AnsiConsole.Write(
            new FigletText("API Mocker")
                .LeftJustified()
                .Color(Color.Blue));
        
        AnsiConsole.MarkupLine($"[grey]v1.0.0[/]\n");

        try
        {
            // OpenAPIファイル読み込み
            AnsiConsole.Status()
                .Start("Loading OpenAPI specification...", ctx =>
                {
                    var parser = new OpenApiParser();
                    var openApiDoc = parser.Parse(file);
                    
                    // 情報表示
                    AnsiConsole.MarkupLine($"[green]✓[/] OpenAPI: {file}");
                    AnsiConsole.MarkupLine($"  Title: {openApiDoc.Title}");
                    AnsiConsole.MarkupLine($"  Version: {openApiDoc.Version}\n");
                    
                    // エンドポイント一覧表示
                    var table = new Table();
                    table.AddColumn("Method");
                    table.AddColumn("Path");
                    table.AddColumn("Description");
                    
                    foreach (var endpoint in openApiDoc.Endpoints)
                    {
                        table.AddRow(
                            $"[yellow]{endpoint.Method}[/]",
                            endpoint.Path,
                            endpoint.Description ?? "");
                    }
                    
                    AnsiConsole.Write(table);
                    AnsiConsole.WriteLine();
                    
                    // サーバー構築
                    ctx.Status("Starting mock server...");
                    
                    var options = new MockOptions
                    {
                        Port = port,
                        Host = host,
                        EnableCors = cors,
                        Verbose = verbose,
                        DelayMs = ParseDelay(delay),
                        PersistFile = persist
                    };
                    
                    var builder = new MockServerBuilder(openApiDoc, options);
                    var webApp = builder.Build();
                    
                    // 起動URL表示
                    AnsiConsole.MarkupLine($"[green]Server running at:[/]");
                    AnsiConsole.MarkupLine($"  • [link]http://{host}:{port}[/]\n");
                    AnsiConsole.MarkupLine("[grey]Press Ctrl+C to stop[/]");
                    
                    // サーバー起動 (Ctrl+Cまでブロック)
                    await webApp.RunAsync(cancellationToken);
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            if (verbose)
            {
                AnsiConsole.WriteException(ex);
            }
            Environment.Exit(1);
        }
    }
    
    private static int ParseDelay(string? delay)
    {
        if (string.IsNullOrEmpty(delay)) return 0;
        
        // "500" or "500-1000"
        if (delay.Contains('-'))
        {
            var parts = delay.Split('-');
            var min = int.Parse(parts[0]);
            var max = int.Parse(parts[1]);
            return Random.Shared.Next(min, max);
        }
        
        return int.Parse(delay);
    }
}
```

### ValidateCommand.cs - validateコマンド実装

```csharp
using ConsoleAppFramework;
using Spectre.Console;

public static class ValidateCommand
{
    /// <summary>
    /// OpenAPI定義ファイルを検証
    /// </summary>
    /// <param name="file">OpenAPI definition file</param>
    /// <param name="strict">--strict, Enable strict validation</param>
    [Command("validate")]
    public static Task Execute(
        [Argument] string file,
        bool strict = false)
    {
        AnsiConsole.MarkupLine($"[blue]Validating:[/] {file}\n");
        
        try
        {
            var parser = new OpenApiParser();
            var doc = parser.Parse(file);
            
            AnsiConsole.MarkupLine("[green]✓ Valid OpenAPI specification[/]");
            AnsiConsole.MarkupLine($"  Endpoints: {doc.Endpoints.Count}");
            AnsiConsole.MarkupLine($"  Schemas: {doc.Schemas.Count}");
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Validation failed[/]");
            AnsiConsole.MarkupLine($"  {ex.Message}");
            Environment.Exit(1);
            return Task.CompletedTask;
        }
    }
}
```

---

## 🎨 ConsoleAppFrameworkの特徴

### 1. Source Generator ベース
- **Zero Reflection**: リフレクション不要
- **Zero Allocation**: 余計なメモリ確保なし
- **AOT Safe**: Native AOTコンパイル対応

### 2. 超高速パフォーマンス
```
System.CommandLine: 280ms
ConsoleAppFramework: 1ms (280倍速い!)
```

### 3. シンプルなAPI
```csharp
// ラムダ式スタイル
ConsoleApp.Run(args, (string name, int age) => 
    Console.WriteLine($"Hello, {name} ({age})"));

// メソッドスタイル
app.Add("serve", ServeCommand.Execute);
```

### 4. 自動ヘルプ生成
```bash
$ api-mocker --help

Usage: api-mocker <Command>

Commands:
  serve      Start mock API server
  validate   Validate OpenAPI specification

$ api-mocker serve --help

Usage: api-mocker serve <file> [options]

Arguments:
  <file>    OpenAPI definition file (YAML or JSON)

Options:
  -p, --port <Int32>        Port number (Default: 5000)
  -h, --host <String>       Host address (Default: localhost)
  --cors                    Enable CORS (Default: True)
  --verbose                 Show verbose logs
  --delay <String>          Response delay in milliseconds
  --persist <String>        Persist data to file
```

---

## 🔄 CLI実装パターン比較

### ❌ System.CommandLine (旧設計)
```csharp
var rootCommand = new RootCommand("API Mocker");

var serveCommand = new Command("serve", "Start mock server");
var fileArgument = new Argument<string>("file", "OpenAPI file");
var portOption = new Option<int>("--port", () => 5000, "Port number");

serveCommand.AddArgument(fileArgument);
serveCommand.AddOption(portOption);

serveCommand.SetHandler(async (string file, int port) =>
{
    // 実装
}, fileArgument, portOption);

rootCommand.AddCommand(serveCommand);
await rootCommand.InvokeAsync(args);
```

**問題点**:
- 冗長なコード
- リフレクションベース
- パフォーマンス低い

### ✅ ConsoleAppFramework (新設計)
```csharp
var app = ConsoleApp.Create();
app.Add("serve", ServeCommand.Execute);
await app.RunAsync(args);

// コマンド定義
public static async Task Execute(
    [Argument] string file,
    int port = 5000)
{
    // 実装
}
```

**メリット**:
- シンプル
- Source Generator
- 超高速

---

## 📝 プロジェクト設定

### ApiMocker.Cli.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    
    <!-- PackAsToolの設定 -->
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>api-mocker</ToolCommandName>
    <PackageId>ApiMocker.Cli</PackageId>
    <Version>1.0.0</Version>
    <Authors>MCK9595</Authors>
    <PackageProjectUrl>https://github.com/yourusername/api-mocker</PackageProjectUrl>
    <RepositoryUrl>https://github.com/yourusername/api-mocker</RepositoryUrl>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <Description>OpenAPI specification-based mock API server</Description>
    <PackageTags>openapi;mock;api;cli;dnx</PackageTags>
    
    <!-- RuntimeIdentifier設定 (dnx用) -->
    <RuntimeIdentifiers>win-x64;linux-x64;osx-x64;osx-arm64;any</RuntimeIdentifiers>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ConsoleAppFramework" Version="5.7.13" />
    <PackageReference Include="Spectre.Console" Version="0.49.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\ApiMocker.Core\ApiMocker.Core.csproj" />
  </ItemGroup>
</Project>
```

---

## 🧪 テスト戦略

### ConsoleAppFrameworkのテスト

```csharp
[Fact]
public async Task ServeCommand_ValidFile_StartsServer()
{
    // Arrange
    var file = "sample-user-api.yaml";
    var port = 5555;
    
    // Act
    var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(5));
    
    var task = ServeCommand.Execute(
        file: file,
        port: port,
        host: "localhost",
        cors: true,
        verbose: false,
        delay: null,
        persist: null,
        cancellationToken: cts.Token);
    
    // サーバー起動を待つ
    await Task.Delay(1000);
    
    // Assert
    using var client = new HttpClient();
    var response = await client.GetAsync($"http://localhost:{port}/users");
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    
    // クリーンアップ
    cts.Cancel();
    await task;
}
```

---

## 🚀 ビルド & 実行

### ローカル実行
```bash
# デバッグ実行
dotnet run --project src/ApiMocker.Cli -- serve sample-user-api.yaml

# リリースビルド
dotnet build -c Release
```

### ツールとしてインストール
```bash
# ローカルインストール
dotnet pack -c Release
dotnet tool install --global --add-source ./nupkg ApiMocker.Cli

# 実行
api-mocker serve sample.yaml --port 3000
```

### dnx実行
```bash
# インストール不要で実行
dnx api-mocker serve sample.yaml
```

---

## 📈 パフォーマンス最適化

### ConsoleAppFrameworkによる最適化
- ✅ Source Generatorで静的コード生成
- ✅ リフレクション不使用
- ✅ ボクシング回避
- ✅ メモリアロケーション最小化

### 起動時間目標
- **ConsoleAppFramework**: < 100ms
- **System.CommandLine**: > 200ms

---

**作成者**: Claude  
**対象**: Macky (Claude Code開発用)  
**作成日**: 2025-11-30  
**更新日**: 2025-11-30 (ConsoleAppFramework対応)  
**ステータス**: ✅ レビュー待ち
