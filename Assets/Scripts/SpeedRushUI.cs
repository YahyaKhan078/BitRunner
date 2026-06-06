using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Drives the "SPEED RUSH" drop-down alert + depleting energy bar from the GameManager.
// Fully decoupled: it reads GameManager.Instance every frame, so nothing needs to be
// wired on the GameManager. Put this on a HUD object and assign the visual refs.
public class SpeedRushUI : MonoBehaviour
{
    [Header("Drop-down alert")]
    public RectTransform alertRect;     // container for the "SPEED RUSH" label
    public float shownY  = -70f;        // anchoredPosition.y when visible (just below top)
    public float hiddenY = 140f;        // anchoredPosition.y when hidden (above the screen)
    public float slideSpeed = 12f;

    [Header("Energy bar")]
    public Image energyBarFill;         // Image with Image Type = Filled; we set fillAmount
    public CanvasGroup barGroup;        // optional: fades the whole bar in/out

    [Header("Label (optional)")]
    public TMP_Text alertLabel;
    public string alertText = "SPEED RUSH";

    [Header("Seconds countdown (optional)")]
    public TMP_Text secondsLabel;       // shows remaining boost seconds, e.g. "3s"

    void Start()
    {
        if (alertLabel != null) alertLabel.text = alertText;
        if (alertRect  != null)
        {
            Vector2 p = alertRect.anchoredPosition; p.y = hiddenY;
            alertRect.anchoredPosition = p;        // start hidden
        }
        if (barGroup != null) barGroup.alpha = 0f;
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        bool  active    = gm != null && gm.SpeedRushActive;
        float remaining = gm != null ? gm.SpeedRushRemaining01 : 0f;

        if (alertRect != null)
        {
            Vector2 p = alertRect.anchoredPosition;
            p.y = Mathf.Lerp(p.y, active ? shownY : hiddenY, Time.deltaTime * slideSpeed);
            alertRect.anchoredPosition = p;
        }

        if (energyBarFill != null)
            energyBarFill.fillAmount = remaining;

        if (secondsLabel != null)
            secondsLabel.text = active
                ? Mathf.CeilToInt(gm.SpeedRushSecondsLeft) + "s"
                : "";

        if (barGroup != null)
            barGroup.alpha = Mathf.Lerp(barGroup.alpha, active ? 1f : 0f, Time.deltaTime * slideSpeed);
    }
}
