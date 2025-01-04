using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Skill : MonoBehaviour
{
    protected Player player;
    private SkillController skillController;
    protected ActionBasedController controller;
    public SkillController SkillController => skillController;

    public float damage;
    public float manaCost;
    public float cooldown;

    public delegate void ControllerEventHandler(ActionBasedController controller);
    public event ControllerEventHandler OnControllerAttached;
    public event ControllerEventHandler OnControllerDetached;

    protected bool isActivating = false;
    protected bool wasActivatedThisFrame = false;
    protected float activateValue = 0f;

    public void Init(Player player, SkillController skillController)
    {
        this.player = player;
        this.skillController = skillController;
    }

    public void AttachController(ActionBasedController controller)
    {
        this.controller = controller;
        OnControllerAttached?.Invoke(controller);
    }

    public void DetachController()
    {
        OnControllerDetached?.Invoke(controller);
        this.controller = null;
    }

    protected virtual void Update()
    {
        if (controller != null)
        {
            activateValue = controller.activateAction.action.ReadValue<float>();
            wasActivatedThisFrame = controller.activateAction.action.WasPressedThisFrame();
            isActivating = activateValue > 0.1f;

            float manaPerSecond = manaCost;
            float manaThisFrame = manaPerSecond * Time.deltaTime;

            float minimumManaCheck = Mathf.Min(0.1f, manaPerSecond * 0.1f);

            if (isActivating && player.CurrentMana >= minimumManaCheck)
            {
                OnSkillActivate();
            }
            else if (!isActivating)
            {
                OnSkillDeactivate();
            }

            if (wasActivatedThisFrame)
            {
                OnSkillActivateThisFrame();
            }
        }
    }

    protected virtual void OnSkillActivate() { }

    protected virtual void OnSkillDeactivate() { }

    protected virtual void OnSkillActivateThisFrame() { }

    public bool IsActivating => isActivating;
    public bool WasActivatedThisFrame => wasActivatedThisFrame;
    public float ActivateValue => activateValue;
}