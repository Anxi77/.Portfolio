using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InitializationManager : MonoBehaviour
{
    [System.Serializable]
    public class ManagerPrefabData
    {
        public string managerName;
        public GameObject prefab;
    }

    [SerializeField] private ManagerPrefabData[] managerPrefabs;
    [SerializeField] private GameObject eventSystemPrefab;
    [SerializeField] private bool loadTestScene = false;

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

        if (loadTestScene)
        {
            Debug.Log("Loading test scene...");
            StageManager.Instance?.LoadTestScene();
        }

        else
        {
            Debug.Log("Loading main menu...");
            StageManager.Instance?.LoadMainMenu();
        }
    }

    private void CreateManagerObjects()
    {
        foreach (var managerData in managerPrefabs)
        {
            if (managerData.prefab != null)
            {
                var manager = Instantiate(managerData.prefab);
                manager.name = managerData.managerName;
                DontDestroyOnLoad(manager);

                if (managerData.managerName == "PathFindingManager")
                {
                    manager.SetActive(false);
                }
            }
        }
    }

    private void InitializeEventSystem()
    {
        var existingEventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (existingEventSystem == null && eventSystemPrefab != null)
        {
            var eventSystemObj = Instantiate(eventSystemPrefab);
            eventSystemObj.name = "EventSystem";
            DontDestroyOnLoad(eventSystemObj);
        }
    }
}