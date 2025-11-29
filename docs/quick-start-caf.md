# MCK9595.APIMocker - クイックスタートガイド (ConsoleAppFramework版)

## 📦 提供ファイル

Claude Codeで開発を始めるために、以下のファイルを準備しました:

### 📋 ドキュメント
1. **api-mocker-requirements.md** - 要件定義書 (PRD)
2. **api-mocker-technical-design-caf.md** - 技術設計書 (ConsoleAppFramework版) ⭐️ **NEW!**
3. **api-mocker-claude-code-guide.md** - Claude Code開発ガイド
4. **sample-user-api.yaml** - サンプルOpenAPI定義

### 📄 ライセンス関連
5. **LICENSE** - MITライセンス本文
6. **THIRD-PARTY-LICENSES.md** - 使用ライブラリのライセンス一覧
7. **OSS-LICENSE-GUIDE.md** - OSSライセンス管理ガイド ⭐️ **NEW!**

---

## 🚀 5分で始める

### ステップ1: ドキュメントをダウンロード

提供されたすべてのファイルをダウンロードしてください。

### ステップ2: Claude Codeを開く

Claude.aiで「Claude Code」を開始します。

### ステップ3: 最初の依頼

以下をClaude Codeにコピー&ペーストしてください:

```
MCK9595.APIMockerという.NET 10のCLIツールを開発したいです。
OpenAPI定義からモックAPIサーバーを起動するツールです。

まず、プロジェクト構造を作成してください。

以下の構成で:
- src/MCK9595.APIMocker.Cli (CLIツール本体、PackAsTool設定)
- src/MCK9595.APIMocker.Core (コアロジック)
- tests/MCK9595.APIMocker.Core.Tests (xUnitテスト)

必要なNuGetパッケージ:
- ConsoleAppFramework 5.7.13
- Microsoft.OpenApi.Readers 1.6.22
- Bogus 35.6.1
- Spectre.Console 0.49.1
- YamlDotNet 16.2.1
- ZLogger 2.5.10

.NET 10を使用してください。
PackageLicenseExpression は MIT に設定してください。
Authors は MCK9595 に設定してください。
ToolCommandName は api-mocker に設定してください。
```

---

## 📝 ログ機能

### ZLogger による高性能ロギング

api-mockerは**ZLogger** (Cysharp製) を使用して、超高速なロギングを実現します。

#### 特徴
- ⚡️ 標準ロガーの**8倍速**
- 📦 Zero Allocation (メモリ効率的)
- 🎯 ConsoleAppFrameworkと同じCysharp製

### ログレベル

| レベル | 説明 | 例 |
|--------|------|-----|
| Trace | 極めて詳細 | ダミーデータの内容 |
| Debug | デバッグ情報 | エンドポイント登録 |
| Information | 一般情報 (デフォルト) | サーバー起動、リクエスト処理 |
| Warning | 警告 | バリデーションエラー |
| Error | エラー | パース失敗 |
| Critical | 致命的エラー | 起動失敗 |

### CLIパラメータ

```bash
# コンソールのみ (デフォルト)
api-mocker serve openapi.yaml

# コンソール + ファイル出力
api-mocker serve openapi.yaml --log-dir ./logs

# ログレベル変更
api-mocker serve openapi.yaml --log-level Debug --log-dir ./logs

# エラーのみ表示
api-mocker serve openapi.yaml --log-level Error
```

### ログ出力例

#### コンソール
```
[INFO][2025-11-30 14:30:15] Starting API Mocker v1.0.0
[INFO][2025-11-30 14:30:15] Server running at http://localhost:5000
[INFO][2025-11-30 14:30:20] GET /users → 200 OK (12ms)
[WARN][2025-11-30 14:30:25] Validation failed: email is required
```

#### ファイル (logs/api-mocker-2025-11-30_14-30-15.log)
```
[TRACE][2025-11-30 14:30:15] Parsing OpenAPI file
[DEBUG][2025-11-30 14:30:15] Found schema: User
[INFO][2025-11-30 14:30:15] Starting API Mocker v1.0.0
...
```

**詳細**: [logging-design.md](computer:///mnt/user-data/outputs/logging-design.md)

---

## 🎯 ConsoleAppFramework の特徴

### なぜConsoleAppFrameworkを選んだか?

| 項目 | System.CommandLine | ConsoleAppFramework |
|-----|-------------------|---------------------|
| パフォーマンス | 遅い (280ms) | **超高速 (1ms)** ⚡️ |
| 起動オーバーヘッド | 高い | **ゼロ** |
| リフレクション | 使用 | **不使用** |
| メモリアロケーション | 多い | **最小限** |
| AOT対応 | 部分的 | **完全対応** ✅ |
| コード量 | 多い | **シンプル** |

### ConsoleAppFrameworkの書き方

#### ❌ System.CommandLine (旧)
```csharp
var rootCommand = new RootCommand();
var serveCommand = new Command("serve");
var fileArgument = new Argument<string>("file");
var portOption = new Option<int>("--port", () => 5000);
serveCommand.AddArgument(fileArgument);
serveCommand.AddOption(portOption);
serveCommand.SetHandler(async (string file, int port) => { ... });
rootCommand.AddCommand(serveCommand);
await rootCommand.InvokeAsync(args);
```

#### ✅ ConsoleAppFramework (新)
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

**シンプルで高速! 🚀**

---

## 📝 Phase 1: MVP実装の修正点

### Step 2: CLIコマンド実装 (修正版)

**Claude Codeに依頼する内容**:
```
ConsoleAppFrameworkを使って、CLIコマンドを実装してください。

メインコマンド:
  api-mocker serve <file> [options]

オプション:
  -p, --port <port>     ポート番号 (default: 5000)
  -h, --host <host>     ホスト (default: localhost)
  --cors                CORS有効化 (default: true)
  --verbose             詳細ログ
  --delay <string>      レスポンス遅延 (例: "500" or "500-1000")
  --persist <file>      データ永続化ファイル

実装場所:
- src/MCK9595.APIMocker.Cli/Program.cs
- src/MCK9595.APIMocker.Cli/Commands/ServeCommand.cs

以下のように実装してください:

Program.cs:
```csharp
using ConsoleAppFramework;

var app = ConsoleApp.Create();
app.Add("serve", ServeCommand.Execute);
app.Add("validate", ValidateCommand.Execute);
await app.RunAsync(args);
```

ServeCommand.cs:
```csharp
public static class ServeCommand
{
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
        LogLevel logLevel = LogLevel.Information,
        string? logDir = null,
        
        CancellationToken cancellationToken = default)
    {
        // ロガー設定
        var loggerFactory = SetupLogger(logLevel, logDir);
        var logger = loggerFactory.CreateLogger<ServeCommand>();
        
        logger.ZLogInformation($"Starting API Mocker");
        // ... 実装
    }
}
```

詳細は api-mocker-technical-design-caf.md と logging-design.md を参照してください。
```

---

## 📚 ライセンス管理

### OSSとして公開する際の必須ファイル

api-mockerをOSS (オープンソース) として公開する場合、以下のファイルが必要です:

#### 1. LICENSE ファイル
プロジェクトルートに配置:
```
api-mocker/
├── LICENSE          ← MITライセンス本文
```

**内容**:
```
MIT License

Copyright (c) 2025 [Your Name]

Permission is hereby granted, free of charge...
```

#### 2. THIRD-PARTY-LICENSES.md
使用ライブラリのライセンス一覧:
```
api-mocker/
├── THIRD-PARTY-LICENSES.md    ← 外部ライブラリライセンス
```

**内容**:
```markdown
# Third-Party Licenses

## Summary
| Library | Version | License |
|---------|---------|---------|
| ConsoleAppFramework | 5.7.13 | MIT |
| Bogus | 35.6.1 | MIT |
...

## Detailed Licenses
### ConsoleAppFramework
[ライセンス全文]
```

#### 3. README.md にライセンス情報
```markdown
## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file.

### Third-Party Licenses
- [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework) (MIT)
- [Bogus](https://github.com/bchavez/Bogus) (MIT)
...

See [THIRD-PARTY-LICENSES.md](THIRD-PARTY-LICENSES.md) for details.
```

#### 4. csproj にライセンス設定
```xml
<PropertyGroup>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <RepositoryUrl>https://github.com/MCK9595/api-mocker</RepositoryUrl>
  <PackageProjectUrl>https://github.com/MCK9595/api-mocker</PackageProjectUrl>
</PropertyGroup>
```

### ライセンス互換性

使用しているすべてのライブラリは **MIT License** です:

| ライブラリ | ライセンス | 互換性 |
|-----------|-----------|--------|
| ConsoleAppFramework | MIT | ✅ OK |
| Microsoft.OpenApi.Readers | MIT | ✅ OK |
| Bogus | MIT | ✅ OK |
| Spectre.Console | MIT | ✅ OK |
| YamlDotNet | MIT | ✅ OK |
| ZLogger | MIT | ✅ OK |

**全てMITライセンスなので問題なし!** 🎉

### 詳細ガイド

ライセンス管理の詳細は **OSS-LICENSE-GUIDE.md** を参照してください:
- 新しいライブラリを追加する方法
- ライセンス互換性チェック
- よくある質問
- チェックリスト

---

## 🛠️ 開発の進め方

### Phase 1: MVP (Week 1-2)

1. **プロジェクト初期化** (30分)
   - ConsoleAppFrameworkベースのプロジェクト作成
   - NuGetパッケージインストール
   - LICENSE, THIRD-PARTY-LICENSES.md 配置

2. **CLIコマンド実装** (1時間)
   - ConsoleApp.Create()
   - ServeCommand.Execute()
   - 自動ヘルプ生成確認

3. **OpenAPI Parser** (2時間)
4. **Data Generator** (3時間)
5. **InMemoryDataStore** (2時間)
6. **MockServer (GET)** (3時間)
7. **Spectre.Console統合** (1時間)

### Phase 2-4: 順次実装

**api-mocker-claude-code-guide.md** に従って、Phase 2以降も実装していきます。

---

## 🎉 完成デモ

### 起動
```bash
# ローカル実行
dotnet run --project src/MCK9595.APIMocker.Cli -- serve sample-user-api.yaml

# またはdnx (インストール不要)
dnx api-mocker serve sample-user-api.yaml --port 3000
```

### 出力イメージ
```
╔══════════════════════════════════╗
║      API Mocker v1.0.0          ║
╚══════════════════════════════════╝

✓ OpenAPI: sample-user-api.yaml
  Title: Simple User API
  Version: 1.0.0

┌────────┬──────────────┬─────────────────┐
│ Method │ Path         │ Description     │
├────────┼──────────────┼─────────────────┤
│ GET    │ /users       │ ユーザー一覧    │
│ GET    │ /users/{id}  │ ユーザー詳細    │
│ POST   │ /users       │ ユーザー作成    │
│ PUT    │ /users/{id}  │ ユーザー更新    │
│ DELETE │ /users/{id}  │ ユーザー削除    │
└────────┴──────────────┴─────────────────┘

Server running at:
• http://localhost:5000

Press Ctrl+C to stop
```

### API実行
```bash
# ユーザー一覧取得
curl http://localhost:5000/users
# → [{"id":1,"name":"田中太郎",...}, ...]

# ユーザー作成
curl -X POST http://localhost:5000/users \
  -H "Content-Type: application/json" \
  -d '{"name":"佐藤花子","email":"sato@example.com"}'
# → 201 Created
```

---

## 📊 開発チェックリスト

### ✅ Phase 1完了条件

- [ ] ConsoleAppFrameworkでCLI実装
- [ ] `dnx api-mocker serve sample.yaml` で起動
- [ ] GET /users でダミーデータ取得
- [ ] 日本語名、メールアドレス生成
- [ ] Spectre.Consoleで美しい出力
- [ ] LICENSE ファイル配置
- [ ] THIRD-PARTY-LICENSES.md 作成
- [ ] すべてのテストが成功

---

## 🔗 次のステップ

1. **[api-mocker-technical-design-caf.md](computer:///mnt/user-data/outputs/api-mocker-technical-design-caf.md)** で設計を確認
2. **[OSS-LICENSE-GUIDE.md](computer:///mnt/user-data/outputs/OSS-LICENSE-GUIDE.md)** でライセンス管理を学習
3. **Claude Code** で実装開始
4. **GitHub** でリポジトリ公開
5. **NuGet** でパッケージ公開

---

**準備完了! ConsoleAppFrameworkでapi-mockerを作りましょう! 🚀**

まずは [api-mocker-technical-design-caf.md](computer:///mnt/user-data/outputs/api-mocker-technical-design-caf.md) を開いてください。
