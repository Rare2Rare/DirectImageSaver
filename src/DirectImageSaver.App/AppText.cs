using DirectImageSaver.Core.Models;

namespace DirectImageSaver.App;

public static class AppText
{
    public const string ApplicationName = "DirectImageSaver";
    public const string SettingsWindowTitle = "DirectImageSaver 設定";
    public const string MenuSettings = "設定";
    public const string MenuOpenSaveFolder = "保存先を開く";
    public const string MenuOpenLogs = "ログを開く";
    public const string MenuRunAtSignIn = "起動時に常駐";
    public const string MenuExit = "終了";
    public const string LabelSaveFolder = "保存先フォルダー";
    public const string LabelTrigger = "保存トリガー";
    public const string ButtonBrowse = "参照";
    public const string ButtonSave = "保存";
    public const string ButtonCancel = "キャンセル";
    public const string CheckboxSuccessSound = "成功時に音を鳴らす";
    public const string CheckboxErrorSound = "失敗時に音を鳴らす";
    public const string CheckboxAutoStart = "Windows サインイン時に DirectImageSaver を自動起動する";
    public const string ErrorSelectSaveFolder = "保存先フォルダーを選択してください。";
    public const string ErrorApplySettings = "設定を保存できませんでした。自動起動設定または保存先フォルダーの確認に失敗しました。詳細はログを確認してください。";

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
