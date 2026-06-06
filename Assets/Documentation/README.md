# BitRunner — Cyberpunk Infinite Runner

**BitRunner** is a fast-paced 2D infinite runner with a cyberpunk aesthetic, built in Unity 6 using the Universal Render Pipeline (URP). Players navigate a high-speed environment, dodging hazards and collecting stars to progress through handcrafted levels and an endless mode.

## 🚀 Quick Start
1. Open the project in **Unity 6000.4.0f1**.
2. Load the scene `Assets/Scenes/Loading.unity`.
3. Press **Play**.
4. Use **SPACE / W / UP** to jump (tap again for double jump).
5. Hold **S / DOWN** to crouch under lasers.
6. Collect enough Gold Stars to clear Level 1, 2, and 3 to unlock Endless mode.

## 🕹️ Core Features
*   **Polished 2D Physics:** Custom character controller with coyote time, jump buffering, and snappier "Geometry Dash" style gravity.
*   **Dual Gameplay Modes:** 3 progression-based campaign levels and an unlockable distance-based Endless mode.
*   **Dynamic Spawning:** Combines weighted-random obstacle generation with handcrafted designer-authored patterns (ScriptableObjects).
*   **Meta-Progression:** 3-star level ratings, best scores, daily-style missions, and a skin shop using a persistent star currency.
*   **Power-Up System:** Magnet (star pull), Shield (one-hit save with invulnerability blink), and Score x2 multipliers.
*   **Speed Rush:** A high-speed "invincibility" state with URP post-processing effects.
*   **Mobile-Ready:** Supports both Keyboard and Touch/Swipe inputs.

## 🛠️ Architecture
*   **Manager-Centric:** Per-scene `GameManager` for level configuration, alongside persistent singleton managers for cross-run state (Skins, Missions, Power-ups).
*   **Event-Driven:** Uses a static C# event bus (`GameEvents`, `CollectibleEvents`) to decouple the HUD, FX, and meta-systems from core gameplay.
*   **Object Pooling:** Custom `ObjectPool` ensures high performance during obstacle and star spawning by minimizing garbage collection.

## 🎨 Asset Credits
*   **Art:** A mix of custom assets and AI-generated sprites (Neon Spikes, Jump Pads, Power-up Icons) produced via Unity's generative toolchain.
*   **Audio:** Cyberpunk themes and retro SFX library.
*   **Fonts:** Unified "Press Start 2P" pixel font for a consistent arcade feel.

---
*Built as a professional Unity project demonstrating advanced character controllers, decoupled architecture, and meta-progression design.*
