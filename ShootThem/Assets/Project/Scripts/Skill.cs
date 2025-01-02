using UnityEngine;

public class Skill : MonoBehaviour
{
    private Player player;
    private SkillController skillController;
    private float damage;
    private float manaCost;
    private float cooldown;

    public void Init(Player player, SkillController skillController)
    {
        this.player = player;
        this.skillController = skillController;
    }
}