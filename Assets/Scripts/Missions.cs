using System;
using UnityEngine;

/// <summary>
/// Lightweight daily-style mission system (Temple-Run flavor). Three objectives tracked across
/// runs via GameEvents/CollectibleEvents and persisted in PlayerPrefs. Completing one grants
/// bonus total-stars (shop currency). Additive — auto-bootstraps; no existing script changes.
/// </summary>
public class Missions : MonoBehaviour
{
    public enum Kind { CollectStars, ClearLevelNoDeath, ReachCombo }

    [Serializable]
    public class Mission
    {
        public Kind kind;
        public int target;
        public int reward;
        public string id;     // PlayerPrefs progress key
        public string Describe()
        {
            switch (kind)
            {
                case Kind.CollectStars:      return "Collect " + target + " stars";
                case Kind.ClearLevelNoDeath: return "Clear a level without dying";
                case Kind.ReachCombo:        return "Reach a x" + target + " combo";
            }
            return "";
        }
    }

    public static Missions Instance { get; private set; }

    public Mission[] missions = new Mission[]
    {
        new Mission { kind = Kind.CollectStars,      target = 30, reward = 10, id = "M_Stars30" },
        new Mission { kind = Kind.ClearLevelNoDeath, target = 1,  reward = 15, id = "M_NoDeath" },
        new Mission { kind = Kind.ReachCombo,        target = 5,  reward = 10, id = "M_Combo5" },
    };

    int currentCombo;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance == null)
            new GameObject("Missions").AddComponent<Missions>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        CollectibleEvents.Collected += OnCollected;
        GameEvents.LevelCompleted += OnLevelCompleted;
        GameEvents.RunStarted += OnRunStarted;
    }

    void OnDisable()
    {
        CollectibleEvents.Collected -= OnCollected;
        GameEvents.LevelCompleted -= OnLevelCompleted;
        GameEvents.RunStarted -= OnRunStarted;
    }

    void OnRunStarted(int level) { currentCombo = 0; }

    void OnCollected(Collectible.CollectibleType type, Vector3 pos, float amount)
    {
        if (type != Collectible.CollectibleType.Star) return;
        AddProgress(Kind.CollectStars, 1);

        // track a rough combo for the combo mission (chain handled by ComboMeter visually)
        currentCombo++;
        ReportCombo(currentCombo);
    }

    void OnLevelCompleted(int level, bool gameComplete)
    {
        // "no death" credited via GameManager's rating ≥ 2 stars (deathless)
        if (LevelProgress.GetStars(level) >= 2)
            AddProgress(Kind.ClearLevelNoDeath, 1);
    }

    public void ReportCombo(int combo)
    {
        foreach (var m in missions)
            if (m.kind == Kind.ReachCombo && combo >= m.target)
                Complete(m);
    }

    public bool IsComplete(Mission m) => PlayerPrefs.GetInt(m.id + "_done", 0) == 1;
    public int Progress(Mission m) => PlayerPrefs.GetInt(m.id, 0);

    void AddProgress(Kind kind, int amount)
    {
        foreach (var m in missions)
        {
            if (m.kind != kind || IsComplete(m)) continue;
            int p = PlayerPrefs.GetInt(m.id, 0) + amount;
            PlayerPrefs.SetInt(m.id, p);
            if (p >= m.target) Complete(m);
        }
        PlayerPrefs.Save();
    }

    void Complete(Mission m)
    {
        if (IsComplete(m)) return;
        PlayerPrefs.SetInt(m.id + "_done", 1);
        PlayerPrefs.SetInt(m.id, m.target);
        PlayerPrefs.Save();
        LevelProgress.AddTotalStars(m.reward);
    }

    public void ResetAll()
    {
        foreach (var m in missions)
        {
            PlayerPrefs.DeleteKey(m.id);
            PlayerPrefs.DeleteKey(m.id + "_done");
        }
        PlayerPrefs.Save();
    }
}
