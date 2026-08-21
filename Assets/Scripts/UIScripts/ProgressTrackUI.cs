using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressTrackUI : MonoBehaviour
{
    [Header("Track References")]
    [SerializeField] private RectTransform trackLine;
    [SerializeField] private RectTransform playerMarker;

    [Header("Boss Markers")]
    [SerializeField] private GameObject bossMarkerPrefab;
    [SerializeField] private float[]    bossDistances = { 1000f, 2500f, 5000f };
    [SerializeField] private string[]   bossLabels    = { "Boss I", "Boss II", "Boss III" };
    [Tooltip("Icon that replaces the label when the distance milestone is passed")]
    [SerializeField] private Sprite     passedIcon;

    [Header("Settings")]
    [SerializeField] private float maxDistance = 5000f;

    private bool _isVertical;

    private void OnRectTransformDimensionsChange()
    {
        if (trackLine != null)
            _isVertical = trackLine.rect.height > trackLine.rect.width;
    }

    private struct MarkerData
    {
        public RectTransform   rt;
        public Image           icon;
        public TextMeshProUGUI label;
        public float           distance;
        public bool            passed;
    }

    private MarkerData[] _markers;

    // ──────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (trackLine == null) return;
        _isVertical = trackLine.rect.height > trackLine.rect.width;
        PlaceBossMarkers();
    }

    private void Update()
    {
        if (playerMarker == null || trackLine == null || GameManager.Instance == null) return;

        float dist     = GameManager.Instance.Distance;
        float progress = Mathf.Clamp01(dist / maxDistance);
        SetPosition(playerMarker, progress);

        if (_markers == null) return;
        for (int i = 0; i < _markers.Length; i++)
        {
            if (_markers[i].passed) continue;
            if (dist >= _markers[i].distance)
            {
                _markers[i].passed = true;
                MarkAsPassed(ref _markers[i]);
            }
        }
    }

    // ── Boss Marker Placement ─────────────────────────────────────────────────

    private void PlaceBossMarkers()
    {
        if (bossMarkerPrefab == null) return;

        _markers = new MarkerData[bossDistances.Length];

        for (int i = 0; i < bossDistances.Length; i++)
        {
            float normalized = Mathf.Clamp01(bossDistances[i] / maxDistance);

            GameObject marker = Instantiate(bossMarkerPrefab, trackLine);
            RectTransform rt  = marker.GetComponent<RectTransform>();
            if (rt != null) SetPosition(rt, normalized);

            TextMeshProUGUI label = marker.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                string text = (bossLabels != null && i < bossLabels.Length && !string.IsNullOrEmpty(bossLabels[i]))
                    ? bossLabels[i]
                    : $"{bossDistances[i]:0}m";
                label.text = text;
            }

            _markers[i] = new MarkerData
            {
                rt       = rt,
                icon     = marker.GetComponent<Image>(),
                label    = label,
                distance = bossDistances[i],
                passed   = false,
            };
        }
    }

    // ── Visual Update When Passed ─────────────────────────────────────────────

    private void MarkAsPassed(ref MarkerData m)
    {
        if (m.label != null)
            m.label.gameObject.SetActive(false);

        if (m.icon != null && passedIcon != null)
            m.icon.sprite = passedIcon;
    }

    // ── Position Calculation ──────────────────────────────────────────────────

    private void SetPosition(RectTransform marker, float normalized)
    {
        if (_isVertical)
        {
            float h = trackLine.rect.height;
            marker.anchoredPosition = new Vector2(0f, -h * 0.5f + normalized * h);
        }
        else
        {
            float w = trackLine.rect.width;
            marker.anchoredPosition = new Vector2(-w * 0.5f + normalized * w, 0f);
        }
    }
}
