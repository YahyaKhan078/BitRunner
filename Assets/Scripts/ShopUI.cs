using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Skin shop panel controller. Lists skins as a row of swatch buttons; clicking buys (if
/// affordable) then selects. Shows the lifetime total-stars balance. Self-contained — assign a
/// content RectTransform to hold the swatches, a balance label, and a swatch button prefab-less
/// builder runs at enable.
/// </summary>
public class ShopUI : MonoBehaviour
{
    public RectTransform content;     // horizontal container for swatches
    public TMP_Text balanceLabel;
    public TMP_FontAsset font;

    readonly System.Collections.Generic.List<GameObject> built = new System.Collections.Generic.List<GameObject>();

    void OnEnable()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        foreach (var go in built) if (go != null) Destroy(go);
        built.Clear();

        var sm = SkinManager.Instance;
        if (sm == null || content == null) return;

        float x = 0f;
        float step = 150f;
        for (int i = 0; i < sm.skins.Length; i++)
        {
            int index = i;
            var skin = sm.skins[i];

            var cell = new GameObject("Skin_" + skin.name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            cell.transform.SetParent(content, false);
            var crt = cell.GetComponent<RectTransform>();
            crt.anchorMin = crt.anchorMax = new Vector2(0f, 0.5f); crt.pivot = new Vector2(0f, 0.5f);
            crt.anchoredPosition = new Vector2(x, 0); crt.sizeDelta = new Vector2(130, 150);
            cell.GetComponent<Image>().color = skin.color;
            var outline = cell.AddComponent<Outline>(); outline.effectColor = new Color(0,0,0,0.6f); outline.effectDistance = new Vector2(3,-3);

            var lblGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            lblGO.transform.SetParent(cell.transform, false);
            var lrt = lblGO.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = new Vector2(2,2); lrt.offsetMax = new Vector2(-2,-2);
            var lbl = lblGO.GetComponent<TextMeshProUGUI>();
            lbl.fontSize = 16; lbl.alignment = TextAlignmentOptions.Bottom; lbl.color = Color.black;
            if (font != null) lbl.font = font;
            lbl.text = StatusText(sm, index, skin);

            cell.GetComponent<Button>().onClick.AddListener(() => OnClick(index));
            built.Add(cell);
            x += step;
        }
        content.sizeDelta = new Vector2(x, content.sizeDelta.y);
        UpdateBalance();
    }

    string StatusText(SkinManager sm, int index, SkinManager.Skin skin)
    {
        if (sm.SelectedIndex == index) return skin.name + "\n[USED]";
        if (sm.IsOwned(index)) return skin.name + "\n[OWNED]";
        return skin.name + "\n" + skin.cost + "*";
    }

    void OnClick(int index)
    {
        var sm = SkinManager.Instance;
        if (sm == null) return;
        if (sm.IsOwned(index)) sm.Select(index);
        else if (sm.TryBuy(index)) sm.Select(index);
        Rebuild();
    }

    void UpdateBalance()
    {
        if (balanceLabel != null)
            balanceLabel.text = "STARS: " + LevelProgress.GetTotalStars();
    }
}
