using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
public enum PlayerState
{
    IDLE,
    MOVE,
    ATTACK,
    DAMAGED,
    DEBUFF,
    DEATH,
    OTHER,
}
public class SPUM_Prefabs : MonoBehaviour
{
    public float _version;
    public bool EditChk;
    public string _code;
    public Animator _anim;
    private AnimatorOverrideController OverrideController;

    public string UnitType;
    public List<SpumPackage> spumPackages = new List<SpumPackage>();
    public List<PreviewMatchingElement> ImageElement = new();
    public List<SPUM_AnimationData> SpumAnimationData = new();
    public Dictionary<string, List<AnimationClip>> StateAnimationPairs = new();
    public List<AnimationClip> IDLE_List = new();
    public List<AnimationClip> MOVE_List = new();
    public List<AnimationClip> ATTACK_List = new();
    public List<AnimationClip> DAMAGED_List = new();
    public List<AnimationClip> DEBUFF_List = new();
    public List<AnimationClip> DEATH_List = new();
    public List<AnimationClip> OTHER_List = new();

    public void Initialize()
    {
        StateAnimationPairs = new Dictionary<string, List<AnimationClip>>();
        OverrideControllerInit();
        PopulateAnimationLists();
    }

    public void OverrideControllerInit()
    {
        if (_anim == null)
        {
            Debug.LogError("Animator component is missing!");
            return;
        }

        if (_anim.runtimeAnimatorController == null)
        {
            Debug.LogError("No base RuntimeAnimatorController found! Loading default controller...");
            RuntimeAnimatorController defaultController = Resources.Load<RuntimeAnimatorController>("Animations/DefaultAnimatorController");
            if (defaultController == null)
            {
                Debug.LogError("Could not load default animator controller!");
                return;
            }
            _anim.runtimeAnimatorController = defaultController;
        }

        try
        {
            OverrideController = new AnimatorOverrideController(_anim.runtimeAnimatorController);

            AnimationClip[] clips = _anim.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip != null)
                {
                    OverrideController[clip.name] = clip;
                }
            }

            _anim.runtimeAnimatorController = OverrideController;

            StateAnimationPairs = new Dictionary<string, List<AnimationClip>>();
            foreach (PlayerState state in System.Enum.GetValues(typeof(PlayerState)))
            {
                var stateText = state.ToString();
                StateAnimationPairs[stateText] = GetAnimationListForState(state);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing AnimatorOverrideController: {e.Message}\n{e.StackTrace}");
        }
    }

    private List<AnimationClip> GetAnimationListForState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.IDLE: return IDLE_List;
            case PlayerState.MOVE: return MOVE_List;
            case PlayerState.ATTACK: return ATTACK_List;
            case PlayerState.DAMAGED: return DAMAGED_List;
            case PlayerState.DEBUFF: return DEBUFF_List;
            case PlayerState.DEATH: return DEATH_List;
            case PlayerState.OTHER: return OTHER_List;
            default: return new List<AnimationClip>();
        }
    }

    public bool allListsHaveItemsExist()
    {
        List<List<AnimationClip>> allLists = new List<List<AnimationClip>>()
        {
            IDLE_List, MOVE_List, ATTACK_List, DAMAGED_List, DEBUFF_List, DEATH_List, OTHER_List
        };

        return allLists.All(list => list.Count > 0);
    }
    [ContextMenu("PopulateAnimationLists")]
    public void PopulateAnimationLists()
    {
        IDLE_List = new();
        MOVE_List = new();
        ATTACK_List = new();
        DAMAGED_List = new();
        DEBUFF_List = new();
        DEATH_List = new();
        OTHER_List = new();

        var groupedClips = spumPackages
        .SelectMany(package => package.SpumAnimationData)
        .Where(spumClip => spumClip.HasData &&
                        spumClip.UnitType.Equals(UnitType) &&
                        spumClip.index > -1)
        .GroupBy(spumClip => spumClip.StateType)
        .ToDictionary(
            group => group.Key,
            group => group.OrderBy(clip => clip.index).ToList()
        );
        foreach (var kvp in groupedClips)
        {
            var stateType = kvp.Key;
            var orderedClips = kvp.Value;
            switch (stateType)
            {
                case "IDLE":
                    IDLE_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "MOVE":
                    MOVE_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "ATTACK":
                    ATTACK_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "DAMAGED":
                    DAMAGED_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "DEBUFF":
                    DEBUFF_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "DEATH":
                    DEATH_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "OTHER":
                    OTHER_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
            }
        }

    }
    public void PlayAnimation(PlayerState PlayState, int index)
    {
        string stateKey = PlayState.ToString();

        if (!StateAnimationPairs.ContainsKey(stateKey))
        {
            Debug.LogWarning($"Animation state {stateKey} not found in StateAnimationPairs. Initializing empty list.");
            StateAnimationPairs[stateKey] = new List<AnimationClip>();
            return;
        }

        var animations = StateAnimationPairs[stateKey];
        if (animations == null || animations.Count == 0 || index >= animations.Count)
        {
            Debug.LogWarning($"No animations available for state {stateKey} at index {index}");
            return;
        }

        Animator animator = _anim;
        OverrideController[stateKey] = animations[index];

        bool isMove = stateKey.Contains("MOVE");
        bool isDebuff = stateKey.Contains("DEBUFF");
        bool isDeath = stateKey.Contains("DEATH");

        animator.SetBool("1_Move", isMove);
        animator.SetBool("5_Debuff", isDebuff);
        animator.SetBool("isDeath", isDeath);

        if (!isMove && !isDebuff)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            foreach (AnimatorControllerParameter parameter in parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    bool isTrigger = parameter.name.ToUpper().Contains(stateKey.ToUpper());
                    if (isTrigger)
                    {
                        animator.SetTrigger(parameter.name);
                    }
                }
            }
        }
    }
    AnimationClip LoadAnimationClip(string clipPath)
    {
        AnimationClip clip = Resources.Load<AnimationClip>(clipPath.Replace(".anim", ""));

        if (clip == null)
        {
            Debug.LogWarning($"Failed to load animation clip '{clipPath}'.");
        }

        return clip;
    }
}
