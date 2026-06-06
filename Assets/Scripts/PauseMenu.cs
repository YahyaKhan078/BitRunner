using UnityEngine;
using UnityEngine.SceneManagement;

// Esc-driven pause menu. Additive: only pauses while a run is active (GameManager.IsPlaying),
// so it never collides with the idle/loading or game-over (death-freeze) states.
// While paused it sets Time.timeScale = 0, pauses audio, and disables the PlayerController
// so queued jump/crouch input can't leak into the resumed run.
public class PauseMenu : MonoBehaviour
{
    [Header("Refs")]
    public GameObject pausePanel;          // overlay shown while paused (starts inactive)
    public int mainMenuSceneIndex = 0;     // build index of the main menu

    [Header("Input")]
    public KeyCode pauseKey = KeyCode.Escape;

    public bool IsPaused { get; private set; }

    private PlayerController player;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (IsPaused) Resume();
            else TryPause();
        }
    }

    void TryPause()
    {
        var gm = GameManager.Instance;
        if (gm == null || !gm.IsPlaying) return;   // only pause during an active run

        IsPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (player == null && gm.player != null) player = gm.player;
        if (player != null) player.enabled = false; // block jump/crouch input while paused

        if (pausePanel != null) pausePanel.SetActive(true);
    }

    // Wired to the Resume button (and the Esc toggle).
    public void Resume()
    {
        IsPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        AudioListener.pause = false;
        if (player != null) player.enabled = true;
        Time.timeScale = 1f;
    }

    // Wired to the "Main Menu" button.
    public void QuitToMenu()
    {
        IsPaused = false;
        AudioListener.pause = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneIndex);
    }
}
