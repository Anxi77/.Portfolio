using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    MainMenu,
    Town,
    Game,
    Test,
}

public class StageManager : SingletonManager<StageManager>
{
    [Header("Portal Settings")]
    [SerializeField]
    private GameObject portalPrefab;

    [SerializeField]
    private Vector3 townPortalPosition = new(10, 0, 0);

    #region Scene Loading
    public void LoadMainMenu()
    {
        StartCoroutine(LoadSceneCoroutine("MainMenu", SceneType.MainMenu));
    }

    public void LoadTownScene()
    {
        StartCoroutine(LoadSceneCoroutine("TownScene", SceneType.Town));
    }

    public void LoadGameScene()
    {
        StartCoroutine(LoadSceneCoroutine("GameScene", SceneType.Game));
    }

    public void LoadTestScene()
    {
        StartCoroutine(LoadSceneCoroutine("TestScene", SceneType.Test));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, SceneType sceneType)
    {
        UIManager.Instance.ShowLoadingScreen();
        UIManager.Instance.UpdateLoadingProgress(0f);
        Time.timeScale = 0f;

        float progress = 0f;
        while (progress < 10f)
        {
            progress += Time.unscaledDeltaTime * 50f;
            UIManager.Instance.UpdateLoadingProgress(progress);
            yield return null;
        }

        CleanupCurrentScene();

        if (sceneName.Contains("Test"))
        {
            progress = 10f;
            while (progress < 70f)
            {
                progress += Time.unscaledDeltaTime * 100f;
                UIManager.Instance.UpdateLoadingProgress(progress);
                yield return null;
            }

            SceneManager.LoadScene(sceneName);

            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                progress = Mathf.Lerp(10f, 70f, asyncLoad.progress / 0.9f);
                UIManager.Instance.UpdateLoadingProgress(progress);
                yield return null;
            }

            asyncLoad.allowSceneActivation = true;
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        switch (sceneType)
        {
            case SceneType.MainMenu:
                UIManager.Instance.SetupMainMenuUI();
                break;
            default:
                UIManager.Instance.SetupGameUI();
                break;
        }

        switch (sceneType)
        {
            case SceneType.MainMenu:
                GameLoopManager.Instance.ChangeState(GameState.MainMenu);
                break;
            case SceneType.Town:
                GameLoopManager.Instance.ChangeState(GameState.Town);
                break;
            case SceneType.Game:
            case SceneType.Test:
                GameLoopManager.Instance.ChangeState(GameState.Stage);
                break;
        }

        while (!IsSceneReady(sceneType))
        {
            progress = Mathf.Lerp(80f, 95f, Time.unscaledDeltaTime);
            UIManager.Instance.UpdateLoadingProgress(progress);
            yield return null;
        }

        while (progress < 100f)
        {
            progress += Time.unscaledDeltaTime * 50f;
            UIManager.Instance.UpdateLoadingProgress(Mathf.Min(100f, progress));
            yield return null;
        }

        UIManager.Instance.HideLoadingScreen();
        Time.timeScale = 1f;
    }

    private bool IsSceneReady(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.MainMenu:
                return UIManager.Instance != null && UIManager.Instance.IsMainMenuActive();

            case SceneType.Town:
                return GameManager.Instance?.player != null
                    && CameraManager.Instance?.IsInitialized == true
                    && UIManager.Instance?.playerUIPanel != null
                    && UIManager.Instance.IsGameUIReady();

            case SceneType.Game:
            case SceneType.Test:
                bool isReady =
                    GameManager.Instance?.player != null
                    && CameraManager.Instance?.IsInitialized == true
                    && UIManager.Instance?.playerUIPanel != null
                    && UIManager.Instance.IsGameUIReady()
                    && MonsterManager.Instance?.IsInitialized == true;

                if (!isReady)
                {
                    Debug.Log(
                        $"Test Scene not ready: Player={GameManager.Instance?.player != null}, "
                            + $"Camera={CameraManager.Instance?.IsInitialized}, "
                            + $"UI={UIManager.Instance?.playerUIPanel != null}, "
                            + $"GameUI={UIManager.Instance?.IsGameUIReady()}, "
                            + $"Monster={MonsterManager.Instance?.IsInitialized}"
                    );
                }

                return isReady;

            default:
                return true;
        }
    }

    private void CleanupCurrentScene()
    {
        var existingPortals = FindObjectsOfType<Portal>();
        foreach (var portal in existingPortals)
        {
            Destroy(portal.gameObject);
        }
        PoolManager.Instance?.ClearAllPools();
    }
    #endregion

    #region Portal Management
    public void SpawnGameStagePortal()
    {
        SpawnPortal(townPortalPosition, SceneType.Game);
    }

    public void SpawnTownPortal(Vector3 position)
    {
        SpawnPortal(position, SceneType.Town);
    }

    private void SpawnPortal(Vector3 position, SceneType destinationType)
    {
        if (portalPrefab != null)
        {
            GameObject portalObj = Instantiate(portalPrefab, position, Quaternion.identity);
            DontDestroyOnLoad(portalObj);

            if (portalObj.TryGetComponent<Portal>(out var portal))
            {
                portal.Initialize(destinationType);
            }
        }
    }

    public void OnPortalEnter(SceneType destinationType)
    {
        switch (destinationType)
        {
            case SceneType.Town:
                PlayerUnitManager.Instance.SaveGameState();
                LoadTownScene();
                break;
            case SceneType.Game:
                LoadGameScene();
                break;
        }
    }
    #endregion
}
