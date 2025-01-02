using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerManager : SingletonManager<PlayerManager>, IInitializable
{
    public Player player;

    private bool isInitialized;
    public bool IsInitialized { get => isInitialized; }


    public void SpawnPlayer(Transform spawnPoint)
    {
        player = Instantiate(player, spawnPoint.position, Quaternion.identity);
        player.gameObject.name = "Player";
        player.Initialize();
        this.isInitialized = player.IsInitialized;
    }

    public void RespawnPlayer(Transform spawnPoint)
    {
        NavMeshAgent agent = player.Agent;
        if (agent != null)
        {
            agent.enabled = false;
            agent.Warp(spawnPoint.position);
            agent.enabled = true;
        }
        if(player.pet != null) 
        {
            NavMeshAgent petAgent = player.pet.Agent;
            if (petAgent != null)
            {
                petAgent.enabled = false;
                petAgent.Warp(spawnPoint.position);
                petAgent.enabled = true;
            }
        }
        player.ResetPlayer();
    }

    public void Initialize()
    {
    }

    public void Initialize(Transform spawnPoint)
    {
        SpawnPlayer(spawnPoint);
    }
}
