# DirectImageSaver 3 分セットアップ

## 1. インストール

`DirectImageSaver-Setup.exe` を実行。インストール先は好きな場所でよい。

## 2. 拡張を読み込む

セットアップ完了後に「拡張機能をセットアップ」画面が開く。

**ブラウザが起動していなければ:**
「Chrome を開く」または「Edge を開く」を押すだけ。拡張は自動で入る。

**ブラウザがすでに起動中なら:**

1. ボタンでブラウザを開く
2. Chrome: 右上「︙」→ 拡張機能 → 拡張機能を管理
   Edge: 右上「…」→ 拡張機能 → 拡張機能の管理
3. 「デベロッパー モード」を ON
4. 「パッケージ化されていない拡張機能を読み込む」をクリック
5. インストール先の `extension` フォルダーを選ぶ

確認: 拡張名が `DirectImageSaver`、ID が `kblklkfadcpplofmmfkkplglcmomicmm`

**Firefox を使うなら:**

1. アプリの「Firefox を開く」を押す
2. ヒントエリアの「GitHub Release を開く」から `DirectImageSaver.xpi` をダウンロード
3. Firefox のウィンドウに xpi をドラッグ&ドロップ
4. 「追加」をクリック

Release / Beta は AMO 署名済み xpi が必要。Developer Edition / Nightly / ESR は `about:config` で `xpinstall.signatures.required` を `false` にすれば未署名でも導入できる。

## 3. 試す

1. 画像のあるページを開く
2. 画像にカーソルを合わせる
3. `Shift + 右クリック`

保存先: `%USERPROFILE%\Pictures\DirectImageSaver`
成功なら音が鳴り、ブラウザ右下にポップアップが出る。

## うまくいかないとき

1. 常駐アプリが起動しているか
2. 拡張が読み込まれているか
3. 拡張を `Reload` したか
4. ページを再読込したか

ログ: `%AppData%\DirectImageSaver\logs\`
設定: `%AppData%\DirectImageSaver\config.json`
