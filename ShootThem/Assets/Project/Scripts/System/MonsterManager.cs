using UnityEngine;
using System.Collections.Generic;

public class MonsterManager : MonoBehaviour
{
    public Monster enemyPrefab;
    public List<Monster> enemies = new List<Monster>();
    private Player player;

    public void Init(Player player)
    {
        this.player = player;
    }

    public void SpawnMonster()
    {
        float spawnDistance = Random.Range(10f, 15f);
        Vector3 randomPoint = Random.insideUnitSphere * spawnDistance;
        randomPoint.y = 1f;
        Vector3 spawnPoint = player.transform.position + randomPoint;
        Monster monster = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);
        monster.Init(player);
        enemies.Add(monster);
    }
}