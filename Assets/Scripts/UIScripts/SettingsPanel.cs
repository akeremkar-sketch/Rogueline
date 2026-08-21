using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    public static SettingsPanel Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject           panel;
    [SerializeField] private UnityEngine.UI.Button closeButton;


    [Header("Audio")]
    [SerializeField] private Slider       masterSlider;
    [SerializeField] private Slider       musicSlider;
    [SerializeField] private Slider       sfxSlider;
    [SerializeField] private Slider       uiSlider;

    [Header("Display")]
    [SerializeField] private Toggle       fullscreenToggle;
    [SerializeField] private Toggle       vsyncToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Language")]
    [SerializeField] private TMP_Dropdown languageDropdown;

    public bool IsOpen { get; private set; }

    private Resolution[] _resolutions;
    private bool         _suppressCallbacks;
    private bool         _didPause;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        panel?.SetActive(false);
    }

    private void Start()
    {
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        BuildResolutionDropdown();
        BuildLanguageDropdown();
        LoadDisplaySettings();
        RegisterCallbacks();
    }

    private void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    // ── Open / Close ──────────────────────────────────────────────────────────

    public void Open()
    {
        IsOpen = true;
        panel?.SetActive(true);
        RefreshUI();

        if (GameManager.Instance != null && Time.timeScale != 0f)
        {
            _didPause      = true;
            Time.timeScale = 0f;
        }
    }

    public void Close()
    {
        IsOpen = false;
        panel?.SetActive(false);

        if (_didPause)
        {
            _didPause      = false;
            Time.timeScale = 1f;
        }
    }

    // ── Resolution Dropdown ───────────────────────────────────────────────────

    private static readonly (int w, int h)[] _presetResolutions =
    {
        (1280,  720),
        (1600,  900),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160),
    };

    private void BuildResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        // Use the monitor's maximum supported resolution, not the current game window size
        Resolution[] supported = Screen.resolutions;
        int maxW = Screen.currentResolution.width;
        int maxH = Screen.currentResolution.height;
        foreach (var r in supported)
        {
            if (r.width  > maxW) maxW = r.width;
            if (r.height > maxH) maxH = r.height;
        }

        var filtered = new List<(int w, int h)>();
        foreach (var r in _presetResolutions)
            if (r.w <= maxW && r.h <= maxH)
                filtered.Add(r);

        _resolutions = new Resolution[filtered.Count];
        var options  = new List<string>();
        int currentIdx = 0;

        for (int i = 0; i < filtered.Count; i++)
        {
            _resolutions[i] = new Resolution { width = filtered[i].w, height = filtered[i].h };
            options.Add($"{filtered[i].w} x {filtered[i].h}");
            if (filtered[i].w == Screen.width && filtered[i].h == Screen.height)
                currentIdx = i;
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = PlayerPrefs.GetInt("settings_resolution", currentIdx);
        resolutionDropdown.RefreshShownValue();
    }

    // ── Language Dropdown ─────────────────────────────────────────────────────

    private void BuildLanguageDropdown()
    {
        if (languageDropdown == null || LocalizationManager.Instance == null) return;

        var names = LocalizationManager.Instance.LanguageNames;
        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(new System.Collections.Generic.List<string>(names));
        languageDropdown.value = LocalizationManager.Instance.CurrentLanguageIndex;
        languageDropdown.RefreshShownValue();
    }

    private void OnLanguageChanged(int idx)
    {
        if (_suppressCallbacks) return;
        if (LocalizationManager.Instance != null) LocalizationManager.Instance.SetLanguage(idx);
    }

    // ── Load / Refresh ────────────────────────────────────────────────────────

    private void LoadDisplaySettings()
    {
        bool fullscreen = PlayerPrefs.GetInt("settings_fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        FullScreenMode mode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(Screen.width, Screen.height, mode);
        QualitySettings.vSyncCount = PlayerPrefs.GetInt("settings_vsync", QualitySettings.vSyncCount > 0 ? 1 : 0);

        if (_resolutions != null)
        {
            int resIdx = PlayerPrefs.GetInt("settings_resolution", 0);
            if (resIdx < _resolutions.Length)
            {
                Resolution r = _resolutions[resIdx];
                Screen.SetResolution(r.width, r.height, Screen.fullScreen);
            }
        }
    }

    private void RefreshUI()
    {
        _suppressCallbacks = true;

        if (AudioManager.Instance != null)
        {
            if (masterSlider != null) masterSlider.value = AudioManager.Instance.MasterVolume;
            if (musicSlider  != null) musicSlider.value  = AudioManager.Instance.MusicVolume;
            if (sfxSlider    != null) sfxSlider.value    = AudioManager.Instance.SFXVolume;
            if (uiSlider     != null) uiSlider.value     = AudioManager.Instance.UIVolume;
        }

        if (fullscreenToggle != null) fullscreenToggle.isOn = Screen.fullScreen;
        if (vsyncToggle      != null) vsyncToggle.isOn      = QualitySettings.vSyncCount > 0;
        if (languageDropdown != null && LocalizationManager.Instance != null)
            languageDropdown.value = LocalizationManager.Instance.CurrentLanguageIndex;

        _suppressCallbacks = false;
    }

    // ── Callbacks ─────────────────────────────────────────────────────────────

    private void RegisterCallbacks()
    {
        if (masterSlider       != null) masterSlider.onValueChanged.AddListener(OnMasterChanged);
        if (musicSlider        != null) musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (sfxSlider          != null) sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        if (uiSlider           != null) uiSlider.onValueChanged.AddListener(OnUIChanged);
        if (fullscreenToggle   != null) fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        if (vsyncToggle        != null) vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        if (languageDropdown   != null) languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnDestroy()
    {
        if (closeButton        != null) closeButton.onClick.RemoveListener(Close);
        if (masterSlider       != null) masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        if (musicSlider        != null) musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        if (sfxSlider          != null) sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
        if (uiSlider           != null) uiSlider.onValueChanged.RemoveListener(OnUIChanged);
        if (fullscreenToggle   != null) fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        if (vsyncToggle        != null) vsyncToggle.onValueChanged.RemoveListener(OnVSyncChanged);
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        if (languageDropdown   != null) languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
    }

    private void OnMasterChanged(float v)
    {
        if (_suppressCallbacks) return;
        if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(v);
    }

    private void OnMusicChanged(float v)
    {
        if (_suppressCallbacks) return;
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(v);
    }

    private void OnSFXChanged(float v)
    {
        if (_suppressCallbacks) return;
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(v);
    }

    private void OnUIChanged(float v)
    {
        if (_suppressCallbacks) return;
        if (AudioManager.Instance != null) AudioManager.Instance.SetUIVolume(v);
    }

    private void OnFullscreenChanged(bool v)
    {
        if (_suppressCallbacks) return;
        FullScreenMode mode = v ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(Screen.width, Screen.height, mode);
        PlayerPrefs.SetInt("settings_fullscreen", v ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnVSyncChanged(bool v)
    {
        if (_suppressCallbacks) return;
        QualitySettings.vSyncCount = v ? 1 : 0;
        PlayerPrefs.SetInt("settings_vsync", v ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnResolutionChanged(int idx)
    {
        if (_suppressCallbacks || _resolutions == null || idx >= _resolutions.Length) return;
        Resolution r = _resolutions[idx];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
        PlayerPrefs.SetInt("settings_resolution", idx);
        PlayerPrefs.Save();
    }
}
