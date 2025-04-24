using UnityEngine;

public class OperatorButton : ButtonBase
{
    [SerializeField] private string _operator;
    [SerializeField] private InputExpression _expressionUI;

    protected override void OnClicked()
    {
        if (_expressionUI.LastInputType == LastInputType.Operator)
            return;

        _expressionUI.OnOperatorClicked(_operator);
    }
}
