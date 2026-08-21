using TMPro;
using UnityEngine;

/// <summary>
/// Attach to a UI panel. Wire label to a single TextMeshProUGUI object.
/// The panel root (this GameObject) is hidden when WaveManager is absent.
/// </summary>
public class WaveHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;

    private void Update()
    {
        WaveManager wm = WaveManager.Instance;
        if (wm == null)
        {
            gameObject.SetActive(false);
            return;
        }

        int remaining = Mathf.CeilToInt(wm.TimeRemaining);

        if (label == null) return;
        label.text = wm.IsWaveActive
            ? string.Format(Loc.Get("wave_active"), wm.CurrentWave)
            : string.Format(Loc.Get("wave_next"), remaining);
    }
}
