using UnityEngine;

/// <summary>
/// Bounces the player upward on contact (Geometry-Dash jump pad). Add to a scrolling prefab
/// alongside an <see cref="ObstacleMover"/> with killOnContact = false and a trigger collider.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class JumpPad : MonoBehaviour
{
    public float bounceForce = 16f;
    public AudioClip bounceClip;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var player = other.GetComponent<PlayerController>();
        if (player == null) player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        player.Bounce(bounceForce);
        if (bounceClip != null)
            AudioSource.PlayClipAtPoint(bounceClip, transform.position);
    }
}
