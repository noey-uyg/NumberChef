using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmitButton : ButtonBase
{
    [SerializeField] private InputExpression _expressionUI;

    protected override void OnClicked()
    {
        _expressionUI.OnSubmit();
    }
}
