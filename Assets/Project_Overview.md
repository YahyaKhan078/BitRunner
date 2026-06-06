# Technical Overview: BitRunner

BitRunner is a fast-paced 2D infinite runner with a cyberpunk aesthetic. Players navigate a high-speed environment, dodging binary-themed obstacles and collecting stars to progress through levels. The project features a progressive difficulty system where the game speed and obstacle density increase over time, complemented by a "Speed Rush" power-up system.

## 1. Project Description
BitRunner is designed as a mobile-friendly side-scrolling runner. The core experience is built around precision jumping and sliding (crouching) to avoid obstacles. It targets casual players who enjoy high-score chasing and level-based progression.

**Core Pillars:**
*   **High-Speed Flow:** Continuous movement where speed increases over time, requiring faster reflexes.
*   **Binary Aesthetics:** Visual style incorporating "0" and "1" blocks, neon effects, and a digital city backdrop.
*   **Progressive Difficulty:** Mechanics that scale spawn rates and movement speed based on current performance.
*   **Power-Up Synergy:** The "Speed Rush" mechanic that provides temporary invincibility-like high speed and scoring multipliers.

## 2. Gameplay Flow / User Loop
1.  **Boot & Menu:** The game starts in the `MainMenu` scene. Players can select levels (if unlocked) or start the first level.
2.  **Idle State:** Upon entering a level, the game is in an `Idle` state. A "Press Space to Start" prompt is displayed.
3.  **Countdown:** Pressing start triggers a 3-2-1-GO countdown (running on unscaled time).
4.  **The Run:**
    *   The environment scrolls left at `CurrentSpeed`.
    *   The player jumps or crouches to avoid obstacles.
    *   Collecting stars increases the score and progresses the level completion bar.
    *   Speed increases gradually, making obstacle spawning tighter.
5.  **Failure/Success:**
    *   **Death:** Hitting an obstacle triggers a death animation (sink-into-ground) and freezes the game world before showing the Game Over screen.
    *   **Completion:** Collecting the required number of stars triggers the `LevelComplete` state, unlocking the next level.
6.  **Loop:** Players return to the menu or retry the level, carrying over high scores and unlock progress.

## 3. Architecture
The project follows a **Manager-Centric** pattern with a strong emphasis on a Singleton `GameManager` that coordinates state across various sub-systems.

*   **State Management:** `GameManager` maintains the `IsPlaying` flag, which all other systems (Spawners, Controllers, Scrollers) poll to determine if they should execute logic.
*   **Decoupled Movement:** Movement is not applied to the player. Instead, the world (obstacles, ground, background) moves leftward based on `GameManager.Instance.CurrentSpeed`.
*   **Event Communication:** Uses a mix of direct references and simple event relays (e.g., `CollectiblePickupRelay`) to handle interactions between the player and world objects.
*   **Time Scaling:** The game uses `Time.timeScale = 0` for pausing and death, while UI and death animations run on `unscaledTime` to remain interactive.

`Location: Assets/Scripts`

## 4. Game Systems & Domain Concepts

### World Movement & Parallax
Calculates scrolling speeds for different layers to create depth.
*   `GroundScroller`: Seamlessly tiles and loops ground sprites.
*   `CameraShake`: Handles screen shake on death and manages background parallax layers.
*   `ObstacleMover`: Moves individual obstacles at the current game speed.
*   **Extension:** Add new layers to the `parallaxSpeeds` array in the `CameraShake` component on the Main Camera.

`Location: Assets/Scripts`

### Obstacle & Spawning System
Manages the generation of hazards based on weighted random selection.
*   `ObstacleSpawner`: Spawns pipes, binary blocks, and lasers at dynamic intervals.
*   `BinaryBlock`: A specific obstacle type that randomizes its appearance between '0' and '1'.
*   `ObjectPool`: Optional system used by the spawner to recycle obstacle GameObjects for performance.
*   **Extension:** Add new prefabs to the `ObstacleSpawner` inspector and adjust their `Weight` variables to integrate them into the spawn pool.

`Location: Assets/Scripts`

### Player Controller
A physics-based controller for a 2D side-scroller.
*   `PlayerController`: Handles `Jump`, `Double Jump`, and `Crouch` logic.
*   **Variable Jump:** Jump height is determined by how long the jump key is held (`jumpCutMultiplier`).
*   **Crouch Logic:** Shrinks the 2D collider and applies a "visual squash" to the sprite transform.
*   **Extension:** Modify the `jumpForce` or `crouchHeightScale` in the Inspector to tune the feel of the character.

`Location: Assets/Scripts`

### Collectible & Speed Rush System
Handles scoring and temporary power-ups.
*   `Collectible`: Defines the type (Star or SpeedRush) and its effects.
*   `SpeedRush`: While active, it increases `speedMultiplier`, score gain, and enables **unlimited multi-jumps**.
*   `SpeedRushScreenFX`: Manages post-processing effects (vignette/chromatic aberration) during the power-up.

`Location: Assets/Scripts`

## 5. Scene Overview
*   **MainMenu:** Entry point containing level selection and persistent high-score display.
*   **LevelX (1, 2, 3):** Functional gameplay levels. Each level is a self-contained scene with a `GameManager` configured for that level's specific goals (stars required, next scene to load).
*   **SampleScene:** A testing environment for new mechanics.

**Scene Rules:**
*   Each level scene must contain a `GameManager` (non-persistent) to ensure inspector references for that specific level are valid.
*   Level transition is handled by `GameManager` via `SceneManager.LoadScene` using string names defined in the Inspector.

## 6. UI System
The project uses **UGUI (Unity UI)** with **TextMesh Pro** for all HUD and menu elements.

*   **Structure:** Managed via `GameManager` which toggles `idlePanel`, `hudPanel`, and `gameOverPanel`.
*   **Binding:** The HUD elements (`scoreText`, `starsText`, etc.) are updated every frame during the `Update` loop of the `GameManager`.
*   **HUD Extras:** `HudExtras` and `CollectiblePickupRelay` handle world-space UI "pops" (e.g., "+100" or "SPEED!") when items are collected.
*   **Touch Input:** `TouchInput` translates screen swipes and taps into `Jump` and `Crouch` commands for the `PlayerController`.

`Location: Assets/Scripts`

## 7. Asset & Data Model
*   **Persistence:** Uses `PlayerPrefs` via the `LevelProgress` static class to store:
    *   `LevelUnlocked_[N]`: Boolean for unlock state.
    *   `LevelCompleted_[N]`: Boolean for completion state.
    *   `HighScore`: The highest score achieved across all sessions.
*   **Prefabs:** Obstacles and collectibles are stored in `Assets/PreFabs` and must include an `ObstacleMover` to participate in world scrolling.
*   **Animations:** Managed by `PlayerAnimator.controller`, using parameters like `isRunning`, `isGrounded`, and `isDead`.

`Location: Assets/Scripts (LevelProgress.cs), Assets/PreFabs`

## 8. Notes, Caveats & Gotchas
*   **TimeScale Zero:** When the player dies, `Time.timeScale` is set to 0. Any logic intended to run during the death animation or game-over screen **must** use `Time.unscaledDeltaTime` or be a Coroutine using `WaitForSecondsRealtime`.
*   **Collider Squash:** The crouch mechanic scales the player's Transform. Ensure the `GroundCheck` child object is positioned such that scaling doesn't push it into or through the ground collider.
*   **Static GameManager:** The `GameManager` uses a Singleton pattern but is **not** marked `DontDestroyOnLoad`. This is intentional so that Level 1, Level 2, etc., can have different configurations (like `starsToComplete`) set in their respective scene inspectors.
*   **Input Handling:** The project supports both legacy Input Manager and the New Input System (configured to "Both"), but `PlayerController` and `GameManager` primarily use `Input.GetKeyDown` methods.

` (Legacy).