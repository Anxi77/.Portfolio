using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SkillController : MonoBehaviour
{
    public Skill CurrentSkill;
    public Skill skills;
    public Transform skillSpawnPoint;

    public void SetSkill(Skill skill)
    {
        CurrentSkill = skill;
        Instantiate(CurrentSkill, skillSpawnPoint.position, skillSpawnPoint.rotation);
    }
}