using UnityEngine;
using UnityEngine.AI;

public abstract class Unit : MonoBehaviour, IDamageable, IMovable, IInitializable
{
    protected NavMeshAgent agent;
    public NavMeshAgent Agent { get => agent; set => agent = value; }

    protected ICharacterStats characterStats;
    private Animator animator;

    private Unit target = null;
    public Unit Target { get => target; set => target = value; }

    public Animator Animator { get; set; }

    private bool isDashing = false;
    public bool IsDashing { get => isDashing; set => isDashing = value; }

    public ICharacterStats CharacterStats => characterStats;

    public AnimatorController AnimController { get; set; }
    public bool IsInitialized { get; set; }
    public bool wasAttacked { get; set; } = false;

    protected float lastAttackTime;
    protected Coroutine attackCoroutine;



    public virtual void Initialize()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        characterStats = GetComponent<ICharacterStats>();
        if (characterStats == null)
        {
            Debug.LogError($"이색기 스탯 안달림 {gameObject.name}");
        }

        characterStats.InitializeStats();

        UpdateMoveSpeed();

        IsInitialized = true;
    }

    #region 전투관련
    public virtual bool IsAlive => characterStats.GetStatValue(StatType.Health) > 0;


    public virtual void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        float defense = characterStats.GetStatValue(StatType.Defense);
        float damageMultiplier = 100f / (100f + defense);
        float finalDamage = damage * damageMultiplier;

        finalDamage = Mathf.Max(1f, finalDamage);

        float currentHealth = characterStats.GetStatValue(StatType.Health);
        finalDamage = Mathf.Min(finalDamage, currentHealth);

        StatModifier healthMod = new StatModifier(-finalDamage, StatModifierType.Flat);
        characterStats.AddModifier(StatType.Health, healthMod);
        wasAttacked = true;
    }
    #endregion

    #region 움직임 관련
    public virtual float MoveSpeed
    {
        get => agent.speed;
        protected set => agent.speed = value;
    }

    public virtual bool IsMoving => agent != null && agent.hasPath && agent.velocity.sqrMagnitude > 0.01f;


    public virtual void MoveTo(Vector3 destination)
    {
        if (agent == null || !agent.isActiveAndEnabled || !IsAlive)
            return;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(destination, out hit, 1f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    public virtual void StopMoving()
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.ResetPath();
        }
    }

    public virtual void UpdateMoveSpeed()
    {
        if (characterStats != null)
        {
            MoveSpeed = characterStats.GetStatValue(StatType.MoveSpeed);
        }
    }
    public virtual bool HasReachedDestination()
    {

        if (agent == null || !agent.isActiveAndEnabled)
            return false;


        if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathPending)
            return false;


        if (agent.remainingDistance <= agent.stoppingDistance)
        {

            if (agent.velocity.sqrMagnitude < 0.01f && !agent.pathPending)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    public void ChangeAnimController(RuntimeAnimatorController newAnimator)
    {
        Animator.runtimeAnimatorController = newAnimator;
    }

    #region 기즈모 관련
#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (characterStats != null)
        {
            float attackRange = characterStats.GetStatValue(StatType.AttackRange);
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
            Gizmos.DrawSphere(transform.position, attackRange);
        }
    }

    private void OnDrawGizmos()
    {
        if (characterStats != null)
        {
            float attackRange = characterStats.GetStatValue(StatType.AttackRange);
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, attackRange);

            if (UnityEditor.Selection.activeGameObject == gameObject)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
                Gizmos.DrawSphere(transform.position, attackRange);
            }
        }
    }

    void IInitializable.Initialize()
    {
        throw new System.NotImplementedException();
    }

#endif
    #endregion
}
