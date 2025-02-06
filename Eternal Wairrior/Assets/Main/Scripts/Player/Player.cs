using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;

public class Player : MonoBehaviour
{
    #region Members

    #region Status
    public enum Status
    {
        Alive = 1,
        Dead,
        Attacking
    }
    private Status _playerStatus;
    public Status playerStatus { get { return _playerStatus; } set { _playerStatus = value; } }
    #endregion

    #region Level & Experience
    [Header("Level Related")]
    [SerializeField]
    public int level = 1;
    public float exp = 0f;

    private List<float> expList = new List<float>
    {
        100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700, 3300
    };
    public List<float> _expList { get { return expList; } }

    public float HpAmount { get => playerStat.GetStat(StatType.CurrentHp) / playerStat.GetStat(StatType.MaxHp); }
    public float ExpAmount { get => (CurrentExp() / (GetExpForNextLevel() - expList[level - 1])); }
    #endregion

    #region References
    public PlayerStatSystem playerStat;  // PlayerStat 참조
    private Rigidbody2D rb;
    private float x = 0;
    private float y = 0;
    public SPUM_Prefabs characterControl;
    public List<Skill> skills;
    private Vector2 velocity;
    #endregion

    #endregion

    // 메서드 대신 프로퍼티로 변경
    public bool IsInitialized { get; private set; }

    #region Unity Message Methods

    private void Awake()
    {
        // 기본 컴포넌트 참조만 가져오기
        playerStat = GetComponent<PlayerStatSystem>();
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Start()
    {
        // PlayerUnitManager를 통한 초기화 요청
        PlayerUnitManager.Instance?.InitializePlayer(this);
        IsInitialized = true;  // 초기화 완료 시점에 설정
    }

    private void OnEnable()
    {
        if (playerStat != null && playerStatus != Status.Dead)
        {
            StartCombatSystems();
        }
    }

    private void OnDisable()
    {
        CleanupPlayer();
    }

    private void CleanupPlayer()
    {
        // 모든 코루틴 정지
        StopAllCoroutines();

        // 전투 시스템 정리
        if (autoAttackCoroutine != null)
        {
            StopCoroutine(autoAttackCoroutine);
            autoAttackCoroutine = null;
        }

        if (healthRegenCoroutine != null)
        {
            StopCoroutine(healthRegenCoroutine);
            healthRegenCoroutine = null;
        }

        // 스킬 정리
        if (skills != null)
        {
            foreach (var skill in skills)
            {
                if (skill != null)
                {
                    Destroy(skill.gameObject);
                }
            }
            skills.Clear();
        }

        // 상태 초기화
        playerStatus = Status.Dead;
        IsInitialized = false;  // 정리 시점에 false로 설정

        Debug.Log("Player cleanup completed");
    }

    public void StartCombatSystems()
    {
        Debug.Log("Starting combat systems...");
        if (playerStatus != Status.Dead)
        {
            if (playerStat == null)
            {
                Debug.LogError("PlayerStat is null!");
                return;
            }

            StartHealthRegeneration();
            StartAutoAttack();
            Debug.Log("Combat systems started successfully");
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Update()
    {
        GetMoveInput();
    }

    #endregion

    #region Methods

    #region Move&Skills
    public void Move()
    {
        if (rb == null) return;

        Vector2 input = new Vector2(x, y).normalized;
        float moveSpeed = playerStat.GetStat(StatType.MoveSpeed);
        velocity = input * moveSpeed;

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        UpdateAnimation();
    }

    private void GetMoveInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
    }

    private void UpdateAnimation()
    {
        if (characterControl != null && playerStatus != Status.Attacking)
        {
            if (velocity != Vector2.zero)
            {
                characterControl.transform.localScale = new Vector3(x > 0 ? 1 : (x < 0 ? -1 : characterControl.transform.localScale.x), 1, 1);
                characterControl.PlayAnimation(PlayerState.MOVE, 0);
            }
            else
            {
                characterControl.PlayAnimation(PlayerState.IDLE, 0);
            }
        }
    }

    #endregion

    #region Level & EXP
    public float CurrentExp()
    {
        if (level >= expList.Count)
        {
            return 0;
        }

        if (level == 1)
        {
            return exp;
        }
        else
        {
            return exp - expList[level - 2];
        }
    }

    public float GetExpForNextLevel()
    {
        if (level >= expList.Count)
        {
            return 99999f;
        }

        if (level == 1)
        {
            return expList[0];
        }
        else
        {
            return expList[level - 1] - expList[level - 2];
        }
    }

    public void GainExperience(float amount)
    {
        if (level >= expList.Count) return;

        exp += amount;

        if (level < expList.Count && exp >= expList[level - 1])
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;

        // 현재 체력 비율 저장
        float currentHpRatio = playerStat.GetStat(StatType.CurrentHp) / playerStat.GetStat(StatType.MaxHp);

        // 레벨업에 따른 스탯 업데이트
        playerStat.UpdateStatsForLevel(level);

        // 새로운 최대 체력
        float maxHp = playerStat.GetStat(StatType.MaxHp);

        // 이전 체력 비율을 유지하면서 체력 설정
        playerStat.SetCurrentHp(maxHp * currentHpRatio);

        Debug.Log($"Level Up! Level: {level}, New MaxHP: {maxHp}, Current HP: {playerStat.GetStat(StatType.CurrentHp)}");
    }
    #endregion

    #region Interactions

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent<IContactable>(out var contact))
        {
            contact.Contact();
        }

        if (other.gameObject.CompareTag("Enemy"))
        {
            rb.constraints = RigidbodyConstraints2D.FreezePosition;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent<IContactable>(out var contact))
        {
            contact.Contact();
        }
    }

    public void TakeHeal(float heal)
    {
        float currentHp = playerStat.GetStat(StatType.CurrentHp);
        float maxHp = playerStat.GetStat(StatType.MaxHp);

        currentHp = Mathf.Min(currentHp + heal, maxHp);
        playerStat.SetCurrentHp(currentHp);
    }

    public void TakeDamage(float damage)
    {
        float defense = playerStat.GetStat(StatType.Defense);
        float actualDamage = Mathf.Max(1, damage - defense);
        float currentHp = playerStat.GetStat(StatType.CurrentHp);

        currentHp -= actualDamage;
        playerStat.SetCurrentHp(currentHp);

        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        playerStatus = Status.Dead;
        StopAllCoroutines();

        // 게임 오버 상태로 전환만 하고, 타운으로의 자동 전환은 하지 않음
        GameLoopManager.Instance.ChangeState(GameLoopManager.GameState.GameOver);
    }

    #endregion

    #region Combat
    private Coroutine autoAttackCoroutine;
    private float attackAngle = 120f;  // 공격 각도는 상수로 유지하거나 PlayerStatData로 이동 가능

    private IEnumerator PerformAttack(Enemy targetEnemy)
    {
        if (characterControl == null) yield break; // 안전 체크 추가

        Vector2 directionToTarget = (targetEnemy.transform.position - transform.position).normalized;

        // 캐릭터 방향 설정
        characterControl.transform.localScale = new Vector3(
            directionToTarget.x > 0 ? -1 : 1, 1, 1);

        // 공격 상태 및 애니메이션 설정
        playerStatus = Status.Attacking;
        characterControl.PlayAnimation(PlayerState.ATTACK, 0);

        // 공격 딜레이
        //yield return new WaitForSeconds(0.2f);

        // 범위 내 적 찾기  데미지 적용
        float attackRange = playerStat.GetStat(StatType.AttackRange);
        float damage = playerStat.GetStat(StatType.Damage);

        var enemiesInRange = GameManager.Instance.enemies
            .Where(enemy => enemy != null)
            .Where(enemy =>
            {
                Vector2 directionToEnemy = enemy.transform.position - transform.position;
                float distanceToEnemy = directionToEnemy.magnitude;
                float angle = Vector2.Angle(directionToTarget, directionToEnemy);

                return distanceToEnemy <= attackRange && angle <= attackAngle / 2f;
            })
            .ToList();

        // 데미지 적용
        foreach (Enemy enemy in enemiesInRange)
        {
            enemy.TakeDamage(damage);
        }

        playerStatus = Status.Alive;
    }

    private Enemy FindNearestEnemy()
    {
        return GameManager.Instance.enemies?
            .Where(enemy => enemy != null)
            .OrderBy(enemy => Vector2.Distance(transform.position, enemy.transform.position))
            .FirstOrDefault();
    }

    #endregion

    #region Passive Skill Effects
    public void ActivateHoming(bool activate)
    {
        playerStat.ActivateHoming(activate);
    }

    public void ResetPassiveEffects()
    {
        playerStat.RemoveStatsBySource(SourceType.Passive);
    }
    #endregion

    #region Health Regeneration
    private Coroutine healthRegenCoroutine;
    private const float REGEN_TICK_RATE = 1f;

    private void StartHealthRegeneration()
    {
        if (healthRegenCoroutine != null)
            StopCoroutine(healthRegenCoroutine);

        healthRegenCoroutine = StartCoroutine(HealthRegenCoroutine());
    }

    private IEnumerator HealthRegenCoroutine()
    {
        WaitForSeconds wait = new WaitForSeconds(REGEN_TICK_RATE);

        while (true)
        {
            if (playerStat != null)
            {
                float regenAmount = playerStat.GetStat(StatType.HpRegenRate);
                if (regenAmount > 0)
                {
                    TakeHeal(regenAmount);
                }
            }
            yield return wait;
        }
    }
    #endregion

    #endregion

    #region Skills
    public bool AddOrUpgradeSkill(SkillData skillData)
    {
        if (skillData == null) return false;

        // SkillManager를 통해 스킬 추가/업그레이드
        SkillManager.Instance.AddOrUpgradeSkill(skillData);
        return true;
    }

    public void RemoveSkill(SkillID skillID)
    {
        var skillToRemove = skills.Find(s => s.SkillID == skillID);
        if (skillToRemove != null)
        {
            skills.Remove(skillToRemove);
            Destroy(skillToRemove.gameObject);
        }
    }
    #endregion

    private void StartAutoAttack()
    {
        Debug.Log("Attempting to start auto attack...");

        if (autoAttackCoroutine != null)
        {
            Debug.Log("Stopping existing auto attack coroutine");
            StopCoroutine(autoAttackCoroutine);
            autoAttackCoroutine = null;
        }

        Debug.Log("Starting new auto attack coroutine");
        autoAttackCoroutine = StartCoroutine(AutoAttackCoroutine());
    }

    private IEnumerator AutoAttackCoroutine()
    {
        Debug.Log("Auto attack coroutine started");

        while (true)
        {
            if (playerStatus != Status.Dead)
            {
                Enemy nearestEnemy = FindNearestEnemy();
                if (nearestEnemy != null)
                {
                    float distanceToEnemy = Vector2.Distance(transform.position, nearestEnemy.transform.position);
                    float attackRange = playerStat.GetStat(StatType.AttackRange);

                    if (distanceToEnemy <= attackRange)
                    {
                        yield return StartCoroutine(PerformAttack(nearestEnemy));
                    }
                }
            }

            float attackDelay = 1f / playerStat.GetStat(StatType.AttackSpeed);
            yield return new WaitForSeconds(attackDelay);
        }
    }
}
