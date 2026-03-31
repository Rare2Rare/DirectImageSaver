using System.Diagnostics;
using System.IO;

namespace DirectImageSaver.App.Services;

public sealed class FolderLauncherService
{
    public void OpenFolder(string path, bool ensureExists = true)
    {
        if (ensureExists)
        {
            Directory.CreateDirectory(path);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }
}
