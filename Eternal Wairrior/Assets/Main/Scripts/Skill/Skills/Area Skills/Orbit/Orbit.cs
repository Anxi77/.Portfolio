using UnityEngine;

public class Orbit : AreaSkills
{
    private RotatingOrb orbs;
    private int currentOrbCount = 0;
    [SerializeField] private int ORBS_PER_LEVEL = 2;
    [SerializeField] private int BASE_ORB_COUNT = 1;

    protected override void Awake()
    {
        base.Awake();
        if (skillData == null)
        {
            skillData = new SkillData
            {
                skillName = "Orbit",
                description = "Rotating orbs that damage enemies",
                type = SkillType.Area,
                element = ElementType.None,
                tier = 1
            };
            InitializeSkillStats();

        }
        orbs = GetComponentInChildren<RotatingOrb>();
    }

    private void InitializeSkillStats()
    {
        if (skillData.GetStatsForLevel(1) == null)
        {
            var stats = new AreaSkillStat
            {
                baseStat = new BaseSkillStat
                {
                    damage = 8f,
                    skillName = skillData.skillName,
                    skillLevel = 1,
                    maxSkillLevel = 5,
                    element = skillData.element,
                    elementalPower = 1f

                },
                radius = 2f,
                duration = 0f,
                tickRate = 0f,
                isPersistent = true,
                moveSpeed = 500f
            };
            skillData.SetStatsForLevel(1, stats);
        }
    }

    private void Start()
    {
        if (orbs != null)
        {
            UpdateOrbCount();
        }
    }

    private void Update()
    {
        transform.Rotate(Vector3.forward, MoveSpeed * Time.deltaTime);
    }

    private void UpdateOrbCount()
    {
        int newOrbCount = BASE_ORB_COUNT + (SkillLevel - 1) * ORBS_PER_LEVEL;

        if (currentOrbCount != newOrbCount)
        {
            currentOrbCount = newOrbCount;
            orbs.InitializeOrbs(currentOrbCount);
        }
    }

    public override bool SkillLevelUpdate(int newLevel)
    {
        bool success = base.SkillLevelUpdate(newLevel);

        if (success)
        {
            UpdateOrbCount();
        }

        return success;
    }

    public override string GetDetailedDescription()
    {
        string baseDesc = "Rotating orbs that damage enemies";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            int orbCount = BASE_ORB_COUNT + (SkillLevel - 1) * ORBS_PER_LEVEL;
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDamage per Orb: {Damage:F1}" +
                       $"\nOrb Count: {orbCount}" +
                       $"\nOrbit Radius: {Radius:F1}" +
                       $"\nRotation Speed: {MoveSpeed:F1}";
        }
        return baseDesc;
    }

    public override SkillType GetSkillType() => SkillType.Area;
    protected override string GetDefaultSkillName() => "Orbit";
    protected override string GetDefaultDescription() => "Rotating orbs that damage enemies";
}

