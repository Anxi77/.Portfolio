using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponHolder : MonoBehaviour
{
    [System.Serializable]
    public class WeaponIKSetup
    {
        public ItemType weaponType;
        public TwoBoneIKConstraint mainHandIK;
        public Transform mainHandTarget;
        public TwoBoneIKConstraint offHandIK; 
        public Transform offHandTarget;
    }

    [Header("Rig Settings")]
    [SerializeField] private Rig weaponRig;
    [SerializeField] private WeaponIKSetup[] weaponIKSetups;
    [SerializeField] private RigBuilder rigBuilder;  

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugControls = false;
    [SerializeField] private bool showGizmos = false;
    [SerializeField] private Color gizmoColor = Color.yellow;

    [Header("Weapon Settings")]
    [SerializeField] private float weaponScale = 1f;
    [SerializeField] private float ikWeightSpeed = 10f;

    private Vector3 mainHandPositionOffset;
    private Vector3 mainHandRotationOffset;
    private Vector3 offHandPositionOffset;
    private Vector3 offHandRotationOffset;

    private GameObject currentWeaponObject;
    private ItemType currentWeaponType;
    private WeaponIKSetup currentIKSetup;

    public GameObject CurrentWeaponObject => currentWeaponObject;
    public WeaponIKSetup CurrentIKSetup => currentIKSetup;

    private void Start()
    {
        if (weaponRig != null)
        {
            weaponRig.weight = 0f;
        }

        foreach (var setup in weaponIKSetups)
        {
            if (setup.mainHandIK != null) setup.mainHandIK.weight = 0f;
            if (setup.offHandIK != null) setup.offHandIK.weight = 0f;
        }
    }

    private WeaponIKSetup GetIKSetupForWeaponType(ItemType weaponType)
    {
        return System.Array.Find(weaponIKSetups, setup => setup.weaponType == weaponType);
    }

    public void EquipWeapon(GameObject weaponPrefab, ItemType weaponType)
    {
        UnequipCurrentWeapon();
        currentWeaponType = weaponType;
        currentIKSetup = GetIKSetupForWeaponType(weaponType);

        if (weaponPrefab != null && currentIKSetup != null)
        {
            currentWeaponObject = Instantiate(weaponPrefab, currentIKSetup.mainHandTarget);
            currentWeaponObject.transform.localScale = Vector3.one * (weaponScale * 100f);

            //Transform mainGrip = currentWeaponObject.transform.Find("GripPoint");
            //if (mainGrip != null)
            //{
            //    currentWeaponObject.transform.position +=
            //        currentIKSetup.mainHandTarget.position - mainGrip.position;

            //    // 활인 경우 오프핸드 IK 설정
            //    if (weaponType == ItemType.Bow && currentIKSetup.offHandIK != null)
            //    {
            //        Transform offHandGrip = currentWeaponObject.transform.Find("OffHandGripPoint");
            //        if (offHandGrip != null)
            //        {
            //            currentIKSetup.offHandTarget.position = offHandGrip.position;
            //            currentIKSetup.offHandTarget.rotation = offHandGrip.rotation;
            //            currentIKSetup.offHandIK.weight = 1f;
            //        }
            //    }
            //}

            //if (weaponRig != null)
            //{
            //    StartCoroutine(LerpRigWeight(1f));
            //}

        }
    }

    public void UnequipCurrentWeapon()
    {
        if (currentWeaponObject != null)
        {
            if (weaponRig != null)
            {
                StartCoroutine(LerpRigWeight(0f));
            }

            if (currentIKSetup != null)
            {
                if (currentIKSetup.mainHandIK != null) currentIKSetup.mainHandIK.weight = 0f;
                if (currentIKSetup.offHandIK != null) currentIKSetup.offHandIK.weight = 0f;
            }

            Destroy(currentWeaponObject);
            currentWeaponObject = null;
            currentWeaponType = ItemType.None;
            currentIKSetup = null;
        }
    }

    private System.Collections.IEnumerator LerpRigWeight(float targetWeight)
    {
        float startWeight = weaponRig.weight;

        while (Mathf.Abs(weaponRig.weight - targetWeight) > 0.01f)
        {
            weaponRig.weight = Mathf.MoveTowards(weaponRig.weight, targetWeight,
                ikWeightSpeed * Time.deltaTime);
            yield return null;
        }

        weaponRig.weight = targetWeight;
    }


#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!showDebugControls || currentIKSetup == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.BeginVertical("box");

        GUILayout.Label($"Current Weapon: {currentWeaponType}");

        if (currentIKSetup.mainHandIK != null)
        {
            GUILayout.Label("Main Hand IK Weight");
            currentIKSetup.mainHandIK.weight = GUILayout.HorizontalSlider(
                currentIKSetup.mainHandIK.weight, 0f, 1f);

            GUILayout.Label("Main Hand Position Offset");
            mainHandPositionOffset = Vector3Field(mainHandPositionOffset, 0.01f);

            GUILayout.Label("Main Hand Rotation Offset");
            mainHandRotationOffset = Vector3Field(mainHandRotationOffset, 1f);

            if (GUILayout.Button("Apply Main Hand Offset"))
            {
                ApplyMainHandOffset();
            }
        }

        if (currentIKSetup.offHandIK != null)
        {
            GUILayout.Label("Off Hand IK Weight");
            currentIKSetup.offHandIK.weight = GUILayout.HorizontalSlider(
                currentIKSetup.offHandIK.weight, 0f, 1f);

            GUILayout.Label("Off Hand Position Offset");
            offHandPositionOffset = Vector3Field(offHandPositionOffset, 0.01f);

            GUILayout.Label("Off Hand Rotation Offset");
            offHandRotationOffset = Vector3Field(offHandRotationOffset, 1f);

            if (GUILayout.Button("Apply Off Hand Offset"))
            {
                ApplyOffHandOffset();
            }
        }

        if (GUILayout.Button("Reset All Offsets"))
        {
            ResetOffsets();
        }

        showGizmos = GUILayout.Toggle(showGizmos, "Show Gizmos");

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private Vector3 Vector3Field(Vector3 value, float sensitivity)
    {
        value.x = GUILayout.HorizontalSlider(value.x, -1f, 1f) * sensitivity;
        value.y = GUILayout.HorizontalSlider(value.y, -1f, 1f) * sensitivity;
        value.z = GUILayout.HorizontalSlider(value.z, -1f, 1f) * sensitivity;
        return value;
    }

    private void ApplyMainHandOffset()
    {
        if (currentIKSetup?.mainHandTarget != null)
        {
            currentIKSetup.mainHandTarget.position += mainHandPositionOffset;
            currentIKSetup.mainHandTarget.rotation *= Quaternion.Euler(mainHandRotationOffset);
        }
    }

    private void ApplyOffHandOffset()
    {
        if (currentIKSetup?.offHandTarget != null)
        {
            currentIKSetup.offHandTarget.position += offHandPositionOffset;
            currentIKSetup.offHandTarget.rotation *= Quaternion.Euler(offHandRotationOffset);
        }
    }

    private void ResetOffsets()
    {
        mainHandPositionOffset = Vector3.zero;
        mainHandRotationOffset = Vector3.zero;
        offHandPositionOffset = Vector3.zero;
        offHandRotationOffset = Vector3.zero;

        if (currentWeaponObject != null)
        {
            Transform mainGrip = currentWeaponObject.transform.Find("GripPoint");
            Transform offGrip = currentWeaponObject.transform.Find("OffHandGripPoint");

            if (mainGrip != null && currentIKSetup?.mainHandTarget != null)
            {
                currentIKSetup.mainHandTarget.position = mainGrip.position;
                currentIKSetup.mainHandTarget.rotation = mainGrip.rotation;
            }

            if (offGrip != null && currentIKSetup?.offHandTarget != null)
            {
                currentIKSetup.offHandTarget.position = offGrip.position;
                currentIKSetup.offHandTarget.rotation = offGrip.rotation;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || currentIKSetup == null) return;

        Gizmos.color = gizmoColor;

        if (currentIKSetup.mainHandTarget != null)
        {
            Gizmos.DrawWireSphere(currentIKSetup.mainHandTarget.position, 0.1f);
            DrawAxisGizmo(currentIKSetup.mainHandTarget, 0.2f);
        }

        if (currentIKSetup.offHandTarget != null)
        {
            Gizmos.DrawWireSphere(currentIKSetup.offHandTarget.position, 0.1f);
            DrawAxisGizmo(currentIKSetup.offHandTarget, 0.2f);
        }
    }

    private void DrawAxisGizmo(Transform transform, float size)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * size);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up * size);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * size);
    }
#endif
}