using System.Collections;
using UnityEngine;

public enum SoundCategory
{
    Critical = 0,
    Combat   = 1,
    Hit      = 2,
    Pickup   = 3,
    Ambient  = 4
}

public enum MusicContext { None, UI, Gameplay, Boss }

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Default Volumes (0–1)")]
    [Range(0f, 1f)] [SerializeField] private float defaultMaster = 1f;
    [Range(0f, 1f)] [SerializeField] private float defaultSFX    = 1f;
    [Range(0f, 1f)] [SerializeField] private float defaultUI     = 1f;
    [Range(0f, 1f)] [SerializeField] private float defaultMusic  = 0.6f;

    [Header("Music Playlists")]
    [SerializeField] private AudioClip[] uiMusic;
    [SerializeField] private AudioClip[] gameplayMusic;
    [SerializeField] private AudioClip[] bossMusic;

    [Header("Music Settings")]
    [Tooltip("Crossfade duration between tracks (seconds)")]
    [SerializeField] private float crossfadeDuration = 1.5f;

    // ── Volume properties ────────────────────────────────────────────────────────

    public float MasterVolume { get; private set; }
    public float SFXVolume    { get; private set; }
    public float UIVolume     { get; private set; }
    public float MusicVolume  { get; private set; }

    public float EffectiveSFX   => MasterVolume * SFXVolume;
    public float EffectiveUI    => MasterVolume * UIVolume;
    public float EffectiveMusic => MasterVolume * MusicVolume;

    // ── Audio sources ────────────────────────────────────────────────────────────

    private AudioSource _sfxSource;
    private AudioSource _musicA;
    private AudioSource _musicB;
    private AudioSource _activeMusic;   // currently playing
    private AudioSource _idleMusic;     // crossfade target

    // ── Music state ──────────────────────────────────────────────────────────────

    private MusicContext _currentContext = MusicContext.None;
    private AudioClip[]  _currentPlaylist;
    private int          _currentTrackIndex = -1;
    private bool         _bossWasActive;
    private int          _musicPollFrame;

    // ── Per-category frame limits ─────────────────────────────────────────────────

    private static readonly int[] _maxPerFrame    = { 0, 4, 2, 3, 1 };
    private readonly int[]         _countThisFrame = new int[5];
    private int                    _frameTracker   = -1;

    // ── Lifecycle ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        MasterVolume = PlayerPrefs.GetFloat("vol_master", defaultMaster);
        SFXVolume    = PlayerPrefs.GetFloat("vol_sfx",    defaultSFX);
        UIVolume     = PlayerPrefs.GetFloat("vol_ui",     defaultUI);
        MusicVolume  = PlayerPrefs.GetFloat("vol_music",  defaultMusic);

        _sfxSource                    = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake        = false;
        _sfxSource.spatialBlend       = 0f;
        _sfxSource.loop               = false;
        _sfxSource.ignoreListenerPause = true; // continue playing when timeScale=0

        _musicA = CreateMusicSource();
        _musicB = CreateMusicSource();

        _activeMusic = _musicA;
        _idleMusic   = _musicB;
    }

    private AudioSource CreateMusicSource()
    {
        var src              = gameObject.AddComponent<AudioSource>();
        src.playOnAwake      = false;
        src.spatialBlend     = 0f;
        src.loop             = false;
        src.volume           = 0f;
        return src;
    }

    private void Update()
    {
        // Auto-detect boss transition (gameplay scene only)
        if (_currentContext == MusicContext.Gameplay || _currentContext == MusicContext.Boss)
        {
            bool bossNow = BossChunk.IsActive;
            if (bossNow && !_bossWasActive)
                SwitchContext(MusicContext.Boss);
            else if (!bossNow && _bossWasActive)
                SwitchContext(MusicContext.Gameplay);
            _bossWasActive = bossNow;
        }

        // Track finished? Check every 45 frames (approx. 0.75s)
        if (++_musicPollFrame >= 45)
        {
            _musicPollFrame = 0;
            if (_currentPlaylist != null && _activeMusic != null
                && !_activeMusic.isPlaying && _currentContext != MusicContext.None)
                PlayNextTrack();
        }
    }

    // ── Public context methods ────────────────────────────────────────────────────

    public void PlayUIMusic()      => SwitchContext(MusicContext.UI);
    public void PlayGameplayMusic() => SwitchContext(MusicContext.Gameplay);
    public void PlayBossMusic()    => SwitchContext(MusicContext.Boss);
    public void StopMusic()
    {
        StopAllCoroutines();
        if (_musicA != null) _musicA.Stop();
        if (_musicB != null) _musicB.Stop();
        _currentContext  = MusicContext.None;
        _currentPlaylist = null;
    }

    // ── Internal context switch ───────────────────────────────────────────────────

    private void SwitchContext(MusicContext ctx)
    {
        if (_currentContext == ctx) return;
        _currentContext = ctx;

        _currentPlaylist = ctx switch
        {
            MusicContext.UI       => uiMusic,
            MusicContext.Gameplay => gameplayMusic,
            MusicContext.Boss     => bossMusic,
            _                     => null,
        };

        if (_currentPlaylist == null || _currentPlaylist.Length == 0)
        {
            StopMusic();
            return;
        }

        _currentTrackIndex = -1;
        PlayNextTrack();
    }

    private void PlayNextTrack()
    {
        if (_currentPlaylist == null || _currentPlaylist.Length == 0) return;
        if (_activeMusic == null || _idleMusic == null) return;

        int next = _currentTrackIndex;
        if (_currentPlaylist.Length > 1)
        {
            while (next == _currentTrackIndex)
                next = Random.Range(0, _currentPlaylist.Length);
        }
        else
        {
            next = 0;
        }
        _currentTrackIndex = next;

        AudioClip clip = _currentPlaylist[_currentTrackIndex];
        if (clip == null) return;

        _idleMusic.clip   = clip;
        _idleMusic.volume = 0f;
        _idleMusic.Play();

        StopAllCoroutines();
        StartCoroutine(Crossfade(_activeMusic, _idleMusic));

        (_activeMusic, _idleMusic) = (_idleMusic, _activeMusic);
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to)
    {
        float elapsed   = 0f;
        float fromStart = from.volume;

        while (elapsed < crossfadeDuration)
        {
            elapsed     += Time.unscaledDeltaTime;
            float t      = Mathf.Clamp01(elapsed / crossfadeDuration);
            to.volume    = Mathf.Lerp(0f,         EffectiveMusic, t); // dynamic — reflects volume changes immediately
            from.volume  = Mathf.Lerp(fromStart,  0f,             t);
            yield return null;
        }

        from.Stop();
        from.volume = 0f;
        to.volume   = EffectiveMusic;
    }

    private void ApplyMusicVolume()
    {
        // Both sources can be non-zero during crossfade — update both
        if (_musicA != null && _musicA.isPlaying) _musicA.volume = EffectiveMusic;
        if (_musicB != null && _musicB.isPlaying) _musicB.volume = EffectiveMusic;
    }

    // For testing Inspector sliders at runtime
    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        MasterVolume = defaultMaster;
        MusicVolume  = defaultMusic;
        SFXVolume    = defaultSFX;
        ApplyMusicVolume();
    }

    // ── Legacy single-clip API (backwards compatibility) ─────────────────────────

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        StopMusic();
        _currentContext  = MusicContext.None;
        _activeMusic.clip   = clip;
        _activeMusic.loop   = loop;
        _activeMusic.volume = EffectiveMusic;
        _activeMusic.Play();
    }

    // ── SFX ──────────────────────────────────────────────────────────────────────

    public void PlaySFX(AudioClip clip, SoundCategory category = SoundCategory.Combat, float baseVolume = 1f)
    {
        if (clip == null) return;

        if (Time.frameCount != _frameTracker)
        {
            System.Array.Clear(_countThisFrame, 0, _countThisFrame.Length);
            _frameTracker = Time.frameCount;
        }

        int idx = (int)category;
        int max = _maxPerFrame[idx];
        if (max > 0 && _countThisFrame[idx] >= max) return;
        _countThisFrame[idx]++;

        _sfxSource.PlayOneShot(clip, baseVolume * EffectiveSFX);
    }

    // ── Volume setters ────────────────────────────────────────────────────────────

    public void SetMasterVolume(float v)
    {
        MasterVolume = Mathf.Clamp01(v);
        ApplyMusicVolume();
        PlayerPrefs.SetFloat("vol_master", MasterVolume);
    }

    public void SetSFXVolume(float v)
    {
        SFXVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat("vol_sfx", SFXVolume);
    }

    public void SetUIVolume(float v)
    {
        UIVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat("vol_ui", UIVolume);
    }

    public void SetMusicVolume(float v)
    {
        MusicVolume = Mathf.Clamp01(v);
        ApplyMusicVolume();
        PlayerPrefs.SetFloat("vol_music", MusicVolume);
    }
}
