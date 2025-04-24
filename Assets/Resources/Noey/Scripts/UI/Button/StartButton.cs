using UnityEngine;

public class StartButton : ButtonBase
{
    [SerializeField] private GameObject _expressionUI;
    [SerializeField] private GameObject _titleUI;

    protected override void OnClicked()
    {
        _titleUI.SetActive(false);
        _expressionUI.SetActive(true);
        GameManager.Instance.StartGame();
    }
}
