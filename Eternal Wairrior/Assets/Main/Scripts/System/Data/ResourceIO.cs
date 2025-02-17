using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Collections.Generic;

public static class ResourceIO<T> where T : Object
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

#if UNITY_EDITOR
            string assetPath = $"Assets/Resources/{key}.{GetExtensionForType()}";
            if (File.Exists(assetPath))
            {
                T data = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (data != null)
                {
                    cache[key] = data;
                    return data;
                }
            }
#endif

            T resourceData = Resources.Load<T>(key);
            if (resourceData != null)
            {
                cache[key] = resourceData;
                return resourceData;
            }

            Debug.LogWarning($"Failed to load resource: {key}");
            return null;
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
#if UNITY_EDITOR
            string assetPath = $"Assets/Resources/{key}.{GetExtensionForType()}";
            if (File.Exists(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
                cache.Remove(key);
                AssetDatabase.Refresh();
                return true;
            }
#endif
            cache.Remove(key);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error deleting resource: {e.Message}");
            return false;
        }
    }

    public static void ClearCache()
    {
        cache.Clear();
        Resources.UnloadUnusedAssets();
    }

#if UNITY_EDITOR
    private static void SaveSprite(string path, Sprite sprite)
    {
        try
        {
            string sourcePath = AssetDatabase.GetAssetPath(sprite);
            if (string.IsNullOrEmpty(sourcePath))
            {
                Debug.LogError("Source sprite path is null or empty");
                return;
            }

            string targetPath = $"Assets/Resources/{path}.png";
            string directory = Path.GetDirectoryName(targetPath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(targetPath))
            {
                AssetDatabase.DeleteAsset(targetPath);
            }

            bool success = AssetDatabase.CopyAsset(sourcePath, targetPath);
            if (success)
            {
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
            string targetPath = $"Assets/Resources/{path}.prefab";
            string directory = Path.GetDirectoryName(targetPath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(targetPath))
            {
                AssetDatabase.DeleteAsset(targetPath);
            }

            GameObject prefabInstance = Object.Instantiate(prefab);
            bool success = PrefabUtility.SaveAsPrefabAsset(prefabInstance, targetPath, out bool prefabSuccess);
            Object.DestroyImmediate(prefabInstance);

            if (success)
            {
                Debug.Log($"Saved prefab to: {targetPath}");
            }
            else
            {
                Debug.LogError($"Failed to save prefab to: {targetPath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving prefab: {e.Message}");
        }
    }

    private static string GetExtensionForType()
    {
        if (typeof(T) == typeof(Sprite) || typeof(T) == typeof(Texture2D))
            return "png";
        if (typeof(T) == typeof(GameObject))
            return "prefab";
        return "";
    }
#endif
}
