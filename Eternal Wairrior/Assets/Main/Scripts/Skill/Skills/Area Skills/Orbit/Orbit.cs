using Unity.VisualScripting;
using UnityEngine;

public class Orbit : AreaSkills
{
    private RotatingOrb orbs;
    private int currentOrbCount = 0;
    [SerializeField] private int ORBS_PER_LEVEL = 2;
    [SerializeField] private int BASE_ORB_COUNT = 1;

    public override void Initialize()
    {
        base.Initialize();
        orbs = GetComponentInChildren<RotatingOrb>();
    }

    protected override void Start()
    {
        base.Start();
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
        int newOrbCount = BASE_ORB_COUNT + (skillData.GetCurrentTypeStat().baseStat.skillLevel - 1) * ORBS_PER_LEVEL;

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
            int orbCount = BASE_ORB_COUNT + (skillData.GetCurrentTypeStat().baseStat.skillLevel - 1) * ORBS_PER_LEVEL;
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDamage per Orb: {Damage:F1}" +
                       $"\nOrb Count: {orbCount}" +
                       $"\nOrbit Radius: {Radius:F1}" +
                       $"\nRotation Speed: {MoveSpeed:F1}";
        }
        return baseDesc;
    }
}

