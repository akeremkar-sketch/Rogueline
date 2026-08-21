using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isGameOver = false;
    private bool isPaused = false;

    public int   KillCount    { get; private set; } = 0;
    public float Distance     { get; private set; } = 0f;
    public float DamageDealt  { get; private set; } = 0f;
    public float DamageTaken  { get; private set; } = 0f;
    public int   CoinsGained  { get; private set; } = 0;
    public int   LevelsGained { get; private set; } = 0;

    [Header("Enemy Scaling")]
    [Tooltip("Every X meters, enemy health and damage multiplier increases by 1.0x")]
    [SerializeField] private float scalingDistance = 500f;
    [Tooltip("Maximum enemy scaling multiplier")]
    [SerializeField] private float maxScalingMultiplier = 5f;

    public float EnemyScalingMultiplier => Mathf.Min(1f + Distance / scalingDistance, maxScalingMultiplier);
    public float ScalingDistance        => scalingDistance;

    [Header("Score Multipliers")]
    [SerializeField] private float distanceMultiplier = 10f;
    [SerializeField] private float killMultiplier      = 100f;
    [SerializeField] private float damageDealtMult     = 0.5f;
    [SerializeField] private float levelMultiplier     = 300f;
    [SerializeField] private float coinMultiplier      = 25f;
    [SerializeField] private float damageTakenPenalty  = 0.3f;

    private float startY;
    private Transform player;

    // Fired whenever an enemy dies — VampireAbility subscribes to fill blood meter
    public event Action OnEnemyKilled;

    public void RegisterKill()       { KillCount++; OnEnemyKilled?.Invoke(); }
    public void RegisterDamageDealt(float dmg) => DamageDealt += dmg;
    public void RegisterDamageTaken(float dmg) => DamageTaken += dmg;
    public void RegisterCoin(int amount)       => CoinsGained += amount;
    public void RegisterLevelUp()    => LevelsGained++;

    public int CalculateScore()
    {
        int score = 0;
        score += Mathf.FloorToInt(Distance    * distanceMultiplier);
        score += Mathf.FloorToInt(KillCount   * killMultiplier);
        score += Mathf.FloorToInt(DamageDealt * damageDealtMult);
        score += Mathf.FloorToInt(LevelsGained * levelMultiplier);
        score += Mathf.FloorToInt(CoinsGained  * coinMultiplier);
        score -= Mathf.FloorToInt(DamageTaken  * damageTakenPenalty);
        return Mathf.Max(0, score);
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Disable physical collision between Player and Enemy (Vampire Survivors style)
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer  = LayerMask.NameToLayer("Enemy");
        if (playerLayer >= 0 && enemyLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
    }

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            player = PlayerController.Instance.transform;
            startY = player.position.y;
        }

        AudioManager.Instance?.PlayGameplayMusic();

        // Pre-compile all shaders so they don't hitch on first use mid-game
        Shader.WarmupAllShaders();

        // Force a GC collect now (while loading screen is up) to flush init garbage
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
    }

    private void Update()
    {
        if (isGameOver) return;

        // Distance: maximum progress the player has made from the start
        if (player != null)
            Distance = Mathf.Max(Distance, player.position.y - startY);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool otherPanelOpen = (UpgradeUI.Instance      != null && UpgradeUI.Instance.IsShowing)
                               || (StatsPanel.Instance    != null && StatsPanel.Instance.IsOpen)
                               || (ShopUI.Instance        != null && ShopUI.Instance.IsOpen)
                               || (ChestUI.Instance       != null && ChestUI.Instance.IsOpen)
                               || (SettingsPanel.Instance != null && SettingsPanel.Instance.IsOpen);

            if (otherPanelOpen) return;

            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isGameOver) return;
        isPaused = true;
        Time.timeScale = 0f;
        UIManager.Instance.ShowPauseMenu();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        UIManager.Instance.HidePauseMenu();
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Time.timeScale = 0f;
        UIManager.Instance.ShowDeathScreen();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
