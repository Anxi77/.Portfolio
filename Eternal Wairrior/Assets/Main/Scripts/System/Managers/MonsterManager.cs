using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterManager : SingletonManager<MonsterManager>
{
    #region Members

    private static MonsterManager instance;

    #region Stats

    [Tooltip("�ѹ��� ������ ���� ��. \nX : �ּ� , Y : �ִ�")]
    public Vector2Int minMaxCount;

    [Tooltip("������ �� �÷��̾�κ����� �ִ�/�ּ� �Ÿ�.\n X : �ּ� , Y : �ִ�")]
    public Vector2 minMaxDist;

    public float spawnInterval;

    #endregion

    public Enemy enemyPrefab;

    #endregion

    #region Unity Message Methods

    private void Start()
    {
        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            int enemyCount = Random.Range(minMaxCount.x, minMaxCount.y);
            Spawn(enemyCount);

        }
    }

    private void Spawn(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 playerPos = GameManager.Instance.player.transform.position;

            Vector2 ranPos = Random.insideUnitCircle;

            Vector2 spawnPos = (ranPos * (minMaxDist.y - minMaxDist.x)) + (ranPos.normalized * minMaxDist.x);            

            Vector2 FinalPos = playerPos + spawnPos;

            Enemy enemy = LeanPool.Spawn(enemyPrefab);

        }
    }
    #endregion
}

