using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string     gameSceneName        = "SampleScene";
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject characterSelectPanel;

    private void Start()
    {
        AudioManager.Instance?.PlayUIMusic();
    }

    // Transition from main panel to character selection
    public void PlayGame()
    {
        if (characterSelectPanel != null) characterSelectPanel.SetActive(true);
        if (mainPanel != null)            mainPanel.SetActive(false);
    }

    // Called from character selection buttons (0=Knight, 1=Archer, 2=Mage)
    public void SelectCharacterAndStart(int characterIndex)
    {
        PlayerPrefs.SetInt("SelectedCharacter", characterIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }

    // Return from character selection to main panel
    public void BackToMain()
    {
        if (mainPanel != null)            mainPanel.SetActive(true);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (SettingsPanel.Instance != null) SettingsPanel.Instance.Open();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
