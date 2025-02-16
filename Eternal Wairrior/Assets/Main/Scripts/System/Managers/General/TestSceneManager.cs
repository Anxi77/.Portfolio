using UnityEngine;
using static InitializationManager;

public class TestSceneManager : MonoBehaviour
{
    [SerializeField] private ManagerPrefabData[] managerPrefabs;
    [SerializeField] private bool autoStartGameLoop = true;
    private void Start()
    {
        if (!IsInitialized())
        {
            Debug.Log("Loading InitScene for proper initialization...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("InitScene");
            return;
        }

        InitializeManagers();

        if (autoStartGameLoop && StageManager.Instance != null)
        {
            StageManager.Instance.LoadTestScene();
        }
    }

    private bool IsInitialized()
    {
        return GameManager.Instance != null &&
               UIManager.Instance != null &&
               GameLoopManager.Instance != null;
    }

    private void InitializeManagers()
    {
        foreach (var managerData in managerPrefabs)
        {
            if (managerData.prefab != null &&
                GameObject.Find(managerData.managerName) == null)
            {
                var manager = Instantiate(managerData.prefab);
                manager.name = managerData.managerName;
                DontDestroyOnLoad(manager);
                Debug.Log($"Initialized {managerData.managerName}");
            }
        }
    }
}