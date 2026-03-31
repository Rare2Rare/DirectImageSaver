using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using DirectImageSaver.App.Services;
using DirectImageSaver.App.Views;
using DirectImageSaver.Core;
using DirectImageSaver.Core.Ipc;
using DirectImageSaver.Core.Models;
using DirectImageSaver.Core.Services;
using Forms = System.Windows.Forms;

namespace DirectImageSaver.App;

public partial class App : System.Windows.Application
{
    private Mutex? _singleInstanceMutex;
    private Forms.NotifyIcon? _notifyIcon;
    private Icon? _trayIcon;
    private PipeServer? _pipeServer;
    private SettingsService? _settingsService;
    private SaveRequestHandler? _saveRequestHandler;
    private LogService? _logService;
    private AutoStartService? _autoStartService;
    private FolderLauncherService? _folderLauncherService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (!TryAcquireSingleInstance())
        {
            Shutdown();
            return;
        }

        _settingsService = new SettingsService();
        var settings = _settingsService.GetCurrentSettings();
        _logService = new LogService(settings.LogLevel);
        _saveRequestHandler = new SaveRequestHandler(
            _settingsService,
            new FilenameService(),
            new DownloadService(),
            new AudioService(),
            _logService);
        _autoStartService = new AutoStartService();
        _folderLauncherService = new FolderLauncherService();

        try
        {
            _autoStartService.SetEnabled(
                settings.AutoStart,
                Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule!.FileName!);
        }
        catch (Exception exception)
        {
            _logService.LogSaveFailure(null, null, SaveErrorCode.UnhandledException, "Failed to update auto start.", exception);
        }

        InitializeTrayIcon();

        _pipeServer = new PipeServer(
            NativeHostConstants.PipeName,
            HandleNativeRequestAsync,
            (message, exception) => _logService?.LogError("PipeServer", "UnhandledPipeError", message, exception));
        _pipeServer.Start();

        if (ShouldOpenSettings(e.Args) || !EnsureSaveDirectoryReady(settings))
        {
            Dispatcher.BeginInvoke(OpenSettingsDialog);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _notifyIcon?.Dispose();
        _trayIcon?.Dispose();

        if (_pipeServer is not null)
        {
            await _pipeServer.DisposeAsync();
        }

        _logService?.Dispose();

        if (_singleInstanceMutex is not null)
        {
            _singleInstanceMutex.ReleaseMutex();
            _singleInstanceMutex.Dispose();
        }

        base.OnExit(e);
    }

    private bool TryAcquireSingleInstance()
    {
        _singleInstanceMutex = new Mutex(initiallyOwned: true, @"Global\DirectImageSaver.SingleInstance", out var createdNew);
        return createdNew;
    }

    private void InitializeTrayIcon()
    {
        _trayIcon = LoadTrayIcon();
        _notifyIcon = new Forms.NotifyIcon
        {
            Text = AppText.ApplicationName,
            Icon = _trayIcon ?? SystemIcons.Information,
            Visible = true
        };

        _notifyIcon.DoubleClick += (_, _) => OpenSettingsDialog();
        RefreshTrayMenu();
    }

    private void RefreshTrayMenu()
    {
        if (_notifyIcon is null || _settingsService is null)
        {
            return;
        }

        var settings = _settingsService.Reload();
        var menu = new Forms.ContextMenuStrip();

        menu.Items.Add(AppText.MenuSettings, null, (_, _) => OpenSettingsDialog());
        menu.Items.Add(AppText.MenuOpenSaveFolder, null, (_, _) => OpenSaveFolder());
        menu.Items.Add(AppText.MenuOpenLogs, null, (_, _) => OpenLogFolder());

        var autoStartItem = new Forms.ToolStripMenuItem(AppText.MenuRunAtSignIn)
        {
            Checked = settings.AutoStart,
            CheckOnClick = true
        };
        autoStartItem.Click += (_, _) => ToggleAutoStart(autoStartItem.Checked);
        menu.Items.Add(autoStartItem);

        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add(AppText.MenuExit, null, (_, _) => Shutdown());

        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.ContextMenuStrip = menu;
    }

    private void OpenSaveFolder()
    {
        if (_settingsService is null || _folderLauncherService is null)
        {
            return;
        }

        var settings = _settingsService.Reload();
        if (string.IsNullOrWhiteSpace(settings.SaveDirectory))
        {
            OpenSettingsDialog();
            return;
        }

        _folderLauncherService.OpenFolder(settings.SaveDirectory);
    }

    private void OpenLogFolder()
    {
        _folderLauncherService?.OpenFolder(AppPaths.LogDirectoryPath);
    }

    private void ToggleAutoStart(bool enabled)
    {
        if (_settingsService is null || _autoStartService is null)
        {
            return;
        }

        var settings = _settingsService.Reload();
        settings.AutoStart = enabled;
        _settingsService.Save(settings);
        _autoStartService.SetEnabled(enabled, Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule!.FileName!);
        RefreshTrayMenu();
    }

    private void OpenSettingsDialog()
    {
        if (_settingsService is null || _logService is null || _autoStartService is null)
        {
            return;
        }

        var window = new SettingsWindow(_settingsService.Reload());
        var dialogResult = window.ShowDialog();
        if (dialogResult != true)
        {
            return;
        }

        var updatedSettings = _settingsService.Save(window.ResultSettings);
        _logService.Reconfigure(updatedSettings.LogLevel);

        try
        {
            _autoStartService.SetEnabled(
                updatedSettings.AutoStart,
                Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule!.FileName!);
            EnsureSaveDirectoryReady(updatedSettings);
        }
        catch (Exception exception)
        {
            _logService.LogSaveFailure(
                null,
                updatedSettings.SaveDirectory,
                SaveErrorCode.UnhandledException,
                "Failed to apply settings.",
                exception);

            System.Windows.MessageBox.Show(
                AppText.ErrorApplySettings,
                AppText.ApplicationName,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        RefreshTrayMenu();
    }

    private bool EnsureSaveDirectoryReady(AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.SaveDirectory))
        {
            return false;
        }

        try
        {
            Directory.CreateDirectory(settings.SaveDirectory);
            return true;
        }
        catch (Exception exception)
        {
            _logService?.LogSaveFailure(
                null,
                settings.SaveDirectory,
                SaveErrorCode.SaveDirectoryUnavailable,
                exception.Message,
                exception);
            return false;
        }
    }

    private static bool ShouldOpenSettings(IEnumerable<string> args) =>
        args.Any(argument =>
            argument.Equals("--show-settings", StringComparison.OrdinalIgnoreCase)
            || argument.Equals("/show-settings", StringComparison.OrdinalIgnoreCase));

    private static Icon? LoadTrayIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "direct-image-saver.ico");
        return File.Exists(iconPath) ? new Icon(iconPath) : null;
    }

    private Task<NativeResponse> HandleNativeRequestAsync(NativeRequest request, CancellationToken cancellationToken)
    {
        if (_settingsService is null || _saveRequestHandler is null)
        {
            return Task.FromResult(NativeResponse.Error(SaveErrorCode.UnhandledException, "The application is not initialized."));
        }

        _settingsService.Reload();

        switch (request.Type)
        {
            case "saveImage":
                return _saveRequestHandler.HandleSaveAsync(request.Payload, cancellationToken);
            case "getConfig":
                return Task.FromResult(
                    NativeResponse.ConfigResult(ConfigSnapshot.FromSettings(_settingsService.GetCurrentSettings())));
            default:
                _logService?.LogError(
                    "PipeServer",
                    SaveErrorCode.UnsupportedRequest.ToString(),
                    $"Unsupported request type '{request.Type}'.",
                    null,
                    requestType: request.Type);
                return Task.FromResult(
                    NativeResponse.Error(SaveErrorCode.UnsupportedRequest, $"Unsupported request type '{request.Type}'."));
        }
    }
}
