using UnityEngine;

// Lightweight static event bus so HUD/FX can react to pickups without the existing
// Collectible.cs needing any changes. A CollectiblePickupRelay (added to the pickup
// prefabs) raises Collected on the same trigger frame the Collectible applies its effect.
public static class CollectibleEvents
{
    // (type, worldPosition, amount) — amount is starPoints for Star, 0 for SpeedRush.
    public static event System.Action<Collectible.CollectibleType, Vector3, float> Collected;

    public static void RaiseCollected(Collectible.CollectibleType type, Vector3 worldPos, float amount)
    {
        Collected?.Invoke(type, worldPos, amount);
    }
}
