# API Mocker - Claude Code 開発ガイド

## 🎯 このガイドについて

このガイドは、**Claude Code**で api-mocker を段階的に開発するための手順書です。

### 開発の進め方
1. **Phase 1 (MVP)** から順番に実装
2. 各ステップごとに動作確認
3. テストを書きながら進める
4. 問題があれば前のステップに戻る

---

## 🚀 Phase 1: MVP実装 (Week 1-2)

### ゴール
`dnx api-mocker serve openapi.yaml` でGETエンドポイントが動作する

---

### Step 1: プロジェクト初期化 (30分)

**Claude Codeに依頼する内容**:
```
.NET 10のCLIツールプロジェクトを作成してください。
以下の構成で:
- src/ApiMocker.Cli (CLIツール本体)
- src/ApiMocker.Core (コアロジック)
- tests/ApiMocker.Core.Tests (テスト)

必要なNuGetパッケージ:
- Microsoft.OpenApi.Readers
- System.CommandLine (beta4)
- Bogus
- Spectre.Console

プロジェクトをPackAsToolとして設定してください。
```

**期待される出力**:
```
api-mocker/
├── src/
│   ├── ApiMocker.Cli/
│   │   ├── Program.cs
│   │   └── ApiMocker.Cli.csproj
│   └── ApiMocker.Core/
│       └── ApiMocker.Core.csproj
└── tests/
    └── ApiMocker.Core.Tests/
```

**確認方法**:
```bash
cd src/ApiMocker.Cli
dotnet build
# エラーなくビルド成功
```

---

### Step 2: 基本的なCLIコマンド実装 (1時間)

**Claude Codeに依頼する内容**:
```
System.CommandLineを使って、以下のCLIコマンドを実装してください:

コマンド:
  api-mocker serve <openapi-file> [options]

オプション:
  -p, --port <port>     ポート番号 (default: 5000)
  -h, --host <host>     ホスト (default: localhost)
  --cors                CORS有効化
  --verbose             詳細ログ

実装場所:
- src/ApiMocker.Cli/Commands/ServeCommand.cs
- src/ApiMocker.Cli/Program.cs

現時点では、オプションを表示するだけでOKです。
```

**期待される出力**:
```csharp
// Program.cs
var rootCommand = new RootCommand("API Mocker - Mock server from OpenAPI");

var serveCommand = new Command("serve", "Start mock API server");
var fileArgument = new Argument<string>("openapi-file", "OpenAPI specification file");
var portOption = new Option<int>("--port", () => 5000, "Port number");

serveCommand.AddArgument(fileArgument);
serveCommand.AddOption(portOption);
// ...

serveCommand.SetHandler(async (string file, int port) =>
{
    Console.WriteLine($"Starting mock server from {file} on port {port}");
}, fileArgument, portOption);

rootCommand.AddCommand(serveCommand);
await rootCommand.InvokeAsync(args);
```

**確認方法**:
```bash
dotnet run -- serve test.yaml --port 3000
# → "Starting mock server from test.yaml on port 3000"
```

---

### Step 3: OpenAPI Parser実装 (2時間)

**Claude Codeに依頼する内容**:
```
Microsoft.OpenApi.Readersを使って、OpenAPIファイルをパースする機能を実装してください。

実装するクラス:
1. src/ApiMocker.Core/OpenApi/IOpenApiParser.cs (インターフェース)
2. src/ApiMocker.Core/OpenApi/OpenApiParser.cs (実装)
3. src/ApiMocker.Core/Models/OpenApiDocument.cs (内部モデル)

機能:
- YAMLとJSONの両方をサポート
- パースエラーは例外をスロー
- エンドポイント一覧を抽出
- スキーマ定義を抽出

テストも書いてください。
サンプルOpenAPIファイル (samples/petstore.yaml) も作成してください。
```

**期待される実装**:
```csharp
public interface IOpenApiParser
{
    OpenApiDocument Parse(string filePath);
}

public class OpenApiParser : IOpenApiParser
{
    public OpenApiDocument Parse(string filePath)
    {
        var reader = new OpenApiStreamReader();
        using var stream = File.OpenRead(filePath);
        var openApiDoc = reader.Read(stream, out var diagnostic);
        
        if (diagnostic.Errors.Any())
        {
            var errors = string.Join("\n", diagnostic.Errors.Select(e => e.Message));
            throw new InvalidOperationException($"OpenAPI parsing failed:\n{errors}");
        }
        
        return MapToInternalModel(openApiDoc);
    }
}
```

**確認方法**:
```bash
dotnet test
# → すべてのテストが成功
```

---

### Step 4: ダミーデータ生成実装 (3時間)

**Claude Codeに依頼する内容**:
```
Bogusを使って、OpenAPIスキーマからリアルなダミーデータを生成する機能を実装してください。

実装するクラス:
1. src/ApiMocker.Core/Generator/IDataGenerator.cs
2. src/ApiMocker.Core/Generator/DataGenerator.cs

要件:
- フィールド名から推測してリアルなデータ生成
  - name → 日本人名
  - email → メールアドレス
  - age → 18-65
  - createdAt → 日付時刻
- 型に応じた生成
  - string, number, boolean, array, object
- enum対応
- 日本語ロケール使用

以下のようなスキーマをテストケースに含めてください:
{
  "type": "object",
  "properties": {
    "id": { "type": "integer" },
    "name": { "type": "string" },
    "email": { "type": "string", "format": "email" },
    "age": { "type": "integer", "minimum": 18, "maximum": 65 }
  }
}
```

**期待される出力例**:
```json
{
  "id": 1,
  "name": "田中太郎",
  "email": "tanaka.taro@example.com",
  "age": 28
}
```

**確認方法**:
```bash
dotnet test --filter DataGenerator
# → DataGeneratorのテストが成功
```

---

### Step 5: InMemoryDataStore実装 (2時間)

**Claude Codeに依頼する内容**:
```
メモリ内でデータを保持するデータストアを実装してください。

実装するクラス:
1. src/ApiMocker.Core/Storage/IDataStore.cs
2. src/ApiMocker.Core/Storage/InMemoryDataStore.cs

機能:
- GetAll<T>(string collection)
- GetById<T>(string collection, object id)
- Create<T>(string collection, T item) → ID自動採番
- Update<T>(string collection, object id, T item)
- Delete(string collection, object id)

データ構造:
Dictionary<string, List<object>> で実装

ID自動採番:
- "id"プロパティに連番を設定
- リフレクションを使用
```

**期待される実装**:
```csharp
public class InMemoryDataStore : IDataStore
{
    private readonly Dictionary<string, List<object>> _data = new();
    private readonly Dictionary<string, int> _nextIds = new();
    private readonly object _lock = new();
    
    public T Create<T>(string collection, T item)
    {
        lock (_lock)
        {
            if (!_data.ContainsKey(collection))
                _data[collection] = new List<object>();
            
            // ID自動採番
            var id = GetNextId(collection);
            var type = item.GetType();
            var idProp = type.GetProperty("id") ?? type.GetProperty("Id");
            if (idProp != null && idProp.CanWrite)
            {
                idProp.SetValue(item, Convert.ChangeType(id, idProp.PropertyType));
            }
            
            _data[collection].Add(item);
            return item;
        }
    }
}
```

**確認方法**:
```bash
dotnet test --filter InMemoryDataStore
# → すべてのテストが成功
```

---

### Step 6: MockServer実装 - GET対応 (3時間)

**Claude Codeに依頼する内容**:
```
ASP.NET Minimal APIを使って、モックサーバーを実装してください。

実装するクラス:
1. src/ApiMocker.Core/Server/MockServerBuilder.cs

機能:
- OpenApiDocumentからエンドポイントを動的生成
- GETリクエストのみ対応 (Phase 1)
- /users → リスト返却
- /users/{id} → 単一オブジェクト返却
- 初回アクセス時にダミーデータ生成 (10件)

ServeCommandから呼び出せるようにしてください。
```

**期待される実装**:
```csharp
public class MockServerBuilder
{
    private readonly OpenApiDocument _openApiDoc;
    private readonly IDataStore _dataStore;
    private readonly IDataGenerator _dataGenerator;
    
    public WebApplication Build(int port)
    {
        var builder = WebApplication.CreateBuilder();
        
        builder.WebHost.UseUrls($"http://localhost:{port}");
        
        var app = builder.Build();
        
        // GETエンドポイント生成
        foreach (var endpoint in _openApiDoc.Endpoints.Where(e => e.Method == "GET"))
        {
            RegisterGetEndpoint(app, endpoint);
        }
        
        return app;
    }
    
    private void RegisterGetEndpoint(WebApplication app, ApiEndpoint endpoint)
    {
        if (endpoint.Path.Contains("{id}"))
        {
            // GET /users/{id}
            app.MapGet(endpoint.Path, (string id) =>
            {
                var collection = ExtractCollectionName(endpoint.Path);
                var item = _dataStore.GetById<object>(collection, id);
                return item != null ? Results.Ok(item) : Results.NotFound();
            });
        }
        else
        {
            // GET /users
            app.MapGet(endpoint.Path, () =>
            {
                var collection = ExtractCollectionName(endpoint.Path);
                var items = _dataStore.GetAll<object>(collection);
                
                // 初回アクセス時はダミーデータ生成
                if (items.Count == 0)
                {
                    var schema = FindSchemaForEndpoint(endpoint);
                    for (int i = 0; i < 10; i++)
                    {
                        var data = _dataGenerator.GenerateFromSchema(schema);
                        _dataStore.Create(collection, data);
                    }
                    items = _dataStore.GetAll<object>(collection);
                }
                
                return Results.Ok(items);
            });
        }
    }
}
```

**確認方法**:
```bash
# ターミナル1
dotnet run -- serve samples/petstore.yaml

# ターミナル2
curl http://localhost:5000/users
# → ユーザーリストが返ってくる

curl http://localhost:5000/users/1
# → ユーザー1が返ってくる
```

---

### Step 7: Spectre.Console で見やすい出力 (1時間)

**Claude Codeに依頼する内容**:
```
Spectre.Consoleを使って、起動時の出力を見やすくしてください。

実装場所:
- src/ApiMocker.Cli/Commands/ServeCommand.cs

出力内容:
1. バナー表示
2. 読み込んだOpenAPI情報
3. 検出されたエンドポイント一覧 (テーブル)
4. アクセス可能なURL

以下のようなイメージ:
╔══════════════════════════════════╗
║      API Mocker v1.0.0          ║
╚══════════════════════════════════╝

OpenAPI: petstore.yaml
Title: Swagger Petstore
Version: 1.0.0

Endpoints:
┌────────┬──────────────┬─────────────┐
│ Method │ Path         │ Description │
├────────┼──────────────┼─────────────┤
│ GET    │ /users       │ List users  │
│ GET    │ /users/{id}  │ Get user    │
└────────┴──────────────┴─────────────┘

Server running at:
• http://localhost:5000

Press Ctrl+C to stop
```

**確認方法**:
```bash
dotnet run -- serve samples/petstore.yaml
# → 美しい出力が表示される
```

---

### Step 8: MVP統合テスト (1時間)

**Claude Codeに依頼する内容**:
```
統合テストを書いてください。

実装場所:
- tests/ApiMocker.Integration.Tests/MockServerTests.cs

テストケース:
1. サーバー起動成功
2. GET /users でリスト取得
3. GET /users/{id} で単一オブジェクト取得
4. 存在しないIDで404

WebApplicationFactoryを使用してください。
```

**確認方法**:
```bash
dotnet test
# → すべてのテストが成功
```

---

## ✅ Phase 1 完了チェックリスト

Phase 1 (MVP) が完了したら、以下を確認:

- [ ] `dnx api-mocker serve openapi.yaml` でサーバーが起動
- [ ] `curl http://localhost:5000/users` でダミーデータ取得
- [ ] `curl http://localhost:5000/users/1` で単一データ取得
- [ ] 日本語名、メールアドレス等のリアルなデータ
- [ ] Spectre.Consoleで見やすい出力
- [ ] すべてのテストが成功
- [ ] ビルドエラーなし

---

## 🚀 Phase 2: CRUD実装 (Week 3-4)

### Step 9: POST実装 (2時間)

**Claude Codeに依頼する内容**:
```
POSTエンドポイントを実装してください。

機能:
- リクエストボディをJSON受信
- バリデーション (required, type)
- ID自動採番
- DataStoreに保存
- 201 Created返却

実装場所:
- MockServerBuilder.cs に RegisterPostEndpoint 追加
```

**確認方法**:
```bash
curl -X POST http://localhost:5000/users \
  -H "Content-Type: application/json" \
  -d '{"name":"佐藤花子","email":"sato@example.com","age":25}'

# → 201 Created + 新規データ返却
```

---

### Step 10: PUT/PATCH実装 (2時間)

**Claude Codeに依頼する内容**:
```
PUT/PATCHエンドポイントを実装してください。

機能:
- PUT: 全フィールド更新
- PATCH: 部分更新
- 存在チェック → 404
- 200 OK返却
```

**確認方法**:
```bash
curl -X PUT http://localhost:5000/users/1 \
  -H "Content-Type: application/json" \
  -d '{"name":"更新太郎","email":"update@example.com","age":30}'

# → 200 OK + 更新データ
```

---

### Step 11: DELETE実装 (1時間)

**Claude Codeに依頼する内容**:
```
DELETEエンドポイントを実装してください。

機能:
- データ削除
- 存在チェック → 404
- 204 No Content返却
```

---

### Step 12: バリデーション実装 (3時間)

**Claude Codeに依頼する内容**:
```
リクエストボディのバリデーションを実装してください。

検証項目:
- required
- type (string, number, boolean)
- format (email, date, uuid)
- minimum/maximum
- minLength/maxLength
- enum

エラーレスポンス:
{
  "error": "Validation failed",
  "details": [
    { "field": "email", "message": "email is required" }
  ]
}
```

---

## 📊 各Phaseの進行目安

| Phase | 期間 | 作業時間 | 成果物 |
|-------|------|---------|--------|
| Phase 1 | Week 1-2 | 15-20h | GET動作するMVP |
| Phase 2 | Week 3-4 | 15-20h | CRUD完全対応 |
| Phase 3 | Week 5-6 | 10-15h | 高度な機能 |
| Phase 4 | Week 7 | 5-10h | リリース準備 |

---

## 💡 Claude Code開発のコツ

### 1. 段階的に依頼する
❌ 悪い例:
```
「api-mockerをすべて実装してください」
```

✅ 良い例:
```
「まず、OpenApiParserクラスを実装してください。
YAMLファイルを読み込んで、エンドポイント一覧を抽出する機能です。
Microsoft.OpenApi.Readersを使ってください。」
```

### 2. テストを含める
```
「DataGeneratorクラスを実装してください。
ユニットテストも一緒に書いてください。」
```

### 3. サンプルを示す
```
「以下のようなJSON出力になるようにしてください:
{
  "id": 1,
  "name": "田中太郎",
  ...
}」
```

### 4. エラー時の対処
```
「ビルドエラーが出ています。
〇〇のエラーを修正してください。」
```

---

## 🎯 トラブルシューティング

### 問題1: OpenAPIパースエラー
**症状**: YAMLファイルが読めない
**解決**: Microsoft.OpenApi.Readers のバージョン確認

### 問題2: ダミーデータが英語
**症状**: 日本語名が生成されない
**解決**: Bogusのロケール設定を確認 `new Faker("ja")`

### 問題3: エンドポイントが登録されない
**症状**: curl で 404
**解決**: ルーティングのパターンマッチング確認

---

## ✨ 完成後のデモシナリオ

```bash
# 1. サーバー起動
dnx api-mocker serve samples/blog-api.yaml --port 3000

# 2. 記事一覧取得
curl http://localhost:3000/posts

# 3. 新規記事作成
curl -X POST http://localhost:3000/posts \
  -H "Content-Type: application/json" \
  -d '{"title":"初めての投稿","content":"こんにちは"}'

# 4. 記事更新
curl -X PUT http://localhost:3000/posts/1 \
  -H "Content-Type: application/json" \
  -d '{"title":"更新した投稿","content":"内容変更"}'

# 5. 記事削除
curl -X DELETE http://localhost:3000/posts/1
```

---

**Claude Codeで開発を始める準備はできましたか?**

Phase 1のStep 1から順番に進めていきましょう! 🚀
