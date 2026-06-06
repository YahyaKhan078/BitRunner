using System;

/// <summary>
/// Lightweight static event bus for run-lifecycle moments. Lets HUD, FX and meta systems
/// (juice, attempt counter, results screen, missions, ratings) react without tight coupling.
/// GameManager raises these; everything else only subscribes. Mirrors CollectibleEvents.
/// </summary>
public static class GameEvents
{
    /// <summary>A run started (after the countdown). Arg = level number.</summary>
    public static event Action<int> RunStarted;
    /// <summary>The player died (world has frozen). </summary>
    public static event Action PlayerDied;
    /// <summary>A level was completed. Args = (level number, isGameComplete).</summary>
    public static event Action<int, bool> LevelCompleted;
    /// <summary>Difficulty level-up tick. Arg = new internal level.</summary>
    public static event Action<int> LeveledUp;

    public static void RaiseRunStarted(int level) => RunStarted?.Invoke(level);
    public static void RaisePlayerDied() => PlayerDied?.Invoke();
    public static void RaiseLevelCompleted(int level, bool gameComplete) => LevelCompleted?.Invoke(level, gameComplete);
    public static void RaiseLeveledUp(int level) => LeveledUp?.Invoke(level);
}
