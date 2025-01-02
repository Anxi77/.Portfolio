using System.Collections.Generic;
using UnityEngine;

public class SkillTreeUI : MonoBehaviour
{
    private Player player;
    private SkillController skillController;
    [SerializeField] private SkillTreeSlot skillSlotPrefab;
    [SerializeField] private Transform SkillTreeViewPort;
    private SkillSelectUI skillSelectUI;

    private Dictionary<SkillID, SkillTreeSlot> skillSlots = new Dictionary<SkillID, SkillTreeSlot>();

    public void Initialize(Player player)
    {
        this.player = player;
        skillController = player.GetComponent<SkillController>();
        skillController.OnSkillLevelChanged += UpdateSkillSlotUI;
        skillSelectUI = UIManager.Instance.SkillSelectUI;
        CreateSkillSlots();
    }

    private void CreateSkillSlots()
    {
        foreach (Transform child in SkillTreeViewPort)
        {
            Destroy(child.gameObject);
        }
        skillSlots.Clear();

        foreach (Skill skillPrefab in SkillManager.Instance.allSkillsPrefabs)
        {
            SkillTreeSlot slot = Instantiate(skillSlotPrefab, SkillTreeViewPort);
            slot.Initialize(skillPrefab);
            skillSlots.Add(skillPrefab.data.skillID, slot);
        }
    }

    private void UpdateSkillSlotUI(Skill skill)
    {
        if (skillSlots.TryGetValue(skill.data.skillID, out SkillTreeSlot slot))
        {
            slot.UpdateLevel(skill.skillStat.GetStatValue<int>(SkillStatType.SkillLevel));
        }
    }

    public void OnSkillSlotClicked(Skill skillPrefab)
    {
 
        if (player.SkillPoint > 0)
        {
            if (skillController.TryLevelUpSkill(skillPrefab))
            {
                player.SkillPoint--;

                if (skillSelectUI != null)
                {
                    skillSelectUI.RefreshButtons();
                }
            }
        }
    }
}