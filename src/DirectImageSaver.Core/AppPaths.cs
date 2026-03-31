namespace DirectImageSaver.Core;

public static class AppPaths
{
    public const string AppDirectoryName = "DirectImageSaver";

    public static string AppDataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDirectoryName);

    public static string ConfigFilePath => Path.Combine(AppDataDirectory, "config.json");

    public static string LogDirectoryPath => Path.Combine(AppDataDirectory, "logs");

    public static string DefaultSaveDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), AppDirectoryName);
}
