using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SkillEditorData", menuName = "SkillSystem/Editor Data Container")]
public class SkillEditorDataContainer : ScriptableObject
{
    [System.Serializable]
    public class SkillLevelStats
    {
        public SkillID skillID;
        public List<SkillStatData> levelStats = new List<SkillStatData>();

        public List<SkillStatData> GetSkillStatDataList()
        {
            return levelStats;
        }
    }

    // �����ͺ��̽����� �����ϴ� ��� ��ų ������
    public List<SkillData> skillList = new List<SkillData>();

    // ��ų�� ���� ����
    public List<SkillLevelStats> skillStats = new List<SkillLevelStats>();

    // ������ ���� ����
    public SkillID lastSelectedSkillID;
    public Vector2 scrollPosition;
    public bool showBaseStats = true;
    public bool showLevelStats = true;

    // OnEnable���� ������ ����
    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }

    // OnDisable���� ������ ����
    private void OnDisable()
    {
        UnityEditor.EditorUtility.SetDirty(this);
    }
}