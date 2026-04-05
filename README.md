# DirectImageSaver

ブラウザ上の画像や直リンク動画を、ホットキー一発で指定フォルダーに保存する Windows 常駐アプリ + Chrome / Edge 拡張。

## ダウンロード

| ファイル | 説明 |
|---------|------|
| [DirectImageSaver-Setup.exe](https://github.com/Rare2Rare/DirectImageSaver/releases/latest/download/DirectImageSaver-Setup.exe) | インストーラー |
| [DirectImageSaver.zip](https://github.com/Rare2Rare/DirectImageSaver/releases/latest/download/DirectImageSaver.zip) | ZIP（手動インストール用） |

> 最新リリース: [Releases](https://github.com/Rare2Rare/DirectImageSaver/releases)

## インストール

ダウンロードした `DirectImageSaver-Setup.exe` を実行する。インストール先は自由に変更できる。

セットアップ完了後、「拡張機能をセットアップ」画面が開く。

### ブラウザが起動していない場合

「Chrome を開く」または「Edge を開く」を押すだけ。拡張は自動で読み込まれる。

### ブラウザがすでに起動している場合

ボタンでブラウザを開いたあと、手動で拡張を読み込む。

1. Chrome: 右上「︙」→ 拡張機能 → 拡張機能を管理
   Edge: 右上「…」→ 拡張機能 → 拡張機能の管理
2. 「デベロッパー モード」を ON
3. 「パッケージ化されていない拡張機能を読み込む」をクリック
4. インストール先の `extension` フォルダーを選ぶ（デフォルト: `%LocalAppData%\DirectImageSaver\current\extension`）
5. 拡張名が `DirectImageSaver`、ID が `kblklkfadcpplofmmfkkplglcmomicmm` であることを確認

アドレスバーに `chrome://extensions` / `edge://extensions` と入力しても設定できる。

### 使う

画像の上で `Shift + 右クリック`。

## 3 分セットアップ

[QUICKSTART.ja.md](QUICKSTART.ja.md) を参照。

## 対応

- Windows / Chrome / Edge
- X (Twitter) Web、Chrome / Edge の X PWA
- `<img>` 要素
- `<video>` / `<source>` の http/https 直リンク動画
- 固定フォルダーへの即保存

## 非対応

- CSS `background-image`、`canvas`、`video` poster
- `blob:` URL、MediaSource、HLS / MSE / `m3u8`
- ネイティブ X アプリ
- 認証必須や anti-hotlink の完全対応

## 制限

- トリガーは `Shift + 右クリック` が初期値。`CtrlRightClick`、`AltRightClick`、`CtrlShiftS` に変更可
- 保存は Windows 側の `HttpClient` 経由。サイト側の制限で保存できない場合がある
- 直リンク動画のみ対応

## 設定

| 項目 | パス |
|------|------|
| 設定ファイル | `%AppData%\DirectImageSaver\config.json` |
| ログ | `%AppData%\DirectImageSaver\logs\` |
| 初期保存先 | `%USERPROFILE%\Pictures\DirectImageSaver` |

設定画面から変更できるもの: 保存先、トリガー、成功音 / 失敗音、動画保存 ON/OFF、自動起動

## ログ

- `directimagesaver-app-YYYYMMDD.log`
- `directimagesaver-nativehost-YYYYMMDD.log`

`nativehost` ログが出ない → 拡張からアプリに届いていない
`nativehost` は出るが `app` が出ない → native host からトレイアプリに届いていない
`app` ログが出ている → 保存処理まで到達済み

## トラブルシュート

### 保存できない

1. 拡張が読み込まれているか確認
2. 拡張カードで `Reload`
3. 対象ページを再読込
4. `Shift + 右クリック` を試す

確認ポイント:
- 拡張名: `DirectImageSaver`
- 拡張 ID: `kblklkfadcpplofmmfkkplglcmomicmm`
- 読み込み元: インストール先の `extension` フォルダー

### 拡張の管理画面に行けない

- アドレスバーに `chrome://extensions` / `edge://extensions` を入力
- または Chrome 右上「︙」→ 拡張機能 → 拡張機能を管理

### ログ・設定

- ログ: `%AppData%\DirectImageSaver\logs\`
- 設定: `%AppData%\DirectImageSaver\config.json`

## 開発

### 配布物の生成

```powershell
.\scripts\publish.ps1
```

`artifacts\publish\`、`artifacts\DirectImageSaver.zip`、`artifacts\DirectImageSaver-Setup.exe` が生成される。

### ZIP から手動インストール

```powershell
.\scripts\install.ps1
```

### テスト

```powershell
dotnet test .\DirectImageSaver.sln
```
