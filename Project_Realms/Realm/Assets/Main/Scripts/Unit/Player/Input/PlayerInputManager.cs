using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.AI;

public class PlayerInputManager : MonoBehaviour
{
    private Player player;
    private Camera mainCamera;
    private Dictionary<KeyCode, System.Action> skillKeyBindings;
    private bool isProcessingBasicAttack = false;
    private bool isHandlingInput = false;

    public void Initialize(Player player)
    {
        this.player = player;
        mainCamera = Camera.main;
        InitializeKeyBindings();
    }

    private void InitializeKeyBindings()
    {
        skillKeyBindings = new Dictionary<KeyCode, System.Action>
        {
            { KeyCode.Q, () => TryUseSkill(KeyCode.Q) },
            { KeyCode.W, () => TryUseSkill(KeyCode.W) },
            { KeyCode.E, () => TryUseSkill(KeyCode.E) },
            { KeyCode.R, () => TryUseSkill(KeyCode.R) },
            { KeyCode.Space, () => TryUseSkill(KeyCode.Space) }
        };
    }

    public void HandleInput()
    {
        if (!player.IsAlive || isHandlingInput) return;

        if (player.skillController.CurrentSkill != null &&
            player.skillController.CurrentSkill.IsSkillInProgress)
        {
            return;
        }

        isHandlingInput = true;
        try
        {
            if (!isProcessingBasicAttack)
            {
                HandleMouseInput();
            }

            CheckSkillInputs();
            CheckItemHover();
        }
        finally
        {
            isHandlingInput = false;
        }
    }

    private void CheckSkillInputs()
    {
        foreach (var binding in skillKeyBindings)
        {
            if (Input.GetKey(binding.Key))
            {
                binding.Value.Invoke();
            }
        }
    }

    private void TryUseSkill(KeyCode keyCode)
    {
        if (player.skillController.TryUseSkillByKey(keyCode))
        {
            if (player.PlayerHandler.CurrentState is not PlayerSkillState)
            {
                player.PlayerHandler.TransitionTo(new PlayerSkillState(player));
            }
        }
    }

    private void HandleMouseInput()
    {
        if (!Input.GetMouseButton(0)) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent<WorldDropItem>(out WorldDropItem dropItem))
            {
                HandleDropItemClick(dropItem);
                return;
            }
        }

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent<Monster>(out Monster monster))
            {
                HandleMonsterClick(monster);
                return;
            }
        }

        if (Physics.Raycast(ray, out RaycastHit groundHit, Mathf.Infinity, player.GroundLayer))
        {
            HandleGroundClick(groundHit.point);
        }
    }

    private void HandleMonsterClick(Monster monster)
    {
        if (isProcessingBasicAttack) return;

        try
        {
            isProcessingBasicAttack = true;
            float distanceToTarget = Vector3.Distance(player.transform.position, monster.transform.position);
            float attackRange = player.CharacterStats.GetStatValue(StatType.AttackRange);

            player.SetTarget(monster);

            if (distanceToTarget <= attackRange)
            {
                player.PlayerHandler.TransitionTo(new PlayerSkillState(player));
            }
            else
            {
                player.PlayerHandler.TransitionTo(new PlayerMoveState(player));
            }
        }
        finally
        {
            isProcessingBasicAttack = false;
        }
    }

    private void HandleGroundClick(Vector3 position)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, 5f, NavMesh.AllAreas))
        {
            NavMeshPath path = new NavMeshPath();
            if (player.Agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                player.SetDestination(hit.position);
                player.PlayerHandler.TransitionTo(new PlayerMoveState(player));
            }
        }
    }

    private void HandleDropItemClick(WorldDropItem dropItem)
    {
        if (!dropItem.IsNameTextActive) return;

        float distanceToItem = Vector3.Distance(player.transform.position, dropItem.transform.position);
        if (distanceToItem <= dropItem.InteractionRadius)
        {
            dropItem.TryPickupItem();
        }
        else
        {
            player.SetDestination(dropItem.transform.position);
            dropItem.TryPickupItem();
        }
    }

    public void CheckItemHover()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

        WorldDropItem closestItem = null;
        float closestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent<WorldDropItem>(out WorldDropItem dropItem))
            {
                float distance = Vector3.Distance(hit.point, mainCamera.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = dropItem;
                }
            }
        }

        WorldDropItem.UpdateHoveredItem(closestItem);
    }
}