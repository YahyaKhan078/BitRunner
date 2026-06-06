using UnityEngine;
using UnityEngine.SceneManagement;

// Put this on a MainMenu object and wire your UI Buttons' OnClick events to these
// methods (PlayScene takes the level scene name, e.g. "Level1").
public class MainMenu : MonoBehaviour
{
    public string firstLevelScene = "Level1";

    void Awake()
    {
        Time.timeScale = 1f;   // ensure time is running on the menu (e.g. after a freeze)
    }

    public void PlayScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // Convenient no-arg hook for a "Play" button.
    public void PlayFirstLevel() => PlayScene(firstLevelScene);

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
