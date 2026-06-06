# BitRunner 🏃‍♂️⚡

A fast, neon **cyberpunk endless runner** built in **Unity 6**. Sprint through a glowing
city at night, jump and slide past hazards, grab stars to clear each level, and trigger
"Speed Rush" for a high-risk, high-reward dash.

<!-- Replace this banner with your own image: put it in the screenshots/ folder -->
![BitRunner banner](screenshots/banner.png)

## ▶️ Play it
**[Play in your browser (itch.io)](https://itch.io/) — _link coming soon_**
A downloadable Windows build is also available in the [Releases](../../releases) section.

## 📸 Screenshots
<!-- Add your own screenshots to the screenshots/ folder, then they'll show up here -->
| Main Menu | Gameplay | Game Over |
|---|---|---|
| ![Menu](screenshots/menu.png) | ![Gameplay](screenshots/gameplay.png) | ![Game Over](screenshots/gameover.png) |

## ✨ What it does
- **Three campaign levels + an unlockable Endless mode.** Collect enough stars to clear a
  level; finishing one unlocks the next, and your progress is saved between sessions.
- **Tight, fair controls.** The jump is tuned to feel responsive — small forgiving timing
  windows mean a jump still works if you press it a hair early or late, and tapping gives a
  short hop while holding gives a full leap.
- **Risk vs. reward.** Run through green chevrons to enter **Speed Rush**: the world speeds
  up (less time to react) but your score climbs much faster and you can jump freely.
- **Lots to do.** Power-ups (magnet, shield, double-score), a star "shop" for player skins,
  rotating missions, 3-star level ratings, and best-score tracking.
- **A real game loop.** Animated start countdown, pause menu, help screen, sound toggle,
  game-over → play-again / main-menu, and a level-select screen with locked/unlocked cards.

## 🎮 Controls
| Action | Keys |
|---|---|
| Jump / Double-jump | **Space**, **W**, or **↑** |
| Crouch / slide under lasers | **S** or **↓** (hold) |
| Pause | **Esc** |
| Start / Play again | **Space** |

Touch & swipe controls are also supported for mobile.

## 🛠️ Built with
- **Unity 6** (6000.4.0f1) with the **Universal Render Pipeline (URP)** for the neon glow.
- **C#** — a modular, event-driven codebase: a per-level game manager, decoupled UI/FX via
  a small event system, object pooling for smooth spawning, and saved progress.
- **TextMesh Pro** for crisp pixel-font UI; a mix of custom and AI-generated neon art.

## 🚀 Run from source
1. Install **Unity 6000.4.0f1** (Unity Hub).
2. Clone this repo and open the folder in Unity.
3. Open `Assets/Scenes/Loading.unity` and press **Play**.

> Detailed design notes are in [`Assets/Documentation/`](Assets/Documentation) (overview,
> portfolio write-up, and a technical deep-dive).

## 📄 License
Personal portfolio project. Fonts and some audio/art are under their respective licenses
(see the asset folders). Feel free to learn from the code.

---
*Made by **Yahya Khan**. Feedback and contributions welcome!*
