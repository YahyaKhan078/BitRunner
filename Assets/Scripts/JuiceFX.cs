using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Screen "juice": a brief full-screen color flash on pickups, death, and level-up. Lives on a
/// full-screen UI Image (alpha 0). Self-driven via GameEvents / CollectibleEvents — purely
/// additive. Runs on unscaled time so the death flash shows during the freeze.
/// </summary>
[RequireComponent(typeof(Image))]
public class JuiceFX : MonoBehaviour
{
    public Color starFlash = new Color(1f, 0.85f, 0.1f, 0.28f);
    public Color rushFlash = new Color(0.1f, 1f, 0.3f, 0.30f);
    public Color deathFlash = new Color(1f, 0.1f, 0.1f, 0.45f);
    public Color levelUpFlash = new Color(0.3f, 0.7f, 1f, 0.25f);
    public float flashDecay = 4f;

    Image img;
    Coroutine routine;

    void Awake()
    {
        img = GetComponent<Image>();
        img.raycastTarget = false;
        SetAlpha(0f);
    }

    void OnEnable()
    {
        CollectibleEvents.Collected += OnCollected;
        GameEvents.PlayerDied += OnDied;
        GameEvents.LeveledUp += OnLeveledUp;
    }

    void OnDisable()
    {
        CollectibleEvents.Collected -= OnCollected;
        GameEvents.PlayerDied -= OnDied;
        GameEvents.LeveledUp -= OnLeveledUp;
    }

    void OnCollected(Collectible.CollectibleType type, Vector3 pos, float amount)
        => Flash(type == Collectible.CollectibleType.SpeedRush ? rushFlash : starFlash);

    void OnDied() => Flash(deathFlash);
    void OnLeveledUp(int lvl) => Flash(levelUpFlash);

    void Flash(Color c)
    {
        if (img == null) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FlashRoutine(c));
    }

    IEnumerator FlashRoutine(Color c)
    {
        img.color = c;
        float a = c.a;
        while (a > 0.001f)
        {
            a = Mathf.MoveTowards(a, 0f, flashDecay * Time.unscaledDeltaTime);
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(0f);
        routine = null;
    }

    void SetAlpha(float a)
    {
        if (img == null) return;
        var col = img.color; col.a = a; img.color = col;
    }
}
