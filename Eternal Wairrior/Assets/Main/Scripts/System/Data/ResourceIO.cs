using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Collections.Generic;

public static class ResourceIO<T> where T : UnityEngine.Object
{
    private static readonly Dictionary<string, T> cache = new Dictionary<string, T>();

    public static void SaveData(string path, T data)
    {
        if (data == null || string.IsNullOrEmpty(path)) return;

        try
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

#if UNITY_EDITOR
            // 에셋 타입에 따른 저장 처리
            if (data is Sprite sprite)
            {
                SaveSprite(path, sprite);
            }
            else if (data is GameObject prefab)
            {
                SavePrefab(path, prefab);
            }

            AssetDatabase.Refresh();
#endif
            cache[path] = data;
            Debug.Log($"Saved resource to: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving resource: {e.Message}\n{e.StackTrace}");
        }
    }

    public static T LoadData(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;

        try
        {
            if (cache.TryGetValue(key, out T cachedData))
                return cachedData;

            T data = Resources.Load<T>(key);

            if (data != null)
                cache[key] = data;

            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading resource: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    public static bool DeleteData(string key)
    {
        try
        {
            string fullPath = Path.Combine(Application.dataPath, "Resources", "Items", "Icons", key);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                cache.Remove(key);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
                return true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error deleting resource: {e.Message}");
        }
        return false;
    }

    public static void ClearAll()
    {
        try
        {
            string directory = Path.Combine(Application.dataPath, "Resources", "Items", "Icons");
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
                Directory.CreateDirectory(directory);
            }
            cache.Clear();
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error clearing resources: {e.Message}");
        }
    }
#if UNITY_EDITOR
    private static void SaveSprite(string path, Sprite sprite)
    {
        try
        {
            Debug.Log($"SaveSprite called with path: {path}");

            string sourcePath = AssetDatabase.GetAssetPath(sprite);
            if (string.IsNullOrEmpty(sourcePath))
            {
                Debug.LogError("Source sprite path is null or empty");
                return;
            }

            // Resources 폴더 내 경로 구성
            string targetPath = $"Assets/Resources/{path}.png";
            string directory = Path.GetDirectoryName(targetPath);

            // 디렉토리 생성
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 기존 파일이 있다면 삭제
            if (File.Exists(targetPath))
            {
                AssetDatabase.DeleteAsset(targetPath);
            }

            // 파일 복사
            bool success = AssetDatabase.CopyAsset(sourcePath, targetPath);
            if (success)
            {
                // 스프라이트 설정 적용
                TextureImporter importer = AssetImporter.GetAtPath(targetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.SaveAndReimport();
                }

                Debug.Log($"Successfully saved sprite to: {targetPath}");
            }
            else
            {
                Debug.LogError($"Failed to copy sprite from {sourcePath} to {targetPath}");
            }

            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving sprite: {e.Message}\n{e.StackTrace}");
        }
    }

    private static void SavePrefab(string path, GameObject prefab)
    {
        try
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 기존 파일이 있으면 삭제
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            // 프리팹 인스턴스 생성
            GameObject prefabInstance = Object.Instantiate(prefab);
            bool success = PrefabUtility.SaveAsPrefabAsset(prefabInstance, path, out bool prefabSuccess);
            Object.DestroyImmediate(prefabInstance);

            if (success)
            {
                Debug.Log($"Saved prefab to: {path}");
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError($"Failed to save prefab to: {path}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving prefab: {e.Message}");
        }
    }
#endif
}
