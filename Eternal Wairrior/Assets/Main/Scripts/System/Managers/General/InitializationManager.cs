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
    [SerializeField] private bool loadTestScene = false; // 테스트 씬 로드 여부

    private void Start()
    {
        // 1. EventSystem 초기화
        InitializeEventSystem();

        // 2. 매니저 오브젝트들만 생성 (초기화는 아직 안함)
        CreateManagerObjects();

        // 3. GameLoopManager를 통한 순차 초기화 시작
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.StartInitialization();
            StartCoroutine(WaitForInitialization());
        }
    }

    private IEnumerator WaitForInitialization()
    {
        // 모든 매니저의 초기화가 완료될 때까지 대기
        while (!GameLoopManager.Instance.IsInitialized)
        {
            yield return null;
        }

        // 초기화 완료 후 테스트 씬 또는 메인 메뉴로 이동
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

                // PathFindingManager는 초기에 비활성화
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