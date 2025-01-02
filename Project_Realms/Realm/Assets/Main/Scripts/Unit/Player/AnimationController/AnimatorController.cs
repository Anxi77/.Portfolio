using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum UnitState
{
    None,
    Fly,
    Ground,
}
[Serializable]
public class AnimaState
{
    public UnitState UnitState;
    public ItemType ItemType;
    public RuntimeAnimatorController AnimaController;
}


public class AnimatorController : MonoBehaviour
{
    private Unit owner;

    [SerializeField] public List<AnimaState> animaStates = new List<AnimaState>();

    private AnimatorOverrideController currentController;

    public void Initialize(Unit owner)
    {
       this.owner = owner;
    }

    private AnimatorOverrideController SetupOverrideController()
    {
        RuntimeAnimatorController Controller = owner.Animator.runtimeAnimatorController;
        AnimatorOverrideController overrideController = new AnimatorOverrideController(Controller);
        owner.Animator.runtimeAnimatorController = overrideController;
        return overrideController;
    }

    public void PlayerAnimatorChange(ItemType itemType)
    {
        foreach(AnimaState state in animaStates)
        {
            if(state.ItemType == itemType)
            {
                owner.ChangeAnimController(state.AnimaController);
            }
        }
    }

    public void DragonAnimatorChange(UnitState unitState)
    {

        foreach (AnimaState state in animaStates)
        {
            if (unitState == state.UnitState)
            {
                owner.ChangeAnimController(state.AnimaController);
            }
        
        }
    }


    public void Clipchange(AnimationClip animationClip)
    {
        if (currentController == null ||
            currentController.runtimeAnimatorController != owner.Animator.runtimeAnimatorController)
        {
            currentController = SetupOverrideController();
        }

        if (animationClip != null)
        {
            currentController["Attack"] = animationClip;
        }
    }
}
