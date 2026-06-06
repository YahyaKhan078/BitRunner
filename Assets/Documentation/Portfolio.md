# Portfolio: BitRunner
**Role:** Solo Developer (Programming, Design, Technical Art, Testing)  
**Engine:** Unity 6 (URP)  
**Platform:** Windows Standalone (Mobile-Ready)

## 🎯 Project Vision
BitRunner was developed to demonstrate a professional-grade implementation of the "infinite runner" genre. The goal was to bridge the gap between simple random runners and high-polish titles like *Geometry Dash* and *Temple Run* by implementing handcrafted rhythms, robust "game feel" mechanics, and a deep meta-progression layer.

## 🌟 Key Accomplishments

### 1. Advanced Game Feel Engineering
Developed a 2D character controller focusing on "fairness" and precision. This includes:
*   **Coyote Time & Jump Buffering:** Forgiving input windows that reduce player frustration.
*   **Variable Jump Arc:** Custom gravity multipliers that distinguish between a quick hop and a full leap.
*   **Stable Crouch Physics:** Implemented a transform-squash system with "planted feet" position compensation to ensure the player never slips through the ground or flickers during scale changes.

### 2. Handcrafted vs. Random Spawning
Built a hybrid spawning system. While typical runners are purely random, BitRunner uses **ScriptableObject-driven Patterns**. This allows for the creation of specific rhythmic challenges (Geometry Dash style) that can be interspersed with random variety, ensuring levels feel intentional rather than chaotic.

### 3. Meta-Progression & Economy
Implemented a complete player-loop with:
*   **Star-based Economy:** Lifetime currency collection used in a **Skin Shop**.
*   **Missions System:** 3 rotating cross-run objectives (Collect X, Clear without death, Reach combo) to drive retention.
*   **3-Star Rating Logic:** Performance-based rewards (completion, no-death, high score) that persist across sessions.

### 4. AI-Integrated Art Pipeline
Leveraged Unity's AI generation tools to rapidly prototype and iterate on hazard and UI art. Developed a workflow for **Text-to-Sprite generation -> Background Removal -> Code-driven Prefab Assembly**, allowing for high-quality neon-cyberpunk assets that fit the game's specific style.

## 🛠️ Technical Skills Demonstrated
*   **Software Architecture:** Singleton pattern, Observer pattern (Static Event Bus), Object Pooling.
*   **Unity Features:** URP Volume framework, TextMesh Pro styling, New Input System, Coroutines, ScriptableObjects.
*   **Persistence:** Persistent Save/Load system via a static API layer over PlayerPrefs.
*   **Testing:** Automated in-editor **Play Mode tests** used to validate character physics and level unlock logic.

## 📈 Outcome
The project resulted in a highly scalable runner framework. The decoupled architecture allows for adding new hazards, power-ups, or levels in minutes by simply creating a new prefab or ScriptableObject and wiring it into the existing managers.
