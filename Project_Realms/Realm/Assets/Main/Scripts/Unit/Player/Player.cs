using ProjectDawn.Navigation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class Player : Unit
{
    [System.Serializable]
    public class LevelData
    {
        public float baseExpRequired = 100f;  // 1레벨에서 2레벨로 가는데 필요한 경험치

        [Tooltip("레벨 구간별 경험치 증가율")]
        public float[] growthRates = new float[] { 1.2f, 1.4f, 1.6f };  // 구간별 증가율

        [Tooltip("증가율이 변경되는 레벨")]
        public int[] levelBreakpoints = new int[] { 10, 30, 50 };  // 구간 분기점
    }

    [SerializeField] private LevelData levelData;

    internal SkillController skillController;

    private float totalExp = 0f;  // 누적 경험치

    private LayerMask groundLayerMask;

    private StatPointSystem statPoint;

    public StatPointSystem statPointSystem => statPoint;


    private PlayerStateHandler playerHandler;
    public PlayerStateHandler PlayerHandler => playerHandler;

    private int skillPoint;
    public int SkillPoint { get => skillPoint; set => skillPoint = value; }



    private Vector3 targetPos = Vector3.zero;
    public Vector3 TargetPos => targetPos;

    private PlayerInventorySystem inventorySystem;
    public PlayerInventorySystem InventorySystem => inventorySystem;

    [Header("Regeneration Settings")]
    [SerializeField] private float regenTickTime = 1f;
    private Coroutine healthRegenCoroutine;
    private Coroutine manaRegenCoroutine;

    public LayerMask GroundLayer => groundLayerMask;

    private PlayerInputManager inputManager;
    public PlayerInputManager InputManager => inputManager;

    public Pet pet { get; set; }

    public override void Initialize()
    {

        if (characterStats == null)
        {
            characterStats = GetComponent<PlayerStat>();

            if (characterStats == null)
            {
                Debug.LogError("PlayerStat component not found!");
                return;
            }
        }

        groundLayerMask = LayerMask.GetMask("Ground");

        statPoint = GetComponent<StatPointSystem>();

        statPoint.Initialize(this);

        GameManager.Instance.player = this;

        skillController = GetComponent<SkillController>();

        if (skillController == null)
        {
            skillController = gameObject.AddComponent<SkillController>();
        }

        skillController.Initialize();

        inventorySystem = GetComponent<PlayerInventorySystem>();

        if (inventorySystem == null)
        {
            inventorySystem = gameObject.AddComponent<PlayerInventorySystem>();
        }

        AnimController = GetComponent<AnimatorController>();

        if (AnimController == null)
        {
            AnimController = gameObject.AddComponent<AnimatorController>();
        }
        AnimController.Initialize(this);

        Animator = GetComponent<Animator>();

        if (Animator == null)
        {
            Animator = gameObject.AddComponent<Animator>();
        }

        playerHandler = new PlayerStateHandler(this);
        playerHandler.Initialize();

        base.Initialize();

        StartRegeneration();

        inputManager = gameObject.AddComponent<PlayerInputManager>();
        inputManager.Initialize(this);

        Debug.Log("Player initialized successfully");


        skillPoint += 3;
    }

    private void Update()
    {
        playerHandler.HandleUpdate();
    }

    #region 레벨 시스템
    public float TotalExp => totalExp;

    public float ExpToNextLevel => CalculateRequiredExp(GetCurrentLevel());

    public float ExpPercentage
    {
        get
        {
            int currentLevel = GetCurrentLevel();

            float expForCurrentLevel = CalculateTotalExpForLevel(currentLevel);

            float expForNextLevel = CalculateTotalExpForLevel(currentLevel + 1);

            return (totalExp - expForCurrentLevel) / (expForNextLevel - expForCurrentLevel);
        }
    }

    public void GainExperience(float amount)
    {
        if (!IsAlive) return;

        float oldExp = totalExp;

        totalExp += amount;

        int oldLevel = CalculateLevelFromExp(oldExp);

        int newLevel = CalculateLevelFromExp(totalExp);

        if (newLevel > oldLevel)
        {
            for (int i = oldLevel + 1; i <= newLevel; i++)
            {
                PerformLevelUp(i);
            }
        }
    }

    private int CalculateLevelFromExp(float exp)
    {
        int level = 1;

        float nextLevelExp = CalculateTotalExpForLevel(level + 1);

        while (exp >= nextLevelExp)
        {
            level++;

            nextLevelExp = CalculateTotalExpForLevel(level + 1);
        }

        return level;
    }

    private float CalculateTotalExpForLevel(int level)
    {
        float total = 0f;

        for (int i = 1; i < level; i++)
        {
            total += CalculateRequiredExp(i);
        }

        return total;
    }

    private float CalculateRequiredExp(int level)
    {
        float currentGrowthRate = levelData.growthRates[0];

        for (int i = 0; i < levelData.levelBreakpoints.Length; i++)
        {
            if (level > levelData.levelBreakpoints[i])
            {
                currentGrowthRate = levelData.growthRates[Mathf.Min(i + 1, levelData.growthRates.Length - 1)];
            }
        }

        return levelData.baseExpRequired * Mathf.Pow(currentGrowthRate, level - 1);
    }

    private void PerformLevelUp(int newLevel)
    {
        StatModifier levelMod = new StatModifier(1f, StatModifierType.Flat, this);

        characterStats.AddModifier(StatType.Level, levelMod);

        statPoint.AddStatPoints(5);

        skillPoint += 3;

        OnLevelUp();
    }

    protected virtual void OnLevelUp()
    {

    }

    private int GetCurrentLevel()
    {
        return Mathf.RoundToInt(characterStats.GetStatValue(StatType.Level));
    }
    #endregion

    public float GetAttack()
    {
        return characterStats.GetStatValue(StatType.Damage);
    }

    public float GetDefense()
    {
        return characterStats.GetStatValue(StatType.Defense);
    }

    private void StartRegeneration()
    {
        if (healthRegenCoroutine != null)
            StopCoroutine(healthRegenCoroutine);
        if (manaRegenCoroutine != null)
            StopCoroutine(manaRegenCoroutine);

        healthRegenCoroutine = StartCoroutine(HealthRegenCoroutine());
        manaRegenCoroutine = StartCoroutine(ManaRegenCoroutine());
    }

    private IEnumerator HealthRegenCoroutine()
    {
        while (IsAlive)
        {
            float currentHealth = characterStats.GetStatValue(StatType.Health);
            float maxHealth = characterStats.GetStatValue(StatType.MaxHealth);
            float regenRate = characterStats.GetStatValue(StatType.HealthRegenRate);

            if (currentHealth < maxHealth)
            {
                characterStats.AddModifier(StatType.Health, new StatModifier(regenRate, StatModifierType.Flat, SourceType.BaseStats));
            }

            yield return new WaitForSeconds(regenTickTime);
        }
    }

    private IEnumerator ManaRegenCoroutine()
    {
        while (IsAlive)
        {
            float currentMana = characterStats.GetStatValue(StatType.Mana);
            float maxMana = characterStats.GetStatValue(StatType.MaxMana);
            float regenRate = characterStats.GetStatValue(StatType.ManaRegenRate);

            if (currentMana < maxMana)
            {
                characterStats.AddModifier(StatType.Mana, new StatModifier(regenRate, StatModifierType.Flat, SourceType.BaseStats));
            }

            yield return new WaitForSeconds(regenTickTime);
        }
    }
    public void PlayerDie()
    {
        StartCoroutine(DieRoutine());
    }

    public IEnumerator DieRoutine()
    {
        Animator.SetTrigger("Die");

        while (!Animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
        {
            yield return null;
        }

        while (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }
        GameManager.Instance.HandlePlayerDeath();
    }
    public void ClearTarget()
    {
        Target = null;
    }


    private void OnDisable()
    {
        if (healthRegenCoroutine != null)
            StopCoroutine(healthRegenCoroutine);
        if (manaRegenCoroutine != null)
            StopCoroutine(manaRegenCoroutine);
    }

    public void SetTarget(Unit monster)
    {
        Target = monster;
        targetPos = Vector3.zero;
    }

    public void SetDestination(Vector3 position)
    {
        targetPos = position;
        Target = null;
    }

    public void ResetPlayer()
    {
        ResetHPMP();
        Animator.Play("Idle");
        playerHandler.Initialize();
        StartRegeneration();
    }

    private void ResetHPMP()
    {
        Debug.Log($"[ResetHPMP 시작] 모든 값 체크:" +
                  $"\nHealth Base: {((FloatStat)CharacterStats.GetStat(StatType.Health)).BaseValue}" +
                  $"\nCurrent Health: {CharacterStats.GetStatValue(StatType.Health)}" +
                  $"\nMax Health: {CharacterStats.GetStatValue(StatType.MaxHealth)}");

        CharacterStats.GetStat(StatType.Health).ClearAllModifiers();
        CharacterStats.GetStat(StatType.Mana).ClearAllModifiers();

        float maxHealth = CharacterStats.GetStatValue(StatType.MaxHealth);
        float maxMana = CharacterStats.GetStatValue(StatType.MaxMana);

        ((FloatStat)CharacterStats.GetStat(StatType.Health)).SetBaseValue(maxHealth);
        ((FloatStat)CharacterStats.GetStat(StatType.Mana)).SetBaseValue(maxMana);

        Debug.Log($"[ResetHPMP 완료] 최종 값 체크:" +
                  $"\nHealth Base: {((FloatStat)CharacterStats.GetStat(StatType.Health)).BaseValue}" +
                  $"\nCurrent Health: {CharacterStats.GetStatValue(StatType.Health)}" +
                  $"\nMax Health: {CharacterStats.GetStatValue(StatType.MaxHealth)}");
    }

    public override void MoveTo(Vector3 destination)
    {
        if (Target != null)
        {
            Vector3 directionToTarget = (destination - transform.position).normalized;
            float attackRange = characterStats.GetStatValue(StatType.AttackRange);

            Vector3 targetPosition = destination - (directionToTarget * attackRange);
            base.MoveTo(targetPosition);
        }
        else
        {
            base.MoveTo(destination);
        }
    }
}
