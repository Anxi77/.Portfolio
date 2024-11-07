using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : SingletonManager<LobbyManager>
{
    private void Start()
    {
        // �κ� ���� �� �ʱ�ȭ
        InitializeLobby();
    }

    private void InitializeLobby()
    {
        // ���� ������ �ʱ�ȭ
        GameManager.Instance?.ClearGameData();
        StageTimeManager.Instance?.ResetTimer();

        // UI �ʱ�ȭ
        UpdateUI();
    }

    public void OnStartNewGame()
    {
        StartCoroutine(StartNewGameCoroutine());
    }

    public void OnLoadGame()
    {
        StartCoroutine(LoadGameCoroutine());
    }

    private IEnumerator StartNewGameCoroutine()
    {
        // 1. �� ���� ������ �ʱ�ȭ
        GameManager.Instance.InitializeNewGame();

        // 2. ������ �̵�
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("TownScene");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 3. �� �÷��̾� �ʱ�ȭ
        PlayerUnitManager.Instance.InitializeNewPlayer();

        // 4. �÷��̾� ���� ��ġ ����
        GameManager.Instance.player.transform.position = GetTownStartPosition();
    }

    private IEnumerator LoadGameCoroutine()
    {
        // 1. ����� ���� �����Ͱ� �ִ��� Ȯ��
        if (!GameManager.Instance.playerDataManager.HasSaveData("CurrentSave"))
        {
            Debug.LogWarning("No saved game found!");
            yield break;
        }

        // 2. ���� ������ �ε�
        GameManager.Instance.LoadGameData();

        // 3. ������ ����� ������ �̵� (�⺻��: ����)
        string savedScene = GameManager.Instance.GetLastSavedScene();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(savedScene);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 4. ����� �÷��̾� ������ �ε�
        PlayerUnitManager.Instance.LoadPlayerData();

        // 5. �÷��̾� ��ġ ����
        Vector3 savedPosition = GameManager.Instance.GetLastSavedPosition();
        GameManager.Instance.player.transform.position = savedPosition;
    }

    private Vector3 GetTownStartPosition()
    {
        // ���� ���� ��ġ ��ȯ
        return new Vector3(0, 0, 0); // ���� ���� ��ġ�� ���� �ʿ�
    }

    private void UpdateUI()
    {
        // ����� ������ �ִ��� Ȯ���Ͽ� UI ������Ʈ
        bool hasSaveData = GameManager.Instance.HasSaveData();
        UIManager.Instance.UpdateLobbyUI(hasSaveData);
    }

    public void OnExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}