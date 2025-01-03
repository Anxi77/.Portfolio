using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Player : MonoBehaviour
{
    private SkillController skillCon;
    [SerializeField] private XRDirectInteractor[] handInteractors;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaRegenRate = 5f;

    private float currentHealth;
    private float currentMana;
    private float currentExp;
    private int currentLevel = 1;

    [Header("Level System")]
    [SerializeField] private float baseExpRequired = 100f;
    [SerializeField] private float expMultiplier = 1.5f;
    [SerializeField] private float healthIncreasePerLevel = 20f;
    [SerializeField] private float manaIncreasePerLevel = 15f;

    public Transform playerPos;

    public float CurrentHealth { get { return currentHealth / maxHealth; } }
    public float MaxHealth { get { return maxHealth; } }
    public float CurrentMana { get { return currentMana / maxMana; } }
    public float MaxMana { get { return maxMana; } }
    public float CurrentExp { get { return currentExp; } }
    public int CurrentLevel { get { return currentLevel; } }

    private void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentExp = 0f;

        if (handInteractors == null || handInteractors.Length == 0)
        {
            handInteractors = GetComponentsInChildren<XRDirectInteractor>();
        }
    }

    private void Update()
    {
        RegenerateMana();
    }

    private void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            currentMana = Mathf.Min(maxMana, currentMana + (manaRegenRate * Time.deltaTime));
        }
    }

    public void UseMana(float amount)
    {
        currentMana = Mathf.Max(0, currentMana - amount);
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public void AddExp(float exp)
    {
        currentExp += exp;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        float requiredExp = GetRequiredExpForLevel(currentLevel);

        while (currentExp >= requiredExp)
        {
            currentExp -= requiredExp;
            LevelUp();
            requiredExp = GetRequiredExpForLevel(currentLevel);
        }
    }

    private void LevelUp()
    {
        currentLevel++;

        maxHealth += healthIncreasePerLevel;
        maxMana += manaIncreasePerLevel;

        currentHealth = maxHealth;
        currentMana = maxMana;
    }

    private float GetRequiredExpForLevel(int level)
    {
        return baseExpRequired * Mathf.Pow(expMultiplier, level - 1);
    }

    private void Die()
    {
        if (handInteractors != null)
        {
            foreach (var interactor in handInteractors)
            {
                if (interactor != null && interactor.hasSelection)
                {
                    interactor.allowSelect = false;
                    interactor.allowSelect = true;
                }
            }
        }

        GameManager.Instance.ResetWand();

        gameObject.transform.position = GameManager.Instance.playerSpawnPoint.position;
        foreach (var enemy in GameManager.Instance.monsterManager.enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        GameManager.Instance.monsterManager.enemies.Clear();
        GameManager.Instance.monsterManager.isSpawning = false;
    }
}