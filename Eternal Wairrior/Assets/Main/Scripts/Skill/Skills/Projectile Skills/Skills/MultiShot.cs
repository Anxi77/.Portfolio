using UnityEngine;

public class MultiShot : ProjectileSkills
{
    public Transform[] shotPoints;

    protected override void Fire()
    {
        foreach (var shotPoint in shotPoints)
        {
            Projectile proj = PoolManager.Instance.Spawn<Projectile>(
                skillData.ProjectilePrefab,
                shotPoint.position,
                transform.rotation
            );

            if (proj != null)
            {
                InitializeProjectile(proj);
                proj.SetDirection(fireDir);
            }
        }
    }

    public override string GetDetailedDescription()
    {
        string baseDesc = "Fires multiple projectiles simultaneously";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDamage per Shot: {Damage:F1}" +
                       $"\nNumber of Shots: {shotPoints.Length}" +
                       $"\nFire Rate: {1 / ShotInterval:F1} volleys/s" +
                       $"\nRange: {AttackRange:F1}" +
                       $"\nPierce: {PierceCount}";

            if (IsHoming)
            {
                baseDesc += $"\nHoming Range: {HomingRange:F1}";
            }
        }
        return baseDesc;
    }
}

