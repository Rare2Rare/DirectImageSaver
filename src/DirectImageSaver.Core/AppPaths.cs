namespace DirectImageSaver.Core;

public static class AppPaths
{
    public const string AppDirectoryName = "DirectImageSaver";

    public static string AppDataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDirectoryName);

    public static string LocalInstallRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, ".."));

    public static string InstalledAppDirectory => Path.Combine(LocalInstallRoot, "app");

    public static string InstalledNativeHostDirectory => Path.Combine(LocalInstallRoot, "nativehost");

    public static string InstalledExtensionDirectory => Path.Combine(LocalInstallRoot, "extension");

    public static string ConfigFilePath => Path.Combine(AppDataDirectory, "config.json");

    public static string LogDirectoryPath => Path.Combine(AppDataDirectory, "logs");

    public static string DefaultSaveDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), AppDirectoryName);
}
