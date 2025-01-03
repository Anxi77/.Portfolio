using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class HandAnimCon : MonoBehaviour
{
    private ActionBasedController controller;
    public Animator animator;

    private quaternion defaultRotation;

    private quaternion gripRotation;

    void Start()
    {
        controller = GetComponentInParent<ActionBasedController>();
        defaultRotation = gameObject.transform.rotation;
        gripRotation = defaultRotation;
        controller.selectAction.action.performed += Grip;
        controller.selectAction.action.canceled += Grip;
    }

    void Grip(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animator.SetFloat("Grip", 1);
            Vector3 currentRotation = gameObject.transform.rotation.eulerAngles;
            gripRotation = Quaternion.Euler(27, currentRotation.y, currentRotation.z);
            gameObject.transform.rotation = gripRotation;
        }
        else
        {
            animator.SetFloat("Grip", 0);
            gameObject.transform.rotation = defaultRotation;
        }

    }
}