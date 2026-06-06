using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // set by PlayAgain() so the reloaded scene starts the countdown immediately
    private static bool autoStartOnLoad = false;

    // ── Inspector References ───────────────────────────
    [Header("Panels")]
    public GameObject idlePanel;        // shown before game starts (title + countdown)
    public GameObject hudPanel;         // score / speed shown during play
    public GameObject gameOverPanel;    // shown on death (Game Over + Play Again / Menu)

    [Header("HUD Text")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI starsText;   // "★ 3 / 5" progress toward the level goal

    [Header("Game Over Text")]
    public TextMeshProUGUI goScoreText;
    public TextMeshProUGUI goBestText;

    [Header("Speed")]
    public float startSpeed = 7f;
    public float maxSpeed = 13f;
    public float speedGainPerSecond = 0.35f; // progressive difficulty: base speed climb per second
    public float scoreRate = 3f;             // passive score per (speed × second)
    public float speedStep = 0.4f;           // speed per "level" (drives level-up events)
    public float speedRampPer = 80f;         // legacy (unused by the time-based ramp)

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioClip levelUpClip;
    public AudioClip gameOverClip;   // sting played on death (music stops)

    [Header("Start / Countdown")]
    public Image loadingBar;                 // legacy (unused; replaced by the countdown)
    public float loadDuration = 1.5f;        // legacy
    public TextMeshProUGUI startPromptLabel; // "Press Space to Start" on the idle screen
    public TextMeshProUGUI countdownText;    // big "3 2 1 GO!" countdown before the run
    public int   countdownFrom = 3;
    public float countdownStep = 0.7f;       // seconds per number (unscaled)

    [Header("Level")]
    public int levelNumber = 1;          // 1/2/3 — used to save unlock/completion progress
    public int starsToComplete = 0;      // collect this many stars to finish (0 = endless)
    public int levelTargetScore = 0;     // optional alternative: finish at this score (0 = off)
    public string nextLevelScene = "";   // scene to load on completion ("" = game complete)
    public string mainMenuScene = "MainMenu";
    public GameObject levelCompletePanel;     // overlay shown on completion
    public TextMeshProUGUI levelCompleteText;

    [Header("References")]
    public PlayerController player;
    public ObstacleSpawner spawner;
    public CameraShake cameraShake;

    // ── Runtime state ──────────────────────────────────
    public float CurrentSpeed { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool SpeedRushActive { get; private set; }

    public int StarsCollected => starsCollected;
    public int StarsToComplete => starsToComplete;

    // 1 → just activated, 0 → expired (for an optional bar)
    public float SpeedRushRemaining01 =>
        SpeedRushActive ? Mathf.Clamp01((rushEndTime - Time.unscaledTime) / rushTotalDuration) : 0f;

    // whole seconds of Speed Rush left (for the on-screen countdown)
    public float SpeedRushSecondsLeft =>
        SpeedRushActive ? Mathf.Max(0f, rushEndTime - Time.unscaledTime) : 0f;

    public float SpeedIntensity01 =>
        Mathf.Clamp01((CurrentSpeed - startSpeed) / Mathf.Max(0.01f, maxSpeed - startSpeed));

    private float score;
    private float highScore;
    private int lastLevel;
    private int starsCollected;
    private bool isGameOver;
    private bool diedThisRun;   // for the 3-star "no-death" rating
    private int lastRating = 1; // stars earned on the last completion (for results display)

    private float speedMultiplier = 1f;
    private float scoreMultiplier = 1f;
    private float rushEndTime;
    private float rushTotalDuration = 1f;
    private float baseSpeed;
    private bool  isStarting;

    // ── Unity lifecycle ────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // NOTE: keep GameManager per-scene (NOT DontDestroyOnLoad) so its Inspector
        // references resolve correctly in each Level scene.
    }

    void Start()
    {
        highScore = PlayerPrefs.GetFloat("HighScore", 0f);

        if (autoStartOnLoad)
        {
            autoStartOnLoad = false;
            StartCoroutine(StartSequence());   // restarted level → straight into the countdown
        }
        else
        {
            ShowIdle();
        }
    }

    void Update()
    {
        if (!IsPlaying)
        {
            // input while not playing: restart from Game Over, or start from idle
            if (!isStarting
             && (Input.GetKeyDown(KeyCode.Space)
              || Input.GetKeyDown(KeyCode.UpArrow)
              || Input.GetKeyDown(KeyCode.W)))
            {
                if (isGameOver) PlayAgain();
                else            StartCoroutine(StartSequence());
            }
            return;
        }

        if (SpeedRushActive && Time.unscaledTime >= rushEndTime)
            EndSpeedRush();

        // progressive difficulty: base speed climbs over time (Subway-Surfers style)
        baseSpeed = Mathf.Min(baseSpeed + speedGainPerSecond * Time.deltaTime, maxSpeed);
        CurrentSpeed = baseSpeed * speedMultiplier;

        // passive score tied to speed → faster game = faster scoring
        score += CurrentSpeed * Time.deltaTime * scoreRate * scoreMultiplier;

        int newLevel = Mathf.FloorToInt((baseSpeed - startSpeed) / speedStep) + 1;
        if (newLevel > lastLevel) { lastLevel = newLevel; OnLevelUp(); }

        UpdateHUD();

        // optional score-based finish (stars are the primary goal — see CollectStar)
        if (levelTargetScore > 0 && score >= levelTargetScore)
            CompleteLevel();
    }

    // ── Start / countdown ──────────────────────────────
    public void StartGame()
    {
        Time.timeScale = 1f;

        score = 0f;
        starsCollected = 0;
        baseSpeed = startSpeed;
        CurrentSpeed = startSpeed;
        lastLevel = 1;
        IsPlaying = true;
        isGameOver = false;

        EndSpeedRush();

        player.ResetPlayer();
        spawner.ClearAllObstacles();
        spawner.StartSpawning();

        SetPanels(idle: false, hud: true, over: false);

        if (musicSource != null && !musicSource.isPlaying)
            musicSource.Play();

        GameEvents.RaiseRunStarted(levelNumber);
    }

    // Shows a 3-2-1-GO countdown, then starts the run. Unscaled so it works even when
    // entered from a frozen state.
    IEnumerator StartSequence()
    {
        isStarting = true;
        Time.timeScale = 1f;
        SetPanels(idle: true, hud: false, over: false);

        if (startPromptLabel != null) startPromptLabel.gameObject.SetActive(false);
        if (loadingBar != null) loadingBar.gameObject.SetActive(false);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            for (int n = countdownFrom; n > 0; n--)
            {
                countdownText.text = n.ToString();
                yield return StartCoroutine(PopText(countdownText, countdownStep));
            }
            countdownText.text = "GO!";
            yield return StartCoroutine(PopText(countdownText, 0.45f));
            countdownText.gameObject.SetActive(false);
        }

        StartGame();
        if (startPromptLabel != null) startPromptLabel.gameObject.SetActive(true);
        isStarting = false;
    }

    // small scale "punch" so each countdown number feels like a ticking clock
    IEnumerator PopText(TextMeshProUGUI label, float duration)
    {
        float t = 0f;
        Transform tr = label.transform;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            float s = Mathf.Lerp(1.6f, 1.0f, k);   // shrink from big → normal
            tr.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        tr.localScale = Vector3.one;
    }

    // ── Restart / navigation ───────────────────────────
    public void PlayAgain()
    {
        // reload the level fresh (clean Animator + start position) and auto-run the countdown
        autoStartOnLoad = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    // legacy alias
    public void RestartGame() => PlayAgain();

    // ── Level progression ──────────────────────────────
    void CompleteLevel()
    {
        if (!IsPlaying) return;   // guard against double-trigger (stars + score same frame)
        IsPlaying = false;
        if (spawner != null) spawner.StopSpawning();
        SaveHighScore();

        LevelProgress.MarkCompleted(levelNumber);
        LevelProgress.Unlock(levelNumber + 1);

        // 3-star rating: 1 = cleared, +1 for no-death, +1 for a strong score (≥150 per goal star)
        int rating = 1;
        if (!diedThisRun) rating++;
        if (starsToComplete > 0 && score >= starsToComplete * 150) rating++;
        rating = Mathf.Clamp(rating, 1, 3);
        LevelProgress.SetStars(levelNumber, rating);
        LevelProgress.SetBestScore(levelNumber, Mathf.FloorToInt(score));
        LevelProgress.AddTotalStars(starsCollected);
        lastRating = rating;

        GameEvents.RaiseLevelCompleted(levelNumber, string.IsNullOrEmpty(nextLevelScene));

        StartCoroutine(LevelCompleteRoutine());
    }

    IEnumerator LevelCompleteRoutine()
    {
        Time.timeScale = 1f;
        SetPanels(idle: false, hud: false, over: false);

        bool gameComplete = string.IsNullOrEmpty(nextLevelScene);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);

        // rating row using '*' (the star glyph U+2605 is absent from the pixel font)
        string starsRow = "";
        for (int i = 0; i < 3; i++)
            starsRow += i < lastRating ? "<color=#FFD54A>*</color>" : "<color=#444444>*</color>";

        if (levelCompleteText != null)
        {
            string title = gameComplete ? "YOU WIN!" : "LEVEL COMPLETE";
            levelCompleteText.text =
                "<size=130%><b>" + title + "</b></size>\n\n" +
                "<size=160%>" + starsRow + "</size>\n\n" +
                "Score: " + Mathf.FloorToInt(score) + "\n" +
                "Best: " + LevelProgress.GetBestScore(levelNumber) + "\n" +
                "Stars: " + starsCollected;
        }

        yield return new WaitForSecondsRealtime(3.2f);

        if (gameComplete) SceneManager.LoadScene(mainMenuScene);
        else              SceneManager.LoadScene(nextLevelScene);
    }

    // ── Death sequence (called by PlayerController) ────
    public void BeginDeath()
    {
        IsPlaying = false;
        if (spawner != null) spawner.StopSpawning();
        if (cameraShake != null) cameraShake.Shake(0.3f, 0.18f);
        SaveHighScore();
        if (musicSource != null) musicSource.Stop();
        Time.timeScale = 0f;   // world freezes; player Animator keeps playing (unscaled)

        diedThisRun = true;
        GameEvents.RaisePlayerDied();
    }

    public void ShowGameOverScreen()
    {
        if (gameOverClip != null)
        {
            Vector3 p = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(gameOverClip, p);
        }
        ShowGameOver();
    }

    public void OnPlayerDied()   // legacy one-shot path (no death-animation delay)
    {
        BeginDeath();
        ShowGameOverScreen();
    }

    // ── Collectibles ───────────────────────────────────
    public void AddScore(float amount)
    {
        score += amount;
        UpdateHUD();
    }

    // Star pickup: counts toward the level goal AND adds score.
    public void CollectStar(float points)
    {
        starsCollected++;
        score += points;
        UpdateHUD();

        if (starsToComplete > 0 && starsCollected >= starsToComplete)
            CompleteLevel();
    }

    public void ActivateSpeedRush(float speedMult, float scoreMult, float duration)
    {
        speedMultiplier   = Mathf.Max(1f, speedMult);
        scoreMultiplier   = Mathf.Max(1f, scoreMult);
        rushTotalDuration = Mathf.Max(0.01f, duration);
        rushEndTime       = Time.unscaledTime + duration;
        SpeedRushActive   = true;
    }

    void EndSpeedRush()
    {
        speedMultiplier = 1f;
        scoreMultiplier = 1f;
        SpeedRushActive = false;
    }

    // ── UI helpers ─────────────────────────────────────
    void SetPanels(bool idle, bool hud, bool over)
    {
        idlePanel?.SetActive(idle);
        hudPanel?.SetActive(hud);
        gameOverPanel?.SetActive(over);
        if (!over && levelCompletePanel != null) levelCompletePanel.SetActive(false);
    }

    void ShowIdle()
    {
        IsPlaying = false;
        isGameOver = false;
        SetPanels(idle: true, hud: false, over: false);
        if (startPromptLabel != null)
        {
            startPromptLabel.gameObject.SetActive(true);
            startPromptLabel.text = "Press Space to Start";
        }
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    void ShowGameOver()
    {
        isGameOver = true;
        SetPanels(idle: false, hud: false, over: true);

        if (goScoreText != null)
            goScoreText.text = "<size=150%><b>GAME OVER</b></size>\n\nScore: "
                             + Mathf.FloorToInt(score).ToString();
        if (goBestText != null)
            goBestText.text = "Best: " + Mathf.FloorToInt(highScore).ToString()
                            + "\n\n<size=70%>Press Space to Play Again</size>";
    }

    void UpdateHUD()
    {
        if (scoreText != null)
            scoreText.text = Mathf.FloorToInt(score).ToString();
        if (highScoreText != null)
            highScoreText.text = "Best: " + Mathf.FloorToInt(highScore).ToString();
        if (levelText != null)
            levelText.text = "LVL " + levelNumber;
        if (starsText != null)
            starsText.text = starsToComplete > 0
                ? starsCollected + " / " + starsToComplete
                : starsCollected.ToString();
    }

    void OnLevelUp()
    {
        if (musicSource != null && levelUpClip != null)
            musicSource.PlayOneShot(levelUpClip, 0.5f);
        GameEvents.RaiseLeveledUp(lastLevel);
    }

    void SaveHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetFloat("HighScore", highScore);
            PlayerPrefs.Save();
        }
    }
}
