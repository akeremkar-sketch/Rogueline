using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class InteractionPrompt : MonoBehaviour
{
    public static InteractionPrompt Instance { get; private set; }

    private RectTransform    _rect;
    private TextMeshProUGUI  _tmp;
    private Camera           _cam;
    private Canvas           _canvas;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(this); return; }

        _rect   = GetComponent<RectTransform>();
        _tmp    = GetComponent<TextMeshProUGUI>();
        _canvas = GetComponentInParent<Canvas>();

        _rect.pivot        = new Vector2(0.5f, 0f);
        _tmp.raycastTarget = false;

        gameObject.SetActive(false);
    }

    private void Start()
    {
        _cam = Camera.main;
    }

    public void ShowAt(Vector3 worldPos, string text = null)
    {
        if (_cam    == null) _cam    = Camera.main;
        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
        if (_cam == null || _canvas == null) return;

        Vector3 screenPos = _cam.WorldToScreenPoint(worldPos + Vector3.up * 0.7f);
        if (screenPos.z < 0f) { gameObject.SetActive(false); return; }

        if (_tmp != null) _tmp.text = text ?? "[E]";
        gameObject.SetActive(true);

        if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Overlay: position IS screen pixel space
            _rect.position = new Vector3(screenPos.x, screenPos.y, 0f);
        }
        else
        {
            // Camera / World Space: convert screen → canvas local
            RectTransform canvasRect = _canvas.GetComponent<RectTransform>();
            Camera uiCam = _canvas.renderMode == RenderMode.WorldSpace ? _cam : _canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, uiCam, out Vector2 localPos);
            _rect.anchoredPosition = localPos;
        }
    }

    public void Hide() => gameObject.SetActive(false);
}
