using UnityEngine;

public class NumberButton : ButtonBase
{
    [SerializeField] private InputExpression _expressionUI;
    private int _number;

    public void Init(int number)
    {
        _number = number;
        _buttonText.text = number.ToString();
    }

    protected override void OnClicked()
    {
        if (_expressionUI.LastInputType == LastInputType.Nubmer)
            return;

        _expressionUI.OnNumberClicked(_number);
        _button.interactable = false;
    }

    public void ResetState()
    {
        _button.interactable = true;
    }
}
