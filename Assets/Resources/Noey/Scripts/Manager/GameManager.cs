using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : DontDestroySingleton<GameManager>
{
    public enum GameState { None, Playing, Paused, GameOver}

    private GameState _state;

    public GameState CurrentState { get { return _state; } }
    public event Action<GameState> OnStateChanged;

    protected override void OnAwake()
    {
        base.OnAwake();
        Application.runInBackground = true;
        Application.targetFrameRate = 60;
    }

    public void SetGameState(GameState state)
    {
        if(_state == state) return;

        _state = state;
        OnStateChanged?.Invoke(state);
    }

    public void StartGame()
    {
        SetGameState(GameState.Playing);
        ScoreManager.Reset();
        DifficultyManager.Start();
    }
}
