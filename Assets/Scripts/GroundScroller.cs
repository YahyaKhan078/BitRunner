using UnityEngine;

// Attach this to each ground tile. Place ~3 tiles side by side; this script scrolls
// them left at the game speed and recycles the leftmost tile to the right edge so the
// ground looks infinite.
//
// A tile is only recycled once its RIGHT edge has passed the camera's LEFT edge (plus a
// margin), so tiles never vanish while still visible on screen.

public class GroundScroller : MonoBehaviour
{
    [Header("Settings")]
    public float tileWidth = 10f;        // spacing between tiles in world units
    public float marginPastCamera = 1f;  // extra distance past the left edge before recycling
    public float resetAtX = -10f;        // fallback threshold if no camera is found

    private Camera cam;
    private Renderer rend;                // used to read the tile's real right edge (pivot-proof)

    void Start()
    {
        cam  = Camera.main;
        rend = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
        float speed = GameManager.Instance != null
            ? GameManager.Instance.CurrentSpeed
            : 5f;

        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // left edge of the camera view in world space (orthographic)
        float camLeft = (cam != null && cam.orthographic)
            ? cam.transform.position.x - cam.orthographicSize * cam.aspect
            : resetAtX;

        // the tile's right edge — from the renderer bounds so the pivot doesn't matter
        float rightEdge = rend != null
            ? rend.bounds.max.x
            : transform.position.x + tileWidth * 0.5f;

        // recycle only once the whole tile has scrolled past the left edge of the view
        if (rightEdge < camLeft - marginPastCamera)
        {
            GroundScroller[] all = FindObjectsByType<GroundScroller>();
            float maxX = transform.position.x;
            foreach (var g in all)
                if (g.transform.position.x > maxX)
                    maxX = g.transform.position.x;

            transform.position = new Vector3(maxX + tileWidth,
                                             transform.position.y,
                                             transform.position.z);
        }
    }
}
