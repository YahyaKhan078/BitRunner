using UnityEngine;
using TMPro;

/// <summary>
/// Geometry-Dash style "Attempt N" counter. Increments each time a run starts on this level
/// and persists per level via PlayerPrefs. Self-driven from GameEvents.RunStarted — additive.
/// </summary>
public class AttemptCounter : MonoBehaviour
{
    public TMP_Text label;
    public string prefix = "ATTEMPT ";

    void OnEnable()  { GameEvents.RunStarted += OnRunStarted; }
    void OnDisable() { GameEvents.RunStarted -= OnRunStarted; }

    void OnRunStarted(int level)
    {
        string key = "Attempts_" + level;
        int n = PlayerPrefs.GetInt(key, 0) + 1;
        PlayerPrefs.SetInt(key, n);
        PlayerPrefs.Save();
        if (label != null) label.text = prefix + n;
    }
}
