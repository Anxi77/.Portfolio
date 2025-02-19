using System.Collections;
using UnityEngine;

public class InitializationManager : MonoBehaviour
{
    private void Start()
    {
        InitializeEventSystem();
        CreateManagerObjects();
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.StartInitialization();
            StartCoroutine(WaitForInitialization());
        }
    }

    private IEnumerator WaitForInitialization()
    {
        while (!GameLoopManager.Instance.IsInitialized)
        {
            yield return null;
        }
        StageManager.Instance?.LoadMainMenu();
    }

    private void CreateManagerObjects()
    {
        GameObject[] managers = Resources.LoadAll<GameObject>("Prefabs/Managers");

        foreach (GameObject manager in managers)
        {
            GameObject instance = Instantiate(manager);
            DontDestroyOnLoad(instance);

            if (manager.name == "PathFindingManager")
            {
                instance.SetActive(false);
            }
        }
    }

    private void InitializeEventSystem()
    {
        var existingEventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (existingEventSystem == null)
        {
            var eventSystemPrefab = Resources.Load<GameObject>("Prefabs/System/EventSystem");
            var eventSystemObj = Instantiate(eventSystemPrefab);
            DontDestroyOnLoad(eventSystemObj);
        }
    }
}
