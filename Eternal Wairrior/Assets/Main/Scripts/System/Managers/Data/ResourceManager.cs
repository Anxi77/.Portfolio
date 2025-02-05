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

            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Debug.Log($"Creating directory: {directory}");
                Directory.CreateDirectory(directory);
            }

            // 텍스처 파일 경로 가져오기
            string texturePath = AssetDatabase.GetAssetPath(sprite.texture);
            Debug.Log($"Texture path: {texturePath}");

            // 파일이 존재하면 복사
            if (File.Exists(texturePath))
            {
                Debug.Log($"Copying sprite from {texturePath} to {path}");
                File.Copy(texturePath, path, true);
                Debug.Log($"Copied sprite from {texturePath} to {path}");
            }
            else
            {
                Debug.Log($"Creating new texture from sprite");
                // 텍스처 임포터 설정 변경
                TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                if (importer != null)
                {
                    bool originalReadable = importer.isReadable;
                    TextureImporterCompression originalCompression = importer.textureCompression;

                    try
                    {
                        importer.isReadable = true;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.SaveAndReimport();
                        Debug.Log("Texture importer settings updated");

                        // 스프라이트의 실제 크기로 새 텍스처 생성
                        Rect spriteRect = sprite.rect;
                        Texture2D tempTexture = new Texture2D(
                            (int)spriteRect.width,
                            (int)spriteRect.height,
                            TextureFormat.RGBA32,
                            false);

                        // 스프라이트의 픽셀 데이터를 새로 복사
                        var pixels = sprite.texture.GetPixels(
                            (int)spriteRect.x,
                            (int)spriteRect.y,
                            (int)spriteRect.width,
                            (int)spriteRect.height);
                        tempTexture.SetPixels(pixels);
                        tempTexture.Apply();

                        byte[] bytes = tempTexture.EncodeToPNG();
                        if (bytes != null && bytes.Length > 0)
                        {
                            File.WriteAllBytes(path, bytes);
                            Debug.Log($"Saved sprite to: {path}");
                        }
                        else
                        {
                            Debug.LogError("Failed to encode texture to PNG");
                        }

                        Object.DestroyImmediate(tempTexture);
                    }
                    finally
                    {
                        // 원래 설정 복구
                        importer.isReadable = originalReadable;
                        importer.textureCompression = originalCompression;
                        importer.SaveAndReimport();
                        Debug.Log("Texture importer settings restored");
                    }
                }
                else
                {
                    Debug.LogError($"Could not get TextureImporter for sprite: {texturePath}");
                }
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
