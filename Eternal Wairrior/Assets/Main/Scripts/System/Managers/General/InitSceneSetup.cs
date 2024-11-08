#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class InitSceneSetup
{
    [MenuItem("Setup/Create Init Scene")]
    public static void CreateInitScene()
    {
        // ���ο� �� ����
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

        // InitializationManager ����
        var initGO = new GameObject("InitializationManager");
        initGO.AddComponent<InitializationManager>();

        // �� ����
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/InitScene.unity");

        Debug.Log("Init Scene created successfully!");
    }
}
#endif