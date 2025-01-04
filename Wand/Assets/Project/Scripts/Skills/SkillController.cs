using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class SkillController : MonoBehaviour, IXRGrabTransformer
{
    public Skill skill;
    public Transform skillSpawnPoint;

    private ActionBasedController controller;
    public ActionBasedController Controller => controller;
    private Transform attachPoint;

    public int Order => 0;
    public bool canProcess => true;

    private XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            var existingManager = FindObjectOfType<XRInteractionManager>();
            if (existingManager != null)
            {
                grabInteractable.interactionManager = existingManager;
            }

            grabInteractable.selectEntered.AddListener(OnSelectEntered);
            grabInteractable.selectExited.AddListener(OnSelectExited);
        }
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRBaseInteractor interactor)
        {
            controller = interactor.GetComponentInParent<ActionBasedController>();
            if (controller != null)
            {
                attachPoint = controller.transform.Find("AttachPoint");
                if (attachPoint == null)
                {
                    attachPoint = new GameObject("AttachPoint").transform;
                    attachPoint.SetParent(controller.transform);
                    attachPoint.localPosition = Vector3.zero;
                    attachPoint.localRotation = Quaternion.identity;
                }
                gameObject.transform.SetParent(attachPoint);
                SetSkill();
                skill.AttachController(controller);
            }
            if (GameManager.Instance.monsterManager != null)
            {
                if (!GameManager.Instance.monsterManager.isSpawning)
                {
                    GameManager.Instance.monsterManager.StartSpawn();
                }
            }
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        gameObject.transform.SetParent(null);
        if (skill != null)
        {
            skill.DetachController();
        }
        controller = null;
        attachPoint = null;
    }

    public void ResetSkill()
    {
        gameObject.transform.SetParent(null);
        if (skill != null)
        {
            skill.DetachController();
        }
        controller = null;
        attachPoint = null;
    }

    public void SetSkill()
    {
        if (skill != null && GameManager.Instance.player != null)
        {
            skill.Init(GameManager.Instance.player, this);
        }
        else
        {
            print($"Skill : {skill}");
            print($"Player : {GameManager.Instance.player}");
            Debug.LogWarning("Skill or Player is null!");
        }
    }

    #region IXRGrabTransformer Implementation
    public bool CanProcess(XRGrabInteractable grabInteractable) => true;

    public void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase,
        ref Pose targetPose, ref Vector3 localScale)
    {
        if (attachPoint != null)
        {
            targetPose.position = attachPoint.position;
            targetPose.rotation = attachPoint.rotation;
        }
    }

    public void OnLink(XRGrabInteractable grabInteractable) { }
    public void OnUnlink(XRGrabInteractable grabInteractable) { }
    public void OnGrab(XRGrabInteractable grabInteractable) { }
    public void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale) { }
    #endregion
}