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

    private void Start()
    {
        // 1. EventSystem �ʱ�ȭ
        InitializeEventSystem();

        // 2. �Ŵ��� ������Ʈ�鸸 ���� (�ʱ�ȭ�� ���� ����)
        CreateManagerObjects();

        // 3. GameLoopManager�� ���� ���� �ʱ�ȭ ����
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.StartInitialization();
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

                // PathFindingManager�� �ʱ⿡ ��Ȱ��ȭ
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