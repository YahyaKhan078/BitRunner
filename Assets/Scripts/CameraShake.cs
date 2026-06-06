using UnityEngine;
using System.Collections;

// Two jobs in one script:
// 1. Camera shake on player death (called by GameManager)
// 2. Parallax background scrolling (clouds scroll slower than ground)

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float defaultDuration  = 0.25f;
    public float defaultMagnitude = 0.15f;

    [Header("Parallax Backgrounds")]
    public Transform[] backgroundLayers;    // drag BG sprites here, index 0 = slowest
    public float[]     parallaxSpeeds;      // e.g. { 0.2f, 0.5f } — fraction of game speed

    private Vector3 originalPosition;
    private bool    isShaking = false;

    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        ScrollBackgrounds();
    }

    // ── Public: called by GameManager.OnPlayerDied ─────
    public void Shake(float duration = -1f, float magnitude = -1f)
    {
        if (duration  < 0) duration  = defaultDuration;
        if (magnitude < 0) magnitude = defaultMagnitude;

        if (isShaking) StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    // ── Shake coroutine ────────────────────────────────
    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = originalPosition.x + Random.Range(-1f, 1f) * magnitude;
            float y = originalPosition.y + Random.Range(-1f, 1f) * magnitude;
            transform.position = new Vector3(x, y, originalPosition.z);

            // unscaled so the death shake plays even though the game freezes
            // (Time.timeScale = 0) the moment the player dies
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        isShaking = false;
    }

    // ── Parallax scrolling ─────────────────────────────
    void ScrollBackgrounds()
    {
        if (backgroundLayers == null || parallaxSpeeds == null) return;

        float gameSpeed = GameManager.Instance != null
            ? GameManager.Instance.CurrentSpeed
            : 5f;

        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            if (backgroundLayers[i] == null) continue;

            float speed = i < parallaxSpeeds.Length ? parallaxSpeeds[i] : 0.3f;
            backgroundLayers[i].Translate(Vector3.left * gameSpeed * speed * Time.deltaTime);

            // wrap background when it scrolls too far left
            // assumes your background sprite is wide enough (use a wide repeating sprite)
            if (backgroundLayers[i].position.x < -25f)
            {
                Vector3 pos = backgroundLayers[i].position;
                backgroundLayers[i].position = new Vector3(25f, pos.y, pos.z);
            }
        }
    }
}
