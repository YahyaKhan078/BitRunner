# BitRunner — Bug Fixes + Geometry Dash / Temple Run Enhancement Roadmap

> Two parts. **Part A** = the immediate fixes from your screenshots (root causes already
> diagnosed via read-only inspection — exact, not guesses). **Part B** = a phased plan to
> evolve BitRunner toward Geometry-Dash / Temple-Run quality as a 2D runner.
> Existing Claude-written scripts are respected; new behaviour is added via small additive
> scripts unless a one-line edit is clearly safer (each such edit is flagged).

---

# PART A — Immediate Fixes (from the 4 screenshots)

## A1. "Press Space to Start" prompt missing (Screenshot 3)
- **Root cause:** `PressToStartLabel` lives **inside `IdlePanel > LoadingBarPanel`**. In the
  previous pass I deactivated `LoadingBarPanel` to remove the loading bar — which also hid the
  prompt. `GameManager.startPromptLabel` still points at it and calls `SetActive(true)`, but the
  inactive parent keeps it invisible.
- **Fix:** Re-parent `PressToStartLabel` from `LoadingBarPanel` up to `IdlePanel` (direct child),
  center it below the "BIT RUNNER" title (anchoredPos ≈ (0,-220), size ~1200×90, ~36px Press
  Start 2P, cyan). Keep `GameManager.startPromptLabel` pointing at it (reference survives
  re-parenting). Apply to **Level1/2/3**.
- **Optional polish:** add a gentle alpha/scale pulse so it reads as "interactive".

## A2. Star counter shows `□` instead of `★` (Screenshot 4 + console warning)
- **Root cause:** `GameManager` writes `"★ x / y"`, but `PressStart2P-Regular SDF` has no `★`
  (U+2605) glyph → TMP renders tofu `□`. Console: *"character \u2605 not found in
  [PressStart2P-Regular SDF]… replaced by \u25A1"*.
- **Fix (cleanest):** Add `LiberationSans SDF` (which contains `★`) to the **fallback list** of
  the `PressStart2P-Regular SDF` asset (`fallbackFontAssetTable`). Pixel digits stay pixel; only
  the missing `★` falls back. One asset change, fixes every scene.
- **Alt (if you prefer no shared-asset change):** point only `StarCounter`'s font to a star-
  capable font, or swap `★` for a small TMP sprite. Recommend the fallback approach.

## A3. Settings / Play / Quit visible through the Help panel (Screenshot 1)
- **Root cause (confirmed child order):** `Canvas` children are … `SoundButton`(4),
  `HelpPanel`(5), `HelpButton`(6), `SettingsPanel`(7), `SettingsButton`(8). Buttons after a panel
  render **on top of** it, so opening Help/Settings leaves buttons (and the other panel's button)
  bleeding through.
- **Fix:** `SetAsLastSibling()` on `HelpPanel` and `SettingsPanel` (both become topmost, after all
  buttons). They're full-screen opaque and mutually exclusive, so each fully covers the menu when
  open. Their own Close/Sound child buttons still render correctly (children draw after parent).
- **Levels:** verified the level Help panels are already last-sibling — no bleed there. Just fix
  MainMenu.

## A4. Level Select fonts don't match Main Menu (Screenshot 2)
- **Root cause:** Every LevelSelect text is **`LiberationSans SDF`** (the TMP default), not
  `Press Start 2P` — the font asset reference came back null when that scene was built, so TMP
  used the default. Main Menu correctly uses Press Start 2P, hence the mismatch.
- **Fix:** Reassign `PressStart2P-Regular SDF` to all LevelSelect texts (Title, per-card Header /
  Theme / Info / Play label / Lock label, Back, Sound) with sizes tuned for the pixel font
  (Title ~58, Header ~36, Theme ~20, Info ~16 with word-wrap, buttons auto-size). Keep the
  per-level accent colors. Result: visually consistent with the rest of the game.

## A5. Obstacles float, are too big, and the game is too hard (Screenshot 4)
Measured: player feet rest at **y = -2.45** (the visual ground line). Spawner uses `groundY=-3`
and lifts blocks by a random `0–0.5`, so the Data Cube hovers above the line; scales are large
(Pipe 1.44, Block 1.37); intervals are tight (0.8–1.6 s).
- **Stick to ground:** new additive **`GroundSnap.cs`** on `PipeObstacle` & `BlockObstacle`. On
  enable (pooling-safe) it shifts the object so its **collider bottom = surface Y (-2.45)** —
  exact ground contact regardless of size/scale, and it makes the block's random lift irrelevant.
  (Laser is a crouch-under hazard and stays elevated — not snapped.)
- **Smaller obstacles:** Pipe scale 1.44 → ~1.0, Block 1.37 → ~0.95 (colliders already fit; they
  scale together).
- **Laser fairness:** set `laserHeight` so a standing player is hit but a crouch clears with
  margin (≈1.4 → beam center ~-1.6; standing top -1.55 hits, crouched top ~-1.96 clears).
- **Spawn frequency (your spec):**
  | Level | minInterval | maxInterval |
  |---|---|---|
  | 1 | 4.0 | 5.0 |
  | 2 | 3.0 | 4.0 |
  | 3 | 2.0 | 3.0 |
  Note: `ObstacleSpawner.SetNextSpawnTime()` already shrinks intervals as speed rises (down to
  ~55–65%), so each level still ramps up — but starts at your comfortable spacing.
- **Keep stars flowing despite fewer spawns:** raise `collectibleChance` (L1 0.9 / L2 0.75 /
  L3 0.6) and set `collectiblePrefabs = [Star, Star, Star, SpeedRush]` so the green chevron stays
  rarer than gold stars (currently it's a 50/50 pick). PlatformSpawner also keeps dropping stars
  on ledges.

## A6. Jump feel (your question — "is the jump too hard?")
Yes — a few cheap, well-known "game feel" tweaks make a runner feel fair without making it easy.
**Recommended (additive fields on `PlayerController`, small edits):**
- **Coyote time** (~0.10 s): allow a jump for a brief moment after leaving a ledge/ground.
- **Jump buffering** (~0.12 s): if the player presses jump just before landing, it fires on land.
- **Slightly floatier arc:** drop `gravityScale` 3.5 → ~3.1, and/or a small `fallMultiplier` so
  rising is forgiving but falling stays snappy (GD-style).
These three changes are the single biggest perceived-fairness upgrade. *(These touch
`PlayerController` — flagged. If you'd rather not edit it, I'll do the gravity-only tune via the
Inspector and add coyote/buffer in a separate opt-in script.)*

**Part A acceptance:** prompt visible on every idle screen; `★ x / y` renders correctly;
Help/Settings fully cover the menu; LevelSelect uses Press Start 2P; obstacles sit on the ground,
are smaller, spaced per spec; jump feels forgiving.

---

# PART B — Geometry Dash / Temple Run Roadmap (2D)

## Where BitRunner is today
A solid, complete random-spawn runner: jump/double-jump/crouch, speed ramp, stars-to-clear,
SpeedRush, combo, pause, level select, save/unlock, help/sound/settings. What separates it from
GD/Temple-Run polish: **handcrafted rhythm, mechanical variety, power-ups, juice, and meta
progression**. Proposals below, grouped by phase with effort and new-vs-edit notes. Each phase is
independently shippable; pick the scope you want.

### Phase 1 — Feel & Fairness (foundation; do first)
1. **Jump feel** — coyote time + buffering + gravity tune (Part A6). *Edit PlayerController.*
2. **Level progress bar (GD)** — a top bar that fills toward the star goal (or distance), with a
   moving "ship" icon. Huge feedback boost. *New `LevelProgressBar.cs` reading
   `GameManager.StarsCollected/StarsToComplete`; UI added per level.*
3. **Juice pass** — landing squash, death burst particles + brief hit-stop, jump/collect screen
   flashes, beat-ish background pulse. Most via existing `CameraShake`/particles + a small
   `JuiceFX.cs`. *Mostly new additive FX, no core edits.*
4. **Attempt counter + instant restart (GD)** — show "Attempt N"; on death, fast-restart (skip or
   shorten countdown on retry). *Small `GameManager` option or additive counter.*

### Phase 2 — Handcrafted Patterns (the Geometry-Dash core)
5. **Obstacle pattern chunks** — replace pure random with a pool of **designed sequences**
   (ScriptableObject `ObstaclePattern` = ordered list of {type, gap, height}). Spawner picks
   fair, readable, escalating patterns; difficulty curated per level. This is what makes GD feel
   intentional instead of random. *New `ObstaclePattern` SO + `PatternSpawner.cs` (can coexist
   with / replace `ObstacleSpawner`).*
6. **New obstacle mechanics:**
   - **Spikes** (instant-death, clearly telegraphed).
   - **Pits/gaps (Temple Run)** — fall = death; platforms become required crossings.
   - **Jump pads / orbs (GD)** — bounce the player higher; introduces vertical play.
   - **Gravity-flip portals (GD)** — flip gravity for a section (advanced; Level 3 / endless).
   - **Moving saws / rotating hazards.**
   *New small scripts per mechanic; reuse `ObstacleMover`.*

### Phase 3 — Power-ups & Collectible Depth (Temple Run flavor)
7. **Power-ups** extending the SpeedRush system:
   - **Magnet** — auto-pulls nearby stars for a few seconds.
   - **Shield** — survive one hit (consumes instead of dying).
   - **Score x2** — temporary multiplier.
   *New `PowerUp` types + handlers; HUD timers like the SpeedRush seconds label.*
8. **Star trails & coin lines** — stars arranged in arcs that reward good jumps (Temple Run).
   *Pattern data + spawner.*

### Phase 4 — Meta Progression & Replayability
9. **3-star level rating** (mobile/GD) — rate each level run (e.g., cleared / no-death / all-stars)
   and show stars on the Level Select cards. Strong replay hook. *Extend `LevelProgress` keys +
   results screen.*
10. **Results screen** after each level — stars earned, score, best, combo, next/retry.
    *New panel + small controller.*
11. **Missions / daily objectives (Temple Run)** — "collect 30 stars total", "clear L2 without
    dying", "reach x2 combo". *New `Missions.cs` + PlayerPrefs.*
12. **Endless mode + leaderboard** — unlocked after clearing L3; distance-based scoring, best
    distance saved. *New scene/mode reusing PatternSpawner with infinite escalation.*
13. **Character/skin shop** — spend accumulated stars on player skins (Temple Run/GD). *New shop
    UI + `SkinManager.cs`.*

### Phase 5 — Audio-reactive & Theming (GD signature)
14. **Beat-synced spawning & visuals** — drive pattern beats and background pulse from the BGM
    tempo so the level "dances" with the music. *New `BeatClock.cs`.*
15. **Distinct per-level mechanics & palettes** — e.g., L1 jumps, L2 adds lasers+pits, L3 adds
    gravity flips — so each level teaches a new idea.

## Recommended scope
- **Now:** Part A (all fixes) + **Phase 1** (jump feel, progress bar, juice, attempt counter) —
  this alone makes the game feel dramatically more "GD/Temple-Run".
- **Next:** Phase 2 (handcrafted patterns + spikes/pits/jump-pads) — the biggest gameplay leap.
- **Later:** Phases 3–5 as you want depth/replayability.

---

# Implementation Steps

### Step 1 — Part A fixes (fast, high-impact)
- **Description:** A1 re-parent prompt (L1/2/3); A2 font fallback on PressStart2P asset; A3
  SetAsLastSibling on MainMenu HelpPanel+SettingsPanel; A4 reassign Press Start 2P across
  LevelSelect; A5 create `GroundSnap.cs`, add to Pipe/Block prefabs, shrink scales, set
  per-level intervals + collectibleChance + `[Star,Star,Star,SpeedRush]`, set laserHeight≈1.4.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes (independent sub-fixes)

### Step 2 — Jump feel (Phase 1.1)
- **Description:** Add coyote-time + jump-buffer + gravity/fall tuning to `PlayerController`
  (additive serialized fields; logic guarded so existing behaviour is preserved). Tune in
  Inspector across levels.
- **Assigned role:** developer
- **Dependencies:** Step 1 (so testing reflects new obstacle spacing)
- **Parallelizable:** No

### Step 3 — Level progress bar + juice + attempt counter (Phase 1.2–1.4)
- **Description:** `LevelProgressBar.cs` + UI per level; `JuiceFX.cs` (landing squash, death burst,
  flashes) hooked to existing events; attempt counter.
- **Assigned role:** developer
- **Dependencies:** Step 1
- **Parallelizable:** Partly (progress bar vs juice independent)

### Step 4 — Handcrafted patterns + new mechanics (Phase 2) [optional, larger]
- **Description:** `ObstaclePattern` SO, `PatternSpawner.cs`, spikes/pits/jump-pad/portal scripts;
  author 1–2 patterns per level to validate.
- **Assigned role:** developer (consider an explorer pass first to map spawner/pooling coupling)
- **Dependencies:** Steps 1–2
- **Parallelizable:** No

### Step 5 — Power-ups, ratings, results, missions, endless, shop (Phases 3–5) [optional]
- **Description:** Implement chosen items from Phases 3–5.
- **Assigned role:** developer
- **Dependencies:** Steps 1–4
- **Parallelizable:** By feature

---

# Verification & Testing
- **Part A:** Play each level — prompt shows on idle; `★ x / y` renders (no `□`, no console
  warning); open Help & Settings in MainMenu (menu fully covered, no bleed); LevelSelect text is
  pixel font matching MainMenu; obstacles sit exactly on the ground, are smaller, and spaced
  4–5 / 3–4 / 2–3 s; jump clears obstacles comfortably with coyote/buffer.
- **Automated (Play Mode):** assert obstacle `collider.bounds.min.y ≈ -2.45` after spawn; assert
  prompt label active & parented to IdlePanel; assert `starsText` contains a non-tofu `★`; combo
  & completion regression from before still pass.
- **Phase 1+:** progress bar fills to goal; attempt counter increments on death; juice fires on
  land/death; (Phase 2) patterns spawn fair, beatable sequences.

# Open Questions
1. **Jump feel:** OK to make the small `PlayerController` edits (coyote + buffer + gravity), or do
   gravity-only via Inspector and keep coyote/buffer in a separate opt-in script?
2. **Scope to execute now:** just **Part A**, or **Part A + Phase 1**? (I recommend Part A + Phase 1.)
3. **Patterns (Phase 2):** want me to move to handcrafted GD-style patterns, or keep weighted-
   random spawning (simpler) for now?
4. **Star `★` fix:** add the shared font fallback (recommended) or restrict the change to the
   star counter only?

---

# ROUND 2 — Pre-Build Polish (investigated, exact fixes)

> All root causes below confirmed via read-only inspection of live scenes/prefabs/animator.
> Decisions made (full autonomy). Additive scripts; existing scripts touched only where flagged.

## R1. Main Menu background music
- **Finding:** MainMenu has 0 AudioSources. `Assets/Audio/BGMusic.wav` exists (menu theme).
- **Fix:** Add looping AudioSource (clip=BGMusic, loop, playOnAwake, volume≈0.3) to MainMenu.
  Respects existing SoundToggle (AudioListener.volume).

## R2. Loading / splash scene (pre-build)
- **Fix:** New `Assets/Scenes/Loading.unity` — cyberpunk bg + "BIT RUNNER" splash + animated
  "LOADING..." (Press Start 2P), no buttons. `LoadingScreen.cs` async-loads MainMenu (2.5s min),
  plays BGMusic at boot. Build order: Loading=0, MainMenu=1, LevelSelect=2, Levels=3-5, Endless=6.

## R3. Crouch animation fix (root cause found)
- **Finding:** No Crouch state (Idle/Run/Jump/Death only; sprite-swap clips). Crouch is a
  transform squash that counter-scales X (widening) and lifts feet/moves child groundCheck →
  isGrounded flickers → crouch toggles ("only after 3-4s" symptom).
- **Fix (edit PlayerController, flagged):** squash Y only (no X counter-scale); keep feet planted
  by shifting position to preserve collider bottom; keep crouch while key held even if grounded
  flickers; drop manual collider resize (scale already shrinks it).

## R4. Star glyph (square) — replace, not fallback
- **Finding:** U+2605 missing from BOTH Press Start 2P and LiberationSans → square + warning.
- **Fix:** HUD uses a real star sprite Image + counter writes "{collected} / {goal}" (no glyph);
  rating rows (results + LevelSelect) use colored '*' (exists in pixel font). Edits GameManager +
  LevelSelectButton (flagged).

## R5. HUD layout + overlaps
- **Finding:** StarCounter overlaps AttemptLabel; FPS/Score/HighScore sit under Sound/Help buttons.
- **Fix:** Left col — StarIcon+Counter y≈-90, Attempt y≈-150. Right col (below buttons) — Score
  y≈-120, Best y≈-165, FPS y≈-205.

## R6. Unify ALL text to Press Start 2P
- **Finding:** RobotoMono/LiberationSans still on Level Score/Level/HighScore/FPS/SpeedRushLabel/
  HelpBody; MainMenu HelpBody/VolumeValue/MissionList.
- **Fix:** Set every TMP_Text.font = PressStart2P across all scenes; shrink body text + wrap.

## R7. Main-menu button tooltips/info
- **Fix:** New Tooltip.cs + TooltipTrigger.cs (hover + click). Messages: PLAY, ENDLESS (states
  "Unlock by clearing Level 3"), SHOP, MISSIONS, SETTINGS, QUIT.

## R8. Hard-to-see obstacle
- **Finding:** BlockObstacle (Data_Cube) tinted (1,0.05,0.05) dark red → reads as black.
- **Fix:** Retint bright orange (1,0.55,0.15); brighten Firewall pipe for parity.

# Round 2 — Steps
1. New scripts: LoadingScreen, Tooltip, TooltipTrigger.
2. Edits: PlayerController crouch (R3); GameManager star text/asterisks (R4); LevelSelectButton
   asterisks (R4).
3. Wiring: MainMenu music (R1); Loading scene+build order (R2); HUD reposition+star icon (R5);
   font unify (R6); MainMenu tooltips (R7); obstacle retint (R8).
4. Final: compile clean, Play Mode regression, leave editor on MainMenu.

# Round 2 — Verification
- Boot → Loading splash (~2.5s, music) → MainMenu (music continues).
- Crouch instant, stays held, no widening/flicker, clears lasers.
- HUD: star icon + "0 / 10" above ATTEMPT; Score/Best/FPS below buttons; no square, no font
  warning; all text Press Start 2P.
- Hover/click Endless/Shop/Missions shows info; Endless states Level-3 unlock.
- Block obstacle clearly visible (bright orange).
