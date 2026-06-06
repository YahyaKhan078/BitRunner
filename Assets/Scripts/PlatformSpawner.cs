using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns optional, landable platforms that scroll left with the world (via the platform
/// prefab's own ObstacleMover). Self-managing: it only spawns while the game is actively
/// playing, so it needs no wiring into GameManager. Platforms are bonus routes — often with
/// a star floating above them — never required to pass a level.
/// </summary>
public class PlatformSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject platformPrefab;

    [Header("Spawn")]
    public float spawnX = 14f;
    public float minY = -0.5f;   // jump-reachable height band
    public float maxY = 1.3f;

    [Header("Interval")]
    public float minInterval = 3.5f;
    public float maxInterval = 6.5f;
    [Range(0f, 1f)] public float spawnChance = 0.7f;

    [Header("Star on top (optional)")]
    public GameObject starPrefab;
    [Range(0f, 1f)] public float starOnTopChance = 0.6f;
    public float starHeightAbovePlatform = 1.1f;

    Coroutine routine;

    void OnEnable()
    {
        routine = StartCoroutine(SpawnLoop());
    }

    void OnDisable()
    {
        if (routine != null) { StopCoroutine(routine); routine = null; }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);

            if (platformPrefab == null) continue;
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) continue;
            if (Random.value > spawnChance) continue;

            SpawnOne();
        }
    }

    void SpawnOne()
    {
        float y = Random.Range(minY, maxY);
        // z = 0.1 so the player (z = 0) renders in front of the platform while standing on it.
        Instantiate(platformPrefab, new Vector3(spawnX, y, 0.1f), Quaternion.identity);

        if (starPrefab != null && Random.value <= starOnTopChance)
        {
            Vector3 starPos = new Vector3(spawnX, y + starHeightAbovePlatform, 0f);
            Instantiate(starPrefab, starPos, Quaternion.identity);
        }
    }
}
