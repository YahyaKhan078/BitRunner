using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Cosmetic loading bar shown on the IdlePanel.
// Fills over fillDuration seconds, then swaps "Hold on..." for "Press to Start".
// Does NOT gate GameManager.StartGame() — input is accepted the moment the bar starts.
// Call ResetBar() if you need to replay the fill animation (e.g. after a restart).
public class LoadingBar : MonoBehaviour
{
    [Header("UI References")]
    public Image     fillImage;            // Image Type = Filled, Fill Method = Horizontal
    public TMP_Text  loadingLabel;         // "Hold on…" text, shown while filling
    public TMP_Text  pressToStartLabel;    // "Press to Start" text, shown after fill

    [Header("Timing")]
    public float fillDuration = 1.2f;     // seconds to fill the bar completely

    // ── private state ──────────────────────────────────────
    private float elapsed;
    private bool  finished;

    // ── Unity lifecycle ────────────────────────────────────
    void OnEnable()
    {
        // Replay the animation every time the IdlePanel is shown
        // (GameManager calls SetActive(true) on it each run or on startup).
        ResetBar();
    }

    void Update()
    {
        if (finished) return;

        elapsed += Time.unscaledDeltaTime;   // unscaled: safe at timeScale = 0

        float t = fillDuration > 0f ? Mathf.Clamp01(elapsed / fillDuration) : 1f;

        if (fillImage != null)
            fillImage.fillAmount = t;

        if (t >= 1f)
            OnFillComplete();
    }

    // ── Public API ─────────────────────────────────────────

    /// <summary>Resets fill to 0 and replays the animation.</summary>
    public void ResetBar()
    {
        elapsed  = 0f;
        finished = false;

        if (fillImage        != null) fillImage.fillAmount = 0f;
        if (loadingLabel     != null) loadingLabel.gameObject.SetActive(true);
        if (pressToStartLabel != null) pressToStartLabel.gameObject.SetActive(false);
    }

    // ── private ────────────────────────────────────────────
    void OnFillComplete()
    {
        finished = true;

        if (fillImage        != null) fillImage.fillAmount = 1f;
        if (loadingLabel     != null) loadingLabel.gameObject.SetActive(false);
        if (pressToStartLabel != null) pressToStartLabel.gameObject.SetActive(true);
    }
}
