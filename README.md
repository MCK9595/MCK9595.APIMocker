# MCK9595.APIMocker

OpenAPI定義ファイルから自動でモックAPIサーバーを生成するCLIツールです。

## 特徴

- OpenAPI 3.0 (YAML/JSON) から自動でモックサーバーを生成
- 日本語対応のリアルなダミーデータ生成
- フル CRUD サポート (GET/POST/PUT/PATCH/DELETE)
- リクエストバリデーション
- クエリパラメータ（ソート、フィルタ、ページネーション）
- シミュレーション機能（遅延、エラー）

## インストール

```bash
dotnet tool install -g MCK9595.APIMocker
```

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
```

### OpenAPI定義の検証

```bash
mck-api-mocker validate api.yaml
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
