using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Pet : Unit
{
    private Player targetPlayer;
    public Transform enemyTarget;
    public Transform attackPoint;
    public Animator P_Animator;
    public float playDis;
    public float EnemyDis;
    public float playerTellme = 15f;

    [SerializeField] private Transform FirePoint;

    public GameObject tellEffect;

    public Skill petSkill;

    public void Initialize(Player player)
    {
        base.Initialize();
        targetPlayer = player;
        agent = GetComponent<NavMeshAgent>();
        P_Animator = GetComponent<Animator>();
        Animator = P_Animator;
        agent.stoppingDistance = 8;
        petSkill = Instantiate(petSkill, transform);
        petSkill.Initialize(this);
    }

    private void Update()
    {
        Move();
        SetAnim();
        PlayerTell();
    }

    private void Move()
    {
        if (enemyTarget != null)
        {
            agent.stoppingDistance = EnemyDis;
            agent.SetDestination(enemyTarget.position);
            transform.LookAt(enemyTarget.position);
        }
        else if (targetPlayer.transform != null)
        {
            agent.stoppingDistance = playDis;
            agent.SetDestination(targetPlayer.transform.position);
        }
    }

    private void PlayerTell()
    {
        if (Vector3.Distance(transform.position, targetPlayer.transform.position) >= playerTellme)
        {
            agent.enabled = false;  
            transform.position = targetPlayer.transform.position;            
            enemyTarget = null;
            agent.enabled = true;
            P_Animator.SetTrigger("Tell");
            StartCoroutine(SpawnEffect());
        }

    }

    private IEnumerator SpawnEffect()
    {
        GameObject petEffect = Instantiate(tellEffect, this.transform);
        yield return new WaitForSeconds(1.5f);
        Destroy(petEffect);
    }

    private void SetAnim()
    {
        if (agent.velocity.sqrMagnitude < 0.01f)
        {
            P_Animator.SetBool("isRunning", false);
        }
        else
        {
            P_Animator.SetBool("isRunning", true);
        }
    }

    private void FireSkill()
    {
        if (enemyTarget == null) return;

        float attackDelay = 1f / CharacterStats.GetStatValue(StatType.AttackSpeed);
        if (Time.time - lastAttackTime < attackDelay) return;

        Vector3 directionToTarget = (enemyTarget.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(directionToTarget);

        if (petSkill.TryUseSkill())
        {
            lastAttackTime = Time.time;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<Monster>(out Monster monster))
        {
            enemyTarget = other.transform;
            FireSkill();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Monster>(out Monster monster))
        {
            enemyTarget = null;
        }
    }
}
