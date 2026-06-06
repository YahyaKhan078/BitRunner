using UnityEngine;

/// <summary>
/// A handcrafted, ordered sequence of obstacles/collectibles (Geometry-Dash style). The
/// PatternSpawner plays one element after another with the given horizontal gap, instead of
/// pure random spawning, so designers can author fair, readable, escalating rhythms.
/// </summary>
[CreateAssetMenu(fileName = "ObstaclePattern", menuName = "BitRunner/Obstacle Pattern")]
public class ObstaclePattern : ScriptableObject
{
    public enum ElementKind { Pipe, Block, Laser, Spike, JumpPad, Saw, Star, SpeedRush, Gap }

    [System.Serializable]
    public struct Element
    {
        public ElementKind kind;
        [Tooltip("World units to advance before the NEXT element spawns.")]
        public float gapAfter;
        [Tooltip("Y offset above groundY for elevated items (lasers, stars, saws).")]
        public float yOffset;
    }

    [Tooltip("Minimum difficulty tier this pattern is allowed to appear at (0 = always).")]
    public int minTier = 0;

    public Element[] elements;
}
