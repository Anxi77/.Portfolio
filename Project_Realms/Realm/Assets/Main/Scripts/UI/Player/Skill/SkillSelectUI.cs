using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SkillSelectUI : MonoBehaviour
{
    public GameObject skillSelectPanel;
    public Transform skillButtonContainer;
    public SkillSelectButton skillButtonPrefab;
    public Vector2 offset = new Vector2(0, 10f);
    public RectTransform panelRectTransform;

    private KeyCode currentSelectedSlot;
    private List<SkillSelectButton> spawnedButtons = new List<SkillSelectButton>();

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (panelRectTransform == null)
        {
            panelRectTransform = skillSelectPanel.GetComponent<RectTransform>();
        }
        if (skillSelectPanel != null)
        {
            skillSelectPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("skillSelectPanel is not assigned!");
        }
    }

    public void ShowSkillSelect(KeyCode slotKey, RectTransform slotTransform)
    {
        if (skillSelectPanel == null) return;

        if (skillSelectPanel.activeSelf && currentSelectedSlot != slotKey)
        {
            ClosePanel();
        }

        currentSelectedSlot = slotKey;
        ClearSkillButtons();
        CreateSkillButtons();

        Vector2 slotPosition = slotTransform.position;
        panelRectTransform.position = slotPosition + offset;

        skillSelectPanel.SetActive(true);
    }

    private void ClearSkillButtons()
    {
        if (spawnedButtons == null) return;

        foreach (var button in spawnedButtons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }
        spawnedButtons.Clear();
    }

    private void CreateSkillButtons()
    {
        foreach (Skill skillPrefab in GameManager.Instance.player.skillController.availableSkillPrefabs)
        {
            Skill initializedSkill = GameManager.Instance.player.skillController.GetInitializedSkill(skillPrefab.data.skillID);

            SkillSelectButton button = Instantiate(skillButtonPrefab, skillButtonContainer);
            button.Initialize(initializedSkill, OnSkillSelected);
            spawnedButtons.Add(button);
        }
    }

    private void OnSkillSelected(Skill selectedSkill)
    {
        GameManager.Instance.player.skillController.EquipSkill(selectedSkill, currentSelectedSlot);
        skillSelectPanel.SetActive(false);
    }

    public void ClosePanel()
    {
        skillSelectPanel.SetActive(false);
    }

    public void RefreshButtons()
    {
        if (!skillSelectPanel.activeSelf) return;

        Vector2 currentPosition = panelRectTransform.position;

        ClearSkillButtons();
        CreateSkillButtons();

        panelRectTransform.position = currentPosition;
    }

    private void Update()
    {
        if (!skillSelectPanel.activeSelf) return;

        // 마우스 클릭 감지
        if (Input.GetMouseButtonDown(0))
        {
            // UI 레이캐스트 결과 확인
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            bool clickedInsidePanel = false;
            foreach (RaycastResult result in results)
            {
                if (result.gameObject == skillSelectPanel ||
                    result.gameObject.transform.IsChildOf(skillSelectPanel.transform))
                {
                    clickedInsidePanel = true;
                    break;
                }
            }

            // 패널 외부 클릭 시 닫기
            if (!clickedInsidePanel)
            {
                ClosePanel();
            }
        }
    }
}