using UnityEngine;

/// <summary>
/// A scrolling power-up pickup (Temple-Run style). On player contact it activates the chosen
/// effect via <see cref="PowerUpManager"/>, plays feedback, and destroys itself. Pair with an
/// <see cref="ObstacleMover"/> (killOnContact = false). Additive — independent of Collectible.
/// </summary>
[RequireComponent(typeof(ObstacleMover))]
public class PowerUp : MonoBehaviour
{
    public PowerUpManager.PowerUpType type = PowerUpManager.PowerUpType.Magnet;
    public float duration = 6f;
    public AudioClip pickupClip;
    public GameObject pickupEffectPrefab;

    void Reset()
    {
        var mover = GetComponent<ObstacleMover>();
        if (mover != null) mover.killOnContact = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (PowerUpManager.Instance != null)
            PowerUpManager.Instance.Activate(type, duration);

        if (pickupClip != null) AudioSource.PlayClipAtPoint(pickupClip, transform.position);
        if (pickupEffectPrefab != null) Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
