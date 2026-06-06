using UnityEngine;
using TMPro;

/// <summary>
/// Rewards collecting stars in quick succession. Each star picked up within
/// <see cref="comboWindow"/> seconds of the previous one extends the chain; the combo
/// grants escalating bonus score and shows a "COMBO xN" popup that fades when the chain
/// breaks. Fully additive — it listens to <see cref="CollectibleEvents"/> and feeds bonus
/// score through <c>GameManager.AddScore</c> without touching any existing script.
/// </summary>
public class ComboMeter : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text comboLabel;     // "COMBO x3"
    public CanvasGroup group;       // fades the label in/out

    [Header("Tuning")]
    public float comboWindow = 2.5f;   // seconds the chain stays alive between stars
    public int minComboToShow = 2;     // only show/bonus from this combo upward
    public float bonusPerStep = 25f;   // bonus score = bonusPerStep * (combo - 1)
    public float fadeSpeed = 6f;
    public float popScale = 1.25f;     // brief scale-pop on each increment

    int combo;
    float lastStarTime;
    bool active;
    RectTransform labelRect;
    float popT;

    void Awake()
    {
        if (comboLabel != null) labelRect = comboLabel.rectTransform;
    }

    void OnEnable()
    {
        combo = 0; active = false; popT = 0f;
        if (group != null) group.alpha = 0f;
        CollectibleEvents.Collected += OnCollected;
    }

    void OnDisable()
    {
        CollectibleEvents.Collected -= OnCollected;
    }

    void OnCollected(Collectible.CollectibleType type, Vector3 worldPos, float amount)
    {
        if (type != Collectible.CollectibleType.Star) return;

        float now = Time.unscaledTime;
        if (active && now - lastStarTime <= comboWindow) combo++;
        else combo = 1;
        lastStarTime = now;
        active = true;

        if (combo >= minComboToShow)
        {
            float bonus = bonusPerStep * (combo - 1);
            var gm = GameManager.Instance;
            if (gm != null && bonus > 0f) gm.AddScore(bonus);
            if (comboLabel != null) comboLabel.text = "COMBO x" + combo;
            popT = 1f; // trigger scale-pop
        }
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm != null && !gm.IsPlaying) { combo = 0; active = false; }

        if (active && Time.unscaledTime - lastStarTime > comboWindow)
        {
            combo = 0;
            active = false;
        }

        bool show = active && combo >= minComboToShow;
        float target = show ? 1f : 0f;
        if (group != null)
            group.alpha = Mathf.MoveTowards(group.alpha, target, fadeSpeed * Time.unscaledDeltaTime);

        // scale-pop decay
        if (labelRect != null)
        {
            if (popT > 0f) popT = Mathf.MoveTowards(popT, 0f, Time.unscaledDeltaTime * 5f);
            float s = 1f + (popScale - 1f) * popT;
            labelRect.localScale = new Vector3(s, s, 1f);
        }
    }
}
