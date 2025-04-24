using System;

public static class ScoreManager
{
    private static int _score = 0;
    
    public static event Action<int> OnUpdateScore;
    public static int CurrentScore { get { return _score; } }

    public static void AddScore(int value)
    {
        _score += value;
        OnUpdateScore?.Invoke(_score);
    }

    public static void Reset()
    {
        _score = 0;
        OnUpdateScore?.Invoke(_score);
    }
}
