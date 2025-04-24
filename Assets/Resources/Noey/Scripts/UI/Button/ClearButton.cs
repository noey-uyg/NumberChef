using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearButton : ButtonBase
{
    [SerializeField] private InputExpression _expressionUI;

    protected override void OnClicked()
    {
        _expressionUI.OnClear();
    }
}
