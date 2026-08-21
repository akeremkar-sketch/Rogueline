using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalizationData", menuName = "Localization/Language Data")]
public class LocalizationData : ScriptableObject
{
    public string        languageName;
    public TMP_FontAsset font;
    [Range(0.5f, 1.5f)]
    public float         fontSizeMultiplier = 1f;

    [SerializeField] private List<LocalizationEntry> entries = new List<LocalizationEntry>();

    private Dictionary<string, string> _dict;

    public void Init()
    {
        _dict = new Dictionary<string, string>(entries.Count);
        foreach (var e in entries)
            if (!string.IsNullOrEmpty(e.key))
                _dict[e.key] = e.value;
    }

    public string Get(string key)
    {
        if (_dict == null) Init();
        return _dict.TryGetValue(key, out var val) ? val : $"[{key}]";
    }

#if UNITY_EDITOR
    public void SetEntries(List<LocalizationEntry> list)
    {
        entries = list;
        _dict   = null;
    }
#endif
}

[System.Serializable]
public class LocalizationEntry
{
    public string key;
    [TextArea(1, 3)] public string value;
}
