using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;

    private TextMeshProUGUI  _text;
    private float            _defaultFontSize;
    private TMP_FontAsset    _defaultFont;

    private void Awake()
    {
        _text            = GetComponent<TextMeshProUGUI>();
        _defaultFontSize = _text.fontSize;
        _defaultFont     = _text.font;
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= Refresh;
    }

    private void Refresh()
    {
        if (_text == null || string.IsNullOrEmpty(key)) return;
        _text.text = Loc.Get(key);
        var f = LocalizationManager.CurrentFont;
        _text.font     = f != null ? f : _defaultFont;
        _text.fontSize = _defaultFontSize * LocalizationManager.CurrentFontSizeMultiplier;
    }
}
