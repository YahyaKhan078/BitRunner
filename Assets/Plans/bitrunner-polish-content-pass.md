# BitRunner — Polish & Content Pass (Plan)

> Source of truth: `Assets/CLAUDE.md`. **All C# is already written by Claude Code. This pass
> wires hooks in the Editor, tunes scenes/prefabs, generates art, and adds new scene/UI
> objects. We do NOT rewrite existing scripts.** The only *new* code proposed is one small,
> additive `PlatformSpawner.cs` (clearly flagged — does not touch existing scripts).

---

# Project Overview
- **Game Title:** BitRunner
- **High-Level Concept:** A fast cyberpunk 2D endless/level-based runner — auto-run right, world scrolls left, jump/double-jump and crouch to dodge binary-themed hazards, collect stars to clear each level.
- **Players:** Single player (high-score / level progression).
- **Inspiration / Reference Games:** Geometry Dash (controls/help clarity), Subway Surfers (speed ramp), Super Mario (platforms/collectibles).
- **Tone / Art Direction:** Neon cyberpunk, binary "0/1" motifs, glowing city parallax.
- **Target Platform:** StandaloneWindows64 (PC). Mobile hooks (`TouchInput`) present for later.
- **Screen Orientation / Resolution:** Landscape, Canvas Scale-With-Screen-Size ref 1920×1080 (match 0.5).
- **Render Pipeline:** Universal RP (URP 2D).

## Decisions confirmed by user
1. **New obstacle art:** GENERATE new sprites (themed neon/binary, small & readable).
2. **Platforms/ledges:** GENERATE new platform art; treat as **optional fun** routes (stars on top), not required paths.
3. **Per-level themes:** "Whatever's best" → use the existing `City_L1_Far/L2_Far/L3_Far` backdrops + per-level ground/parallax tint (scheme below).
4. **Star frequency:** Accept L1 ≈ 0.50, L2 ≈ 0.45, L3 ≈ 0.40 (`collectibleChance`); SpeedRush chevron stays rarer than stars.
5. **Level Select:** Separate scene (`LevelSelect.unity`).
6. **Completion mode:** Switch all levels to **STAR-based** (`starsToComplete` 5/10/15, `levelTargetScore` 0) and fix `levelNumber` to 1/2/3.
7. Plus: add sensible **future-engagement** wiring (per-level music, pickup pop-text, pause menu, screen FX) since these are already scaffolded in the project.

---

# Current State (from investigation)
- **Scenes exist:** `MainMenu`, `Level1`, `Level2`, `Level3`, `SampleScene`. Level1/2/3 are fully built and structurally identical.
- **SampleScene** is a stale *subset* of Level1 (missing LevelCompletePanel, PausePanel, StarCounter, ObjectPool, SpeedRushFX, PauseMenu, TouchInput, HudExtras). **Not in Build Settings.** → safe to delete.
- **Build Settings now:** `MainMenu(0), Level1(1), Level2(2), Level3(3)`.
- **GameManager per-level (needs fixing):**
  | Field | L1 | L2 | L3 | Target |
  |---|---|---|---|---|
  | `levelNumber` | 1 | **1** ⚠ | **1** ⚠ | 1 / 2 / 3 |
  | `starsToComplete` | 0 | 0 | 0 | **5 / 10 / 15** |
  | `levelTargetScore` | 5000 | 10000 | 15000 | **0 / 0 / 0** |
  | `nextLevelScene` | "Level2" | "Level3" | "" | ✓ keep |
  | `mainMenuScene` | "MainMenu" | "MainMenu" | "MainMenu" | ✓ keep |
- **Unwired in all 3 levels:** `starsText` = null (a `StarCounter` TMP exists in HUD but isn't linked); `countdownText` = null (no countdown object exists yet).
- **Loading bar still present:** `IdlePanel > LoadingBarPanel(LoadingBar)`, wired to `GameManager.loadingBar` (legacy, null-guarded). To be removed/hidden; field left unwired.
- **SpeedRushUI_Container** exists (SpeedRushLabel + EnergyBarBG→EnergyBarFill) but `secondsLabel` = null and energy bar is the "bugged" one.
- **SoundToggle.cs & LevelSelectButton.cs exist but are not placed in any scene.**
- **MainMenu** has only Play + Quit (no Level Select / Sound / Help).
- **Already-scaffolded but worth verifying/wiring:** `PauseMenu`+`PausePanel`, `HudExtras`+`CollectiblePickupRelay`+`CollectibleEvents` (pickup pop-text), `TouchInput`, `SpeedRushScreenFX`+`SpeedRushCamera`, `ObjectPool`, `FPSCounter`.
- **Music:** `musicSource.Play()` uses the clip assigned on the AudioSource → per-level music = assign `Music_LevelX.wav` to each scene's GameManager AudioSource (no code).
- **Animator (`PlayerAnimator.controller`):** params `isRunning/isGrounded/isDead/isCrouching`. `Death` state entered on `isDead` (no exit time ✓) but has **no outgoing transition** → must add `Death → Idle` (`isDead == false`, no exit time) safety.
- **Star prefab:** `starPoints = 0` → stars currently give 0 score. Will set to ~100 so stars reward score too.

---

# Game Mechanics

## Core Gameplay Loop
Auto-run → dodge obstacles (jump / double-jump / crouch) → collect stars → fill the level's star goal → Level Complete → next level. Speed ramps over time; SpeedRush chevrons grant temporary speed + unlimited jumps + score multiplier. Death = sink + death anim → Game Over → Play Again (clean reload) or Main Menu.

## Controls and Input Methods
- **Space / W / ↑** = Jump (hold = higher; double-jump; unlimited during SpeedRush) and Start/Restart.
- **S / ↓** (hold) = Crouch/slide under red lasers.
- **Esc** = Pause (PauseMenu already present).
- **Mobile (future):** `PlayerController.TryJump()` / `SetCrouchHeld(bool)` via `TouchInput`.

---

# UI

## HUD (per level, inside HUDPanel)
```
[Score]                                   [FPS]  [🔊 Sound] [? Help]
[★ x / y]  (starsText → StarCounter)
[High Score]
                 ── SPEED RUSH ──   (drop-down alert)
                       3s            (SpeedRushUI.secondsLabel)
```
- Energy bar (EnergyBarBG/Fill) hidden.

## Idle / Start screen
```
            BITRUNNER  (level title)
        "Press Space to Start"   (startPromptLabel)
```
- Loading bar removed.

## Countdown (new, centered, on top)
```
            3 → 2 → 1 → GO!   (countdownText, Press Start 2P, big, scale-pop by code)
```

## Game Over panel
```
            GAME OVER
        Score ...   Best ...
   [ Play Again ]   [ Main Menu ]
     "Press Space to Play Again"
```

## Level Complete panel (already present)
```
        LEVEL COMPLETE!  (levelCompleteText)  → auto-loads nextLevelScene / YOU WIN→menu
```

## Help panel (new, in every Level + MainMenu + LevelSelect)
Explains: controls (Space=jump/double-jump, S/↓=crouch under red lasers, run through green chevrons = Speed Rush), goal ("Collect N stars to clear the level"), item legend (gold star = progress, green chevron = Speed Rush, red block/laser = hazard). Close button.

## Main Menu (updated)
```
            BITRUNNER
        [ Play ] → LevelSelect
        [ Level Select ] → LevelSelect
        [ Quit ]
   top-right: [🔊 Sound] [? Help]
```

## Level Select scene (new)
Three cards (LOCKED / COMPLETED / PLAY) with theme + ground + difficulty + "Collect N stars" info, padlock overlay on locked. Back button → MainMenu. Sound + Help available.

---

# Per-Level Theme Scheme (proposed)
| Level | Name | Far backdrop | Tint accent | Music | Stars | Difficulty |
|---|---|---|---|---|---|---|
| 1 | Boot Sector | `City_L1_Far` | Cyan/blue | `Music_Level1` | 5 | Easiest |
| 2 | Data Stream | `City_L2_Far` | Magenta/purple | `Music_Level2` | 10 | Medium |
| 3 | Kernel Core | `City_L3_Far` | Amber/red | `Music_Level3` | 15 | Hardest |

Shared `City_Near` / `City_Mid` parallax + `NeonGround`; per-level accent applied via SpriteRenderer color tint on ground/parallax to keep distinct without new backdrops for Near/Mid.

---

# Key Asset & Context

## Scripts (exact API — wire, do not modify)
- `GameManager`: `idlePanel/hudPanel/gameOverPanel` (GameObject); `scoreText/highScoreText/levelText/starsText/goScoreText/goBestText` (TextMeshProUGUI); `startPromptLabel/countdownText` (TextMeshProUGUI); `levelNumber/starsToComplete/levelTargetScore` (int); `nextLevelScene/mainMenuScene` (string); `levelCompletePanel` (GameObject), `levelCompleteText` (TextMeshProUGUI); `musicSource` (AudioSource); `player/spawner/cameraShake` (refs). Methods: `PlayAgain()`, `GoToMainMenu()` (Button OnClick), plus `StartGame/CollectStar/AddScore/ActivateSpeedRush` (code-called). `loadingBar` (Image) + `loadDuration` legacy — leave unwired.
- `PlayerController`: `jumpForce=10`, `doubleJumpForce=8.5`, `jumpCutMultiplier`, `crouchHeightScale`, `deathAnimDuration=0.9`, `deathSinkDistance=0.6`, `groundLayer`. Methods `TryJump()/SetCrouchHeld(bool)/Die()/ResetPlayer()`.
- `ObstacleSpawner`: `pipeObstaclePrefab/blockObstaclePrefab/laserObstaclePrefab`; `pipeWeight/blockWeight/laserWeight`, `laserHeight`; `minInterval/maxInterval`; `collectiblePrefabs[]`, `collectibleChance`, `collectibleMinY/MaxY`; `spawnX/groundY`.
- `Collectible`: `collectibleType {Star,SpeedRush}`, `starPoints`, `rushSpeedMultiplier/rushScoreMultiplier/rushDuration`, `pickupClip`, `pickupEffectPrefab`.
- `SpeedRushUI`: `alertRect`, `energyBarFill`, `barGroup`, `alertLabel`, `secondsLabel` (TMP_Text). Self-driven from `GameManager.Instance`.
- `SoundToggle`: `label` (TMP_Text), `icon`/`onSprite`/`offSprite` (optional). Method `Toggle()` (Button OnClick).
- `LevelSelectButton`: `levelNumber` (int), `sceneName` (string), `lockedOverlay` (GameObject), `button`, `statusLabel` (TMP_Text). Method `TryLoad()` (Button OnClick).
- `LevelProgress` (static): `IsUnlocked/Unlock/IsCompleted/MarkCompleted/ResetAll`.
- `MainMenu`: `firstLevelScene`. Methods `PlayScene(string)`, `PlayFirstLevel()`, `QuitGame()`.
- `BinaryBlock`: `spriteRenderer`, `label` (TextMeshPro world), `colorZero/colorOne`. `SetValue(int)`.

## Prefabs (Assets/PreFabs)
- `PipeObstacle` (jump) → re-skin with new low-barrier sprite, shrink collider.
- `BlockObstacle` (jump, BinaryBlock 0/1) → smaller; keep binary look.
- `LaserObstacle` (crouch) → lower spawn (`laserHeight`).
- `Star`, `SpeedRush`, `PickupSparkle` → keep; set `Star.starPoints≈100`.
- **New:** `Platform` prefab (SpriteRenderer + non-trigger BoxCollider2D on Ground layer + `ObstacleMover` killOnContact=false) so player can land on it while it scrolls.

## Art to generate (new)
1. **Obstacle — "Firewall" low barrier** (jump), ~1×1, neon, readable.
2. **Obstacle — "Data Cube"** small crate (works with BinaryBlock 0/1 overlay), ~0.8×0.8.
3. **Platform/ledge** neon slab, ~3 wide, clearly "standable".
4. (Optional) extra collectible variant for variety (e.g., gem) — reuse existing `Diamond`/`gemBlue` if generation is skipped.

## Audio (existing) — wire per level
`Music_Level1/2/3.wav` (per-level), `BGMusic.wav` (fallback), `error_001` (hit), `error_003` (game over), pickup `retro-coin`, SpeedRush `maximize_001`, jump `maximize_001`, double `maximize_004`, land `drop_001`, UI `click_001`/`confirmation_001`.

## Fonts
`PressStart2P-Regular SDF` (titles/countdown), `RobotoMono-Medium SDF` (body/help).

---

# Implementation Steps

> Execution role for all build steps = **developer** (you, in Execute mode). Explorer already
> completed discovery. "Parallelizable" notes which steps are independent.

### Step 0 — Generate new art
- **Description:** Generate the 3 new sprites (Firewall barrier, Data Cube, Platform slab) in the neon/binary style; import as Sprite (2D), pixels-per-unit consistent with existing obstacles, mesh/full-rect, point/no-compression to match the crisp look.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

### Step 1 — Scene cleanup & Build Settings
- **Description:** Delete `Assets/Scenes/SampleScene.unity` (+ `.meta`). Create empty `Assets/Scenes/LevelSelect.unity` placeholder (built in Step 7). Set Build Settings order to `MainMenu(0), LevelSelect(1), Level1(2), Level2(3), Level3(4)`. Verify nothing references SampleScene (grep confirmed none).
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

### Step 2 — Player Animator: Death exit safety
- **Description:** In `PlayerAnimator.controller`, add a `Death → Idle` transition with condition `isDead == false`, `HasExitTime = false`, duration 0. Confirm `Any State → Death` on `isDead == true` (no exit time) and Death clip length; set `PlayerController.deathAnimDuration` to the Death clip length (~0.9s, 4 frames). Verify Idle↔Run↔Jump↔Death wiring intact.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

### Step 3 — Countdown UI + remove loading bar (per Level scene)
- **Description:** In each of Level1/2/3 Canvas, add a centered `CountdownText` TMP (Press Start 2P, large), initially inactive; wire to `GameManager.countdownText`. Disable/delete `IdlePanel > LoadingBarPanel` and leave `GameManager.loadingBar` unwired. Keep `startPromptLabel` = PressToStartLabel ("Press Space to Start").
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes (per scene)

### Step 4 — Game Over restart panel (per Level scene)
- **Description:** On each `GameOverPanel`, add **Play Again** button (OnClick → `GameManager.PlayAgain`) and **Main Menu** button (OnClick → `GameManager.GoToMainMenu`), keep the "Press Space to Play Again" hint (already baked into `goScoreText`/`goBestText`). Follow existing button pattern (Image+Outline+Button+child TMP).
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

### Step 5 — HUD wiring: stars + speed-rush seconds (per Level scene)
- **Description:** Wire `GameManager.starsText` → `StarCounter` TMP. Add a `secondsLabel` TMP under `SpeedRushUI_Container` and wire `SpeedRushUI.secondsLabel`; hide `EnergyBarBG`/`EnergyBarFill` (energy bar removed). Verify `SpeedRushLabel` → `alertLabel`, `alertRect` set. Confirm `FPSCounter.label` wired.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

### Step 6 — Obstacles, platforms & difficulty tuning (prefabs + per Level)
- **Description:**
  - Re-skin `PipeObstacle` (Firewall) and `BlockObstacle` (Data Cube) with new sprites; shrink BoxCollider2D to match; keep BinaryBlock 0/1.
  - Lower `ObstacleSpawner.laserHeight` (~1.0–1.1) so crouch clears the laser cleanly.
  - Set `Star.starPoints ≈ 100` so stars reward score.
  - Tune for "easier & nicer": keep `jumpForce=10`; set player `Rigidbody2D.gravityScale ≈ 3.5` for a snappy arc; verify a normal jump clears ground obstacles and crouch clears the laser.
  - Build **Platform** prefab (non-trigger collider on Ground layer + `ObstacleMover` killOnContact=false) with stars placed above as optional routes.
  - **New code (additive, flagged):** add `PlatformSpawner.cs` — a small MonoBehaviour mirroring `ObstacleSpawner`'s interval pattern to occasionally spawn the Platform prefab; place on a `PlatformSpawner` scene object. *(Does not modify any existing script. If you prefer zero new code, fallback = manually place a few scrolling platforms per level — note they won't infinitely respawn.)*
  - Per-level spawner: set `collectibleChance` = 0.50 / 0.45 / 0.40; keep SpeedRush chevron rarer than gold stars (via prefab order/weight in `collectiblePrefabs`).
- **Assigned role:** developer
- **Dependencies:** Step 0 (art)
- **Parallelizable:** Partly (collider/tuning after art)

### Step 7 — Level Select scene
- **Description:** Build `LevelSelect.unity`: parallax background, title, three cards each with `LevelSelectButton` (`levelNumber` 1/2/3, `sceneName` "Level1/2/3", `lockedOverlay` padlock, `statusLabel`), Button OnClick → `TryLoad()`. Card info text: theme + ground + difficulty + "Collect N stars". Back button → `MainMenu.PlayScene("MainMenu")` (or load MainMenu). Add SoundToggle + Help. EventSystem with InputSystemUIInputModule.
- **Assigned role:** developer
- **Dependencies:** Step 1 (scene + build settings)
- **Parallelizable:** No (depends on Step 1)

### Step 8 — Main Menu updates
- **Description:** Add **Level Select** button (and repoint **Play** → `MainMenu.PlayScene("LevelSelect")`), a SoundToggle button (top-right), and a Help button + Help panel. Keep Quit.
- **Assigned role:** developer
- **Dependencies:** Step 1
- **Parallelizable:** Yes (after Step 1)

### Step 9 — Help panels + Sound toggle (per Level scene)
- **Description:** Add a `HelpPanel` (inactive) to each Level Canvas with controls/goal/legend text + Close button; add a Help button (HUD top area) → `HelpPanel.SetActive(true)`, Close → `SetActive(false)`. Add a SoundToggle button (top-right) with `SoundToggle` component → OnClick `Toggle()`, label "Sound: On/Off". Add SoundToggle inside Help panel too.
- **Assigned role:** developer
- **Dependencies:** None (UI), reuse pattern from Step 8
- **Parallelizable:** Yes (per scene)

### Step 10 — Per-level GameManager fields + theming + music
- **Description:** Set per scene: `levelNumber` 1/2/3; `starsToComplete` 5/10/15; `levelTargetScore` 0; confirm `nextLevelScene` Level2/Level3/"" and `mainMenuScene`="MainMenu"; wire `countdownText`, `starsText`, `levelCompletePanel`/`levelCompleteText`. Apply theme: assign `City_L1/L2/L3_Far` to BG_Far; apply per-level accent tint to ground/parallax. Assign `Music_Level1/2/3.wav` to each GameManager AudioSource clip (loop, vol ~0.3).
- **Assigned role:** developer
- **Dependencies:** Steps 3, 5 (UI objects must exist to wire)
- **Parallelizable:** Per scene

### Step 11 — Engagement enhancements (verify existing scaffolding)
- **Description:** Verify/wire already-present systems: `PauseMenu`+`PausePanel` (Esc → pause; Resume/Menu buttons); `HudExtras`+`CollectiblePickupRelay`+`CollectibleEvents` pickup "+pop" text on star/chevron; `SpeedRushScreenFX`+`SpeedRushCamera` (vignette/chromatic during rush); `TouchInput` on GameManager (mobile-ready). These need no new code — confirm references are assigned.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

### Step 12 — Build & playtest
- **Description:** Full flow validation (see below).
- **Assigned role:** developer
- **Dependencies:** All prior
- **Parallelizable:** No

---

# Verification & Testing

## Manual flow checks
- **Start each level:** Idle shows "Press Space to Start"; press → countdown 3-2-1-GO! (centered, pop) → player runs. No leftover loading bar.
- **Play:** Idle pose on start screen, Run during play, Jump arc; normal jump clears ground obstacles; crouch (S/↓) clears lowered laser cleanly.
- **Stars/HUD:** `★ x / y` updates; collecting the level's star count → Level Complete → loads next level; Level 3 → YOU WIN → MainMenu.
- **Speed Rush:** chevron → "SPEED RUSH" alert + seconds countdown (e.g. "3s"); unlimited jumps while active; energy bar gone.
- **Death:** hit → freeze → death anim + sink (~0.9s) → Game Over panel. **Play Again** reloads clean (Idle→Run→Jump→Death correct on replay, starts at level beginning); **Main Menu** returns to menu.
- **Level Select:** L1 unlocked; L2/L3 locked (padlock) until previous completed; persists after closing the game (PlayerPrefs). Flow MainMenu → Level Select → Level.
- **Sound toggle:** mutes/unmutes everywhere and persists across scenes.
- **Help:** opens panel explaining controls + goal + item legend; closes.
- **SampleScene:** deleted and not in Build Settings.

## Automated / spot checks
- Play Mode test: enter Level1, simulate jump input, assert `GameManager.IsPlaying`, simulate star pickups to `starsToComplete`, assert `CompleteLevel`/scene change.
- Confirm `LevelProgress.IsUnlocked(2)` is false on fresh prefs, true after completing Level1.
- Console clean of NullReferenceExceptions on start/death in every level (GameManager `player`/`spawner` are wired).

## Acceptance checklist (mirrors brief §5)
- [ ] First play AND every replay: correct Idle/Run/Jump/Death, no stuck poses.
- [ ] Countdown 3-2-1-GO; no loading bar.
- [ ] Star count clears level; L3 → YOU WIN → menu.
- [ ] L2/L3 locked until previous completed (persists).
- [ ] Speed Rush seconds countdown; unlimited jumps while active.
- [ ] Sound toggle mutes/unmutes everywhere and persists.
- [ ] Help panel explains controls + goal.
- [ ] SampleScene deleted and not in Build Settings.

---

# Open Items / Risks
- **PlatformSpawner.cs is the only new code.** It's additive and does not modify existing scripts. If you want strictly zero new code, fallback to manually placing a few scrolling platforms per level (no infinite respawn). Flag your preference.
- **Star `starPoints`** will be set to ~100 (currently 0) so stars also reward score — say if you'd rather stars only count toward the goal.
- **Per-level accent tint** is applied via SpriteRenderer color (cheap, distinct). If you'd rather have fully new per-level Near/Mid backdrops, that's an extra art-gen pass.

# Future polish (optional, propose before doing)
Score/star "pop" already scaffolded (HudExtras) — extend to a combo meter; settings screen (volume sliders); richer per-level music transitions; full mobile touch UI; leaderboard.
