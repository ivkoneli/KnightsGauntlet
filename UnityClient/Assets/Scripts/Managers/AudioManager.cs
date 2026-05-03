using UnityEngine;

public static class AudioManager
{
    private static AudioSource _music;
    private static AudioSource _sfx;

    public static bool IsMusicEnabled { get; private set; } = true;
    public static bool IsSoundEnabled { get; private set; } = true;

    static void EnsureInit()
    {
        if (_music != null) return;
        var go = new GameObject("[AudioManager]");
        Object.DontDestroyOnLoad(go);
        _music        = go.AddComponent<AudioSource>();
        _music.loop   = true;
        _music.volume = 0.5f;
        _sfx          = go.AddComponent<AudioSource>();
        _sfx.loop     = false;
        _sfx.volume   = 1f;
    }

    public static void PlayMusic(string resourcePath)
    {
        EnsureInit();
        var clip = Resources.Load<AudioClip>(resourcePath);
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] Music clip not found: {resourcePath}");
            return;
        }
        if (_music.clip == clip && _music.isPlaying) return;
        _music.clip = clip;
        _music.Play();
    }

    public static void StopMusic()
    {
        if (_music != null) _music.Stop();
    }

    public static void PlaySFX(string resourcePath)
    {
        EnsureInit();
        if (!IsSoundEnabled) return;
        var clip = Resources.Load<AudioClip>(resourcePath);
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] SFX clip not found: {resourcePath}");
            return;
        }
        _sfx.PlayOneShot(clip);
    }


    public static void StopSFX()
    {
        if (_sfx != null)
        {
            _sfx.Stop();
            _sfx.loop = false;
            _sfx.clip = null;
        }
    }

    public static void ToggleMusic()
    {
        IsMusicEnabled = !IsMusicEnabled;
        EnsureInit();
        _music.mute = !IsMusicEnabled;
    }

    public static void ToggleSound()
    {
        IsSoundEnabled = !IsSoundEnabled;
        EnsureInit();
        _sfx.mute = !IsSoundEnabled;
    }
}
