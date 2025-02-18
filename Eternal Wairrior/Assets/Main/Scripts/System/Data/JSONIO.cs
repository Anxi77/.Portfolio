using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class JSONIO<T> where T : class
{
    private static readonly string defaultPath;
    private static string customPath;
    private static readonly Dictionary<string, T> cache;

    static JSONIO()
    {
        defaultPath = typeof(T).Name;
        cache = new Dictionary<string, T>();
    }

    public static void SetCustomPath(string path)
    {
        customPath = path;
    }

    public static void SaveData(string key, T data)
    {
        try
        {
            if (data == null)
            {
                Debug.LogError($"Cannot save null data for key: {key}");
                return;
            }

            string savePath = customPath ?? defaultPath;
            string fullPath = Path.Combine(Application.dataPath, "Resources", savePath, $"{key}.json");
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(fullPath, jsonData);

            cache[key] = data;
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            Debug.Log($"[{typeof(T)}] JSON saved successfully: {fullPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving JSON data: {e.Message}\n{e.StackTrace}");
        }
    }

    public static T LoadData(string key)
    {
        if (cache.TryGetValue(key, out T cachedData))
            return cachedData;

        try
        {
            string savePath = customPath ?? defaultPath;
            string resourcePath = Path.Combine(savePath, key);
            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);

            if (jsonAsset != null)
            {
                T data = JsonConvert.DeserializeObject<T>(jsonAsset.text);
                cache[key] = data;
                return data;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading JSON data: {e.Message}");
        }

        return null;
    }

    public static bool DeleteData(string key)
    {
        try
        {
            string fullPath = Path.Combine(Application.dataPath, "Resources", defaultPath, $"{key}.json");
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
        catch (Exception e)
        {
            Debug.LogError($"Error deleting JSON data: {e.Message}");
        }
        return false;
    }

    public static void ClearAll()
    {
        try
        {
            string directory = Path.Combine(Application.dataPath, "Resources", defaultPath);
            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory, "*.json");
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
            cache.Clear();
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"Error clearing JSON data: {e.Message}");
        }
    }
}