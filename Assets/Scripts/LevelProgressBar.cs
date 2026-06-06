using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Geometry-Dash style progress bar. Fills toward the level's star goal and slides a marker
/// icon along the bar. Self-driven from <see cref="GameManager.Instance"/> — only assign the
/// visual refs. If the level has no star goal (endless), it hides itself.
/// </summary>
public class LevelProgressBar : MonoBehaviour
{
    [Header("Refs")]
    public Image fill;              // a Filled (Horizontal) Image
    public RectTransform marker;    // optional icon that rides the fill edge
    public RectTransform track;     // the bar's RectTransform (for marker positioning)
    public CanvasGroup group;       // optional, to hide when no goal

    [Header("Feel")]
    public float smooth = 8f;

    float shown;

    void OnEnable() { shown = 0f; }

    void Update()
    {
        var gm = GameManager.Instance;
        bool hasGoal = gm != null && gm.StarsToComplete > 0;

        if (group != null) group.alpha = Mathf.MoveTowards(group.alpha, hasGoal ? 1f : 0f, 4f * Time.unscaledDeltaTime);
        if (!hasGoal) return;

        float target = Mathf.Clamp01((float)gm.StarsCollected / Mathf.Max(1, gm.StarsToComplete));
        shown = Mathf.MoveTowards(shown, target, smooth * Time.unscaledDeltaTime);

        if (fill != null) fill.fillAmount = shown;

        if (marker != null && track != null)
        {
            float w = track.rect.width;
            var p = marker.anchoredPosition;
            p.x = -w * 0.5f + w * shown;   // assumes track pivot/anchor centered
            marker.anchoredPosition = p;
        }
    }
}
