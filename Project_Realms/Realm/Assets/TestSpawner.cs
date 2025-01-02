using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestSpawner : MonoBehaviour
{
    public Monster monster;
    public Transform MobPos;
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(TestSpawn);
    }

    private void TestSpawn()
    {
        monster = PoolManager.Instance.Spawn<Monster>(monster.gameObject, MobPos.position, Quaternion.identity);
        monster.Initialize();
    }
}