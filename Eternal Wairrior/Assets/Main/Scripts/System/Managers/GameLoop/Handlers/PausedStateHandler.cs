using UnityEngine;

public class PausedStateHandler : BaseStateHandler
{
    public override void OnEnter()
    {
        Time.timeScale = 0f;
        UI.ShowPauseMenu();
    }

    public override void OnExit()
    {
        Time.timeScale = 1f;
        UI.HidePauseMenu();
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameLoop.ChangeState(GameState.Stage);
        }
    }
}
