using UnityEngine;

public class ExpParticle : MonoBehaviour, IContactable
{
    public float expValue = 1f;
    public float moveSpeed = 5f;
    public float spreadSpeed = 2f;
    [SerializeField] private float MAX_SPREAD_TIME = 0.5f;

    private Player player;
    private Vector2 velocity;
    private float spreadTime = 0f;
    private PlayerStatSystem playerStat;
    private void OnEnable()
    {
        player = GameManager.Instance?.player;
        playerStat = GameManager.Instance?.player.GetComponent<PlayerStatSystem>();
    }

    private void Start()
    {
        float randomAngle = Random.Range(0f, 360f);
        velocity = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        ) * spreadSpeed;
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (spreadTime < MAX_SPREAD_TIME)
        {
            spreadTime += Time.deltaTime;
            float spreadProgress = spreadTime / MAX_SPREAD_TIME;
            float currentSpreadSpeed = Mathf.Lerp(spreadSpeed, 0f, spreadProgress);
            transform.position += (Vector3)velocity * currentSpreadSpeed * Time.deltaTime;
        }
        else if (distanceToPlayer <= playerStat.GetStat(StatType.ExpCollectionRadius))
        {
            Vector2 direction = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;

            float distanceRatio = distanceToPlayer / playerStat.GetStat(StatType.ExpCollectionRadius);
            float speedMultiplier = Mathf.Lerp(3f, 1f, distanceRatio);
            float exponentialMultiplier = 1f + Mathf.Pow((1f - distanceRatio), 2f) * 2f;
            float finalSpeed = moveSpeed * speedMultiplier * exponentialMultiplier;

            transform.position += (Vector3)(direction * finalSpeed * Time.deltaTime);
        }
    }

    public void Contact()
    {
        player.GainExperience(expValue);
        PoolManager.Instance.Despawn<ExpParticle>(this);
    }

}
