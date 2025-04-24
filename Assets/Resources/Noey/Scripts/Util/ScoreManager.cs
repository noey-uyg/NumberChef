using System;

public static class ScoreManager
{
    private static int _score = 0;
    
    public static event Action<int> OnUpdateScore;
    public static int CurrentScore { get { return _score; } }

    public static void AddScore(string expression)
    {
        _score += 1 + CountOperators(expression) * 2;
        OnUpdateScore?.Invoke(_score);
    }

    private static int CountOperators(string expression)
    {
        int count = 0;

        foreach(var c in expression){
            if (c == '+' || c == '-' || c == '*' || c == '/')
                count++;
        }

        return count;
    }

    public static void Reset()
    {
        _score = 0;
        OnUpdateScore?.Invoke(_score);
    }
}
