using UnityEngine;
using TMPro;

/// <summary>
/// Simple shared tooltip panel. A button's <see cref="TooltipTrigger"/> calls Show/Hide. The
/// panel positions itself above the hovered element. Singleton; assign panel root + label.
/// </summary>
public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance { get; private set; }

    public RectTransform panel;
    public TMP_Text label;
    public Vector2 offset = new Vector2(0f, 90f);

    RectTransform canvasRect;

    void Awake()
    {
        Instance = this;
        var canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas != null ? canvas.transform as RectTransform : null;
        Hide();
    }

    public void Show(string text, RectTransform target)
    {
        if (panel == null || label == null) return;
        label.text = text;
        panel.gameObject.SetActive(true);

        if (target != null && canvasRect != null)
        {
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            Vector3 worldTop = (corners[1] + corners[2]) * 0.5f;
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, RectTransformUtility.WorldToScreenPoint(null, worldTop), null, out local);
            panel.anchoredPosition = local + offset;
        }
    }

    public void Hide()
    {
        if (panel != null) panel.gameObject.SetActive(false);
    }
}
