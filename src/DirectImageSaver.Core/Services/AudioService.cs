using DirectImageSaver.Core.Models;

namespace DirectImageSaver.Core.Services;

public sealed class AudioService
{
    public void PlaySuccessIfEnabled(AppSettings settings)
    {
        if (settings.SuccessSoundEnabled)
        {
            System.Media.SystemSounds.Asterisk.Play();
        }
    }

    public void PlayFailureIfEnabled(AppSettings settings)
    {
        if (settings.ErrorSoundEnabled)
        {
            System.Media.SystemSounds.Hand.Play();
        }
    }
}
