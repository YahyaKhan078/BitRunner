# BitRunner 🏃‍♂️⚡

A fast, neon **cyberpunk endless runner** built in **Unity 6**. Sprint through a glowing
city at night, jump and slide past hazards, grab stars to clear each level, and trigger
"Speed Rush" for a high-risk, high-reward dash.

[![Play on itch.io](https://img.shields.io/badge/▶%20Play%20on-itch.io-fa5c5c?style=for-the-badge&logo=itch.io&logoColor=white)](https://ykk7.itch.io/bitrunner-cyberpunk)
&nbsp;
[![Download for Windows](https://img.shields.io/badge/⬇%20Download-Windows-2ea44f?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/YahyaKhan078/BitRunner/releases/latest)

![BitRunner](screenshots/mainmenu.png)

## ▶️ Play it

**⬇️ Download & play (Windows):**
1. Open the **[latest release](https://github.com/YahyaKhan078/BitRunner/releases/latest)** and download **`BitRunner.zip`**.
2. Unzip the whole folder (keep `BitRunner.exe` and the `BitRunner_Data` folder together).
3. Run **`BitRunner.exe`**.

> Windows may show a SmartScreen warning because the build isn't code-signed —
> click **More info → Run anyway**. It's a safe indie build.

**🌐 [Play in your browser on itch.io ▶️](https://ykk7.itch.io/bitrunner-cyberpunk)** — no download needed, runs right in the page.

## 📸 Screenshots

| Main Menu | Level Select | Loading |
|---|---|---|
| ![Main Menu](screenshots/mainmenu.png) | ![Level Select](screenshots/levels.png) | ![Loading](screenshots/loadingscreen.png) |

| Level 1 | Level 2 | Level 3 |
|---|---|---|
| ![Level 1](screenshots/level1.png) | ![Level 2](screenshots/level2.png) | ![Level 3](screenshots/level3.png) |

| Start Countdown | Help & Controls | Pause |
|---|---|---|
| ![Start](screenshots/starting.png) | ![Help](screenshots/help.png) | ![Pause](screenshots/pause.png) |

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
