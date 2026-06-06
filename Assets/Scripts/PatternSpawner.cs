using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays handcrafted <see cref="ObstaclePattern"/> sequences instead of pure random spawning
/// (Geometry-Dash style). Distance-based: it advances along the level using
/// GameManager.CurrentSpeed so spacing stays consistent as speed ramps. Additive — it reuses
/// the same prefabs as ObstacleSpawner. Enable this OR ObstacleSpawner per level, not both.
/// </summary>
public class PatternSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject pipePrefab;
    public GameObject blockPrefab;
    public GameObject laserPrefab;
    public GameObject spikePrefab;
    public GameObject jumpPadPrefab;
    public GameObject sawPrefab;
    public GameObject starPrefab;
    public GameObject speedRushPrefab;

    [Header("Patterns")]
    public List<ObstaclePattern> patterns = new List<ObstaclePattern>();
    [Tooltip("World gap inserted between two consecutive patterns.")]
    public float betweenPatternGap = 6f;

    [Header("Spawn")]
    public float spawnX = 13f;
    public float groundY = -2.45f;
    public float laserHeight = 1.4f;
    [Tooltip("Difficulty tier rises with score/time; filters which patterns can play.")]
    public int startTier = 0;

    bool active;
    float distanceToNext;       // world units until the next element spawns
    ObstaclePattern current;
    int elementIndex;
    int tier;

    public void StartSpawning()
    {
        active = true;
        tier = startTier;
        current = null;
        elementIndex = 0;
        distanceToNext = 2f;
    }

    public void StopSpawning() => active = false;

    public void ClearAll()
    {
        foreach (var obs in FindObjectsByType<ObstacleMover>(FindObjectsInactive.Exclude))
            Destroy(obs.gameObject);
    }

    void Update()
    {
        if (!active) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        float speed = GameManager.Instance.CurrentSpeed;
        distanceToNext -= speed * Time.deltaTime;
        if (distanceToNext > 0f) return;

        if (current == null || elementIndex >= current.elements.Length)
        {
            PickNextPattern();
            distanceToNext = betweenPatternGap;
            return;
        }

        var el = current.elements[elementIndex];
        SpawnElement(el);
        elementIndex++;
        distanceToNext = Mathf.Max(0.5f, el.gapAfter);
    }

    void PickNextPattern()
    {
        // tier scales with current speed intensity
        tier = startTier + Mathf.FloorToInt(GameManager.Instance.SpeedIntensity01 * 3f);
        var eligible = new List<ObstaclePattern>();
        foreach (var p in patterns)
            if (p != null && p.elements != null && p.elements.Length > 0 && p.minTier <= tier)
                eligible.Add(p);

        if (eligible.Count == 0) { current = null; elementIndex = 0; return; }
        current = eligible[Random.Range(0, eligible.Count)];
        elementIndex = 0;
    }

    void SpawnElement(ObstaclePattern.Element el)
    {
        GameObject prefab = null;
        float y = groundY;
        switch (el.kind)
        {
            case ObstaclePattern.ElementKind.Pipe:      prefab = pipePrefab; break;
            case ObstaclePattern.ElementKind.Block:     prefab = blockPrefab; break;
            case ObstaclePattern.ElementKind.Laser:     prefab = laserPrefab; y = groundY + laserHeight; break;
            case ObstaclePattern.ElementKind.Spike:     prefab = spikePrefab; break;
            case ObstaclePattern.ElementKind.JumpPad:   prefab = jumpPadPrefab; break;
            case ObstaclePattern.ElementKind.Saw:       prefab = sawPrefab; y = groundY + Mathf.Max(0.3f, el.yOffset); break;
            case ObstaclePattern.ElementKind.Star:      prefab = starPrefab; y = groundY + (el.yOffset > 0 ? el.yOffset : 1.5f); break;
            case ObstaclePattern.ElementKind.SpeedRush: prefab = speedRushPrefab; y = groundY + (el.yOffset > 0 ? el.yOffset : 1.5f); break;
            case ObstaclePattern.ElementKind.Gap:       return; // empty space
        }
        if (prefab == null) return;

        var pos = new Vector3(spawnX, y, 0f);
        var go = ObjectPool.Instance != null ? ObjectPool.Instance.Get(prefab, pos, Quaternion.identity)
                                             : Instantiate(prefab, pos, Quaternion.identity);
        var block = go.GetComponent<BinaryBlock>();
        if (block != null) block.SetValue(Random.value > 0.5f ? 1 : 0);
    }
}
