public interface ISkillModifier
{
    float ModifySkillDamage(float baseDamage, SkillType skillType, ElementType elementType);
    float ModifySkillCooldown(float baseCooldown, SkillType skillType);

    float ModifyProjectileSpeed(float baseSpeed);
    float ModifyProjectileRange(float baseRange);
    bool IsHomingEnabled(bool baseHoming);

    float ModifyAreaRadius(float baseRadius);
    float ModifyAreaDuration(float baseDuration);

    void OnSkillCast(Skill skill);
    void OnSkillHit(Skill skill, Enemy target);
}