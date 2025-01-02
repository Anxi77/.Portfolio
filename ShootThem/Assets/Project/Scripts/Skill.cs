using UnityEngine;

public class Skill : MonoBehaviour
{
    private Player player;
    private SkillController skillController;
    public SkillController SkillController => skillController;
    public float damage;
    public float manaCost;
    public float cooldown;

    public void Init(Player player, SkillController skillController)
    {
        this.player = player;
        this.skillController = skillController;
    }
}