using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedFont : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private float           _defaultFontSize;
    private TMP_FontAsset   _defaultFont;

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
        var f = LocalizationManager.CurrentFont;
        _text.font     = f != null ? f : _defaultFont;
        _text.fontSize = _defaultFontSize * LocalizationManager.CurrentFontSizeMultiplier;
    }
}
