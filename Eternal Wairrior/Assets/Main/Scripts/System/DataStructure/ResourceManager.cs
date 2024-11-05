using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ResourceManager<T> : IDataManager<T> where T : UnityEngine.Object
{
    private readonly string basePath;
    private readonly Dictionary<string, T> cache;

    public ResourceManager(string basePath)
    {
        this.basePath = basePath;
        this.cache = new Dictionary<string, T>();
    }

    public void SaveData(string key, T data)
    {
        if (data == null) return;

        try
        {
            // �ؽ�ó ��� ����
            string fullPath = Path.Combine(Application.dataPath, "Resources", basePath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                Debug.Log($"Created directory: {fullPath}");
            }

            string assetPath = $"Assets/Resources/{basePath}/{key}";

            // ��������Ʈ���� ���ӿ�����Ʈ���� Ȯ��
            if (data is Sprite sprite)
            {
                SaveSprite(assetPath, sprite);
            }
            else if (data is GameObject prefab)
            {
                SavePrefab(assetPath, prefab);
            }

            cache[key] = data;
            AssetDatabase.Refresh();
            Debug.Log($"Saved resource to: {assetPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving resource: {e.Message}\n{e.StackTrace}");
        }
    }

    public T LoadData(string key)
    {
        if (cache.TryGetValue(key, out T cachedData))
            return cachedData;

        string resourcePath = Path.Combine(basePath, key);
        T data = Resources.Load<T>(resourcePath);

        if (data != null)
            cache[key] = data;

        return data;
    }

    public bool DeleteData(string key)
    {
        try
        {
            string fullPath = Path.Combine(Application.dataPath, "Resources", basePath, key);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                cache.Remove(key);
                AssetDatabase.Refresh();
                return true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error deleting resource: {e.Message}");
        }
        return false;
    }

    public void ClearAll()
    {
        try
        {
            string directory = Path.Combine(Application.dataPath, "Resources", basePath);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
                Directory.CreateDirectory(directory);
            }
            cache.Clear();
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error clearing resources: {e.Message}");
        }
    }

    private void SaveSprite(string path, Sprite sprite)
    {
        try
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string fullPath = $"{path}.png";

            // �ؽ�ó ���� ��� ��������
            string texturePath = AssetDatabase.GetAssetPath(sprite.texture);

            // ���� ������ �����ϸ� ���� ����
            if (File.Exists(texturePath))
            {
                File.Copy(texturePath, fullPath, true);
                Debug.Log($"Copied sprite from {texturePath} to {fullPath}");
            }
            else
            {
                // ���� ������ ���� ��쿡�� �ؽ�ó ��ȯ ����
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

                        // ��������Ʈ�� ���� ũ��� �� �ؽ�ó ����
                        Rect spriteRect = sprite.rect;
                        Texture2D tempTexture = new Texture2D(
                            (int)spriteRect.width,
                            (int)spriteRect.height,
                            TextureFormat.RGBA32,
                            false);

                        // ��������Ʈ�� �ȼ� ������ ���� ����
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
                            File.WriteAllBytes(fullPath, bytes);
                            Debug.Log($"Saved sprite to: {fullPath}");
                        }
                        else
                        {
                            Debug.LogError("Failed to encode texture to PNG");
                        }

                        Object.DestroyImmediate(tempTexture);
                    }
                    finally
                    {
                        // ���� ���� ����
                        importer.isReadable = originalReadable;
                        importer.textureCompression = originalCompression;
                        importer.SaveAndReimport();
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

    private void SavePrefab(string path, GameObject prefab)
    {
        try
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string fullPath = $"{path}.prefab";

            // ������ ������ �����ϸ� ����
            if (File.Exists(fullPath))
            {
                AssetDatabase.DeleteAsset(fullPath);
            }

            // ������ �ν��Ͻ� ����
            GameObject prefabInstance = Object.Instantiate(prefab);
            bool success = PrefabUtility.SaveAsPrefabAsset(prefabInstance, fullPath, out bool prefabSuccess);
            Object.DestroyImmediate(prefabInstance);

            if (success)
            {
                Debug.Log($"Saved prefab to: {fullPath}");
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError($"Failed to save prefab to: {fullPath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving prefab: {e.Message}");
        }
    }
}
