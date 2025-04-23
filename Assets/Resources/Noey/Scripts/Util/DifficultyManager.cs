using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DifficultyManager
{
    private static float _startTime = 0f;
    
    public static float ElapsedTime => Time.time - _startTime;

    public static void Start()
    {
        _startTime = Time.time;
    }

    public static float GetCurrentDownDuration()
    {
        return Mathf.Max(3f, 15f - ElapsedTime * 0.01f);
    }

    public static float GetCurrentSpawnDuration()
    {
        return Mathf.Max(1f, 2f - ElapsedTime * 0.005f);
    }

    public static int GetCurrentMinNumber()
    {
        return 1;
    }

    public static int GetCurrentMaxNumber()
    {
        return Mathf.Min(500, 100 + (int)(ElapsedTime * 0.5f));
    }

    public static void Reset()
    {
        _startTime = Time.time;
    }
}
