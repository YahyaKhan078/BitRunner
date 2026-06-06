using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gates the Endless mode button: only interactable once the campaign is finished (Level 3
/// completed). Shows a locked label otherwise. Additive — sits on the Endless button.
/// </summary>
public class EndlessGate : MonoBehaviour
{
    public Button button;
    public TMP_Text label;
    public int requiredLevelCompleted = 3;

    void Start()
    {
        bool unlocked = LevelProgress.IsCompleted(requiredLevelCompleted);
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.interactable = unlocked;
        if (label != null) label.text = unlocked ? "ENDLESS" : "ENDLESS (LOCKED)";
    }
}
