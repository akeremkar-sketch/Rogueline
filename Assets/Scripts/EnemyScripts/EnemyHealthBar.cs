using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;

    private float _lastFill = -1f;

    public void UpdateHealthBar(float current, float max)
    {
        if (healthBarFill == null || max <= 0f) return;

        float fill = Mathf.Clamp01(current / max);
        if (Mathf.Abs(fill - _lastFill) < 0.001f) return; // Prevent canvas rebuild
        _lastFill = fill;

        healthBarFill.fillAmount = fill;
        healthBarFill.color      = fill > 0.6f ? Color.green
                                 : fill > 0.3f ? Color.yellow
                                               : Color.red;
    }
}
