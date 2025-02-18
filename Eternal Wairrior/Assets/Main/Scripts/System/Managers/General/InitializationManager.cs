using UnityEngine;
using System.Collections;

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
            Instantiate(manager);
            if (manager.name == "PathFindingManager")
            {
                manager.SetActive(false);
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