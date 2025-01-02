using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    private const int MAX_ACTIVE_SKILLS = 6;
    public List<Skill> availableSkillPrefabs = new List<Skill>();
    public List<Skill> activeSkills = new List<Skill>();
    private Player player;
    private Dictionary<KeyCode, Skill> skillSlots = new Dictionary<KeyCode, Skill>();
    private Dictionary<SkillID, Skill> initializedSkills = new Dictionary<SkillID, Skill>();
    private SkillBarUI skillBarUI;
    public KeyCode skillActivated;

    //   ų ϱƼ ߰
    private Skill currentSkill;
    public Skill CurrentSkill => currentSkill;


    public void Initialize()
    {
        if (player == null)
            player = GetComponent<Player>();

        skillSlots[KeyCode.Q] = null;
        skillSlots[KeyCode.W] = null;
        skillSlots[KeyCode.E] = null;
        skillSlots[KeyCode.R] = null;
        skillSlots[KeyCode.Mouse0] = null;
        skillSlots[KeyCode.Space] = null;

        foreach (var skillPrefab in availableSkillPrefabs)
        {
            Skill instance = Instantiate(skillPrefab, transform);
            instance.Initialize(player);
            instance.gameObject.SetActive(false);
            initializedSkills[skillPrefab.data.skillID] = instance;
        }
    }

    public void SetSkillBarUI(SkillBarUI skillBarUI)
    {
        this.skillBarUI = skillBarUI;
    }

    public void OnMouseClick()
    {
        if (skillSlots.TryGetValue(KeyCode.Mouse0, out Skill skill) && skill != null)
        {
            currentSkill = skill;
            currentSkill.TryUseSkill();
        }
    }

    public bool TryUseSkillByKey(KeyCode keyCode)
    {
        if (skillSlots.TryGetValue(keyCode, out Skill skill) && skill != null)
        {
            currentSkill = skill;
            return currentSkill.TryUseSkill();
        }
        return false;
    }

    public void AddSkill(Skill skillPrefab)
    {
        if (!initializedSkills.ContainsKey(skillPrefab.data.skillID))
        {
            Skill instance = Instantiate(skillPrefab, transform);
            instance.Initialize(player);
            availableSkillPrefabs.Add(instance);
            initializedSkills[skillPrefab.data.skillID] = instance;
        }
    }

    public bool IsSkillEquipped(Skill skillPrefab)
    {
        foreach (var slot in skillSlots)
        {
            if (slot.Value != null && slot.Value.data.skillID == skillPrefab.data.skillID)
            {
                return true;
            }
        }
        return false;
    }

    public void EquipSkill(Skill skillPrefab, KeyCode newSlot)
    {
        if (!skillSlots.ContainsKey(newSlot))
            return;

        KeyCode existingSlot = KeyCode.None;
        foreach (var slot in skillSlots)
        {
            if (slot.Value != null && slot.Value.data.skillID == skillPrefab.data.skillID)
            {
                existingSlot = slot.Key;
                break;
            }
        }

        if (existingSlot != KeyCode.None)
        {
            skillSlots[existingSlot].gameObject.SetActive(false);
            skillSlots[existingSlot] = null;
            if (skillBarUI != null)
            {
                skillBarUI.UpdateSkillSlot(existingSlot, null);
            }
        }

        if (skillSlots[newSlot] != null)
        {
            skillSlots[newSlot].gameObject.SetActive(false);
            skillSlots[newSlot] = null;
        }

        Skill skillInstance = initializedSkills[skillPrefab.data.skillID];
        skillInstance.gameObject.SetActive(true);
        skillSlots[newSlot] = skillInstance;

        if (!activeSkills.Contains(skillInstance))
        {
            activeSkills.Add(skillInstance);
        }

        if (skillBarUI != null)
        {
            skillBarUI.UpdateSkillSlot(newSlot, skillInstance);
        }
    }

    public void UnequipSkill(KeyCode slot)
    {
        if (skillSlots.ContainsKey(slot) && skillSlots[slot] != null)
        {
            Skill skillToRemove = skillSlots[slot];
            activeSkills.Remove(skillToRemove);

            if (skillToRemove is DefaultSkill)
            {
                if (initializedSkills.ContainsKey(skillToRemove.data.skillID))
                {
                    initializedSkills.Remove(skillToRemove.data.skillID);
                }
                Destroy(skillToRemove.gameObject);
            }

            skillSlots[slot] = null;

            if (skillBarUI != null)
            {
                skillBarUI.UpdateSkillSlot(slot, null);
            }
        }
    }

    public Skill GetInitializedSkill(SkillID skillID)
    {
        if (initializedSkills.TryGetValue(skillID, out Skill skill))
        {
            return skill;
        }
        return null;
    }

    public bool TryLevelUpSkill(Skill skillPrefab)
    {
        if (!initializedSkills.TryGetValue(skillPrefab.data.skillID, out Skill existingSkill))
        {
            Skill instance = Instantiate(skillPrefab, transform);
            instance.Initialize(player);

            if (instance.skillStat != null)
            {
                instance.skillStat.InitializeStats();
                instance.SetLevel(1);
            }

            availableSkillPrefabs.Add(instance);
            initializedSkills[skillPrefab.data.skillID] = instance;
            if (skillPrefab.data.skillID == SkillID.Dash)
            {
                EquipSkill(instance, KeyCode.Space);
            }
            existingSkill = instance;

            OnSkillLevelChanged?.Invoke(existingSkill);
            return true;
        }

        if (existingSkill.skillStat != null)
        {
            existingSkill.LevelUp();
            OnSkillLevelChanged?.Invoke(existingSkill);
        }

        return true;
    }

    public int GetSkillLevel(SkillID skillID)
    {
        if (initializedSkills.TryGetValue(skillID, out Skill skill))
        {
            return skill.skillStat.GetStatValue<int>(SkillStatType.SkillLevel);
        }
        return 0;
    }

    public event System.Action<Skill> OnSkillLevelChanged;

    public void DirectEquipSkill(Skill skillPrefab, KeyCode slot)
    {
        if (!skillSlots.ContainsKey(slot))
            return;

        if (skillSlots[slot] != null)
        {
            UnequipSkill(slot);
        }

        Skill instance = Instantiate(skillPrefab, transform);
        instance.Initialize(player);

        activeSkills.Add(instance);
        initializedSkills[skillPrefab.data.skillID] = instance;
        skillSlots[slot] = instance;

        if (skillBarUI != null)
        {
            skillBarUI.UpdateSkillSlot(slot, instance);
        }
    }

    public void RemoveAllWeaponSkills()
    {
        UnequipSkill(KeyCode.Mouse0);

        List<KeyCode> keysToUnequip = new List<KeyCode>();

        foreach (var slot in skillSlots)
        {
            if (slot.Value != null && slot.Value is DefaultSkill)
            {
                keysToUnequip.Add(slot.Key);
            }
        }

        foreach (var key in keysToUnequip)
        {
            UnequipSkill(key);
        }

        var weaponSkillsToRemove = initializedSkills.Where(pair => pair.Value is DefaultSkill)
                                                   .Select(pair => pair.Key)
                                                   .ToList();

        foreach (var skillID in weaponSkillsToRemove)
        {
            if (initializedSkills.TryGetValue(skillID, out Skill skill))
            {
                Destroy(skill.gameObject);
                initializedSkills.Remove(skillID);
            }
        }
    }
}