# MCK9595.APIMocker

OpenAPI定義ファイルから自動でモックAPIサーバーを生成するCLIツールです。

## 特徴

- OpenAPI 3.0 (YAML/JSON) から自動でモックサーバーを生成
- 日本語対応のリアルなダミーデータ生成
- フル CRUD サポート (GET/POST/PUT/PATCH/DELETE)
- リクエストバリデーション
- クエリパラメータ（ソート、フィルタ、ページネーション）
- シミュレーション機能（遅延、エラー）
- データ永続化（JSON ファイル）
- 認証シミュレーション（Bearer / API Key / Basic）
- カスタムレスポンス定義
- Webhook 通知

## インストール

### グローバルインストール

```bash
dotnet tool install -g MCK9595.APIMocker
```

### dnx でインストールなしで実行 (.NET 10+)

.NET 10 では `dnx` コマンドで、ツールをインストールせずに直接実行できます（npm の `npx` 相当）。

```bash
# インストール不要で直接実行（パッケージIDを指定）
dnx MCK9595.APIMocker serve api.yaml

# バージョン指定
dnx MCK9595.APIMocker@0.2.0 serve api.yaml

# dotnet tool exec でも同様
dotnet tool exec MCK9595.APIMocker -- serve api.yaml
```

> **Note:** `dnx` はNuGetパッケージIDを指定します。インストール後のコマンド名 `mck-api-mocker` ではありません。

**特徴:**
- グローバル/ローカルインストール不要
- 初回実行時に自動ダウンロード
- ローカルマニフェスト（`.config/dotnet-tools.json`）があればそのバージョンを優先
- CI/CDでは `--non-interactive` で確認プロンプトをスキップ

## 使い方

### 基本的な使い方

```bash
mck-api-mocker serve api.yaml
```

### CLIオプション

```bash
mck-api-mocker serve api.yaml [options]

Options:
  -p, --port <port>        ポート番号 (default: 5000)
  --host <host>            ホストアドレス (default: localhost)
  --cors                   CORSを有効化 (default: true)
  -v, --verbose            詳細なリクエスト/レスポンスログを出力
  --delay <ms>             固定遅延をミリ秒で指定
  --delay-min <ms>         ランダム遅延の最小値
  --delay-max <ms>         ランダム遅延の最大値
  --error-rate <0.0-1.0>   エラー発生確率
  --error-codes <codes>    返すエラーコード (カンマ区切り)
  --persist                データをJSONファイルに永続化
  --data-dir <path>        永続化データの保存先ディレクトリ (default: ./.mck-data)
  --seed <file>            初期データを読み込むJSONファイル
  --auth <mode>            認証モード: bearer, apikey, basic, none
  --auth-key <key>         認証に必要なキー/トークン
  --responses <file>       カスタムレスポンス定義JSONファイル
  --webhooks <file>        Webhook設定JSONファイル
```

### 例

```bash
# 基本的な起動
mck-api-mocker serve api.yaml

# ポート3000で起動
mck-api-mocker serve api.yaml -p 3000

# 詳細ログ付きで起動
mck-api-mocker serve api.yaml -v

# 500msの固定遅延
mck-api-mocker serve api.yaml --delay 500

# 100-500msのランダム遅延
mck-api-mocker serve api.yaml --delay-min 100 --delay-max 500

# 10%の確率でエラーを返す
mck-api-mocker serve api.yaml --error-rate 0.1

# 10%の確率で500/502/503のいずれかを返す
mck-api-mocker serve api.yaml --error-rate 0.1 --error-codes 500,502,503

# 全機能を有効にして起動
mck-api-mocker serve api.yaml \
  --persist \
  --data-dir ./data \
  --seed examples/seed-data.json \
  --auth bearer \
  --auth-key "secret-token" \
  --responses examples/responses.json \
  --webhooks examples/webhooks.json
```

### OpenAPI定義の検証

```bash
mck-api-mocker validate api.yaml
```

## データ永続化

`--persist` オプションを使用すると、データがJSONファイルに保存され、サーバー再起動後もデータが維持されます。

```bash
mck-api-mocker serve api.yaml --persist --data-dir ./my-data
```

データは `{data-dir}/{resource}.json` 形式で保存されます（例: `./my-data/users.json`）。

## 初期データ (Seed Data)

`--seed` オプションで、サーバー起動時に読み込む初期データを指定できます。

### seed-data.json の形式

```json
{
  "users": [
    { "id": 1, "name": "山田太郎", "email": "yamada@example.com", "role": "admin" },
    { "id": 2, "name": "鈴木花子", "email": "suzuki@example.com", "role": "user" }
  ],
  "products": [
    { "id": 1, "name": "商品A", "price": 1000 }
  ]
}
```

キー名はOpenAPI定義のパス（例: `/users` → `users`）に対応します。

```bash
mck-api-mocker serve api.yaml --seed examples/seed-data.json
```

## 認証シミュレーション

`--auth` オプションで認証を有効にできます。認証なしのリクエストには `401 Unauthorized` を返します。

### Bearer Token

```bash
mck-api-mocker serve api.yaml --auth bearer
# または特定のトークンを要求
mck-api-mocker serve api.yaml --auth bearer --auth-key "my-secret-token"
```

```bash
# アクセス方法
curl -H "Authorization: Bearer my-secret-token" http://localhost:5000/users
```

### API Key

```bash
mck-api-mocker serve api.yaml --auth apikey --auth-key "my-api-key"
```

```bash
# アクセス方法
curl -H "X-API-Key: my-api-key" http://localhost:5000/users
```

### Basic 認証

```bash
mck-api-mocker serve api.yaml --auth basic --auth-key "user:password"
```

```bash
# アクセス方法
curl -u user:password http://localhost:5000/users
```

## カスタムレスポンス

`--responses` オプションで、特定のリクエストに対してカスタムレスポンスを返すことができます。

### responses.json の形式

```json
{
  "responses": [
    {
      "description": "ヘルスチェック",
      "method": "GET",
      "path": "/health",
      "status": 200,
      "body": { "status": "ok" }
    },
    {
      "description": "特定のメールでエラーを返す",
      "method": "POST",
      "path": "/users",
      "match": { "email": "duplicate@example.com" },
      "status": 409,
      "body": { "error": "このメールアドレスは既に登録されています" }
    },
    {
      "description": "管理者エンドポイントを保護",
      "method": "*",
      "path": "/admin/*",
      "status": 403,
      "body": { "error": "Forbidden" }
    }
  ]
}
```

### プロパティ

| プロパティ | 説明 |
|-----------|------|
| `method` | HTTPメソッド。`*` で全メソッドにマッチ |
| `path` | パス。`*` でワイルドカード（例: `/users/*`） |
| `match` | リクエストボディの条件（部分一致） |
| `status` | レスポンスのHTTPステータスコード |
| `body` | レスポンスボディ |

```bash
mck-api-mocker serve api.yaml --responses examples/responses.json
```

## Webhook 通知

`--webhooks` オプションで、CUD操作（Create/Update/Delete）時に外部URLへ通知を送信できます。

### webhooks.json の形式

```json
{
  "webhooks": [
    {
      "description": "ユーザー作成時に通知",
      "event": "POST:/users",
      "url": "https://webhook.site/your-id",
      "headers": {
        "X-Webhook-Secret": "secret-key"
      }
    },
    {
      "description": "全てのユーザー操作を通知",
      "event": "*:/users/*",
      "url": "https://your-backend.example.com/webhooks"
    }
  ]
}
```

### プロパティ

| プロパティ | 説明 |
|-----------|------|
| `event` | `{METHOD}:{PATH}` 形式。`*` でワイルドカード |
| `url` | 通知先URL |
| `headers` | カスタムヘッダー（オプション） |

### 送信されるペイロード

```json
{
  "event": "POST:/users",
  "timestamp": "2025-01-01T00:00:00Z",
  "data": { /* リクエストボディ */ }
}
```

```bash
mck-api-mocker serve api.yaml --webhooks examples/webhooks.json
```

## クエリパラメータ

### ページネーション

```bash
# 最初の10件を取得
GET /users?_take=10

# 10件目から5件取得
GET /users?_skip=10&_take=5
```

レスポンス:
```json
{
  "items": [...],
  "total": 100,
  "hasMore": true
}
```

### ソート

```bash
# 名前で昇順ソート
GET /users?_sort=name

# 名前で降順ソート
GET /users?_sort=name&_order=desc
```

### フィルタリング

```bash
# statusがactiveのユーザーを取得
GET /users?status=active

# 複数条件
GET /users?status=active&role=admin
```

### カスタムステータスコード

```bash
# 500エラーを返す
GET /users?_status=500

# リクエスト単位での遅延
GET /users?_delay=1000
```

## バリデーション

以下のバリデーションを自動で行います:

- **required**: 必須フィールドのチェック
- **type**: 型チェック (string, integer, number, boolean, array, object)
- **format**: フォーマットチェック (email, uuid, date, date-time, uri)
- **minLength/maxLength**: 文字列長チェック
- **minimum/maximum**: 数値範囲チェック
- **enum**: 列挙値チェック

バリデーションエラー時のレスポンス:
```json
{
  "error": "Validation failed",
  "details": [
    { "field": "email", "message": "Field 'email' must be a valid email" }
  ]
}
```

## ダミーデータ生成

フィールド名やフォーマットから適切なダミーデータを自動生成します:

| フィールド/フォーマット | 生成例 |
|------------------------|--------|
| name | 田中太郎 |
| email / format: email | tanaka@example.com |
| phone / phoneNumber | 090-1234-5678 |
| address | 東京都渋谷区... |
| format: uuid | 550e8400-e29b-41d4-... |
| format: date-time | 2025-11-30T10:00:00Z |
| enum | 定義された値からランダム選択 |

## サンプルファイル

`examples/` ディレクトリにサンプル設定ファイルがあります:

- `examples/seed-data.json` - 初期データのサンプル
- `examples/responses.json` - カスタムレスポンスのサンプル
- `examples/webhooks.json` - Webhook設定のサンプル

## 開発

### ビルド

```bash
dotnet build
```

### テスト

```bash
dotnet test
```

### ローカル実行

```bash
dotnet run --project src/MCK9595.APIMocker.Cli -- serve sample-user-api.yaml
```

## ライセンス

MIT
