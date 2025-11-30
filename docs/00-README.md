# MCK9595.APIMocker - 開発ドキュメント一式 (ConsoleAppFramework版)

OpenAPI定義からモックAPIサーバーを即座に起動する.NET 10 CLIツールの開発ドキュメントです。

**重要**: System.CommandLine → **ConsoleAppFramework** に変更しました! 🎉

## 📦 このパッケージについて

Claude Codeで**MCK9595.APIMocker**を開発するために必要な、すべてのドキュメントと設計書が含まれています。

---

## 📁 ファイル一覧

### 🎯 スタートガイド
- **quick-start-caf.md** ⭐️ **まずはここから!** (ConsoleAppFramework版)
  - 5分で始める手順
  - ConsoleAppFrameworkの特徴
  - ライセンス管理ガイド
  - 開発チェックリスト

### 📋 要件定義・設計書
- **api-mocker-requirements.md** - 要件定義書 (PRD)
  - プロダクト概要とゴール
  - 機能要件 (Phase 1-4)
  - ユーザーストーリー
  - 競合比較
  - 成功指標 (KPI)

- **api-mocker-technical-design-caf.md** - 技術設計書 ⭐️ **ConsoleAppFramework版**
  - システムアーキテクチャ図
  - ConsoleAppFramework実装パターン
  - プロジェクト構造
  - 主要コンポーネント設計
  - データフロー
  - 技術スタック

- **api-mocker-technical-design.md** - 技術設計書 (System.CommandLine版・参考用)

### 🛠️ 開発ガイド
- **api-mocker-claude-code-guide.md** - Claude Code開発ガイド
  - Phase 1-4の段階的実装手順
  - Step 1-12の詳細な実装内容
  - 各Stepでの確認方法
  - Claude Codeへの具体的な依頼例
  - トラブルシューティング

- **logging-design.md** - ログ機能設計書 ⭐️ **NEW!**
  - ZLoggerによる高性能ロギング
  - ログレベル・ログ出力先の設計
  - CLIパラメータ仕様
  - 実装例とテスト戦略

### 📄 サンプル
- **sample-user-api.yaml** - サンプルOpenAPI定義
  - ユーザー管理API
  - CRUD操作完備
  - バリデーション定義
  - 実装テスト用

### 📜 ライセンス関連 ⭐️ **NEW!**
- **LICENSE** - MITライセンス本文
  - プロジェクトのライセンス
  - GitHubリポジトリルートに配置

- **THIRD-PARTY-LICENSES.md** - 外部ライブラリのライセンス一覧
  - 使用ライブラリ5つの詳細情報
  - 全てMIT License (互換性OK)
  - ライセンス全文を含む

- **OSS-LICENSE-GUIDE.md** - OSSライセンス管理ガイド
  - ライセンスの基本知識
  - 外部ライブラリの表記方法
  - 新しいライブラリ追加時の手順
  - ライセンス互換性チェック
  - よくある質問
  - チェックリスト

### 📚 参考資料 (Excel to Bicep関連)
- **architecture-overview.md** - アーキテクチャ概要
- **cli-command-design.md** - CLIコマンド設計
- **getting-started.md** - プロジェクト開始ガイド
- **implementation-roadmap.md** - 実装ロードマップ
- **parameter-sheet-structure.md** - パラメータシート構造

---

## 🚀 開発の始め方 (3ステップ)

### Step 1: quick-start.md を読む
[quick-start.md](computer:///mnt/user-data/outputs/quick-start.md) を開いて、全体像を把握してください。

### Step 2: Claude Codeを開く
Claude.aiで「Claude Code」を開始します。

### Step 3: 最初の依頼
以下をClaude Codeにコピー&ペースト:

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

## 📖 ドキュメントの読み方

### 初回
1. **quick-start.md** - 全体像把握
2. **api-mocker-requirements.md** - 何を作るか理解
3. **api-mocker-technical-design.md** - どう作るか理解
4. **api-mocker-claude-code-guide.md** - 実装開始

### 開発中
- **api-mocker-claude-code-guide.md** を見ながら、Step 1から順番に実装
- 各Stepの「Claude Codeに依頼する内容」をコピー&ペースト
- 確認方法で動作チェック

### 問題発生時
- **quick-start.md** のトラブルシューティング参照
- **api-mocker-technical-design.md** で設計を再確認

---

## 🎯 開発フェーズ

### Phase 1: MVP (Week 1-2)
**ゴール**: `dnx api-mocker serve openapi.yaml` でGETが動く

**実装内容**:
- OpenAPIファイル読み込み
- ダミーデータ生成
- GET エンドポイント
- 美しいCLI出力

**成果物**: フロントエンド開発に使える最小限の機能

### Phase 2: CRUD (Week 3-4)
**ゴール**: POST/PUT/DELETE対応

**実装内容**:
- POST エンドポイント
- PUT/PATCH エンドポイント
- DELETE エンドポイント
- リクエストバリデーション

**成果物**: 実用レベルのモックサーバー

### Phase 3: 高度な機能 (Week 5-6)
**ゴール**: 競合ツール以上の機能

**実装内容**:
- クエリパラメータ対応
- データ永続化
- レスポンス遅延
- examples使用

**成果物**: プロダクション品質

### Phase 4: リリース準備 (Week 7)
**ゴール**: NuGet公開

**実装内容**:
- ドキュメント整備
- テストカバレッジ80%
- パッケージング
- リリース

**成果物**: 誰でも使えるOSSツール

---

## 💡 主要機能

### コア機能
- ✅ OpenAPI 3.0/3.1 完全対応
- ✅ 自動ダミーデータ生成 (日本語対応)
- ✅ CRUD操作完全サポート
- ✅ リクエストバリデーション
- ✅ クエリパラメータ (filter, sort, pagination)

### 開発者体験
- ⚡️ dnx対応 - インストール不要
- 🎨 美しいCLI出力 (Spectre.Console)
- 🔥 高速起動 (.NET 10 + ASP.NET Minimal API)
- 📝 詳細なドキュメント

### 差別化ポイント
- 🇯🇵 日本語ダミーデータ (日本人名、住所)
- 🎯 .NETエコシステム向け
- ⚡️ Node.jsツールより高速
- 🔧 型安全性

---

## 📊 技術スタック

### 必須ライブラリ
- `ConsoleAppFramework` - CLI構築 (MIT License) ⭐️ **NEW!**
  - Zero Dependency, Zero Overhead
  - Source Generator ベース
  - System.CommandLineより280倍高速!
- `Microsoft.OpenApi.Readers` - OpenAPI解析 (MIT License)
- `Bogus` - ダミーデータ生成 (MIT License)
- `Spectre.Console` - リッチCLI UI (MIT License)
- `YamlDotNet` - YAML解析 (MIT License)
- `ZLogger` - 高性能ロギング (MIT License) ⭐️ **NEW!**
  - Zero Allocation
  - 標準ロガーの8倍高速
  - UTF8直接出力

**全てMIT License = ライセンス互換性OK!** ✅

### ライブラリ選定理由

| ライブラリ | 選定理由 |
|-----------|---------|
| ConsoleAppFramework | AOT対応、超高速、シンプルAPI |
| Microsoft.OpenApi.Readers | Microsoft公式、最も信頼性が高い |
| Bogus | 多言語対応、日本語データ生成 |
| Spectre.Console | 美しいCLI、テーブル・色付き出力 |
| YamlDotNet | デファクトスタンダード |
| ZLogger | Zero Allocation、超高速、ConsoleAppFrameworkと同じCysharp製 |

### パフォーマンス比較

```
起動時間 (Cold Start):
  System.CommandLine:  280ms
  ConsoleAppFramework:   1ms  ⚡️ 280倍速い!

メモリアロケーション:
  System.CommandLine:  400KB
  ConsoleAppFramework:    0B  (ほぼゼロ)
```

---

## 🎬 デモシナリオ

完成後は以下のように動作します:

```bash
# サーバー起動
dnx api-mocker serve sample-user-api.yaml --port 3000

# ユーザー一覧
curl http://localhost:3000/users
# → [{"id":1,"name":"田中太郎","email":"tanaka@example.com",...}, ...]

# ユーザー作成
curl -X POST http://localhost:3000/users \
  -H "Content-Type: application/json" \
  -d '{"name":"佐藤花子","email":"sato@example.com","age":25}'
# → 201 Created

# ユーザー更新
curl -X PUT http://localhost:3000/users/1 \
  -d '{"name":"更新太郎","email":"update@example.com","age":30}'
# → 200 OK

# ユーザー削除
curl -X DELETE http://localhost:3000/users/1
# → 204 No Content
```

---

## 📈 開発の進捗管理

### チェックリスト
**quick-start.md** に詳細なチェックリストがあります:
- Phase 1: 約20項目
- Phase 2: 約15項目
- Phase 3: 約10項目
- Phase 4: 約15項目

### マイルストーン
- Week 2: MVP完成
- Week 4: CRUD完成
- Week 6: 高度な機能完成
- Week 7: リリース

---

## 🆘 サポート

### 質問・問題
1. **quick-start.md** のトラブルシューティング確認
2. **api-mocker-technical-design.md** で設計再確認
3. Claude Codeに具体的なエラーメッセージを共有

### 参考資料
- OpenAPI Specification: https://spec.openapis.org/oas/v3.1.0
- ASP.NET Minimal APIs: https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis
- Bogus: https://github.com/bchavez/Bogus

---

## ✨ 次のステップ

1. **[quick-start.md](computer:///mnt/user-data/outputs/quick-start.md)** を開く
2. **Claude Code** を起動
3. **Phase 1 Step 1** から実装開始
4. 段階的に機能を追加
5. GitHubで公開
6. コミュニティにシェア

---

## 📜 ライセンス管理

### OSSとして公開する際の必須ファイル

api-mockerをOSSとして公開する際は、以下のファイルが必要です:

#### 1. LICENSE (プロジェクトルート)
```
MIT License

Copyright (c) 2025 [MCK9595]
...
```

#### 2. THIRD-PARTY-LICENSES.md
使用している外部ライブラリのライセンス一覧:
- ConsoleAppFramework (MIT)
- Microsoft.OpenApi.Readers (MIT)
- Bogus (MIT)
- Spectre.Console (MIT)
- YamlDotNet (MIT)

#### 3. README.md にライセンス情報
```markdown
## License
This project is licensed under the MIT License.

### Third-Party Licenses
See [THIRD-PARTY-LICENSES.md](THIRD-PARTY-LICENSES.md)
```

#### 4. csproj に設定
```xml
<PackageLicenseExpression>MIT</PackageLicenseExpression>
```

### 詳細ガイド

**OSS-LICENSE-GUIDE.md** に以下の情報があります:
- 新しいライブラリを追加する方法
- ライセンス互換性の確認方法
- よくある質問 (FAQ)
- チェックリスト

---

## 📝 ライセンス

このドキュメントはMIT Licenseで提供されます。
開発するapi-mockerもMIT Licenseを推奨します。

---

**準備完了! さあ、Claude Codeでapi-mockerを作りましょう! 🚀**

まずは [quick-start-caf.md](computer:///mnt/user-data/outputs/quick-start-caf.md) を開いてください。
