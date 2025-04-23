using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : DontDestroySingleton<GameManager>
{
    protected override void OnAwake()
    {
        base.OnAwake();
        Application.targetFrameRate = 60;
    }
}
