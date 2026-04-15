using DirectImageSaver.Core.Models;

namespace DirectImageSaver.App;

public static class AppText
{
    public const string ApplicationName = "DirectImageSaver";
    public const string SettingsWindowTitle = "DirectImageSaver 設定";
    public const string MenuSettings = "設定";
    public const string MenuOpenSaveFolder = "保存先を開く";
    public const string MenuOpenLogs = "ログを開く";
    public const string MenuExtensionGuide = "拡張の設定ガイド";
    public const string MenuRunAtSignIn = "起動時に常駐";
    public const string MenuExit = "終了";
    public const string LabelSaveFolder = "保存先フォルダー";
    public const string LabelTrigger = "保存トリガー";
    public const string ButtonBrowse = "参照";
    public const string ButtonSave = "保存";
    public const string ButtonCancel = "キャンセル";
    public const string ButtonClose = "閉じる";
    public const string ButtonOpenChrome = "Chrome を開く";
    public const string ButtonOpenEdge = "Edge を開く";
    public const string ButtonOpenFirefox = "Firefox を開く";
    public const string ButtonOpenExtensionFolder = "拡張フォルダーを開く";
    public const string CheckboxSuccessSound = "成功時に音を鳴らす";
    public const string CheckboxErrorSound = "失敗時に音を鳴らす";
    public const string CheckboxEnableVideoSave = "動画の即保存を有効にする";
    public const string CheckboxAutoStart = "Windows サインイン時に DirectImageSaver を自動起動する";
    public const string ErrorSelectSaveFolder = "保存先フォルダーを選択してください。";
    public const string ErrorApplySettings = "設定を正しく適用できませんでした。自動起動設定または保存先フォルダーを確認してください。";

    public const string OnboardingWindowTitle = "DirectImageSaver 拡張設定";
    public const string OnboardingHeading = "拡張機能をセットアップ";
    public const string OnboardingDescription =
        "Windows 側のセットアップは完了しています。次にブラウザに拡張機能を読み込みます。";
    public const string OnboardingPathLabel = "読み込むフォルダー";
    public const string OnboardingStep1 = "1. 下のボタンで Chrome または Edge を開く";
    public const string OnboardingStep2 = "2. Chrome: 右上「︙」→ 拡張機能 → 拡張機能を管理\n    Edge: 右上「…」→ 拡張機能 → 拡張機能の管理";
    public const string OnboardingStep3 = "3. 「デベロッパー モード」を ON → 「パッケージ化されていない拡張機能を読み込む」をクリック";
    public const string OnboardingStep4 = "4. 上に表示されたフォルダーを選び、拡張名と ID を確認する";
    public const string OnboardingExtensionIdLabel = "拡張 ID";
    public const string OnboardingHint =
        "アドレスバーに chrome://extensions または edge://extensions と入力しても設定できます。";
    public const string OnboardingChromeOpenedAuto = "Chrome に拡張機能を自動で読み込みました。ブラウザで DirectImageSaver が表示されていれば完了です。";
    public const string OnboardingEdgeOpenedAuto = "Edge に拡張機能を自動で読み込みました。ブラウザで DirectImageSaver が表示されていれば完了です。";
    public const string OnboardingChromeOpenedManual = "Chrome を開きました。すでに起動中のため、上の手順 2〜4 で拡張機能を読み込んでください。";
    public const string OnboardingEdgeOpenedManual = "Edge を開きました。すでに起動中のため、上の手順 2〜4 で拡張機能を読み込んでください。";
    public const string OnboardingChromeNotFound = "Chrome が見つかりませんでした。手動で Chrome を開き、下のフォルダーを選んで進めてください。";
    public const string OnboardingEdgeNotFound = "Edge が見つかりませんでした。手動で Edge を開き、下のフォルダーを選んで進めてください。";
    public const string OnboardingChromeOpenFailed = "Chrome を開けませんでした。手動で Chrome を開き、下のフォルダーを選んで進めてください。";
    public const string OnboardingEdgeOpenFailed = "Edge を開けませんでした。手動で Edge を開き、下のフォルダーを選んで進めてください。";
    public const string OnboardingFirefoxOpenedManual = "Firefox を開きました。下の手順で xpi をダウンロードして Firefox にインストールしてください。";
    public const string OnboardingFirefoxNotFound = "Firefox が見つかりませんでした。手動で Firefox を開き、xpi をインストールしてください。";
    public const string OnboardingFirefoxOpenFailed = "Firefox を開けませんでした。手動で Firefox を開き、xpi をインストールしてください。";

    // Browser-specific navigation guide (shown after browser launch)
    public const string ChromeGuideHeader = "▼ Chrome で拡張機能ページを開く手順";
    public const string ChromeGuideSteps =
        "① 右上の「︙」（縦三点）ボタンをクリック\n② 「拡張機能」→「拡張機能を管理」を選択\n③ 拡張機能の一覧ページが開きます";
    public const string ChromeGuideFallback =
        "※ アドレスバーに chrome://extensions と入力しても開けます";

    public const string EdgeGuideHeader = "▼ Edge で拡張機能ページを開く手順";
    public const string EdgeGuideSteps =
        "① 右上の「…」（横三点）ボタンをクリック\n② 「拡張機能」→「拡張機能の管理」を選択\n③ 拡張機能の一覧ページが開きます";
    public const string EdgeGuideFallback =
        "※ アドレスバーに edge://extensions と入力しても開けます";

    public const string FirefoxGuideHeader = "▼ Firefox に DirectImageSaver を追加する手順";
    public const string FirefoxGuideSteps =
        "① 下の「GitHub Release を開く」を押し、最新の .xpi をダウンロード\n② Firefox のウィンドウに xpi ファイルをドラッグ&ドロップ\n③ 表示されるダイアログで「追加」をクリック";
    public const string FirefoxGuideFallback =
        "※ Release / Beta では署名済み xpi が必要です。Developer Edition / Nightly / ESR では未署名でも about:config の xpinstall.signatures.required を false にすれば導入できます。";
    public const string FirefoxReleaseUrl = "https://github.com/Rare2Rare/DirectImageSaver/releases/latest";
    public const string ButtonOpenGithubRelease = "GitHub Release を開く";

    public static string GetTriggerLabel(TriggerMode triggerMode) =>
        triggerMode switch
        {
            TriggerMode.ShiftRightClick => "Shift + 右クリック",
            TriggerMode.CtrlRightClick => "Ctrl + 右クリック",
            TriggerMode.AltRightClick => "Alt + 右クリック",
            TriggerMode.CtrlShiftS => "Ctrl + Shift + S",
            _ => triggerMode.ToString()
        };
}
