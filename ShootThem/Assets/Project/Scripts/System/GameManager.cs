using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameManager() { }
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameManager");
                instance = go.AddComponent<GameManager>();
            }
            return instance;
        }
    }
    public Player player { get; }
    public MonsterManager monsterManager;
    public PlayerManager playerManager;

    public Transform playerSpawnPoint;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        monsterManager = Instantiate(monsterManager, transform, false);
        playerManager = Instantiate(playerManager, transform, false);
        playerManager.spawnPoint = playerSpawnPoint;
        playerManager.SpawnPlayer();
        monsterManager.Init(playerManager.player);
        monsterManager.SpawnMonster();
    }

}