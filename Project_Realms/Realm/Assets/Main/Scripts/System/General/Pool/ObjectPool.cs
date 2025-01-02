using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class ObjectPool : MonoBehaviour
{
    private Dictionary<string, Queue<Component>> poolDictionary;
    private Dictionary<string, Component> prefabDictionary;
    private Dictionary<string, Transform> poolParents;
    private Dictionary<string, PoolStats> poolStats;

    private const int DEFAULT_POOL_SIZE = 20;
    private const int EXPAND_SIZE = 10;
    private const int MAX_POOL_SIZE = 100;
    private const float CLEANUP_INTERVAL = 60f;
    private const float UNUSED_THRESHOLD = 10f;

    [System.Serializable]
    private class PoolStats
    {
        public int maxUsed;
        public int currentActive;
        public float lastUsedTime;
        public int totalSpawns;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        poolDictionary = new Dictionary<string, Queue<Component>>();
        prefabDictionary = new Dictionary<string, Component>();
        poolParents = new Dictionary<string, Transform>();
        poolStats = new Dictionary<string, PoolStats>();

        StartCoroutine(AutoCleanupCoroutine());
    }

    private IEnumerator AutoCleanupCoroutine()
    {
        WaitForSeconds wait = new WaitForSeconds(CLEANUP_INTERVAL);
        while (true)
        {
            yield return wait;
            CleanupUnusedPools();
            OptimizePoolSizes();
        }
    }

    private void OptimizePoolSizes()
    {
        foreach (var tag in poolDictionary.Keys.ToList())
        {
            if (!poolStats.TryGetValue(tag, out PoolStats stats)) continue;

            Queue<Component> pool = poolDictionary[tag];
            int optimalSize = Mathf.Max(stats.maxUsed, DEFAULT_POOL_SIZE);
            optimalSize = Mathf.Min(optimalSize, MAX_POOL_SIZE);

            while (pool.Count > optimalSize)
            {
                var obj = pool.Dequeue();
                Destroy(obj.gameObject);
            }

            //Debug.Log($"Optimized pool {tag}: Size={pool.Count}, MaxUsed={stats.maxUsed}, TotalSpawns={stats.totalSpawns}");
        }
    }

    private void CleanupUnusedPools()
    {
        float currentTime = Time.time;
        foreach (var tag in poolStats.Keys.ToList())
        {
            PoolStats stats = poolStats[tag];
            if (currentTime - stats.lastUsedTime > UNUSED_THRESHOLD && stats.currentActive == 0)
            {
                ClearPool(tag);
                Debug.Log($"Cleaned up unused pool: {tag}");
            }
        }
    }

    public void ClearPool(string tag)
    {
        if (poolParents.TryGetValue(tag, out Transform parent))
        {
            Destroy(parent.gameObject);
            poolParents.Remove(tag);
        }

        poolDictionary.Remove(tag);
        prefabDictionary.Remove(tag);
        poolStats.Remove(tag);
    }

    private Transform GetPoolParent(string tag)
    {
        if (!poolParents.TryGetValue(tag, out Transform parent))
        {
            GameObject poolParent = new GameObject($"Pool_{tag}");
            poolParent.transform.SetParent(transform);
            parent = poolParent.transform;
            poolParents[tag] = parent;
        }
        return parent;
    }

    private Component CreateNewObjectInPool(string tag, Queue<Component> pool)
    {
        if (!prefabDictionary.TryGetValue(tag, out Component prefab))
            return null;

        Component obj = Instantiate(prefab);
        obj.gameObject.SetActive(false);

        Transform poolParent = GetPoolParent(tag);
        obj.transform.SetParent(poolParent);

        string objName = obj.gameObject.name;
        if (objName.EndsWith("(Clone)"))
        {
            objName = objName.Substring(0, objName.Length - 7);
        }
        obj.gameObject.name = objName;

        pool.Enqueue(obj);
        return obj;
    }

    public T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        T component = prefab.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError($"Prefab {prefab.name} does not have component of type {typeof(T)}");
            return null;
        }

        string tag = prefab.name;
        if (tag.EndsWith("(Clone)"))
        {
            tag = tag.Substring(0, tag.Length - 7);
        }

        if (!poolDictionary.ContainsKey(tag))
        {
            poolDictionary[tag] = new Queue<Component>();
            prefabDictionary[tag] = component;

            for (int i = 0; i < DEFAULT_POOL_SIZE; i++)
            {
                CreateNewObjectInPool(tag, poolDictionary[tag]);
            }
        }

        Queue<Component> pool = poolDictionary[tag];

        if (pool.Count == 0)
        {
            for (int i = 0; i < EXPAND_SIZE; i++)
            {
                CreateNewObjectInPool(tag, pool);
            }
        }

        Component obj = pool.Dequeue();
        obj.transform.SetParent(null);

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.MoveGameObjectToScene(obj.gameObject, currentScene);

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);

        if (obj is IPoolable poolable)
        {
            poolable.OnSpawnFromPool();
        }

        if (!poolStats.ContainsKey(tag))
        {
            poolStats[tag] = new PoolStats();
        }

        PoolStats stats = poolStats[tag];
        stats.lastUsedTime = Time.time;
        stats.currentActive++;
        stats.totalSpawns++;
        stats.maxUsed = Mathf.Max(stats.maxUsed, stats.currentActive);

        return obj as T;
    }

    public void Despawn<T>(T obj) where T : Component
    {
        if (obj == null) return;

        string tag = obj.gameObject.name;
        if (tag.EndsWith("(Clone)"))
        {
            tag = tag.Substring(0, tag.Length - 7);
        }

        if (poolStats.TryGetValue(tag, out PoolStats stats))
        {
            stats.currentActive--;
        }

        if (obj is IPoolable poolable)
        {
            poolable.OnReturnToPool();
        }

        obj.gameObject.SetActive(false);
        Transform poolParent = GetPoolParent(tag);
        obj.transform.SetParent(poolParent);
        poolDictionary[tag].Enqueue(obj);
    }

    public void ClearAllPools()
    {
        foreach (var poolParent in poolParents.Values)
        {
            if (poolParent != null)
            {
                Destroy(poolParent.gameObject);
            }
        }

        poolDictionary.Clear();
        prefabDictionary.Clear();
        poolParents.Clear();
    }

    public void LogPoolStats()
    {
        foreach (var kvp in poolStats)
        {
            string tag = kvp.Key;
            PoolStats stats = kvp.Value;
            Debug.Log($"Pool {tag}: Active={stats.currentActive}, MaxUsed={stats.maxUsed}, " +
                     $"TotalSpawns={stats.totalSpawns}, PoolSize={poolDictionary[tag].Count}");
        }
    }
}

public interface IPoolable
{
    void OnSpawnFromPool();
    void OnReturnToPool();
}