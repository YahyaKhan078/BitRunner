using UnityEngine;

// Persistent level unlock/completion state (saved in PlayerPrefs).
// Level 1 is always unlocked; a level unlocks when the previous one is completed.
public static class LevelProgress
{
    public static bool IsUnlocked(int level)
    {
        if (level <= 1) return true;
        return PlayerPrefs.GetInt(UnlockKey(level), 0) == 1;
    }

    public static void Unlock(int level)
    {
        PlayerPrefs.SetInt(UnlockKey(level), 1);
        PlayerPrefs.Save();
    }

    public static bool IsCompleted(int level) => PlayerPrefs.GetInt(DoneKey(level), 0) == 1;

    public static void MarkCompleted(int level)
    {
        PlayerPrefs.SetInt(DoneKey(level), 1);
        PlayerPrefs.Save();
    }

    // ── 3-star rating (0..3) ───────────────────────────
    // Keeps the best (highest) rating ever earned for a level.
    public static int GetStars(int level) => PlayerPrefs.GetInt(StarsKey(level), 0);

    public static void SetStars(int level, int stars)
    {
        stars = Mathf.Clamp(stars, 0, 3);
        if (stars > GetStars(level))
        {
            PlayerPrefs.SetInt(StarsKey(level), stars);
            PlayerPrefs.Save();
        }
    }

    // ── per-level best score ───────────────────────────
    public static int GetBestScore(int level) => PlayerPrefs.GetInt(BestKey(level), 0);

    public static void SetBestScore(int level, int score)
    {
        if (score > GetBestScore(level))
        {
            PlayerPrefs.SetInt(BestKey(level), score);
            PlayerPrefs.Save();
        }
    }

    // ── lifetime total stars (currency for the shop) ───
    public static int GetTotalStars() => PlayerPrefs.GetInt("TotalStars", 0);

    public static void AddTotalStars(int amount)
    {
        PlayerPrefs.SetInt("TotalStars", GetTotalStars() + Mathf.Max(0, amount));
        PlayerPrefs.Save();
    }

    public static bool SpendTotalStars(int amount)
    {
        int t = GetTotalStars();
        if (t < amount) return false;
        PlayerPrefs.SetInt("TotalStars", t - amount);
        PlayerPrefs.Save();
        return true;
    }

    // call from a debug/reset button if you want to wipe progress
    public static void ResetAll()
    {
        for (int i = 1; i <= 10; i++)
        {
            PlayerPrefs.DeleteKey(UnlockKey(i));
            PlayerPrefs.DeleteKey(DoneKey(i));
            PlayerPrefs.DeleteKey(StarsKey(i));
            PlayerPrefs.DeleteKey(BestKey(i));
        }
        PlayerPrefs.DeleteKey("TotalStars");
        PlayerPrefs.Save();
    }

    static string UnlockKey(int level) => "LevelUnlocked_" + level;
    static string DoneKey(int level)   => "LevelCompleted_" + level;
    static string StarsKey(int level)  => "LevelStars_" + level;
    static string BestKey(int level)   => "LevelBest_" + level;
}
