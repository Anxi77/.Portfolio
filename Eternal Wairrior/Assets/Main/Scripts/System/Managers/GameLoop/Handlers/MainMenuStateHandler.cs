using UnityEngine;

public class MainMenuStateHandler : IGameStateHandler
{
    public void OnEnter()
    {
        UIManager.Instance.ShowMainMenu();
        Time.timeScale = 1f;
    }

    public void OnExit()
    {
        UIManager.Instance.HideMainMenu();
        UIManager.Instance.ClearUI();
    }

    public void OnUpdate()
    {
    }

    public void OnFixedUpdate() { }
}