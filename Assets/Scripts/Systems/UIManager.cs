using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject deathScreen;
    [SerializeField] private GameObject pauseMenu;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (deathScreen != null) deathScreen.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
    }

    public void ShowDeathScreen()
    {
        if (deathScreen != null) deathScreen.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        if (pauseMenu != null) pauseMenu.SetActive(true);
    }

    public void HidePauseMenu()
    {
        if (pauseMenu != null) pauseMenu.SetActive(false);
    }

    public void OpenSettings()
    {
        if (SettingsPanel.Instance != null) SettingsPanel.Instance.Open();
    }
}
