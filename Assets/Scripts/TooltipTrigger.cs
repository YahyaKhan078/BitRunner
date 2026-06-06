using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Put on a UI button to show an info tooltip on hover (and on click, useful for locked buttons
/// or touch). Drives the shared <see cref="Tooltip"/> singleton. Additive.
/// </summary>
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [TextArea] public string message;

    public void OnPointerEnter(PointerEventData e)
    {
        if (Tooltip.Instance != null) Tooltip.Instance.Show(message, transform as RectTransform);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (Tooltip.Instance != null) Tooltip.Instance.Hide();
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (Tooltip.Instance != null) Tooltip.Instance.Show(message, transform as RectTransform);
    }
}
