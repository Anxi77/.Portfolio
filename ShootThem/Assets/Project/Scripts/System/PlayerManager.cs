using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Player playerPrefab;
    public Player player { get; private set; }
    public Transform spawnPoint;

    public void SpawnPlayer()
    {
        player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
    }
}