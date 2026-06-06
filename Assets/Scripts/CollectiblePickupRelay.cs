using UnityEngine;

// Added alongside Collectible on pickup prefabs. Mirrors the same Player-trigger
// detection and raises CollectibleEvents.Collected so the HUD can show a score pop /
// star counter. Purely additive — it does NOT modify or replace Collectible's logic.
[RequireComponent(typeof(Collectible))]
public class CollectiblePickupRelay : MonoBehaviour
{
    private Collectible collectible;
    private bool fired;

    void Awake()
    {
        collectible = GetComponent<Collectible>();
    }

    void OnEnable()
    {
        fired = false; // reset for pooling reuse
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (fired) return;
        if (!other.CompareTag("Player")) return;
        fired = true;

        float amount = collectible != null && collectible.collectibleType == Collectible.CollectibleType.Star
            ? collectible.starPoints
            : 0f;
        var type = collectible != null ? collectible.collectibleType : Collectible.CollectibleType.Star;
        CollectibleEvents.RaiseCollected(type, transform.position, amount);
    }
}
