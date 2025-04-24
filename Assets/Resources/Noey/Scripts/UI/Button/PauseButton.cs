public class PauseButton : ButtonBase
{
    protected override void OnClicked()
    {
        switch (GameManager.Instance.CurrentState)
        {
            case GameManager.GameState.Playing:
                {
                    GameManager.Instance.SetGameState(GameManager.GameState.Paused);
                    PopupManager.Instance.ShowPopup<PausePopup>();
                }
                break;
            case GameManager.GameState.Paused:
                {
                    GameManager.Instance.SetGameState(GameManager.GameState.Playing);
                    PopupManager.Instance.HideTopPopup();
                }
                break;
        }
    }
}
