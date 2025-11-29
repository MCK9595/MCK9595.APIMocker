# API Mocker - 要件定義書 (PRD)

## 📋 プロダクト概要

### プロダクト名
**api-mocker** - OpenAPI/Swagger仕様からモックAPIサーバーを即座に起動する.NET CLIツール

### エレベーターピッチ
「OpenAPI定義があれば、1コマンドで即座にモックAPIサーバーが立ち上がる。フロントエンド開発を待たせない。」

### ターゲットユーザー
- **フロントエンドエンジニア**: バックエンドAPI完成を待たずに開発したい
- **バックエンドエンジニア**: API設計をすぐに検証したい
- **QAエンジニア**: テスト用のモックデータが必要
- **アーキテクト**: API設計のプロトタイピング

### 解決する課題
❌ **Before**
- バックエンドAPIができるまでフロントエンド開発が止まる
- 手動でモックサーバーを作るのが面倒
- json-serverは型安全でない
- 複雑なAPIスキーマに対応できない

✅ **After**
- OpenAPI定義から1コマンドでモックサーバー起動
- スキーマに準拠したリアルなレスポンス
- CRUD操作がすべて動作
- 開発効率が劇的に向上

---

## 🎯 プロダクトゴール

### ビジネスゴール
1. .NETエコシステムに不足している「手軽なモックサーバー」を提供
2. dnx経由で即座に使えることでバイラル拡散
3. GitHubで500+ Stars獲得

### ユーザーゴール
1. **5分以内**にモックAPIサーバーを立ち上げられる
2. **設定ファイル不要**でOpenAPIから自動生成
3. **リアルなデータ**でフロントエンド開発を進められる

### 技術ゴール
1. .NET 10のdnx機能を活用
2. ASP.NET Minimal APIで軽量・高速
3. OpenAPI仕様を100%準拠

---

## 📐 機能要件

### Phase 1: MVP (最小限の機能) - Week 1-2

#### 必須機能

##### F1.1: OpenAPIファイル読み込み
```bash
dnx api-mocker serve openapi.yaml
```
- **入力**: OpenAPI 3.0/3.1 YAML or JSON
- **処理**: OpenAPI仕様をパース、検証
- **出力**: エンドポイント一覧とスキーマ

**受け入れ基準**:
- [ ] OpenAPI 3.0形式のYAMLを読み込める
- [ ] OpenAPI 3.1形式のYAMLを読み込める
- [ ] JSONフォーマットも対応
- [ ] 構文エラー時に分かりやすいエラーメッセージ

##### F1.2: GET エンドポイント自動生成
```yaml
# openapi.yaml
paths:
  /users:
    get:
      responses:
        '200':
          content:
            application/json:
              schema:
                type: array
                items:
                  $ref: '#/components/schemas/User'
```

**動作**:
```bash
# サーバー起動
dnx api-mocker serve openapi.yaml

# 別ターミナルでリクエスト
curl http://localhost:5000/users
# → スキーマに準拠したユーザーリストを返す
```

**受け入れ基準**:
- [ ] GET /users → 配列を返す
- [ ] GET /users/{id} → 単一オブジェクトを返す
- [ ] スキーマに定義された全フィールドが含まれる
- [ ] データ型が正しい (string, number, boolean等)

##### F1.3: リアルなダミーデータ生成
```json
{
  "id": 1,
  "name": "田中太郎",
  "email": "tanaka@example.com",
  "age": 28,
  "createdAt": "2025-11-30T10:30:00Z"
}
```

**ルール**:
- `id`: 連番
- `name`: 日本人名をランダム生成
- `email`: name基づいたメールアドレス
- `age`: 18-65のランダム値
- `createdAt`: 現在時刻の前後

**受け入れ基準**:
- [ ] string型フィールドに意味のあるダミー値
- [ ] number型に妥当な数値
- [ ] date/datetime型に妥当な日時
- [ ] enum定義に従う

##### F1.4: 基本的なCLIオプション
```bash
dnx api-mocker serve openapi.yaml [options]

Options:
  -p, --port <port>         ポート番号 (default: 5000)
  -h, --host <host>         ホスト (default: localhost)
  --cors                     CORS有効化
  --verbose                  詳細ログ
```

**受け入れ基準**:
- [ ] ポート番号を指定できる
- [ ] CORS設定を有効化できる
- [ ] 起動時にアクセス可能なURL一覧を表示

---

### Phase 2: 実用化 (CRUD対応) - Week 3-4

##### F2.1: POST エンドポイント対応
```bash
curl -X POST http://localhost:5000/users \
  -H "Content-Type: application/json" \
  -d '{"name":"佐藤花子","email":"sato@example.com","age":25}'

# レスポンス
{
  "id": 3,
  "name": "佐藤花子",
  "email": "sato@example.com",
  "age": 25,
  "createdAt": "2025-11-30T11:00:00Z"
}
```

**受け入れ基準**:
- [ ] リクエストボディを受け取る
- [ ] バリデーション (required, type, format)
- [ ] 新規IDを自動採番
- [ ] メモリ内DBに保存
- [ ] 201 Created ステータス

##### F2.2: PUT/PATCH エンドポイント対応
```bash
curl -X PUT http://localhost:5000/users/3 \
  -H "Content-Type: application/json" \
  -d '{"name":"佐藤花子","email":"hanako@example.com","age":26}'
```

**受け入れ基準**:
- [ ] 既存データを更新
- [ ] 存在しないIDは404
- [ ] PUTは全フィールド置換
- [ ] PATCHは部分更新

##### F2.3: DELETE エンドポイント対応
```bash
curl -X DELETE http://localhost:5000/users/3
# → 204 No Content
```

**受け入れ基準**:
- [ ] データを削除
- [ ] 存在しないIDは404
- [ ] 204 No Content ステータス

##### F2.4: クエリパラメータ対応
```bash
GET /users?age=25&sort=name&limit=10
```

**受け入れ基準**:
- [ ] フィルタリング (age=25)
- [ ] ソート (sort=name)
- [ ] ページネーション (limit, offset)

---

### Phase 3: 高度な機能 - Week 5-6

##### F3.1: レスポンス例(examples)使用
```yaml
responses:
  '200':
    content:
      application/json:
        examples:
          success:
            value:
              id: 1
              name: "サンプルユーザー"
```

**動作**: examplesが定義されていればそれを優先使用

**受け入れ基準**:
- [ ] examples定義を検出
- [ ] 複数examplesから選択可能
- [ ] examples未定義時は自動生成にフォールバック

##### F3.2: リクエスト/レスポンスバリデーション
```bash
# 無効なリクエスト
curl -X POST http://localhost:5000/users \
  -d '{"name":"太郎"}' # email必須なのに欠けている

# レスポンス
{
  "error": "Validation failed",
  "details": [
    {
      "field": "email",
      "message": "email is required"
    }
  ]
}
```

**受け入れ基準**:
- [ ] required検証
- [ ] 型検証 (string, number等)
- [ ] フォーマット検証 (email, date等)
- [ ] 最小/最大値検証

##### F3.3: データ永続化オプション
```bash
dnx api-mocker serve openapi.yaml --persist data.json
```

**動作**: 
- 起動時に data.json から読み込み
- 終了時に data.json へ保存
- 再起動後もデータが残る

**受け入れ基準**:
- [ ] JSON形式でファイル保存
- [ ] 起動時に読み込み
- [ ] 変更時に自動保存 (or 終了時)

##### F3.4: レスポンス遅延シミュレーション
```bash
dnx api-mocker serve openapi.yaml --delay 500-1000
```

**動作**: レスポンスを500-1000msランダムに遅延

**受け入れ基準**:
- [ ] 固定遅延 (--delay 500)
- [ ] 範囲遅延 (--delay 500-1000)
- [ ] エンドポイント別設定可能

---

### Phase 4: プロダクション品質 - Week 7

##### F4.1: エラーレスポンス対応
```yaml
responses:
  '404':
    description: Not found
  '500':
    description: Server error
```

**動作**: OpenAPIに定義されたエラーレスポンスを返す

##### F4.2: 認証シミュレーション
```yaml
security:
  - bearerAuth: []
```

**動作**: 
- Authorizationヘッダー必須
- トークンチェックはスキップ (モックなので)

##### F4.3: WebUI提供
```bash
dnx api-mocker serve openapi.yaml --ui
```

**動作**: `http://localhost:5000/` でSwagger UIを表示

---

## 🚫 非機能要件

### 性能要件
- **起動時間**: 3秒以内
- **レスポンス時間**: 100ms以内 (遅延設定なし時)
- **同時接続**: 100リクエスト/秒

### 信頼性要件
- **エラーハンドリング**: すべての例外をキャッチ、適切なメッセージ
- **入力検証**: 不正なOpenAPIファイルを拒否

### 使いやすさ要件
- **ヘルプ**: `--help` で全オプション表示
- **エラーメッセージ**: 何が問題か明確に示す
- **起動ログ**: アクセス可能なURL一覧を表示

### 互換性要件
- **OpenAPI**: 3.0, 3.1対応
- **OS**: Windows, Linux, macOS
- **.NET**: .NET 10

---

## 📊 成功指標 (KPI)

### ユーザー指標
- **初回起動成功率**: 95%以上
- **ドキュメント参照率**: 30%以下 (直感的に使える)
- **ユーザー満足度**: 4.5/5以上

### プロダクト指標
- **GitHub Stars**: 500+ (6ヶ月以内)
- **NuGetダウンロード**: 10,000+ (3ヶ月以内)
- **週次アクティブユーザー**: 1,000+

### 技術指標
- **バグ報告**: 月5件以下
- **起動失敗率**: 1%以下
- **テストカバレッジ**: 80%以上

---

## 🎨 ユーザーストーリー

### US1: フロントエンドエンジニアのAさん
**背景**: Reactで新機能開発中、バックエンドAPIはまだ未完成

**ストーリー**:
```
AS フロントエンドエンジニア
I WANT OpenAPI仕様からモックサーバーを起動したい
SO THAT バックエンド完成を待たずに開発できる
```

**利用フロー**:
1. バックエンドチームからOpenAPI定義をもらう
2. `dnx api-mocker serve api.yaml`
3. `http://localhost:5000/users` にアクセス
4. リアルなダミーデータでフロントエンド開発

**受け入れ基準**:
- [ ] 3分以内にモックサーバー起動
- [ ] フロントエンドコードが正常動作
- [ ] POST/PUT/DELETEも動作確認できる

### US2: バックエンドエンジニアのBさん
**背景**: 新APIを設計中、実際の動作を確認したい

**ストーリー**:
```
AS バックエンドエンジニア
I WANT API設計を素早く検証したい
SO THAT 実装前に問題を発見できる
```

**利用フロー**:
1. OpenAPI定義を書く
2. `dnx api-mocker serve api.yaml`
3. curl/Postmanで動作確認
4. 問題があれば定義修正 → 再起動

**受け入れ基準**:
- [ ] ホットリロード (定義変更を自動検知)
- [ ] バリデーションエラーが分かりやすい
- [ ] 複雑なスキーマも正しく動作

---

## 🔄 競合比較

| 機能 | api-mocker | json-server | Prism | Stoplight |
|------|-----------|-------------|-------|-----------|
| 言語 | .NET/C# | Node.js | Node.js | SaaS |
| 起動速度 | ⚡️ 超高速 | 速い | 普通 | N/A |
| OpenAPI対応 | ✅ Full | ❌ なし | ✅ Full | ✅ Full |
| 型安全性 | ✅ あり | ❌ なし | ⚠️ 部分的 | ✅ あり |
| dnx対応 | ✅ Yes | ❌ No | ❌ No | ❌ No |
| データ永続化 | ✅ あり | ✅ あり | ❌ なし | ✅ あり |
| 無料 | ✅ 完全無料 | ✅ 無料 | ✅ 無料 | ⚠️ 制限あり |

**差別化ポイント**:
1. ✨ **dnx対応**: インストール不要、即実行
2. ⚡️ **.NET高速性**: Node.jsより起動・実行が高速
3. 🎯 **OpenAPI完全準拠**: 複雑なスキーマも対応
4. 🇯🇵 **日本語ダミーデータ**: 日本人名、日本語住所等

---

## 🚀 リリース計画

### v0.1.0 - MVP (Week 2)
- GET エンドポイント
- 基本的なダミーデータ生成
- CLI基本オプション

### v0.5.0 - Beta (Week 4)
- POST/PUT/PATCH/DELETE
- バリデーション
- クエリパラメータ

### v1.0.0 - GA (Week 7)
- 全機能実装
- ドキュメント完備
- NuGet公開

---

## 📚 参考資料

### 技術参考
- [OpenAPI 3.1 Specification](https://spec.openapis.org/oas/v3.1.0)
- [ASP.NET Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
- [Bogus - Fake Data Generator](https://github.com/bchavez/Bogus)

### 競合調査
- [json-server](https://github.com/typicode/json-server)
- [Prism](https://stoplight.io/open-source/prism)
- [Mockoon](https://mockoon.com/)

---

## ✅ 要件確認チェックリスト

開発開始前に以下を確認:

- [ ] OpenAPI 3.0/3.1仕様を理解している
- [ ] ASP.NET Minimal API経験がある
- [ ] .NET 10 SDK がインストール済み
- [ ] dnxの仕組みを理解している
- [ ] Bogusライブラリを知っている
- [ ] このPRDの全要件を理解した

---

**承認者**: Macky  
**作成日**: 2025-11-30  
**最終更新**: 2025-11-30  
**ステータス**: ✅ 承認待ち → Claude Code開発開始
