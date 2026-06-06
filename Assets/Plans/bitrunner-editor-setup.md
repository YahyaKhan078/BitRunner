# BitRunner — Editor Setup, Levels & Polish (full scope)

> **Editor-only work.** All gameplay logic is already written. Do NOT modify any C# scripts.
> If a script change ever seems necessary, STOP and ask first. Match the exact component
> field names and Animator parameter names documented in `Assets/CLAUDE.md`.

---

# Project Overview
- **Game Title:** BitRunner
- **High-Level Concept:** A cyberpunk-neon 2D endless runner — auto-run right, jump/double-jump over obstacles, crouch under lasers, grab collectibles, survive escalating speed across 3 levels then win.
- **Players:** Single player.
- **Inspiration / Reference Games:** Subway Surfers / Canabalt pacing; cyberpunk-neon city aesthetic (per attached reference images).
- **Tone / Art Direction:** Cyberpunk neon — dark city skyline, glowing magenta/cyan/orange accents, bloom-heavy. **(AI-generated art, distinct background per level.)**
- **Target Platform:** StandaloneWindows64 (PC). Mobile touch input added in Section 7.
- **Screen Orientation / Resolution:** Landscape, reference 1920×1080 (Canvas Scale With Screen Size, match 0.5).
- **Render Pipeline:** URP (2D Renderer — `Assets/Settings/Renderer2D.asset`).

---

# Discovery Summary (verified current state)

| Area | Reality on disk | Implication |
|---|---|---|
| **PlayerAnimator.controller** | Already correct: params `isRunning,isGrounded,isDead`(Bool),`velocityY`(Float),`isCrouching`(Bool); default state **Idle**; transitions Idle↔Run (`isRunning`), (Idle/Run)→Jump (`isGrounded` false), Jump→Run (`isGrounded` true), **AnyState→Death** (`isDead`); all Has Exit Time=false, dur=0. | **Controller does NOT need rebuilding.** |
| **Animation clips** | `Idle/Run/Jump/Death.anim` each only **0.083s = 1 frame**. This is the real "stuck on one frame" bug. | **Rebuild the 4 clips** with full sprite frames. |
| **Player sprite sheets** | `Astronaut_Idle`(6), `Run`(6), `Jump`(5), `Death`(4) — already grid-sliced, named `_0.._n`, PPU 100, Point filter. | Ready to author clips directly. |
| **Loading bar** | `BarFill` already Filled/Horizontal/Left, assigned to `GameManager.loadingBar`. BUT a competing **`LoadingBar.cs`** on `LoadingBarPanel` also animates BarFill; `GameManager.startPromptLabel` = **None**; `LoadingLabel` has hard-coded "Hold on...". | Disable LoadingBar.cs; wire startPromptLabel; clear text. |
| **Parallax / Bloom** | `CameraShake.backgroundLayers`=[BG_Far,BG_Mid,BG_Near], `parallaxSpeeds`=[0.15,0.35,0.6]. `GlobalVolume` exists with **Bloom + Vignette + ColorAdjustments** (`BitRunnerPostProcess.asset`). Camera ortho size already 4. | Wired; just swap in new neon art + per-level palettes. |
| **GameManager level fields** | `levelTargetScore=0`, `nextLevelScene=""`, `levelCompletePanel=None`, `levelCompleteText=None`. No LevelCompletePanel object exists. | Build LevelComplete UI + set per-level values. |
| **Scenes / Build** | Only `SampleScene.unity` in build (index 0). No MainMenu, no Level1/2/3. | Create all from scratch. |
| **MainMenu.cs** | Fields: `firstLevelScene` (string, default "Level1"); methods `PlayFirstLevel()`, `PlayScene(name)`, `QuitGame()`. | Wire buttons to these. |
| **Art gap** | No cyberpunk-city art; only `blue_grass.png` + green grass ground spritesheet. No green-chevron sprite. | **Generate AI art** (3 bg sets, ground, chevron). |
| **PipeObstacle prefab** | Has NO `ObstacleMover` (others do). | Flag — may need mover to scroll. Confirm before adding. |

---

# Game Mechanics
## Core Gameplay Loop
Auto-run → jump/double-jump over pipe/block obstacles, crouch under lasers, collect Stars (score) and SpeedRush (temp boost + unlimited multi-jump) → base speed ramps over time → reach `levelTargetScore` to complete level → next level (harder) → after Level3, "YOU WIN" → menu. Death freezes the world (`timeScale=0`), death animation plays on unscaled time, then Game Over panel.

## Controls and Input Methods
- **Jump / start / restart:** Space / W / ↑ (double-jump; unlimited during SpeedRush).
- **Crouch:** S / ↓ (hold) — shrinks collider to clear lasers.
- **Mobile (Section 7):** tap = jump, swipe down = crouch.

---

# UI
- **MainMenu scene:** full-screen neon city bg, "BIT RUNNER" title (Press Start 2P), **Play** + **Quit** buttons centered.
- **IdlePanel (in levels):** neon city bg, "BIT RUNNER" title, neon-outlined loading bar + centered prompt ("Press Space to Start" → "Hold on...").
- **HUDPanel:** top-left Score/Level/Best stack, top-right FPS, SpeedRush alert + energy bar, (Section 7) coin/star counter + score-pop.
- **GameOverPanel:** centered GAME OVER + score/best (baked by script).
- **LevelCompletePanel (new):** centered overlay "LEVEL COMPLETE" / "YOU WIN" + TMP text.

---

# Key Asset & Context

## Exact field/parameter names (do not rename)
- **GameManager:** `loadingBar`(Image), `startPromptLabel`(TMP), `levelTargetScore`(int), `nextLevelScene`(string), `levelCompletePanel`(GameObject), `levelCompleteText`(TMP), plus existing `player, spawner, cameraShake, idlePanel, hudPanel, gameOverPanel, scoreText, highScoreText, levelText, goScoreText, goBestText, musicSource, gameOverClip, loadDuration, startSpeed, maxSpeed, speedGainPerSecond, scoreRate`.
- **PlayerController:** `deathAnimDuration`(float), `jumpForce, doubleJumpForce, groundCheck, groundCheckRadius, groundLayer, speedTrail, trailTimeMin/Max, trailWidthMin/Max, crouchKey, crouchHeightScale`.
- **ObstacleSpawner:** `pipeObstaclePrefab, blockObstaclePrefab, laserObstaclePrefab, pipeWeight, blockWeight, laserWeight, minInterval, maxInterval, laserHeight, spawnX, groundY, collectiblePrefabs[], collectibleChance, collectibleMinY/MaxY`.
- **CameraShake:** `backgroundLayers[]`, `parallaxSpeeds[]`.
- **Animator params:** `isRunning, isGrounded`(Bool), `isDead`(Bool), `velocityY`(Float), `isCrouching`(Bool).
- **MainMenu:** `firstLevelScene` → set "Level1".

## Assets to GENERATE (AI, cyberpunk-neon, match reference images 62980/62988/62994)
Per level a distinct background set (far skyline, mid buildings, near foreground), plus shared:
- **Backgrounds:** `City_L1_{Far,Mid,Near}`, `City_L2_{Far,Mid,Near}`, `City_L3_{Far,Mid,Near}` — wide tileable, wrap=Repeat, dark with neon accents (L1 cyan, L2 magenta, L3 orange/red).
- **Ground:** `NeonGround` tile — seamless horizontal tiling, neon edge glow.
- **Collectible:** `SpeedChevron` — green chevron/arrow sprite for SpeedRush pickup.
- (Reuse existing: `star_0` gold for Star, `Obstacle_3_0` red for laser, `boxCrate_0` for block, sparkle flipbook.)

## Existing assets reused
- Fonts: `Assets/Fonts/PressStart2P-Regular SDF.asset` (title), `RobotoMono-Medium SDF` (HUD).
- Audio: `BGMusic.wav`, `error_001/003`, `maximize_001/004`, `drop_001`, retro-coin mp3.
- Post FX: `Assets/Profiles/BitRunnerPostProcess.asset` (Bloom/Vignette/ColorAdjustments).
- Prefabs: `PipeObstacle, BlockObstacle, LaserObstacle, Star, SpeedRush, PickupSparkle`.

---

# Implementation Steps

### Step 1 — Generate cyberpunk-neon art assets
- **Description:** Generate AI sprites matching reference images (InstanceIDs 62980/62988/62994): 3 background sets (Far/Mid/Near each), `NeonGround` tile, `SpeedChevron` (green). Import as Sprite, PPU 100, wrap=Repeat (backgrounds/ground), Point or Bilinear per look. Place under `Assets/Sprites/BackGround/` and `Assets/Sprites/Ground|Collectibles/`.
- **Assigned role:** developer (asset generation)
- **Dependencies:** None
- **Parallelizable:** Yes (independent of code/scene wiring until Steps 3,6,7)

### Step 2 — FIX Player animation clips (highest priority)
- **Description:** The controller is fine; the **clips are 1-frame**. Rebuild `Assets/Animations/Idle.anim, Run.anim, Jump.anim, Death.anim` so each cycles its full sprite frames on the `SpriteRenderer.sprite` curve:
  - Idle = `Astronaut_Idle_0..5` (loop), Run = `Astronaut_Run_0..5` (loop), Jump = `Astronaut_Jump_0..4` (no loop), Death = `Astronaut_Death_0..3` (no loop).
  - Use a sensible sample rate (e.g. 10–12 fps): set keyframe spacing so each clip reads clearly (Idle/Run ~0.6–1.0s loop, Jump ~0.4s, Death ~0.5–0.8s).
  - Keep Run `isLooping=true`; Death/Jump `isLooping=false`.
  - Do NOT touch `PlayerAnimator.controller` (params/states/transitions already match).
- After rebuilding, **set `PlayerController.deathAnimDuration` = Death clip length** (so Game Over appears as death finishes). Leave Animator on Unscaled Time (script forces it at Awake).
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

### Step 3 — Loading bar wiring (GameManager owns it)
- **Description:** In `SampleScene` IdlePanel:
  - **Disable** (uncheck) the `LoadingBar` component on `LoadingBarPanel` (do not delete the GameObject — keep BarBG/BarFill).
  - Confirm `BarFill` Image = Filled / Horizontal / Left, fillAmount 0; assigned to `GameManager.loadingBar` (already true).
  - Assign a TMP label to `GameManager.startPromptLabel` — use `PressToStartLabel` (set it active) and **clear the hard-coded "Hold on..."** text on `LoadingLabel` (or repurpose one label; only one is needed since the script swaps the text). Recommended: make `PressToStartLabel` the single prompt label, hide/disable `LoadingLabel`.
  - Style: neon-outlined bar (BarBG dark + neon border via Outline/sprite), BarFill neon color, title "BIT RUNNER" (Press Start 2P), bar+prompt centered.
- **Verify:** start screen reads "Press Space to Start"; on press → "Hold on...", bar fills 0→100% over ~1.5s, run begins.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes

### Step 4 — Background, parallax & bloom polish (SampleScene)
- **Description:** Swap the neon art into the existing parallax rig:
  - Assign generated `City_L1_Far/Mid/Near` to `BG_Far/Mid/Near` SpriteRenderers (slowest=Far). `CameraShake.backgroundLayers`/`parallaxSpeeds` already wired (0.15/0.35/0.6) — tune if needed.
  - Set `NeonGround` on Ground_1/2/3 SpriteRenderers (keep `GroundScroller` + colliders).
  - Confirm `GlobalVolume` Bloom is on (already present); tune intensity/threshold for neon glow.
  - Keep camera ortho size ~4 (already tightened); adjust slightly if framing needs it.
- **Assigned role:** developer
- **Dependencies:** Step 1
- **Parallelizable:** No (needs art)

### Step 5 — Create MainMenu scene
- **Description:** New scene `Assets/Scenes/MainMenu.unity`:
  - Camera + URP, Canvas (Scale With Screen Size 1920×1080, match 0.5), EventSystem (InputSystemUIInputModule).
  - Full-screen neon city bg image, "BIT RUNNER" title (Press Start 2P), **Play** and **Quit** Buttons (neon-styled).
  - Empty GameObject "MainMenu" with `MainMenu.cs`; set `firstLevelScene` = "Level1". Wire Play button OnClick → `MainMenu.PlayFirstLevel()`, Quit → `MainMenu.QuitGame()`.
  - Add a GlobalVolume referencing `BitRunnerPostProcess.asset` for consistent glow.
- **Assigned role:** developer
- **Dependencies:** Step 1 (bg art)
- **Parallelizable:** Partially (layout can start before art; art assignment needs Step 1)

### Step 6 — Build LevelComplete UI + finalize gameplay scene as Level1
- **Description:** In `SampleScene` (which becomes Level1):
  - Add a **LevelCompletePanel** under Canvas (centered overlay) with a TMP child for "LEVEL COMPLETE / YOU WIN" text. Route through the same exclusive panel pattern (it's an overlay the script enables/fills).
  - Assign `GameManager.levelCompletePanel` = the panel, `GameManager.levelCompleteText` = its TMP.
  - Confirm ALL GameManager refs assigned (loadingBar, startPromptLabel, levelTargetScore, nextLevelScene, levelCompletePanel/Text, player, spawner, cameraShake, panels, HUD/GO text, musicSource, gameOverClip).
  - Save As `Assets/Scenes/Level1.unity`. Set Level1: `levelTargetScore=5000`, `nextLevelScene="Level2"`.
- **Assigned role:** developer
- **Dependencies:** Steps 2,3,4 (player/bar/bg done in the base scene first)
- **Parallelizable:** No

### Step 7 — Create Level2 & Level3 (duplicate + escalate, distinct bg)
- **Description:** Duplicate Level1 → `Level2.unity`, `Level3.unity`. Per level:
  - **Backgrounds:** assign `City_L2_*` (magenta) to Level2, `City_L3_*` (orange/red) to Level3 BG layers; tune Volume ColorAdjustments per palette.
  - **GameManager:** Level2 `levelTargetScore=10000`, `nextLevelScene="Level3"`; Level3 `levelTargetScore=15000`, `nextLevelScene=""` (→ YOU WIN → menu).
  - **Difficulty escalation (ObstacleSpawner):** lower min/maxInterval each level (e.g. L1 0.8–1.6 → L2 0.7–1.3 → L3 0.6–1.1), raise `laserWeight` and `collectibleChance`, add more collectible prefab entries. Keep `laserHeight≈1.45` so standing player is hit but crouch clears.
  - Re-verify all GameManager refs in EACH scene (refs can break on duplicate).
- **Assigned role:** developer
- **Dependencies:** Steps 1,6
- **Parallelizable:** Level2/Level3 can be set up in parallel after Level1 exists

### Step 8 — Build Settings registration
- **Description:** Set EditorBuildSettings scene order: index0 `MainMenu`, 1 `Level1`, 2 `Level2`, 3 `Level3` (all enabled). Remove SampleScene if superseded by Level1 (or keep as scratch, unchecked).
- **Assigned role:** developer
- **Dependencies:** Steps 5,6,7
- **Parallelizable:** No

### Step 9 — Collectibles / obstacles polish
- **Description:**
  - SpeedRush prefab → use generated **green `SpeedChevron`** sprite (keep green tint/Collectible/ObstacleMover, killOnContact=false).
  - Star → gold (`star_0`, already).
  - Laser (`LaserObstacle`) → neon red; verify `laserHeight≈1.45` (standing hit, crouch clears); keep `SpriteFlipbook` animation, fit BoxCollider2D.
  - Optionally animate sparkle/laser via `SpriteFlipbook`.
  - **Flag:** `PipeObstacle` lacks `ObstacleMover` — confirm with user whether to add one (other obstacles have it). Do not change without approval if it implies prefab-structural change beyond editor wiring.
- **Assigned role:** developer
- **Dependencies:** Step 1 (chevron)
- **Parallelizable:** Yes (after art)

### Step 10 — Section 7 enhancements (full scope, propose before structural changes)
> These require NEW scripts or significant additions. Per the "tell me before script changes" rule, **each sub-item that needs a script will be confirmed with the user before implementation.**
- **10a Score "pop" + coin/star counter HUD:** add TMP counter to HUDPanel; score-pop needs a small script — **propose first**.
- **10b Per-level music:** assign distinct music clips to each level's `musicSource` (editor-only, no script). Source/import clips.
- **10c Pause menu (Esc):** new pause UI + small pause script — **propose first** (script needed).
- **10d Object pooling (obstacles/ground):** refactor spawner/scroller to pooling — **script change, propose first**.
- **10e Speed-Rush screen FX:** drive chromatic aberration / speed lines from `SpeedRushActive` — Volume override is editor-only, but binding may need a small script — **propose first**.
- **10f Mobile touch input:** tap=jump, swipe down=crouch — Input handling is in PlayerController; adding touch likely needs a script — **propose first**.
- **10g Camera follow/zoom on Speed Rush:** small script tied to `SpeedRushActive` — **propose first**.
- **Assigned role:** developer (+ user approval gate for each script item)
- **Dependencies:** Steps 1–9
- **Parallelizable:** Item-dependent

---

# Verification & Testing
1. **Animations (Play Mode):** Idle plays/loops on start screen → Run loops on play → Jump arc on jump → on hit, full 4-frame Death plays once, brief pause, THEN Game Over panel. Confirm Game Over timing = `deathAnimDuration` ≈ Death clip length.
2. **Loading bar:** Start screen shows "Press Space to Start"; press → "Hold on...", bar fills 0→100% in ~1.5s, run starts. No flicker/fighting (LoadingBar.cs disabled).
3. **Parallax/Bloom:** Layers scroll at different speeds (Far slowest); neon sprites glow via Bloom; framing correct at ortho ~4.
4. **MainMenu:** Play → loads Level1; Quit → exits play mode (editor) / quits (build).
5. **Level flow:** Level1 reaching 5000 → LevelComplete → Level2; 10000 → Level3; 15000 → "YOU WIN" → MainMenu (scene 0). Score resets each level.
6. **Difficulty:** Each level spawns faster, more lasers/collectibles; crouch clears lasers, standing gets hit.
7. **Refs audit (every scene):** Run a read-only check that GameManager.loadingBar, startPromptLabel, levelTargetScore, nextLevelScene, levelCompletePanel/Text, player, spawner, cameraShake, panels, HUD/GO texts, musicSource, gameOverClip are all assigned (catches the OPEN null-ref risk noted in CLAUDE.md).
8. **Build Settings:** MainMenu(0), Level1(1), Level2(2), Level3(3), all enabled.
9. **Section 7:** verify each implemented item individually (pause toggles timeScale, counters increment, mobile input on device/simulator, etc.).

---

# Open Questions / Flags for User
- **PipeObstacle** has no `ObstacleMover` (Step 9) — add one? (others have it).
- **Per-level music (10b):** provide/choose clips, or generate?
- **Section 7 script items (10a,c,d,e,f,g):** each will be confirmed before any script is written.
