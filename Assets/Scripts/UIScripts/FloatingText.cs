using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float lifetime  = 0.8f;

    private TextMeshPro _tmp;
    private float       _timer;
    private Color       _startColor;
    private bool        _active;
    private float       _defaultFontSize;

    private void Awake()
    {
        _tmp = GetComponent<TextMeshPro>();
        if (_tmp == null) _tmp = GetComponentInChildren<TextMeshPro>();
        if (_tmp == null) { Debug.LogError("[FloatingText] Missing TextMeshPro component!"); return; }
        _defaultFontSize = _tmp.fontSize;
    }

    public void Spawn(string text, Color color, float sizeMultiplier = 1f)
    {
        if (_tmp == null) return;
        _tmp.text     = text;
        _tmp.color    = color;
        _startColor   = color;
        _tmp.fontSize = _defaultFontSize * sizeMultiplier;
        _timer        = 0f;
        _active       = true;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!_active || _tmp == null) return;

        _timer += Time.deltaTime;
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        _startColor.a = Mathf.Lerp(1f, 0f, _timer / lifetime);
        _tmp.color = _startColor;

        if (_timer >= lifetime)
        {
            _active = false;
            FloatingTextManager.Instance?.ReturnToPool(this);
        }
    }
}
