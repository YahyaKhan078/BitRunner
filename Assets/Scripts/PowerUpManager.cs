using UnityEngine;

/// <summary>
/// Central handler for timed/charge power-ups (Temple-Run style). Additive singleton:
///   • Magnet   — pulls nearby Star collectibles toward the player for a duration.
///   • Shield   — one stored charge; consumed instead of dying (PlayerController asks).
///   • ScoreX2  — doubles star pickup score for a duration (listens to CollectibleEvents).
/// Auto-creates itself on first access so scenes need no manual placement.
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    public enum PowerUpType { Magnet, Shield, ScoreX2 }

    public static PowerUpManager Instance { get; private set; }

    [Header("Magnet")]
    public float magnetRange = 6f;
    public float magnetPullSpeed = 12f;

    float magnetEndTime;
    float scoreX2EndTime;
    int shieldCharges;

    public bool MagnetActive  => Time.unscaledTime < magnetEndTime;
    public bool ScoreX2Active => Time.unscaledTime < scoreX2EndTime;
    public bool ShieldActive  => shieldCharges > 0;

    public float MagnetSecondsLeft  => Mathf.Max(0f, magnetEndTime - Time.unscaledTime);
    public float ScoreX2SecondsLeft => Mathf.Max(0f, scoreX2EndTime - Time.unscaledTime);
    public int   ShieldCharges      => shieldCharges;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance == null)
        {
            var go = new GameObject("PowerUpManager");
            go.AddComponent<PowerUpManager>();
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);   // survive scene loads (per-run state resets on RunStarted)
    }

    void OnEnable()  { CollectibleEvents.Collected += OnCollected; GameEvents.RunStarted += OnRunStarted; }
    void OnDisable() { CollectibleEvents.Collected -= OnCollected; GameEvents.RunStarted -= OnRunStarted; }

    void OnRunStarted(int level)
    {
        // power-ups don't carry across runs
        magnetEndTime = 0f; scoreX2EndTime = 0f; shieldCharges = 0;
    }

    void OnCollected(Collectible.CollectibleType type, Vector3 pos, float amount)
    {
        // Score x2: add the star value again so the pickup is effectively doubled.
        if (type == Collectible.CollectibleType.Star && ScoreX2Active && GameManager.Instance != null && amount > 0f)
            GameManager.Instance.AddScore(amount);
    }

    public void Activate(PowerUpType type, float duration)
    {
        switch (type)
        {
            case PowerUpType.Magnet:  magnetEndTime  = Time.unscaledTime + duration; break;
            case PowerUpType.ScoreX2: scoreX2EndTime = Time.unscaledTime + duration; break;
            case PowerUpType.Shield:  shieldCharges  = Mathf.Max(shieldCharges, 1);  break;
        }
    }

    /// <summary>Consumes a shield charge if available. Returns true if a death was prevented.</summary>
    public bool TryConsumeShield()
    {
        if (shieldCharges <= 0) return false;
        shieldCharges--;
        return true;
    }

    void Update()
    {
        if (!MagnetActive) return;
        var gm = GameManager.Instance;
        if (gm == null || !gm.IsPlaying || gm.player == null) return;

        Vector3 playerPos = gm.player.transform.position;
        // pull nearby stars toward the player
        foreach (var c in FindObjectsByType<Collectible>(FindObjectsInactive.Exclude))
        {
            if (c.collectibleType != Collectible.CollectibleType.Star) continue;
            float d = Vector3.Distance(c.transform.position, playerPos);
            if (d > magnetRange) continue;
            c.transform.position = Vector3.MoveTowards(c.transform.position, playerPos, magnetPullSpeed * Time.deltaTime);
        }
    }
}
