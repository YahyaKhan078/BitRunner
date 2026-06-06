using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    public GameObject pipeObstaclePrefab;   // tall block — JUMP over
    public GameObject blockObstaclePrefab;  // binary 0/1 block — JUMP over
    public GameObject laserObstaclePrefab;  // low red laser beam — CROUCH under (optional)
    public GameObject spikeObstaclePrefab;  // neon spikes — JUMP over (optional)

    [Header("Obstacle Weights (relative)")]
    [Range(0f, 1f)] public float pipeWeight  = 0.4f;
    [Range(0f, 1f)] public float blockWeight = 0.35f;
    [Range(0f, 1f)] public float laserWeight = 0.25f;
    [Range(0f, 1f)] public float spikeWeight = 0.25f;
    public float laserHeight = 1.5f;        // Y above groundY for the laser beam

    [Header("Spawn Settings")]
    public float spawnX = 12f;              // how far right of camera to spawn
    public float groundY = -2.5f;          // Y position obstacles sit on

    [Header("Interval (seconds)")]
    public float minInterval = 0.8f;
    public float maxInterval = 1.6f;

    [Header("Collectibles (optional)")]
    // Leave empty to disable. Each must have a Collectible + ObstacleMover.
    public GameObject[] collectiblePrefabs;
    [Range(0f, 1f)] public float collectibleChance = 0.3f; // chance per spawn tick
    // height ABOVE groundY (positive), within jump reach
    public float collectibleMinY = 0.5f;
    public float collectibleMaxY = 3f;

    // ── private ────────────────────────────────────────
    private float timer;
    private float nextSpawnTime;
    private bool isSpawning = false;

    // ── Unity lifecycle ────────────────────────────────
    void Start()
    {
        SetNextSpawnTime();
    }

    void Update()
    {
        if (!isSpawning) return;

        timer += Time.deltaTime;
        if (timer >= nextSpawnTime)
        {
            SpawnObstacle();
            timer = 0f;
            SetNextSpawnTime();
        }
    }

    // ── public API called by GameManager ───────────────
    public void StartSpawning()
    {
        isSpawning = true;
        timer = 0f;
        SetNextSpawnTime();
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public void ClearAllObstacles()
    {
        // destroy every live obstacle in the scene
        foreach (var obs in FindObjectsByType<ObstacleMover>())
            Destroy(obs.gameObject);
    }

    // ── spawn logic ────────────────────────────────────
    void SpawnObstacle()
    {
        GameObject prefab = PickObstaclePrefab(out bool isLaser);
        if (prefab == null)
        {
            Debug.LogWarning("ObstacleSpawner: no obstacle prefab assigned!");
            return;
        }

        // lasers sit at crouch height; blocks vary slightly; pipes sit on the ground
        float y;
        if (isLaser)                             y = groundY + laserHeight;
        else if (prefab == blockObstaclePrefab)  y = groundY + Random.Range(0f, 0.5f);
        else                                     y = groundY;

        Vector3 spawnPos = new Vector3(spawnX, y, 0f);
        GameObject obs = Spawn(prefab, spawnPos);

        // if it is a binary block, randomise 0 or 1 label
        BinaryBlock block = obs.GetComponent<BinaryBlock>();
        if (block != null)
            block.SetValue(Random.value > 0.5f ? 1 : 0);

        TrySpawnCollectible();
    }

    // Weighted pick among the assigned obstacle prefabs. Unassigned prefabs get weight 0
    // and are skipped, so leaving laserObstaclePrefab empty preserves the old behaviour.
    GameObject PickObstaclePrefab(out bool isLaser)
    {
        isLaser = false;
        float wPipe  = pipeObstaclePrefab  != null ? pipeWeight  : 0f;
        float wBlock = blockObstaclePrefab != null ? blockWeight : 0f;
        float wSpike = spikeObstaclePrefab != null ? spikeWeight : 0f;
        float wLaser = laserObstaclePrefab != null ? laserWeight : 0f;
        float total  = wPipe + wBlock + wSpike + wLaser;
        if (total <= 0f) return null;

        float r = Random.value * total;
        if (r < wPipe)  return pipeObstaclePrefab;
        r -= wPipe;
        if (r < wBlock) return blockObstaclePrefab;
        r -= wBlock;
        if (r < wSpike) return spikeObstaclePrefab;
        isLaser = true;
        return laserObstaclePrefab;
    }

    // ── collectibles ───────────────────────────────────
    void TrySpawnCollectible()
    {
        if (collectiblePrefabs == null || collectiblePrefabs.Length == 0) return;
        if (Random.value > collectibleChance) return;

        GameObject prefab = collectiblePrefabs[Random.Range(0, collectiblePrefabs.Length)];
        if (prefab == null) return;

        // spawn a little ahead of the obstacle and at a random jump-reachable height
        float y = groundY + Random.Range(collectibleMinY, collectibleMaxY);
        Vector3 pos = new Vector3(spawnX + 2f, y, 0f);
        Spawn(prefab, pos);
    }

    // Pool-aware spawn: uses ObjectPool if present, otherwise plain Instantiate.
    GameObject Spawn(GameObject prefab, Vector3 pos)
    {
        if (ObjectPool.Instance != null)
            return ObjectPool.Instance.Get(prefab, pos, Quaternion.identity);
        return Instantiate(prefab, pos, Quaternion.identity);
    }

    void SetNextSpawnTime()
    {
        // difficulty: shrink interval as game manager speed increases
        float speedFactor = GameManager.Instance != null
            ? Mathf.Clamp01((GameManager.Instance.CurrentSpeed - 4f) / 10f)
            : 0f;

        float adjustedMin = Mathf.Lerp(minInterval, minInterval * 0.55f, speedFactor);
        float adjustedMax = Mathf.Lerp(maxInterval, maxInterval * 0.65f, speedFactor);

        nextSpawnTime = Random.Range(adjustedMin, adjustedMax);
    }
}
