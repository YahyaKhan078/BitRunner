using UnityEngine;

/// <summary>
/// Player skin system (Geometry-Dash / Temple-Run style cosmetics). Skins are color schemes
/// applied to the player's SpriteRenderer — no extra art required. Skins are purchased with the
/// lifetime total-stars currency and persisted. Auto-applies the selected skin when a run
/// starts. Additive — auto-bootstraps; no existing scripts modified.
/// </summary>
public class SkinManager : MonoBehaviour
{
    [System.Serializable]
    public struct Skin
    {
        public string name;
        public Color color;
        public int cost;   // 0 = free/default
    }

    public static SkinManager Instance { get; private set; }

    public Skin[] skins = new Skin[]
    {
        new Skin { name = "Classic",  color = Color.white,                       cost = 0 },
        new Skin { name = "Cyan",     color = new Color(0.3f, 0.95f, 1f),        cost = 20 },
        new Skin { name = "Magenta",  color = new Color(1f, 0.35f, 0.9f),        cost = 30 },
        new Skin { name = "Gold",     color = new Color(1f, 0.84f, 0.2f),        cost = 40 },
        new Skin { name = "Toxic",    color = new Color(0.5f, 1f, 0.3f),         cost = 50 },
        new Skin { name = "Inferno",  color = new Color(1f, 0.45f, 0.15f),       cost = 60 },
    };

    const string OwnedKeyPrefix = "SkinOwned_";
    const string SelectedKey = "SkinSelected";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance == null)
            new GameObject("SkinManager").AddComponent<SkinManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // default skin always owned
        PlayerPrefs.SetInt(OwnedKeyPrefix + 0, 1);
    }

    void OnEnable()  { GameEvents.RunStarted += OnRunStarted; }
    void OnDisable() { GameEvents.RunStarted -= OnRunStarted; }

    void OnRunStarted(int level) => ApplySelected();

    public int SelectedIndex => Mathf.Clamp(PlayerPrefs.GetInt(SelectedKey, 0), 0, skins.Length - 1);

    public bool IsOwned(int index) => index == 0 || PlayerPrefs.GetInt(OwnedKeyPrefix + index, 0) == 1;

    public bool TryBuy(int index)
    {
        if (index < 0 || index >= skins.Length) return false;
        if (IsOwned(index)) return true;
        if (!LevelProgress.SpendTotalStars(skins[index].cost)) return false;
        PlayerPrefs.SetInt(OwnedKeyPrefix + index, 1);
        PlayerPrefs.Save();
        return true;
    }

    public void Select(int index)
    {
        if (!IsOwned(index)) return;
        PlayerPrefs.SetInt(SelectedKey, index);
        PlayerPrefs.Save();
        ApplySelected();
    }

    public void ApplySelected()
    {
        var gm = GameManager.Instance;
        PlayerController pc = gm != null ? gm.player : Object.FindAnyObjectByType<PlayerController>();
        if (pc == null) return;
        var sr = pc.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = skins[SelectedIndex].color;
    }
}
