using System.Diagnostics;
using System.Windows;
using DirectImageSaver.App.Services;
using DirectImageSaver.Core;

namespace DirectImageSaver.App.Views;

public partial class ExtensionSetupWindow : Window
{
    private readonly FolderLauncherService _launcherService = new();
    private readonly BrowserLauncherService _browserLauncherService = new();
    private static readonly System.Windows.Media.Brush SuccessBrush =
        new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x0B, 0x6E, 0x4F));
    private static readonly System.Windows.Media.Brush ErrorBrush =
        new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x9F, 0x12, 0x39));

    public ExtensionSetupWindow()
    {
        InitializeComponent();

        Title = AppText.OnboardingWindowTitle;
        HeadingTextBlock.Text = AppText.OnboardingHeading;
        DescriptionTextBlock.Text = AppText.OnboardingDescription;
        PathLabelTextBlock.Text = AppText.OnboardingPathLabel;
        Step1TextBlock.Text = AppText.OnboardingStep1;
        Step2TextBlock.Text = AppText.OnboardingStep2;
        Step3TextBlock.Text = AppText.OnboardingStep3;
        Step4TextBlock.Text = AppText.OnboardingStep4;
        ExtensionIdTextBlock.Text = $"{AppText.OnboardingExtensionIdLabel}: {NativeHostConstants.ExtensionId}";
        ExtensionPathTextBox.Text = AppPaths.InstalledExtensionDirectory;
        OpenChromeButton.Content = AppText.ButtonOpenChrome;
        OpenEdgeButton.Content = AppText.ButtonOpenEdge;
        OpenFirefoxButton.Content = AppText.ButtonOpenFirefox;
        OpenFolderButton.Content = AppText.ButtonOpenExtensionFolder;
        OpenReleaseUrlButton.Content = AppText.ButtonOpenGithubRelease;
        HintTextBlock.Text = AppText.OnboardingHint;
        CloseButton.Content = AppText.ButtonClose;
    }

    private void OpenChromeButton_OnClick(object sender, RoutedEventArgs e)
    {
        var success = _browserLauncherService.TryOpenChrome(out var message, out var autoLoaded);
        SetStatus(success, message);
        if (success && !autoLoaded) ShowBrowserGuide("chrome");
    }

    private void OpenEdgeButton_OnClick(object sender, RoutedEventArgs e)
    {
        var success = _browserLauncherService.TryOpenEdge(out var message, out var autoLoaded);
        SetStatus(success, message);
        if (success && !autoLoaded) ShowBrowserGuide("edge");
    }

    private void OpenFirefoxButton_OnClick(object sender, RoutedEventArgs e)
    {
        var success = _browserLauncherService.TryOpenFirefox(out var message, out _);
        SetStatus(success, message);
        if (success) ShowBrowserGuide("firefox");
    }

    private void OpenReleaseUrlButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = AppText.FirefoxReleaseUrl,
                UseShellExecute = true
            });
        }
        catch
        {
            // Ignore failures; the user can copy the URL manually.
        }
    }

    private void OpenFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        _launcherService.OpenFolder(AppPaths.InstalledExtensionDirectory);
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ShowBrowserGuide(string browser)
    {
        HintTextBlock.Visibility = Visibility.Collapsed;
        BrowserGuidePanel.Visibility = Visibility.Visible;
        OpenReleaseUrlButton.Visibility = Visibility.Collapsed;

        if (browser == "chrome")
        {
            GuideHeaderTextBlock.Text = AppText.ChromeGuideHeader;
            GuideHeaderTextBlock.Foreground =
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x1A, 0x73, 0xE8));
            GuideStepsTextBlock.Text = AppText.ChromeGuideSteps;
            GuideFallbackTextBlock.Text = AppText.ChromeGuideFallback;
        }
        else if (browser == "edge")
        {
            GuideHeaderTextBlock.Text = AppText.EdgeGuideHeader;
            GuideHeaderTextBlock.Foreground =
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x00, 0x78, 0xD4));
            GuideStepsTextBlock.Text = AppText.EdgeGuideSteps;
            GuideFallbackTextBlock.Text = AppText.EdgeGuideFallback;
        }
        else
        {
            GuideHeaderTextBlock.Text = AppText.FirefoxGuideHeader;
            GuideHeaderTextBlock.Foreground =
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xE6, 0x6A, 0x00));
            GuideStepsTextBlock.Text = AppText.FirefoxGuideSteps;
            GuideFallbackTextBlock.Text = AppText.FirefoxGuideFallback;
            OpenReleaseUrlButton.Visibility = Visibility.Visible;
        }
    }

    private void SetStatus(bool success, string? message)
    {
        StatusTextBlock.Foreground = success ? SuccessBrush : ErrorBrush;
        StatusTextBlock.Text = message ?? string.Empty;
    }
}
