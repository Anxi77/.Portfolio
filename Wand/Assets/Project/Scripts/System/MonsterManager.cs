using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MonsterManager : MonoBehaviour
{
    public Monster enemyPrefab;
    public List<Monster> enemies = new List<Monster>();
    private Player player;
    public float spawnDelay = 5f;

    public bool isSpawning = false;

    public void Init(Player player)
    {
        this.player = player;
    }

    public void StartSpawn()
    {
        isSpawning = true;
        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        while (isSpawning)
        {
            if (enemyPrefab != null)
            {
                float spawnDistance = Random.Range(10f, 15f);
                Vector3 randomPoint = Random.insideUnitSphere * spawnDistance;
                randomPoint.y = 0.1f;
                Vector3 spawnPoint = player.transform.position + randomPoint;
                Monster monster = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);
                monster.Init(player);
                enemies.Add(monster);
            }
            yield return new WaitForSeconds(spawnDelay);

        }
    }
}