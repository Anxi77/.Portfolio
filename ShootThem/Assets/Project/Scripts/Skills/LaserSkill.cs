using UnityEngine;

public class LaserSkill : Skill
{
    public ProgressControlV3D progressControl;
    public EndPointEffectControllerV3D endPointEffect;

    private void Start()
    {
        // 시작할 때 레이저가 꺼진 상태로 시작
        if (progressControl != null)
        {
            progressControl.globalProgress = 1f;
            endPointEffect.emit = false;
        }
    }

    protected override void OnSkillActivate()
    {
        if (progressControl != null)
        {
            progressControl.globalProgress = 0f;
            endPointEffect.emit = true;

            if (controller != null)
            {
                controller.SendHapticImpulse(0.5f, 0.1f);
            }
            player.UseMana(manaCost * Time.deltaTime);
        }
    }

    protected override void OnSkillDeactivate()
    {
        if (progressControl != null)
        {
            progressControl.globalProgress = 1f;
            endPointEffect.emit = false;
        }
    }

    protected override void OnSkillActivateThisFrame()
    {
        if (progressControl != null)
        {
            progressControl.globalImpactProgress = 0f;
        }
    }
}
