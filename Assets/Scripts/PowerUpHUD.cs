using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Small HUD strip showing active power-ups and their remaining time (Temple-Run style).
/// Self-driven from <see cref="PowerUpManager.Instance"/>. Assign three icon/label pairs.
/// </summary>
public class PowerUpHUD : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public GameObject root;   // container toggled on/off
        public TMP_Text label;    // seconds / charges
    }

    public Slot magnet;
    public Slot shield;
    public Slot scoreX2;

    void Update()
    {
        var pm = PowerUpManager.Instance;
        if (pm == null) { Hide(magnet); Hide(shield); Hide(scoreX2); return; }

        Set(magnet, pm.MagnetActive,  Mathf.CeilToInt(pm.MagnetSecondsLeft) + "s");
        Set(shield, pm.ShieldActive,  "x" + pm.ShieldCharges);
        Set(scoreX2, pm.ScoreX2Active, Mathf.CeilToInt(pm.ScoreX2SecondsLeft) + "s");
    }

    void Set(Slot s, bool active, string text)
    {
        if (s == null || s.root == null) return;
        if (s.root.activeSelf != active) s.root.SetActive(active);
        if (active && s.label != null) s.label.text = text;
    }

    void Hide(Slot s)
    {
        if (s != null && s.root != null && s.root.activeSelf) s.root.SetActive(false);
    }
}
