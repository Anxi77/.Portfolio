using UnityEngine;

public class PausedStateHandler : IGameStateHandler
{
    public void OnEnter()
    {
        Time.timeScale = 0f;
        UIManager.Instance.ShowPauseMenu();
    }

    public void OnExit()
    {
        Time.timeScale = 1f;
        UIManager.Instance.HidePauseMenu();
    }

    public void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameLoopManager.Instance.ChangeState(GameState.Stage);
        }
    }

    public void OnFixedUpdate() { }
}