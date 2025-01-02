using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class SkillController : MonoBehaviour
{
    public Skill CurrentSkill;
    public List<Skill> skills;
    public Transform skillSpawnPoint;
    private ActionBasedController controller;
    public ActionBasedController Controller => controller;
    private Button testButton;

    public Canvas SkillUI;

    void Start()
    {
        controller = GetComponentInParent<ActionBasedController>();
        controller.selectAction.action.performed += ToggleSkillUI;
        SkillUI.enabled = false;
        testButton = SkillUI.GetComponentInChildren<Button>();
        testButton.onClick.AddListener(() => SetSkill(skills[0]));
    }

    public void SetSkill(Skill skill)
    {
        if (CurrentSkill != null)
        {
            DestroyImmediate(CurrentSkill.gameObject);
        }
        CurrentSkill = skill;
        Skill temp = Instantiate(CurrentSkill, skillSpawnPoint);
        temp.transform.localPosition = Vector3.zero;
        temp.Init(GetComponentInParent<Player>(), this);
    }

    private void ToggleSkillUI(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SkillUI.enabled = !SkillUI.enabled;
        }
    }

}