using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;
using UnityEditor;

public static class CSVIO<T> where T : class, new()
{
    private static readonly string basePath;
    private static string customPath;
    private static readonly Dictionary<string, T> cache;

    static CSVIO()
    {
        basePath = typeof(T).Name;
        cache = new Dictionary<string, T>();
    }

    public static void SetCustomPath(string path)
    {
        customPath = path;
    }

    private static string GetSavePath()
    {
        return customPath ?? basePath;
    }

    public static void SaveData(string key, T data)
    {
        string fullPath = Path.Combine(Application.dataPath, "Resources", GetSavePath(), $"{key}.csv");
        string directory = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var csv = new StringBuilder();
        var properties = typeof(T).GetProperties();

        csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));

        var values = properties.Select(p => p.GetValue(data)?.ToString() ?? "");
        csv.AppendLine(string.Join(",", values));

        File.WriteAllText(fullPath, csv.ToString());
        cache[key] = data;
    }

    public static void SaveBulkData(string key, IEnumerable<T> dataList, bool overwrite = true)
    {
        try
        {
            if (!dataList.Any())
            {
                return;
            }

            string fullPath = Path.Combine(Application.dataPath, "Resources", GetSavePath(), $"{key}.csv");
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var csv = new StringBuilder();
            var properties = typeof(T).GetProperties
            (System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance);

            var headerLine = string.Join(",", properties.Select(p => p.Name.ToLower()));
            csv.AppendLine(headerLine);

            int count = 0;
            foreach (var data in dataList)
            {
                if (data == null)
                {
                    Debug.LogWarning("Skipping null data entry");
                    continue;
                }

                var values = properties.Select(p =>
                {
                    var value = p.GetValue(data);
                    if (value == null) return "";

                    if (value is string strValue && strValue.Contains(","))
                        return $"\"{strValue}\"";

                    if (value is bool boolValue)
                        return boolValue ? "1" : "0";

                    return value.ToString();
                });

                var line = string.Join(",", values);
                csv.AppendLine(line);
                count++;
            }

            File.WriteAllText(fullPath, csv.ToString());
            Debug.Log($"Successfully saved {count} entries to {fullPath}");
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving bulk CSV data: {e.Message}\n{e.StackTrace}");
        }
    }

    public static T LoadData(string key)
    {
        if (cache.TryGetValue(key, out T cachedData))
            return cachedData;

        string fullPath = Path.Combine(Application.dataPath, "Resources", GetSavePath(), $"{key}.csv");
        if (!File.Exists(fullPath))
            return null;

        var lines = File.ReadAllLines(fullPath);
        if (lines.Length < 2)
            return null;

        var headers = lines[0].Split(',');
        var values = lines[1].Split(',');

        T data = new T();
        var properties = typeof(T).GetProperties();

        for (int i = 0; i < headers.Length; i++)
        {
            var prop = properties.FirstOrDefault(p => p.Name == headers[i]);
            if (prop != null && i < values.Length)
            {
                try
                {
                    if (prop.PropertyType.IsEnum)
                    {
                        prop.SetValue(data, Enum.Parse(prop.PropertyType, values[i]));
                    }
                    else
                    {
                        prop.SetValue(data, Convert.ChangeType(values[i], prop.PropertyType));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to parse value '{values[i]}' for property '{prop.Name}': {e.Message}");
                }
            }
        }

        cache[key] = data;
        return data;
    }

    public static IEnumerable<T> LoadBulkData(string key)
    {
        string fullPath = Path.Combine(Application.dataPath, "Resources", GetSavePath(), $"{key}.csv");
        if (!File.Exists(fullPath))
            yield break;

        var lines = File.ReadAllLines(fullPath);
        if (lines.Length < 2)
            yield break;

        var headers = lines[0].Split(',');
        var properties = typeof(T).GetProperties();

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            T data = new T();

            for (int j = 0; j < headers.Length; j++)
            {
                var prop = properties.FirstOrDefault(p => p.Name == headers[j]);
                if (prop != null && j < values.Length)
                {
                    try
                    {
                        if (prop.PropertyType.IsEnum)
                        {
                            prop.SetValue(data, Enum.Parse(prop.PropertyType, values[j]));
                        }
                        else
                        {
                            prop.SetValue(data, Convert.ChangeType(values[j], prop.PropertyType));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Failed to parse value '{values[j]}' for property '{prop.Name}': {e.Message}");
                    }
                }
            }

            yield return data;
        }
    }

    public static bool DeleteData(string key)
    {
        string fullPath = Path.Combine(Application.dataPath, "Resources", GetSavePath(), $"{key}.csv");
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            cache.Remove(key);
            return true;
        }
        return false;
    }

    public static void ClearAll()
    {
        cache.Clear();
        string directory = Path.Combine(Application.dataPath, "Resources", GetSavePath());
        if (Directory.Exists(directory))
        {
            var files = Directory.GetFiles(directory, "*.csv");
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }

    public static void CreateDefaultFile(string fileName, string headers)
    {
        try
        {
            string fullPath = Path.Combine(Application.dataPath, "Resources", GetSavePath(), $"{fileName}.csv");
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(fullPath))
            {
                File.WriteAllText(fullPath, headers + "\n");
                Debug.Log($"Created new CSV file: {fullPath}");
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating default CSV file: {e.Message}\n{e.StackTrace}");
        }
    }
}