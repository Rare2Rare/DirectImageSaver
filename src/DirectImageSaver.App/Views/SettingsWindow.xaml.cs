using DirectImageSaver.Core.Models;
using System.Windows;
using Forms = System.Windows.Forms;

namespace DirectImageSaver.App.Views;

public partial class SettingsWindow : Window
{
    private sealed class TriggerModeOption
    {
        public required TriggerMode Value { get; init; }

        public required string Label { get; init; }
    }

    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();

        Title = AppText.SettingsWindowTitle;
        TriggerModeComboBox.ItemsSource = Enum.GetValues<TriggerMode>()
            .Select(triggerMode => new TriggerModeOption
            {
                Value = triggerMode,
                Label = AppText.GetTriggerLabel(triggerMode)
            })
            .ToList();
        TriggerModeComboBox.DisplayMemberPath = nameof(TriggerModeOption.Label);
        TriggerModeComboBox.SelectedValuePath = nameof(TriggerModeOption.Value);
        SaveDirectoryLabel.Text = AppText.LabelSaveFolder;
        BrowseButton.Content = AppText.ButtonBrowse;
        TriggerModeLabel.Text = AppText.LabelTrigger;
        SuccessSoundCheckBox.Content = AppText.CheckboxSuccessSound;
        ErrorSoundCheckBox.Content = AppText.CheckboxErrorSound;
        EnableVideoSaveCheckBox.Content = AppText.CheckboxEnableVideoSave;
        AutoStartCheckBox.Content = AppText.CheckboxAutoStart;
        SaveButton.Content = AppText.ButtonSave;
        CancelButton.Content = AppText.ButtonCancel;
        SaveDirectoryTextBox.Text = settings.SaveDirectory;
        TriggerModeComboBox.SelectedValue = settings.TriggerMode;
        SuccessSoundCheckBox.IsChecked = settings.SuccessSoundEnabled;
        ErrorSoundCheckBox.IsChecked = settings.ErrorSoundEnabled;
        EnableVideoSaveCheckBox.IsChecked = settings.EnableVideoSave;
        AutoStartCheckBox.IsChecked = settings.AutoStart;
        ResultSettings = settings.Clone();
    }

    public AppSettings ResultSettings { get; private set; }

    private void BrowseButton_OnClick(object sender, RoutedEventArgs e)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            InitialDirectory = string.IsNullOrWhiteSpace(SaveDirectoryTextBox.Text)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                : SaveDirectoryTextBox.Text,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == Forms.DialogResult.OK)
        {
            SaveDirectoryTextBox.Text = dialog.SelectedPath;
        }
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SaveDirectoryTextBox.Text))
        {
            System.Windows.MessageBox.Show(this, AppText.ErrorSelectSaveFolder, AppText.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ResultSettings = new AppSettings
        {
            SaveDirectory = SaveDirectoryTextBox.Text.Trim(),
            TriggerMode = TriggerModeComboBox.SelectedValue is TriggerMode triggerMode
                ? triggerMode
                : TriggerMode.ShiftRightClick,
            SuccessSoundEnabled = SuccessSoundCheckBox.IsChecked == true,
            ErrorSoundEnabled = ErrorSoundCheckBox.IsChecked == true,
            EnableVideoSave = EnableVideoSaveCheckBox.IsChecked != false,
            AutoStart = AutoStartCheckBox.IsChecked == true,
            FilenamePattern = ResultSettings.FilenamePattern,
            SupportedBrowsers = new List<string>(ResultSettings.SupportedBrowsers),
            LogLevel = ResultSettings.LogLevel
        };

        DialogResult = true;
        Close();
    }
}
