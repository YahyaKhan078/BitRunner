# BitRunner: Technical Deep Dive

This document details the underlying logic and architectural decisions that power the BitRunner systems.

## 1. The Manager Architecture
The project follows a "Hybrid Persistence" manager pattern:
*   **GameManager:** Local to each scene. It holds level-specific references (HUD targets, music, star goals). This ensures we don't have broken inspector references when switching scenes.
*   **Cross-Run Managers:** `SkinManager`, `Missions`, `PowerUpManager`, and `LevelProgress`. These use `DontDestroyOnLoad` and the Singleton pattern. They handle data that must survive scene loads, such as currency, active power-up timers, and skin selections.

## 2. Event-Driven Decoupling
To avoid "spaghetti code," systems communicate via static C# events:
```csharp
// Example from GameEvents.cs
public static event Action<int> RunStarted;
public static void RaiseRunStarted(int level) => RunStarted?.Invoke(level);
```
**Advantage:** When a player dies, the `GameManager` simply raises `GameEvents.PlayerDied()`. The `JuiceFX` script (screen flash), `CameraShake`, and `AttemptCounter` all react independently without the `GameManager` needing to know they exist.

## 3. Character Controller Logic
### The Crouch Fix (The "Planted Feet" Problem)
A common issue in runners is that scaling a character's transform on Y also moves their center and bottom.
*   **Naive Approach:** Scale Y to 0.5. Result: Feet lift off ground, `groundCheck` loses contact, character flickers.
*   **BitRunner Solution:** 
    1. Measure `collider.bounds.min.y` (feet position) before scaling.
    2. Apply Y-scale squash.
    3. Measure `collider.bounds.min.y` again.
    4. Shift the entire `transform.position` by the delta to pin the feet to the ground.
    5. Sync physics transforms immediately.

### Custom Gravity Arc
Uses a dual-multiplier system in `FixedUpdate`:
*   `fallMultiplier`: Applied when `velocity.y < 0`, making the fall faster than the rise for a snappier feel.
*   `lowJumpMultiplier`: Applied when the player releases the jump key while still rising, allowing for precise "short hops."

## 4. Spawning Logic
### Weighted Random (`ObstacleSpawner`)
Obstacles are picked using a weighted probability array. Each level can adjust weights (e.g., Level 3 has a higher weight for Spikes and Lasers than Level 1).
### Pattern System (`PatternSpawner`)
Uses an array of `ObstaclePattern` ScriptableObjects. A pattern is an ordered list of `Elements`:
```csharp
[Serializable]
public struct Element {
    public ElementKind kind; // Spike, JumpPad, etc.
    public float gapAfter;   // Distance until next spawn
    public float yOffset;    // Vertical placement
}
```
The spawner calculates distance based on `CurrentSpeed * Time.deltaTime`, ensuring that obstacle gaps remain physically consistent even as the game speeds up.

## 5. Persistence System
`LevelProgress.cs` provides a static abstraction layer over `PlayerPrefs`. It uses structured keys like `LevelStars_1`, `LevelBest_2`, etc. It handles clamped values and high-score logic internally, providing a clean API for the rest of the game:
```csharp
if (score > LevelProgress.GetBestScore(level)) {
    LevelProgress.SetBestScore(level, (int)score);
}
```

## 6. AI Asset Pipeline
The neon hazards and power-up icons were generated using the following workflow:
1. **Generation:** Prompting Gemini for 2D flat sprites with isolation requirements.
2. **Post-Processing:** AI background removal to generate clean PNGs.
3. **Automated Integration:** A custom Editor script (RunCommand) handled:
    *   Sprite importing and slicing.
    *   Calculation of pixel-perfect content bounds.
    *   Automatic attachment of `BoxCollider2D` and `ObstacleMover` components.
    *   Prefab creation and registration in the Spawner's prefab arrays.
