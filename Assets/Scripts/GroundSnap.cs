using UnityEngine;

/// <summary>
/// Snaps this object so the bottom of its Collider2D rests exactly on the ground surface.
/// Runs on enable so it is pooling-safe. Purely additive — add it to ground-resting obstacle
/// prefabs (pipes, blocks, spikes). Lasers/flying hazards should NOT use it.
/// </summary>
[DisallowMultipleComponent]
public class GroundSnap : MonoBehaviour
{
    [Tooltip("World Y of the ground surface the player stands on.")]
    public float surfaceY = -2.45f;

    [Tooltip("Extra vertical nudge after snapping (e.g. to bury a base slightly).")]
    public float yOffset = 0f;

    void OnEnable()
    {
        Snap();
    }

    public void Snap()
    {
        var col = GetComponent<Collider2D>();
        if (col == null) return;

        // Collider bounds are world-space and account for scale/offset.
        float bottom = col.bounds.min.y;
        float delta = (surfaceY + yOffset) - bottom;
        transform.position += new Vector3(0f, delta, 0f);
    }
}
