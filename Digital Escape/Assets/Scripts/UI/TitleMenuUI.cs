using UnityEngine;

/*
 
    TitleMenuUI : UI
    UI screen for the title menu.
 
 */

public class TitleMenuUI : MonoBehaviour
{
    // Variables
    [SerializeField] private string firstLevelSceneName = "Level1";

    // Start Button: Loads the first level
    public void StartButton()
    {
        if (!string.IsNullOrEmpty(firstLevelSceneName))
        {
            GameManager.instance?.LoadScene(firstLevelSceneName);
        }
        else
        {
            Debug.LogWarning("[TitleMenuUI] First level scene name is not set.");
        }
    }

    // Settings Button: To be implemented
    public void SettingsButton()
    {
        Debug.Log("[TitleMenuUI] Settings button pressed. (To be implemented)");
    }

    // Quit Button: Exits the game
    public void QuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
