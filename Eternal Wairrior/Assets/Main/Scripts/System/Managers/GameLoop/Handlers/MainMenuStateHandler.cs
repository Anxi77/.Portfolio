using UnityEngine;

public class MainMenuStateHandler : BaseStateHandler
{
    public override void OnEnter()
    {
        base.OnEnter();
        UI.ShowMainMenu();
        Time.timeScale = 1f;
    }

    public override void OnExit()
    {
        UI.HideMainMenu();
        base.OnExit();
    }
}
