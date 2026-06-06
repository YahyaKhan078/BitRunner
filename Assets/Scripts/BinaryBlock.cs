using UnityEngine;
using TMPro;

public class BinaryBlock : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public TextMeshPro label;           // place a TMP text child on the prefab

    [Header("Colors")]
    public Color colorZero = new Color(0.9f, 0.2f, 0.2f); // red  for 0
    public Color colorOne  = new Color(0.2f, 0.5f, 1.0f); // blue for 1

    private int value;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (label == null)
            label = GetComponentInChildren<TextMeshPro>();
    }

    public void SetValue(int v)
    {
        value = v;

        if (spriteRenderer != null)
            spriteRenderer.color = (value == 0) ? colorZero : colorOne;

        if (label != null)
            label.text = value.ToString();
    }

    public int GetValue() => value;
}
