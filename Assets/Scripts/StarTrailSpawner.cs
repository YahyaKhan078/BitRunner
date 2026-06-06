using UnityEngine;

/// <summary>
/// Periodically spawns a short arc or line of stars that reward a well-timed jump (Temple-Run
/// "coin line" feel). Distance-based so spacing stays consistent as speed ramps. Additive and
/// self-gated to active play — add to a scene object and assign the star prefab.
/// </summary>
public class StarTrailSpawner : MonoBehaviour
{
    public GameObject starPrefab;

    [Header("Spawn")]
    public float spawnX = 13f;
    public float groundY = -2.45f;

    [Header("Trail shape")]
    public int minStars = 4;
    public int maxStars = 7;
    public float starSpacing = 0.9f;     // world units between stars in the trail
    public float arcPeakHeight = 3.0f;   // top of the jump arc
    public float baseHeight = 1.2f;

    [Header("Timing")]
    public float minDelay = 6f;
    public float maxDelay = 11f;
    [Range(0f,1f)] public float arcChance = 0.6f;  // else a flat line

    float distanceToNext;
    float nextDelay;
    float spawnTimer;

    void OnEnable()
    {
        nextDelay = Random.Range(minDelay, maxDelay);
        spawnTimer = 0f;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || !gm.IsPlaying || starPrefab == null) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer < nextDelay) return;
        spawnTimer = 0f;
        nextDelay = Random.Range(minDelay, maxDelay);
        SpawnTrail();
    }

    void SpawnTrail()
    {
        int count = Random.Range(minStars, maxStars + 1);
        bool arc = Random.value <= arcChance;

        for (int i = 0; i < count; i++)
        {
            float x = spawnX + i * starSpacing;
            float y;
            if (arc)
            {
                float t = count > 1 ? (float)i / (count - 1) : 0.5f;
                // parabola peaking in the middle
                float h = 4f * t * (1f - t); // 0..1..0
                y = groundY + baseHeight + h * (arcPeakHeight - baseHeight);
            }
            else
            {
                y = groundY + baseHeight;
            }
            var pos = new Vector3(x, y, 0f);
            if (ObjectPool.Instance != null) ObjectPool.Instance.Get(starPrefab, pos, Quaternion.identity);
            else Instantiate(starPrefab, pos, Quaternion.identity);
        }
    }
}
