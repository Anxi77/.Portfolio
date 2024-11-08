using UnityEngine;

public class MainMenuStateHandler : IGameStateHandler
{
    public void OnEnter()
    {
        Debug.Log("Entering MainMenu state");
        UIManager.Instance.ShowMainMenu();
        Time.timeScale = 1f;
    }

    public void OnExit()
    {
        Debug.Log("Exiting MainMenu state");
        UIManager.Instance.HideMainMenu();
        UIManager.Instance.ClearUI(); // UI ����
    }

    public void OnUpdate()
    {
        // ���� �޴� ������Ʈ ����
    }

    public void OnFixedUpdate() { }
}