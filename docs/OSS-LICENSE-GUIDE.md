# OSS License Management Guide for API Mocker

このガイドは、api-mockerをOSS (オープンソースソフトウェア) として公開・管理する際のライセンス関連のベストプラクティスをまとめたものです。

---

## 📋 目次

1. [ライセンスの基本](#ライセンスの基本)
2. [api-mockerのライセンス構成](#api-mockerのライセンス構成)
3. [外部ライブラリのライセンス表記方法](#外部ライブラリのライセンス表記方法)
4. [新しいライブラリを追加する時](#新しいライブラリを追加する時)
5. [ライセンス互換性チェック](#ライセンス互換性チェック)
6. [よくある質問](#よくある質問)

---

## 📜 ライセンスの基本

### MITライセンスとは?

**MIT License**は最も人気のある、制限の少ないオープンソースライセンスです。

#### ✅ 許可されること
- ✅ 商用利用 (Commercial use)
- ✅ 修正 (Modification)
- ✅ 配布 (Distribution)
- ✅ 私的利用 (Private use)

#### ⚠️ 条件
- ⚠️ ライセンスと著作権表示の保持 (License and copyright notice)

#### ❌ 制限
- ❌ 責任なし (Liability)
- ❌ 保証なし (Warranty)

---

## 🏗️ api-mockerのライセンス構成

### ファイル構成

```
api-mocker/
├── LICENSE                      # プロジェクトのライセンス (MIT)
├── THIRD-PARTY-LICENSES.md      # 使用ライブラリのライセンス一覧
├── README.md                    # ライセンス情報へのリンク
└── src/
    └── ApiMocker.Cli/
        └── ApiMocker.Cli.csproj # PackageLicenseExpression設定
```

### 1. LICENSE ファイル

プロジェクトルートに配置します。

```plaintext
MIT License

Copyright (c) 2025 [MCK9595]

Permission is hereby granted, free of charge, to any person obtaining a copy
...
```

**重要ポイント**:
- `[Your Name]` を実際の名前 or GitHub名に置き換える
- 年度は最初の公開年を記載
- ファイル名は必ず `LICENSE` (拡張子なし)

### 2. THIRD-PARTY-LICENSES.md

使用している外部ライブラリのライセンスをまとめます。

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
Copyright (c) 2020 Yoshifumi Kawai / Cysharp, Inc.
...
```

### 3. README.md にライセンス情報

READMEの最後に必ず含めます:

```markdown
## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### Third-Party Licenses

This project uses the following open-source libraries:
- [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework) (MIT)
- [Bogus](https://github.com/bchavez/Bogus) (MIT)
- [Microsoft.OpenApi.Readers](https://github.com/microsoft/OpenAPI.NET) (MIT)
- [Spectre.Console](https://github.com/spectreconsole/spectre.console) (MIT)
- [YamlDotNet](https://github.com/aaubry/YamlDotNet) (MIT)

See [THIRD-PARTY-LICENSES.md](THIRD-PARTY-LICENSES.md) for full license texts.
```

### 4. csprojファイルにライセンス設定

```xml
<PropertyGroup>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <RepositoryUrl>https://github.com/yourusername/api-mocker</RepositoryUrl>
  <PackageProjectUrl>https://github.com/yourusername/api-mocker</PackageProjectUrl>
</PropertyGroup>
```

---

## 🔍 外部ライブラリのライセンス表記方法

### ステップ1: ライセンスを確認

各ライブラリのGitHubリポジトリでライセンスを確認:

1. GitHubリポジトリを開く
2. `LICENSE` ファイルを確認
3. または、About セクションでライセンスを確認

### ステップ2: THIRD-PARTY-LICENSES.md に追加

```markdown
### [Library Name]

**Version**: X.Y.Z  
**License**: MIT  
**Repository**: https://github.com/...  
**Copyright**: Copyright (c) Year Author Name  

```
[LICENSEファイルの全文をコピー]
```
```

### ステップ3: サマリーテーブルに追加

```markdown
| [Library Name] | X.Y.Z | MIT | [Purpose] |
```

---

## ➕ 新しいライブラリを追加する時

### チェックリスト

新しいNuGetパッケージを追加する際は、以下を確認:

- [ ] ライブラリのライセンスを確認
- [ ] MIT License または互換性のあるライセンスか確認
- [ ] THIRD-PARTY-LICENSES.md にライセンス情報を追加
- [ ] サマリーテーブルを更新

### 具体例: 新しいライブラリ追加

```bash
# 1. NuGetパッケージをインストール
dotnet add package NewLibrary

# 2. GitHubでライセンスを確認
# https://github.com/author/NewLibrary → LICENSE

# 3. THIRD-PARTY-LICENSES.md に追加
```

```markdown
### NewLibrary

**Version**: 1.0.0  
**License**: MIT  
**Repository**: https://github.com/author/NewLibrary  
**Copyright**: Copyright (c) 2025 Author Name  

[ライセンス全文]
```

---

## ⚖️ ライセンス互換性チェック

### MITライセンスと互換性のあるライセンス

api-mockerはMITライセンスなので、以下のライセンスは**互換性あり**:

| ライセンス | 互換性 | 注意点 |
|-----------|--------|--------|
| MIT | ✅ 互換 | そのまま使える |
| Apache 2.0 | ✅ 互換 | 特許条項に注意 |
| BSD (2-clause, 3-clause) | ✅ 互換 | そのまま使える |
| ISC | ✅ 互換 | MITと似ている |
| CC0 / Public Domain | ✅ 互換 | 制限なし |

### 互換性のないライセンス (使用禁止)

| ライセンス | 互換性 | 理由 |
|-----------|--------|------|
| GPL v2/v3 | ❌ 非互換 | コピーレフト条項 |
| LGPL | ⚠️ 条件付き | 動的リンクのみ可 |
| AGPL | ❌ 非互換 | 強いコピーレフト |
| Commercial (Proprietary) | ❌ 非互換 | 有償ライセンス |

### ライセンス確認ツール

#### NuGetパッケージのライセンス確認
```bash
# dotnet list package コマンド
dotnet list package --include-transitive

# または、NuGet.orgで確認
# https://www.nuget.org/packages/[PackageName]
```

#### ライセンス自動チェックツール
```bash
# dotnet-project-licenses をインストール
dotnet tool install --global dotnet-project-licenses

# プロジェクトのライセンスを一覧表示
dotnet-project-licenses -i ./src/ApiMocker.Cli --export-license-texts

# Markdown形式で出力
dotnet-project-licenses -i ./src/ApiMocker.Cli -f markdown
```

---

## 🛠️ 実践的なワークフロー

### 開発開始時

```bash
# 1. プロジェクト作成
dotnet new console -n ApiMocker.Cli

# 2. LICENSEファイル作成
cat > LICENSE << 'EOF'
MIT License

Copyright (c) 2025 [Your Name]
...
EOF

# 3. .gitignore作成
dotnet new gitignore
```

### ライブラリ追加時

```bash
# 1. パッケージ追加
dotnet add package ConsoleAppFramework

# 2. ライセンス確認
open https://github.com/Cysharp/ConsoleAppFramework

# 3. THIRD-PARTY-LICENSES.md 更新
# (手動で追加)
```

### リリース前

```bash
# 1. ライセンス一覧を生成
dotnet-project-licenses -i ./src/ApiMocker.Cli

# 2. THIRD-PARTY-LICENSES.md と照合

# 3. README.mdのライセンスセクション確認
```

---

## ❓ よくある質問

### Q1: MITライセンスでも商用利用できますか?

**A:** はい、できます。MITライセンスは商用利用を明示的に許可しています。

### Q2: ライブラリのライセンスを全文コピーする必要がありますか?

**A:** はい、**必須**です。MITライセンスの条件として、ライセンス全文の保持が求められています。

### Q3: NuGetパッケージのライセンスはどこで確認できますか?

**A:** 以下の方法があります:
1. NuGet.org のパッケージページ
2. GitHubリポジトリの LICENSE ファイル
3. `dotnet-project-licenses` ツール

### Q4: 依存ライブラリの依存ライブラリ (transitive dependencies) のライセンスも記載すべきですか?

**A:** 
- **直接依存**: 必ず記載 ✅
- **間接依存**: できれば記載 (推奨)
- **配布形態**: バイナリ配布なら間接依存も記載推奨

```bash
# 間接依存も含めて確認
dotnet list package --include-transitive
```

### Q5: ライセンスファイルは日本語でもいいですか?

**A:** いいえ、**英語が推奨**です。理由:
- 国際的な標準
- 法的解釈が明確
- GitHubが自動認識

### Q6: 著作権表示の年度はどうすべきですか?

**A:** 最初の公開年を記載します:
```
Copyright (c) 2025 [Your Name]
```

更新時は範囲で表記:
```
Copyright (c) 2025-2026 [Your Name]
```

### Q7: Forkしたプロジェクトの場合は?

**A:** 元のライセンスを保持し、自分の著作権を追加:
```
Original work Copyright (c) 2024 Original Author
Modified work Copyright (c) 2025 Your Name
```

### Q8: ライセンス違反があった場合は?

**A:** 以下の対応:
1. GitHubでIssue報告
2. 該当ライブラリを削除または置き換え
3. THIRD-PARTY-LICENSES.md から削除

---

## 📚 参考リンク

### 公式リソース
- [MIT License (公式)](https://opensource.org/licenses/MIT)
- [Choose a License](https://choosealicense.com/)
- [SPDX License List](https://spdx.org/licenses/)
- [GitHub Licensing Guide](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository)

### ツール
- [dotnet-project-licenses](https://github.com/tomchavakis/nuget-license)
- [FOSSA](https://fossa.com/) - ライセンス自動チェック
- [WhiteSource](https://www.whitesourcesoftware.com/)

### 日本語リソース
- [オープンソースライセンスの談話室](https://www.ipa.go.jp/jinzai/license/index.html) (IPA)
- [GitHub ライセンスガイド (日本語)](https://docs.github.com/ja/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository)

---

## ✅ チェックリスト

プロジェクト公開前に以下を確認:

### 必須項目
- [ ] `LICENSE` ファイルがルートディレクトリにある
- [ ] `THIRD-PARTY-LICENSES.md` がある
- [ ] README.md にライセンス情報がある
- [ ] csproj に `PackageLicenseExpression` がある
- [ ] すべての依存ライブラリのライセンスを確認済み
- [ ] 互換性のないライセンス (GPL等) を使っていない

### 推奨項目
- [ ] 各ライブラリのライセンス全文を記載
- [ ] `dotnet-project-licenses` で自動チェック済み
- [ ] 間接依存も確認済み
- [ ] CONTRIBUTINGガイドにライセンスポリシー記載

---

**最終更新**: 2025-11-30  
**メンテナー**: Macky
