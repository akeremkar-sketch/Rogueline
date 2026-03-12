using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;

    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.rotation = cameraTransform.rotation;
    }

    public void UpdateHealthBar(float current, float max)
    {
        healthBarFill.fillAmount = current / max;

        if (healthBarFill.fillAmount > 0.6f)
            healthBarFill.color = Color.green;
        else if (healthBarFill.fillAmount > 0.3f)
            healthBarFill.color = Color.yellow;
        else
            healthBarFill.color = Color.red;
    }
}