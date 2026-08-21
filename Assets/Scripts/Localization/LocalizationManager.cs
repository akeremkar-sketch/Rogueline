using UnityEngine;

[DefaultExecutionOrder(-100)]
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }
    public static event System.Action OnLanguageChanged;

    [SerializeField] private LocalizationData[] languages;

    public int CurrentLanguageIndex { get; private set; }

    public string[] LanguageNames
    {
        get
        {
            if (languages == null) return new string[0];
            var names = new string[languages.Length];
            for (int i = 0; i < languages.Length; i++)
                names[i] = languages[i] != null ? languages[i].languageName : $"Lang {i}";
            return names;
        }
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        int saved = PlayerPrefs.GetInt("settings_language", 0);
        SetLanguage(saved, notify: false);
    }

    public static TMPro.TMP_FontAsset CurrentFont
    {
        get
        {
            if (Instance == null || Instance.languages == null) return null;
            int idx = Instance.CurrentLanguageIndex;
            if (idx < 0 || idx >= Instance.languages.Length) return null;
            return Instance.languages[idx]?.font;
        }
    }

    public static float CurrentFontSizeMultiplier
    {
        get
        {
            if (Instance == null || Instance.languages == null) return 1f;
            int idx = Instance.CurrentLanguageIndex;
            if (idx < 0 || idx >= Instance.languages.Length) return 1f;
            return Instance.languages[idx]?.fontSizeMultiplier ?? 1f;
        }
    }

    public static string Get(string key)
    {
        if (Instance == null || Instance.languages == null || Instance.languages.Length == 0)
            return key;
        int idx = Instance.CurrentLanguageIndex;
        if (idx < 0 || idx >= Instance.languages.Length || Instance.languages[idx] == null)
            return key;
        return Instance.languages[idx].Get(key);
    }

    public void SetLanguage(int index, bool notify = true)
    {
        if (languages == null || index < 0 || index >= languages.Length) return;
        CurrentLanguageIndex = index;
        if (languages[index] != null) languages[index].Init();
        PlayerPrefs.SetInt("settings_language", index);
        if (notify) OnLanguageChanged?.Invoke();
    }
}

// Short alias — Loc.Get("key") instead of LocalizationManager.Get("key")
public static class Loc
{
    public static string Get(string key) => LocalizationManager.Get(key);
}
