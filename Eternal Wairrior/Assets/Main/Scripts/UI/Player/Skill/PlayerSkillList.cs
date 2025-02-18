using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSkillList : MonoBehaviour
{
    public PlayerSkillIcon skillIconPrefab;
    private Player player;
    private List<PlayerSkillIcon> currentIcons = new List<PlayerSkillIcon>();

    private void Awake()
    {
        StartCoroutine(WaitForPlayer());
    }

    private IEnumerator WaitForPlayer()
    {
        while (GameManager.Instance == null || GameManager.Instance.player == null)
        {
            yield return null;
        }
        player = GameManager.Instance.player;
        if (gameObject.activeInHierarchy)
        {
            skillListUpdate();
        }
    }

    public void skillListUpdate()
    {
        if (!gameObject.activeInHierarchy) return;

        try
        {
            ClearCurrentIcons();

            if (player == null)
            {
                player = GameManager.Instance?.player;
                if (player == null)
                {
                    StartCoroutine(WaitForPlayer());
                    return;
                }
            }

            if (player.skills != null)
            {
                var sortedSkills = player.skills
                    .Where(s => s != null)
                    .OrderBy(s => s.GetSkillData()?.Type)
                    .ThenByDescending(s => s.currentLevel)
                    .ToList();

                foreach (Skill skill in sortedSkills)
                {
                    SkillData skillData = skill.GetSkillData();
                    if (skillData != null)
                    {
                        PlayerSkillIcon icon = Instantiate(skillIconPrefab, transform);
                        icon.SetSkillIcon(skillData.Icon, skill);
                        currentIcons.Add(icon);
                    }
                    else
                    {
                        Debug.LogWarning($"Skill data is null for skill: {skill.name}");
                    }
                }
            }
            else
            {
                Debug.LogError("Player skills list is null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating skill list: {e.Message}\n{e.StackTrace}");
        }
    }

    private void ClearCurrentIcons()
    {
        foreach (var icon in currentIcons)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }
        currentIcons.Clear();
    }

    private void OnEnable()
    {
        skillListUpdate();
    }
}
