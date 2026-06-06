using System.Collections.Generic;
using UnityEngine;

// Lightweight per-prefab pool. The spawner asks for instances via Get(prefab,...) and
// ObstacleMover returns them via Release(go) instead of Destroy. Instances remember
// their source prefab through a PooledObject marker so Release knows which list to use.
// Falls back to plain Instantiate/Destroy if no pool exists, so nothing breaks.
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (!pools.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
        }

        GameObject go = null;
        while (queue.Count > 0 && go == null)
            go = queue.Dequeue();   // skip any destroyed entries

        if (go == null)
        {
            go = Instantiate(prefab, position, rotation);
            var marker = go.AddComponent<PooledObject>();
            marker.sourcePrefab = prefab;
        }
        else
        {
            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);
        }
        return go;
    }

    public void Release(GameObject go)
    {
        if (go == null) return;
        var marker = go.GetComponent<PooledObject>();
        if (marker == null || marker.sourcePrefab == null)
        {
            Destroy(go);   // not pooled → behave like before
            return;
        }
        go.SetActive(false);
        if (!pools.TryGetValue(marker.sourcePrefab, out var queue))
        {
            queue = new Queue<GameObject>();
            pools[marker.sourcePrefab] = queue;
        }
        queue.Enqueue(go);
    }
}

// Marker stamped on pooled instances so Release can route them back to the right queue.
public class PooledObject : MonoBehaviour
{
    public GameObject sourcePrefab;
}
