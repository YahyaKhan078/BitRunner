using UnityEngine;

// Pickup that scrolls left with the world (movement is handled by ObstacleMover,
// which should be added alongside this with killOnContact = false).
//
// On contact with the Player it applies its effect and destroys itself:
//   • Star      → flat score bonus
//   • SpeedRush → temporary speed + passive-score multiplier
[RequireComponent(typeof(ObstacleMover))]
public class Collectible : MonoBehaviour
{
    public enum CollectibleType { Star, SpeedRush }

    [Header("Type")]
    public CollectibleType collectibleType = CollectibleType.Star;

    [Header("Star settings")]
    public float starPoints = 50f;          // flat score added on pickup

    [Header("Speed Rush settings")]
    public float rushSpeedMultiplier = 1.6f; // CurrentSpeed multiplier while active
    public float rushScoreMultiplier = 2f;   // passive score-gain multiplier while active
    public float rushDuration        = 4f;   // seconds the rush lasts

    [Header("Feedback (optional)")]
    public AudioClip pickupClip;             // played at the pickup position
    public GameObject pickupEffectPrefab;    // e.g. a burst particle

    void Reset()
    {
        // collectibles must not kill the player — disable the mover's contact damage
        var mover = GetComponent<ObstacleMover>();
        if (mover != null) mover.killOnContact = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            switch (collectibleType)
            {
                case CollectibleType.Star:
                    gm.CollectStar(starPoints);   // counts toward the level goal + adds score
                    break;

                case CollectibleType.SpeedRush:
                    gm.ActivateSpeedRush(rushSpeedMultiplier, rushScoreMultiplier, rushDuration);
                    break;
            }
        }

        if (pickupClip != null)
            AudioSource.PlayClipAtPoint(pickupClip, transform.position);

        if (pickupEffectPrefab != null)
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
