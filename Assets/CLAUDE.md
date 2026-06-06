# BitRunner

A **Unity 6 2D endless runner**. The player auto-runs to the right, the world scrolls
left toward the player, and obstacles spawn ahead. Press **Space / W / ↑** to jump
(double-jump supported) and to start/restart the game. Touching an obstacle ends the
run and freezes the screen. Score accumulates over time and game speed ramps up with
score. Optional collectibles add score or a temporary speed boost.

## Project layout

- `Assets/Scripts` — all gameplay C# scripts (see below)
- `Assets/Scenes/SampleScene.unity` — the single playable scene
- `Assets/PreFabs` — obstacle and effect prefabs
- `Assets/Sprites`, `Assets/Audio`, `Assets/DemonStick` — art and audio assets
- `Assets/TextMesh Pro` — TMP package assets (HUD / block labels)

## Core actors

- **PlayerGameObject** — the player. Has `PlayerController`, `Rigidbody2D`, `Animator`,
  `AudioSource`, and a child `groundCheck` transform. Tagged `Player`.
- **GameManager** — a single scene object holding the `GameManager` singleton. Owns
  game state and wires up references to the player, spawner, camera shake, UI panels,
  and HUD text via the Inspector.
- **Spawner** — a scene object with `ObstacleSpawner` that instantiates obstacle (and
  optionally collectible) prefabs ahead of the camera on a timer.

## Scripts (`Assets/Scripts`)

- **GameManager.cs** — Singleton (`GameManager.Instance`) that controls game state.
  Tracks `CurrentSpeed`, `IsPlaying`, `SpeedRushActive`, score, and high score
  (persisted via `PlayerPrefs` key `"HighScore"`). Handles the idle → playing →
  game-over flow, switches panels via the exclusive `SetPanels()` helper, ramps the
  **base speed over time** (`speedGainPerSecond`, Subway-Surfers style) with passive
  score tied to speed (`scoreRate`), fires level-up events, and on the start key runs a
  **loading-bar coroutine** (`StartSequence` → fills the Filled `loadingBar` Image over
  `loadDuration`, then `StartGame`). Freezes the whole game on death with
  `Time.timeScale = 0` and restores it on start/restart. Collectible hooks: `AddScore(amount)` (Star) and
  `ActivateSpeedRush(speedMult, scoreMult, duration)` (Speed Rush). Death is **two-phase**:
  `BeginDeath()` freezes the world (so the death animation can play), then
  `ShowGameOverScreen()` shows the panel. **Levels:** collect `starsToComplete` stars
  (via `CollectStar`) to finish — `CompleteLevel()` saves progress
  (`LevelProgress.MarkCompleted`/`Unlock`) and loads `nextLevelScene` (or "YOU WIN").
  Start uses a **3-2-1-GO countdown** (`countdownText`, replaces the old loading bar).
  **Restart reloads the scene** (`PlayAgain()` + a static auto-start flag) so the Animator
  and start position reset cleanly; `GoToMainMenu()` for the menu button. Entry points:
  `StartGame()`, `PlayAgain()`, `GoToMainMenu()`, `BeginDeath()`/`ShowGameOverScreen()`,
  `CollectStar()`, `AddScore()`, `ActivateSpeedRush()`.
- **PlayerController.cs** — Player jump/double-jump (Space / W / ↑; **unlimited
  multi-jump while Speed Rush is active**, reverts to double-jump after), **crouch**
  (S / ↓, hold) that shrinks the body collider (`crouchHeightScale`) so the player fits
  under low laser beams, ground detection via `Physics2D.OverlapCircle` against `groundLayer`,
  animator parameter updates, jump/land/death audio, dust particles, and an optional
  `speedTrail` that intensifies with speed. Crouch needs a **Capsule or Box** collider on
  the player and sets an `isCrouching` animator bool **only if that parameter exists**.
  `Die()` plays the death animation, then after `deathAnimDuration` shows Game Over via the
  GameManager's two-phase death. The Animator runs on **unscaled time** so the death clip
  plays during the freeze. `isRunning` is false when not playing (Idle shows on the start
  screen, Run during play); jump/crouch are gated to active play. Jump has **variable
  height** (release early = short hop, `jumpCutMultiplier`); on death the player **sinks
  into the ground** (`deathSinkDistance`) while the clip plays. `TryJump()`/`SetCrouchHeld()`
  are public hooks for external (e.g. mobile) input. `ResetPlayer()` restores it for a new run.
- **ObstacleSpawner.cs** — Spawns one of three obstacle types by relative weight:
  `pipeObstaclePrefab` (jump), `blockObstaclePrefab` (jump, binary 0/1), and an optional
  `laserObstaclePrefab` (crouch under; spawned at `groundY + laserHeight`). Weights are
  `pipeWeight`/`blockWeight`/`laserWeight`; an unassigned prefab gets weight 0, so leaving
  the laser empty preserves the old pipe/block behaviour. Randomized interval
  (`minInterval`–`maxInterval`, ~1.2–2.5s) shrinks as `CurrentSpeed` rises. Optionally
  also spawns a `Collectible` from `collectiblePrefabs` at a jump-reachable height
  (chance = `collectibleChance`; disabled while the array is empty). Exposes
  `StartSpawning()`, `StopSpawning()`, `ClearAllObstacles()` (clears obstacles *and*
  collectibles, since both carry `ObstacleMover`).
- **ObstacleMover.cs** — Moves an object left at `GameManager.CurrentSpeed` and destroys
  it once off-screen. If `killOnContact` is true (default) it calls
  `PlayerController.Die()` on trigger collision with the `Player`-tagged object;
  collectibles reuse this mover with `killOnContact = false` so contact is harmless.
- **Collectible.cs** — Pickup with a `CollectibleType` enum {Star, SpeedRush}. Requires
  an `ObstacleMover` (added automatically; `Reset()` sets its `killOnContact = false`).
  On Player trigger: **Star** adds `starPoints` via `GameManager.AddScore`; **SpeedRush**
  calls `GameManager.ActivateSpeedRush(...)`. Plays optional `pickupClip` audio and
  spawns `pickupEffectPrefab` (the sparkle), then destroys itself.
- **AutoDestroy.cs** — One-shot effect helper: grows + fades a `SpriteRenderer` over
  `lifetime`, then self-destroys. Used by the `PickupSparkle` prefab.
- **SpeedRushUI.cs** — Drives the "SPEED RUSH" drop-down alert (slides `alertRect`
  between `hiddenY`/`shownY`) and a depleting energy bar (`energyBarFill.fillAmount`)
  from `GameManager.SpeedRushActive` / `SpeedRushRemaining01`. Decoupled — reads
  `GameManager.Instance`; assign only the visual refs.
- **FPSCounter.cs** — Smoothed FPS readout for the top-right HUD; writes to a `TMP_Text`
  on unscaled time.
- **SpriteFlipbook.cs** — Cycles a `SpriteRenderer` through `frames` (sparkle animation,
  pulsing laser hazards, etc.); `loop`/`destroyOnFinish` options.
- **MainMenu.cs** — For the main-menu scene: `PlayScene(name)` / `PlayFirstLevel()` /
  `QuitGame()` wired to UI Buttons. Loads level scenes by name.
- **LevelProgress.cs** — Static PlayerPrefs helper for level unlock/completion
  (`IsUnlocked`, `Unlock`, `IsCompleted`, `MarkCompleted`, `ResetAll`). Level 1 always
  unlocked; finishing a level unlocks the next.
- **SoundToggle.cs** — Global mute on/off (persisted), via `AudioListener.volume`. Put on
  a button (HUD top-right, menu, help) → OnClick `Toggle()`; re-applies saved state per scene.
- **LevelSelectButton.cs** — One per level-select card: shows LOCKED/COMPLETED/PLAY from
  `LevelProgress` and loads the scene on `TryLoad()` only if unlocked.
- **Collectible.cs** Star path now calls `GameManager.CollectStar` (counts toward the
  level's star goal **and** adds score); SpeedRush unchanged.
- **BinaryBlock.cs** — Block obstacle that displays a 0 (red) or 1 (blue) via a
  `SpriteRenderer` + child `TextMeshPro` label. `SetValue()` is called by the spawner.
- **GroundScroller.cs** — Attach to each ground tile. Scrolls tiles left at game speed
  and recycles the leftmost tile to the right edge for an infinite ground. Recycles only
  once the tile's **right edge passes the camera's left edge** (`marginPastCamera`), using
  renderer bounds so tiles never vanish while on screen. Expects ~3 tiles side by side.
- **CameraShake.cs** — Two jobs: camera shake on death (`Shake()`, called by the
  GameManager, runs on **unscaled** time so it animates while the game is frozen) and
  parallax scrolling of background layers at fractions of game speed.

## Systems

### Panel visibility (mutually exclusive)
`GameManager.SetPanels(idle, hud, over)` is the single source of truth — exactly one of
IdlePanel / HUDPanel / GameOverPanel is ever active. `ShowIdle()`, `StartGame()`, and
`ShowGameOver()` all route through it. HUD score/best/level text must live **inside**
HUDPanel; game-over score/best text inside GameOverPanel, so hiding a panel hides its
text.

### Death freeze
`OnPlayerDied()` sets `Time.timeScale = 0f` after showing the game-over panel, freezing
the player, ground, obstacles, and spawner. `StartGame()` and `RestartGame()` set it
back to `1f` (timeScale persists across scene loads, so this must be done explicitly).
`Update()` and `Input.GetKeyDown` still run at timeScale 0, so the restart key works
while frozen. Anything that must animate during the freeze (the death shake) uses
`Time.unscaledDeltaTime`.

### Speed Rush
`ActivateSpeedRush` sets a `speedMultiplier` and `scoreMultiplier` plus an expiry time
(`Time.unscaledTime + duration`). `Update()` recomputes the base speed ramp every frame
and applies `CurrentSpeed = baseSpeed * speedMultiplier`, and boosts passive score by
`scoreMultiplier`; when the timer expires `EndSpeedRush()` resets both multipliers to 1,
so values revert cleanly without fighting the ramp. Picking up another Speed Rush
refreshes the timer. `SpeedRushActive` drives the `SpeedRushUI` alert/bar and also
**unlocks unlimited multi-jump** in `PlayerController` while active.

### Audio
`musicSource` (on the GameManager object) loops `BGMusic.wav` at a **lowered volume
(~0.3)**. On death, `OnPlayerDied()` stops the music and plays `gameOverClip`
(`error_003`) via `AudioSource.PlayClipAtPoint` so the sting is full-volume and survives
the `timeScale = 0` freeze. The obstacle-hit sound is the player's `deathSound`
(`error_001`), played in `PlayerController.Die()`. Player SFX: jump = `maximize_001`,
double-jump = `maximize_004`, land = `drop_001`. Pickups: Star = retro-coin,
SpeedRush = `maximize_001`.

### UI / Canvas
Canvas uses **Scale With Screen Size** (reference 1920×1080, match 0.5). Panels are
full-screen: IdlePanel/GameOverPanel are dark dim overlays; HUDPanel is transparent
(alpha 0) so it never dims gameplay. HUD text is a top-left stack (Score, Level, Best);
idle + game-over text are centered and use TMP rich-text (`<size>`, `<b>`) for title
hierarchy — the game-over "GAME OVER" title and restart hint are baked into
`goScoreText`/`goBestText` by `ShowGameOver()`, so no extra UI objects are needed.

### Vision hooks (for editor wiring)
Code scaffolding exists for a cyberpunk neon look; the visuals/wiring are done in the
editor:
- **Speed Rush UI:** `GameManager.SpeedRushActive` + `SpeedRushRemaining01` → wire a
  `SpeedRushUI` component to a "SPEED RUSH" label (RectTransform) + a Filled `Image`.
- **FPS/Score HUD:** `FPSCounter` → a TMP text (top-right). Score text already updates.
- **Speed trail:** `PlayerController.speedTrail` (assign a `TrailRenderer`); width/length
  lerp with `GameManager.SpeedIntensity01` (rises with speed, maxes during Speed Rush).
- **Parallax:** `CameraShake.backgroundLayers` + `parallaxSpeeds` (already supported) —
  assign neon city layers, slowest first.
- **Flipbook fx:** `SpriteFlipbook` for the sparkle animation and pulsing laser hazards.

## Game flow

1. `GameManager.Start()` → `ShowIdle()` shows the IdlePanel, `IsPlaying = false`.
2. Jump input while not playing → `StartGame()`: sets `timeScale = 1`, resets player,
   clears + starts spawner, shows HUD (hides idle/game-over), plays music. This path
   serves both first-start and restart-from-game-over.
3. `Update()` accumulates score, ramps `CurrentSpeed` up to `maxSpeed`, applies any
   Speed Rush, and fires level-up events.
4. Obstacle collision → `PlayerController.Die()` → `GameManager.OnPlayerDied()`:
   stops spawning, shakes camera, saves high score, shows the game-over panel, then
   freezes via `timeScale = 0`.
5. Restart: press Space/W/↑ (handled by `Update()` at timeScale 0) to re-enter
   `StartGame()`, or call `RestartGame()` to reload the scene.

## Known issues / status

- **Game start (FIXED):** `Update()` now starts the game on Space / W / ↑ while idle.
- **Panels not mutually exclusive (FIXED):** all panel switches go through `SetPanels()`.
- **World kept scrolling on death (FIXED):** `OnPlayerDied()` now sets
  `Time.timeScale = 0`; start/restart restore it to 1.
- **Pacing / progressive difficulty (FIXED):** base speed now climbs over time
  (`speedGainPerSecond`) up to `maxSpeed`; passive score is tied to current speed
  (`scoreRate`). Spawn interval lowered to ~0.8–1.6s and shrinks further as speed rises.
- **Jump too floaty (FIXED):** `jumpForce` lowered to 10 / `doubleJumpForce` 8.5 (scene
  values updated too). Tune the player's Rigidbody2D `gravityScale` (~3–4) for feel.
- **Ground tiles vanished on-screen (FIXED):** `GroundScroller` recycles via camera-left
  edge + renderer bounds instead of a fixed `resetAtX`.
- **Unchecked Inspector references (OPEN):** `StartGame()` / `OnPlayerDied()` use
  `player` and `spawner` without null checks (panels use `?.`). Unassigned → a
  `NullReferenceException` on start/death. Currently all are wired in the scene.
- **No player position reset (OPEN):** `ResetPlayer()` restores state/physics but does
  not re-center the player. Masked in practice (restart usually reloads the scene, and
  death happens on the ground).

## Conventions

- Cross-script access goes through `GameManager.Instance`; scripts that read game speed
  fall back to a hardcoded default when the instance is missing.
- Scene-object references (player, spawner, panels, HUD text, audio) are assigned in the
  Inspector — confirm wiring there when behavior is missing rather than assuming a code bug.
- Targets the new Input Manager API (`Input.GetKeyDown`) and Rigidbody2D `linearVelocity`
  (Unity 6 naming).
- Tags in use: `Player` (built-in). Collectibles are intended to use **Star** and
  **SpeedBoost** tags — add these in the Tag Manager (the `Collectible` script keys off
  the `collectibleType` enum, not the tag, but the tags are useful for organization).
