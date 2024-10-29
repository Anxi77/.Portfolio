using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SkillLevelUpPanel : MonoBehaviour
{
    public RectTransform list;
    public SkillLevelUpButton buttonPrefab;

    //�÷��̾ �������� �ϸ� �г� Ȱ��ȭ ��û
    public void LevelUpPanelOpen(List<Skill> skillList, Action<Skill> callback)
    {
        gameObject.SetActive(true);

        Time.timeScale = 0f;
        //��ų 2�� UI�� ǥ���� ����
        if (GameManager.Instance.player.skills.Count > 2)
        {
            List<Skill> selectedSkillList = new();
            while (selectedSkillList.Count < 2) //2���� ��ų�� ���õɶ����� �ݺ�
            {
                int ranNum = Random.Range(0, skillList.Count); //������ ���� �ϳ� �̱�

                Skill selectedSkill = skillList[ranNum]; //�����ϰ� ���õ� ��ų �ϳ� ��������.

                if (selectedSkillList.Contains(selectedSkill)) //�̹� ���� ��ų�� �� ��������
                {
                    continue; // �� ���� �� �����ϰ� �ٽ� �ݺ����� ����.
                }

                selectedSkillList.Add(selectedSkill); //������ ��ų�� �־��ְ�

                SkillLevelUpButton skillbutton = Instantiate(buttonPrefab, list); //��Ƽ�� ���̾ƿ� �׷��� ������ �ִ� ����Ʈ�� �ڽ����� ��ư ����

                skillbutton.SetSkillSelectButton(selectedSkill.skillName,
                    () =>
                    {
                        callback(selectedSkill);
                        LevelUpPanelClose();
                    });

            }
        }
        else 
        {
           SkillLevelUpButton skillbutton = Instantiate (buttonPrefab, list);
           skillbutton.SetSkillSelectButton("No SKills Left",() => LevelUpPanelClose());
        }
    }

    //������ �г��� ���� �� �÷��̾��� LevelUpPanelOpen�� callback�� ȣ��.
    public void LevelUpPanelClose()
    {
        foreach(Transform buttons in list)
        {
            Destroy(buttons.gameObject);
        }
        Time.timeScale = 1f;
        gameObject.SetActive(false);

    }
}
