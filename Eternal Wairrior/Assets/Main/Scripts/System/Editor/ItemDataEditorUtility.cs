using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Newtonsoft.Json;

namespace EternalWarrior.Editor
{
    public static class ItemDataEditorUtility
    {
        #region Constants
        private const string RESOURCE_ROOT = "Assets/Resources";
        private const string ITEM_DB_PATH = "Items/Database";
        private const string ITEM_ICON_PATH = "Items/Icons";
        private const string DROP_TABLES_PATH = "Items/DropTables";
        #endregion

        #region Data Management
        private static Dictionary<string, ItemData> itemDatabase = new();
        private static Dictionary<EnemyType, DropTableData> dropTables = new();

        public static Dictionary<string, ItemData> GetItemDatabase()
        {
            if (!itemDatabase.Any())
            {
                LoadItemDatabase();
            }
            return new Dictionary<string, ItemData>(itemDatabase);
        }

        public static Dictionary<EnemyType, DropTableData> GetDropTables()
        {
            if (!dropTables.Any())
            {
                LoadDropTables();
            }
            return new Dictionary<EnemyType, DropTableData>(dropTables);
        }

        public static void SaveItemData(ItemData itemData)
        {
            if (itemData == null) return;

            try
            {
                // 아이콘 리소스 저장
                if (!string.IsNullOrEmpty(itemData.IconPath))
                {
                    string sourceAssetPath = itemData.IconPath;
                    if (!string.IsNullOrEmpty(sourceAssetPath))
                    {
                        // 이미 Resources 폴더에 있는 경우 경로만 업데이트
                        if (sourceAssetPath.Contains("/Resources/"))
                        {
                            itemData.IconPath = GetResourcePath(sourceAssetPath);
                        }
                        else
                        {
                            // Resources 폴더로 복사
                            string targetPath = Path.Combine(RESOURCE_ROOT, ITEM_ICON_PATH, $"{itemData.ID}_Icon{Path.GetExtension(sourceAssetPath)}").Replace('\\', '/');
                            Debug.Log($"Copying icon from {sourceAssetPath} to {targetPath}");

                            // 디렉토리 생성
                            string directory = Path.GetDirectoryName(targetPath);
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
                            bool success = AssetDatabase.CopyAsset(sourceAssetPath, targetPath);
                            if (!success)
                            {
                                // 실패해도 무시하고 진행 (이미 존재하는 파일일 수 있음)
                                Debug.Log($"Note: Asset might already exist at {targetPath}");
                            }

                            AssetDatabase.Refresh();

                            // Resources 폴더 기준 상대 경로로 변환
                            itemData.IconPath = GetResourcePath(targetPath);
                            Debug.Log($"Icon path updated to: {itemData.IconPath}");
                        }
                    }
                }

                // 데이터베이스에 저장
                var clonedData = itemData.Clone();
                itemDatabase[itemData.ID] = clonedData;

                SaveDatabase();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving item data: {e.Message}\n{e.StackTrace}");
            }
        }

        public static void SaveDatabase()
        {
            try
            {
                var wrapper = new SerializableItemList { items = itemDatabase.Values.ToList() };
                string json = JsonConvert.SerializeObject(wrapper,
                    new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects
                    });

                string path = Path.Combine(RESOURCE_ROOT, ITEM_DB_PATH);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                File.WriteAllText(Path.Combine(path, "ItemDatabase.json"), json);
                Debug.Log($"Database saved successfully");
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving database: {e.Message}\n{e.StackTrace}");
            }
        }

        public static void SaveDropTables()
        {
            try
            {
                if (dropTables == null || !dropTables.Any())
                {
                    CreateDefaultDropTables();
                    return;
                }

                var wrapper = new DropTablesWrapper { dropTables = dropTables.Values.ToList() };
                JSONIO<DropTablesWrapper>.SaveData("DropTables", wrapper);
                Debug.Log("Drop tables saved successfully");
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error saving drop tables: {e.Message}\n{e.StackTrace}");
            }
        }

        public static void SaveWithBackup()
        {
            string backupPath = Path.Combine(RESOURCE_ROOT, ITEM_DB_PATH, "Backups");
            Directory.CreateDirectory(backupPath);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFile = Path.Combine(backupPath, $"ItemDatabase_Backup_{timestamp}.json");

            var wrapper = new SerializableItemList { items = itemDatabase.Values.ToList() };
            File.WriteAllText(backupFile, JsonConvert.SerializeObject(wrapper));

            Debug.Log($"Backup created at: {backupFile}");
            AssetDatabase.Refresh();
        }

        #endregion

        #region Private Methods
        private static void LoadItemDatabase()
        {
            try
            {
                string path = Path.Combine(RESOURCE_ROOT, ITEM_DB_PATH, "ItemDatabase.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var data = JsonConvert.DeserializeObject<SerializableItemList>(json,
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        });

                    if (data?.items != null)
                    {
                        itemDatabase = data.items.ToDictionary(item => item.ID);
                        LoadItemResources();
                    }
                    else
                    {
                        Debug.LogWarning("No item database found or empty database");
                        itemDatabase = new Dictionary<string, ItemData>();
                    }
                }
                else
                {
                    Debug.LogWarning($"Database file not found at {path}");
                    itemDatabase = new Dictionary<string, ItemData>();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading item database: {e.Message}\n{e.StackTrace}");
                itemDatabase = new Dictionary<string, ItemData>();
            }
        }

        private static void LoadDropTables()
        {
            try
            {
                var data = JSONIO<DropTablesWrapper>.LoadData("DropTables");
                if (data != null && data.dropTables != null)
                {
                    dropTables = data.dropTables.ToDictionary(dt => dt.enemyType);
                }
                else
                {
                    Debug.LogWarning("No drop tables found");
                    dropTables = new Dictionary<EnemyType, DropTableData>();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading drop tables: {e.Message}");
                dropTables = new Dictionary<EnemyType, DropTableData>();
            }
        }

        public static void SaveItemResources(ItemData itemData)
        {
            Debug.Log($"SaveItemResources called for item {itemData?.ID}");
            if (itemData == null) return;

            try
            {
                if (!string.IsNullOrEmpty(itemData.IconPath))
                {
                    string sourceAssetPath = itemData.IconPath;
                    Debug.Log($"Source asset path: {sourceAssetPath}");

                    // 이미 Resources 폴더에 있는 경우 경로만 업데이트
                    if (sourceAssetPath.Contains("/Resources/"))
                    {
                        itemData.IconPath = GetResourcePath(sourceAssetPath);
                        Debug.Log($"Asset already in Resources folder. Updated path to: {itemData.IconPath}");
                        return;
                    }

                    // Resources 폴더로 복사
                    string targetPath = Path.Combine(RESOURCE_ROOT, ITEM_ICON_PATH, $"{itemData.ID}_Icon{Path.GetExtension(sourceAssetPath)}").Replace('\\', '/');
                    Debug.Log($"Target path: {targetPath}");

                    // 디렉토리 생성
                    string directory = Path.GetDirectoryName(targetPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        Debug.Log($"Created directory: {directory}");
                    }

                    // 기존 파일이 있다면 삭제
                    if (File.Exists(targetPath))
                    {
                        AssetDatabase.DeleteAsset(targetPath);
                        Debug.Log($"Deleted existing file: {targetPath}");
                    }

                    // 파일 복사
                    AssetDatabase.CopyAsset(sourceAssetPath, targetPath);
                    AssetDatabase.Refresh();
                    Debug.Log($"Copied asset from {sourceAssetPath} to {targetPath}");

                    // Resources 폴더 기준 상대 경로로 변환
                    string resourcePath = GetResourcePath(targetPath);
                    itemData.IconPath = resourcePath;
                    Debug.Log($"Updated IconPath to: {itemData.IconPath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error saving item resources for {itemData.ID}: {e.Message}\n{e.StackTrace}");
            }
        }

        private static void LoadItemResources()
        {
            foreach (var item in itemDatabase.Values)
            {
                try
                {
                    if (!string.IsNullOrEmpty(item.IconPath))
                    {
                        var icon = ResourceIO<Sprite>.LoadData(item.IconPath);
                        if (icon != null)
                        {
                            Debug.Log($"Successfully loaded icon for item {item.ID}");
                        }
                        else
                        {
                            string alternativePath = $"Items/Icons/{item.ID}_Icon";
                            Debug.Log($"Trying alternative path for item {item.ID}: {alternativePath}");
                            icon = ResourceIO<Sprite>.LoadData(alternativePath);

                            if (icon != null)
                            {
                                item.IconPath = alternativePath;
                                Debug.Log($"Successfully loaded icon from alternative path for item {item.ID}");
                            }
                            else
                            {
                                Debug.LogWarning($"Failed to load icon for item {item.ID}. Paths tried:\n1. {item.IconPath}\n2. {alternativePath}");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"No icon path specified for item {item.ID}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error loading resources for item {item.ID}: {e.Message}\n{e.StackTrace}");
                }
            }
        }

        public static string GetResourcePath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return string.Empty;

            // 백슬래시를 슬래시로 변환
            fullPath = fullPath.Replace('\\', '/');

            // Resources/ 이후의 경로 추출
            int resourcesIndex = fullPath.IndexOf("Resources/");
            if (resourcesIndex == -1) return fullPath;

            string relativePath = fullPath.Substring(resourcesIndex + "Resources/".Length);

            // 확장자 제거
            relativePath = Path.ChangeExtension(relativePath, null);

            Debug.Log($"Converting path:\nFull path: {fullPath}\nResource path: {relativePath}");

            return relativePath;
        }

        private static void CreateDefaultDropTables()
        {
            dropTables = new Dictionary<EnemyType, DropTableData>
            {
                {
                    EnemyType.Normal,
                    new DropTableData
                    {
                        enemyType = EnemyType.Normal,
                        guaranteedDropRate = 0.1f,
                        maxDrops = 2,
                        dropEntries = new List<DropTableEntry>()
                    }
                },
                {
                    EnemyType.Elite,
                    new DropTableData
                    {
                        enemyType = EnemyType.Elite,
                        guaranteedDropRate = 0.3f,
                        maxDrops = 3,
                        dropEntries = new List<DropTableEntry>()
                    }
                },
                {
                    EnemyType.Boss,
                    new DropTableData
                    {
                        enemyType = EnemyType.Boss,
                        guaranteedDropRate = 1f,
                        maxDrops = 5,
                        dropEntries = new List<DropTableEntry>()
                    }
                }
            };
            SaveDropTables();
        }

        public static void InitializeDefaultData()
        {
            try
            {
                // 필요한 폴더 구조 생성
                EnsureDirectoryStructure();

                // 기본 아이템 데이터 생성
                var defaultItemData = new ItemData
                {
                    ID = "sword_01",
                    Name = "Basic Sword",
                    Description = "A simple sword",
                    Type = ItemType.Weapon,
                    Rarity = ItemRarity.Common,
                    MaxStack = 1,
                    DropRate = 0.1f
                };

                // 데이터 저장
                SaveItemData(defaultItemData);
                CreateDefaultDropTables();
                SaveDatabase();
                Debug.Log("Created default files successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error creating default files: {e.Message}");
                throw;
            }
        }

        private static void EnsureDirectoryStructure()
        {
            var paths = new[]
            {
                Path.Combine(RESOURCE_ROOT, ITEM_DB_PATH),
                Path.Combine(RESOURCE_ROOT, ITEM_ICON_PATH)
            };

            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Debug.Log($"Created directory: {path}");
                }
            }
            AssetDatabase.Refresh();
        }
        #endregion
    }

    [System.Serializable]
    public class DropTablesWrapper
    {
        public List<DropTableData> dropTables = new();
    }

    [System.Serializable]
    public class SerializableItemList
    {
        public List<ItemData> items = new();
    }
}