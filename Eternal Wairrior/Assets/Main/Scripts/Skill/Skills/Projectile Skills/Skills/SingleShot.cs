using UnityEngine;

public class SingleShot : ProjectileSkills
{
    public override string GetDetailedDescription()
    {
        string baseDesc = "Basic projectile attack that fires single shots";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDamage: {Damage:F1}" +
                       $"\nFire Rate: {1 / ShotInterval:F1} shots/s" +
                       $"\nRange: {AttackRange:F1}" +
                       $"\nPierce: {PierceCount}";

            if (IsHoming)
            {
                baseDesc += $"\nHoming Range: {HomingRange:F1}";
            }
        }
        return baseDesc;
    }

    protected override string GetDefaultSkillName() => "Default Gun";
    protected override string GetDefaultDescription() => "Basic projectile attack that fires single shots";

    public override SkillType GetSkillType() => SkillType.Projectile;
}
