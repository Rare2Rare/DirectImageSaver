using System.Diagnostics;
using System.IO;
using DirectImageSaver.Core;
using Microsoft.Win32;

namespace DirectImageSaver.App.Services;

public sealed class BrowserLauncherService
{
    private static readonly string[] ChromeCandidatePaths =
    [
        @"C:\Program Files\Google\Chrome\Application\chrome.exe",
        @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
    ];

    private static readonly string[] EdgeCandidatePaths =
    [
        @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
        @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
    ];

    private static readonly string[] FirefoxCandidatePaths =
    [
        @"C:\Program Files\Mozilla Firefox\firefox.exe",
        @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe"
    ];

    public bool TryOpenChrome(out string? message, out bool extensionAutoLoaded) =>
        TryOpenBrowser(
            "chrome.exe",
            "chrome",
            ChromeCandidatePaths,
            AppText.OnboardingChromeOpenedAuto,
            AppText.OnboardingChromeOpenedManual,
            AppText.OnboardingChromeNotFound,
            AppText.OnboardingChromeOpenFailed,
            out message,
            out extensionAutoLoaded);

    public bool TryOpenEdge(out string? message, out bool extensionAutoLoaded) =>
        TryOpenBrowser(
            "msedge.exe",
            "msedge",
            EdgeCandidatePaths,
            AppText.OnboardingEdgeOpenedAuto,
            AppText.OnboardingEdgeOpenedManual,
            AppText.OnboardingEdgeNotFound,
            AppText.OnboardingEdgeOpenFailed,
            out message,
            out extensionAutoLoaded);

    public bool TryOpenFirefox(out string? message, out bool extensionAutoLoaded)
    {
        extensionAutoLoaded = false;
        var browserPath = ResolveExecutablePath("firefox.exe", FirefoxCandidatePaths);
        if (string.IsNullOrWhiteSpace(browserPath))
        {
            message = AppText.OnboardingFirefoxNotFound;
            return false;
        }

        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = browserPath,
                UseShellExecute = true
            });
            if (process is null)
            {
                message = AppText.OnboardingFirefoxOpenFailed;
                return false;
            }
            message = AppText.OnboardingFirefoxOpenedManual;
            return true;
        }
        catch
        {
            message = AppText.OnboardingFirefoxOpenFailed;
            return false;
        }
    }

    private static bool TryOpenBrowser(
        string executableName,
        string processName,
        IReadOnlyList<string> fallbackPaths,
        string autoMessage,
        string manualMessage,
        string notFoundMessage,
        string failedMessage,
        out string? message,
        out bool extensionAutoLoaded)
    {
        extensionAutoLoaded = false;
        var browserPath = ResolveExecutablePath(executableName, fallbackPaths);
        if (string.IsNullOrWhiteSpace(browserPath))
        {
            message = notFoundMessage;
            return false;
        }

        bool alreadyRunning = Process.GetProcessesByName(processName).Length > 0;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = browserPath,
                UseShellExecute = true
            };

            if (!alreadyRunning)
            {
                psi.Arguments = $"--load-extension=\"{AppPaths.InstalledExtensionDirectory}\"";
            }

            using var process = Process.Start(psi);
            if (process is null)
            {
                message = failedMessage;
                return false;
            }

            extensionAutoLoaded = !alreadyRunning;
            message = alreadyRunning ? manualMessage : autoMessage;
            return true;
        }
        catch
        {
            message = failedMessage;
            return false;
        }
    }

    private static string? ResolveExecutablePath(string executableName, IReadOnlyList<string> fallbackPaths)
    {
        var registryPath = GetAppPathFromRegistry(Registry.CurrentUser, executableName)
            ?? GetAppPathFromRegistry(Registry.LocalMachine, executableName);
        if (!string.IsNullOrWhiteSpace(registryPath))
        {
            return registryPath;
        }

        foreach (var candidatePath in fallbackPaths)
        {
            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }
        }

        var pathValue = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var segment in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var candidatePath = Path.Combine(segment.Trim(), executableName);
                if (File.Exists(candidatePath))
                {
                    return candidatePath;
                }
            }
            catch
            {
                // Ignore malformed PATH entries and continue searching.
            }
        }

        return null;
    }

    private static string? GetAppPathFromRegistry(RegistryKey root, string executableName)
    {
        using var key = root.OpenSubKey($@"Software\Microsoft\Windows\CurrentVersion\App Paths\{executableName}");
        return key?.GetValue(null) as string;
    }
}
