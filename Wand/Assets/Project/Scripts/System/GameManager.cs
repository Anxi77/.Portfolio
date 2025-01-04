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
    public Player player { get; private set; }
    public MonsterManager monsterManager;
    public PlayerManager playerManager;

    public Transform playerSpawnPoint;

    public Transform wandSpawnPoint;

    public Transform wand;

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
        AudioListener.volume = 0.5f;
        monsterManager = Instantiate(monsterManager, transform, false);
        playerManager = Instantiate(playerManager, transform, false);
        playerManager.spawnPoint = playerSpawnPoint;
        playerManager.SpawnPlayer();
        this.player = playerManager.player;
        monsterManager.Init(playerManager.player);
        wand = Instantiate(wand, wandSpawnPoint.position, wandSpawnPoint.rotation, wandSpawnPoint);
    }

    public void ResetWand()
    {
        wand.gameObject.transform.position = wandSpawnPoint.position;
        wand.gameObject.transform.rotation = wandSpawnPoint.rotation;
    }
}