using UnityEngine;
using TMPro;

// Lives on the HUDPanel. Listens to CollectibleEvents and:
//   • keeps a running Star counter (resets each run when the HUD is enabled)
//   • spawns a floating "+N" / "SPEED!" pop that rises and fades
// Self-contained; reads GameManager.Instance only indirectly via the event.
public class HudExtras : MonoBehaviour
{
    [Header("Star Counter")]
    public TMP_Text starCountText;          // shows the collected-star count this run
    public string starPrefix = "x ";

    [Header("Score Pop")]
    public RectTransform popParent;         // a Canvas/RectTransform to parent pops under (defaults to this)
    public TMP_FontAsset popFont;
    public float popFontSize = 40f;
    public float popRisePixels = 90f;
    public float popLifetime = 0.9f;
    public Color starPopColor = new Color(1f, 0.85f, 0.1f, 1f);
    public Color rushPopColor = new Color(0.1f, 1f, 0.3f, 1f);

    private int starCount;
    private Canvas canvas;
    private RectTransform canvasRect;
    private Camera cam;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas != null ? canvas.transform as RectTransform : null;
        if (popParent == null) popParent = canvasRect != null ? canvasRect : (transform as RectTransform);
    }

    void OnEnable()
    {
        cam = Camera.main;
        starCount = 0;
        UpdateCounter();
        CollectibleEvents.Collected += OnCollected;
    }

    void OnDisable()
    {
        CollectibleEvents.Collected -= OnCollected;
    }

    void OnCollected(Collectible.CollectibleType type, Vector3 worldPos, float amount)
    {
        if (type == Collectible.CollectibleType.Star)
        {
            starCount++;
            UpdateCounter();
            SpawnPop("+" + Mathf.RoundToInt(amount), starPopColor, worldPos);
        }
        else
        {
            SpawnPop("SPEED!", rushPopColor, worldPos);
        }
    }

    void UpdateCounter()
    {
        if (starCountText != null) starCountText.text = starPrefix + starCount;
    }

    void SpawnPop(string text, Color color, Vector3 worldPos)
    {
        if (popParent == null) return;
        if (cam == null) cam = Camera.main;

        var go = new GameObject("ScorePop", typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(popParent, false);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.font = popFont;
        tmp.fontSize = popFontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;

        // place at the pickup's screen position, converted into the parent rect's local space
        Vector2 local = Vector2.zero;
        if (cam != null && canvas != null)
        {
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
            Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(popParent, screen, uiCam, out local);
        }
        rt.anchoredPosition = local;
        rt.sizeDelta = new Vector2(220, 70);

        var anim = go.AddComponent<ScorePopAnim>();
        anim.Init(tmp, rt, popRisePixels, popLifetime);
    }
}

// Small self-destroying animator for a floating score pop (unscaled time so pops still
// animate if the world is briefly frozen).
public class ScorePopAnim : MonoBehaviour
{
    private TMP_Text tmp;
    private RectTransform rt;
    private float rise;
    private float life;
    private float t;
    private Vector2 start;

    public void Init(TMP_Text text, RectTransform rect, float risePixels, float lifetime)
    {
        tmp = text; rt = rect; rise = risePixels; life = Mathf.Max(0.01f, lifetime);
        start = rt.anchoredPosition;
    }

    void Update()
    {
        t += Time.unscaledDeltaTime;
        float k = Mathf.Clamp01(t / life);
        if (rt != null) rt.anchoredPosition = start + new Vector2(0f, rise * k);
        if (tmp != null)
        {
            var c = tmp.color; c.a = 1f - k; tmp.color = c;
        }
        if (k >= 1f) Destroy(gameObject);
    }
}
