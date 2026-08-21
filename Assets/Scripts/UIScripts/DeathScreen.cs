using UnityEngine;
using TMPro;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI breakdownText;

    private void OnEnable()
    {
        if (GameManager.Instance == null) return;

        var gm = GameManager.Instance;
        int totalScore = gm.CalculateScore();

        if (totalScoreText != null)
            totalScoreText.text = string.Format(Loc.Get("death_score"), totalScore);

        if (breakdownText != null)
        {
            breakdownText.text =
                string.Format(Loc.Get("death_distance"),     Mathf.FloorToInt(gm.Distance),      Mathf.FloorToInt(gm.Distance * 10f)) + "\n" +
                string.Format(Loc.Get("death_kills"),        gm.KillCount,                        gm.KillCount * 100) + "\n" +
                string.Format(Loc.Get("death_damage_dealt"), Mathf.FloorToInt(gm.DamageDealt),   Mathf.FloorToInt(gm.DamageDealt * 0.5f)) + "\n" +
                string.Format(Loc.Get("death_level"),        LevelSystem.Instance?.CurrentLevel ?? 1, gm.LevelsGained * 300) + "\n" +
                string.Format(Loc.Get("death_coins"),        gm.CoinsGained,                      gm.CoinsGained * 25) + "\n" +
                string.Format(Loc.Get("death_damage_taken"), Mathf.FloorToInt(gm.DamageTaken),   Mathf.FloorToInt(gm.DamageTaken * 0.3f));
        }
    }
}
