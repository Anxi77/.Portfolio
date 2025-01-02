using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private ParticleSystem hitParticle;
    protected ProjectileData data;
    private Vector3 startPosition;
    private float distanceTraveled;
    private int remainingPierceCount;
    private Transform target;
    private bool isOwnerPlayer;

    public void Initialize(ProjectileData data)
    {
        this.data = data;
        startPosition = transform.position;
        remainingPierceCount = data.PierceCount;
        isOwnerPlayer = data.owner is Player;
        transform.localScale = Vector3.one * data.Scale;

        if (data.IsHoming)
            FindTarget();
    }

    private void Update()
    {
        ProjectileMove();
    }

    private void ProjectileMove()
    {
        Vector3 moveDirection = transform.forward;
        float moveDistance = data.Speed * Time.deltaTime;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, moveDirection, out hit, moveDistance, LayerMask.GetMask("Obstacle","Ground")))
        {
            transform.position = hit.point;
            PlayHitParticle();
            PoolManager.Instance.Despawn(this);
            return;
        }

        if (data.IsHoming && target != null)
        {
            Unit targetUnit = target.GetComponent<Unit>();
            if (!targetUnit.IsAlive)
            {
                target = null;
            }
            else
            {
                Vector3 direction = (target.position - transform.position).normalized;
                transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * data.Speed);
            }
        }

        transform.position += moveDirection * moveDistance;
        distanceTraveled = Vector3.Distance(startPosition, transform.position);

        if (distanceTraveled >= data.Range)
        {
            PlayHitParticle();
            PoolManager.Instance.Despawn(this);
        }
    }

    private void PlayHitParticle()
    {
        if (hitParticle != null)
        {
            ParticleSystem hp = PoolManager.Instance.Spawn<ParticleSystem>(hitParticle.gameObject, transform.position, Quaternion.identity);
            hp.Play();
            PoolManager.Instance.Despawn(hp, 1.5f);
        }
    }

    private void FindTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, data.HomingRange);
        float closestDistance = float.MaxValue;

        foreach (var collider in colliders)
        {
            if (!collider.TryGetComponent<Unit>(out Unit targetUnit))
                continue;

            bool isValidTarget = false;
            if (isOwnerPlayer && targetUnit is Monster)
                isValidTarget = true;
            else if (!isOwnerPlayer && targetUnit is Player && !(data.owner is Pet))
                isValidTarget = true;
            else if (data.owner is Pet && targetUnit is Monster)
                isValidTarget = true;

            if (isValidTarget)
            {
                float distance = Vector3.Distance(transform.position, targetUnit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    target = targetUnit.transform;
                }
            }
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Projectile collision with: {other.gameObject.name}");
        Debug.Log($"Owner is: {data.owner.GetType().Name}");

        if (other.gameObject == null || data.owner.gameObject == null)
        {
            return;
        }

        if (other.gameObject == data.owner.gameObject)
        {
            Debug.Log("Collision with owner, ignoring");
            return;
        }

        if (data.owner is Pet && other.gameObject == GameManager.Instance.player.gameObject)
        {
            Debug.Log("Pet projectile hit player, ignoring");
            return;
        }

        if (!other.TryGetComponent<Unit>(out Unit targetUnit))
        {
            Debug.Log("Target is not a Unit, ignoring");
            return;
        }

        Debug.Log($"Target Unit type: {targetUnit.GetType().Name}");

        bool isValidTarget = false;
        if ((data.owner is Player || data.owner is Pet) && targetUnit is Monster)
        {
            Debug.Log("Valid target: Player/Pet hitting Monster");
            isValidTarget = true;
        }
        else if (!(data.owner is Player || data.owner is Pet) && targetUnit is Player)
        {
            Debug.Log("Valid target: Monster hitting Player");
            isValidTarget = true;
        }
        else
        {
            Debug.Log("Invalid target combination");
        }

        if (isValidTarget)
        {
            Debug.Log($"Dealing damage to: {targetUnit.gameObject.name}");
            targetUnit.TakeDamage(data.Damage);
            PlayHitParticle();

            remainingPierceCount--;
            if (remainingPierceCount == 0)
            {
                PoolManager.Instance.Despawn(this);
            }
        }
    }
}