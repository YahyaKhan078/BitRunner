using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// One per level card on the Level Select screen. Shows locked/unlocked/completed state
// and loads the level scene when clicked (only if unlocked).
// Wire the card's Button OnClick → TryLoad().
public class LevelSelectButton : MonoBehaviour
{
    public int levelNumber = 1;
    public string sceneName = "Level1";

    [Header("Optional visuals")]
    public GameObject lockedOverlay;   // shown over the card when locked (e.g. a padlock)
    public Button button;              // the clickable button (auto-found if null)
    public TMP_Text statusLabel;       // optional: "LOCKED" / "COMPLETED" / "PLAY"
    public TMP_Text ratingLabel;       // optional: shows ★★☆ earned rating + best score

    void Start()
    {
        if (button == null) button = GetComponent<Button>();

        bool unlocked = LevelProgress.IsUnlocked(levelNumber);
        if (button != null) button.interactable = unlocked;
        if (lockedOverlay != null) lockedOverlay.SetActive(!unlocked);
        if (statusLabel != null)
            statusLabel.text = !unlocked ? "LOCKED"
                             : (LevelProgress.IsCompleted(levelNumber) ? "COMPLETED" : "PLAY");

        if (ratingLabel != null)
        {
            int stars = LevelProgress.GetStars(levelNumber);
            string row = "";
            for (int i = 0; i < 3; i++)
                row += i < stars ? "<color=#FFD54A>*</color>" : "<color=#444444>*</color>";
            int best = LevelProgress.GetBestScore(levelNumber);
            ratingLabel.text = row + (best > 0 ? "\nBest " + best : "");
        }
    }

    public void TryLoad()
    {
        if (!LevelProgress.IsUnlocked(levelNumber)) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
