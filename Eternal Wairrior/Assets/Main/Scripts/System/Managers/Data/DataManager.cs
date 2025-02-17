using UnityEngine;

public abstract class DataManager<T> : SingletonManager<T>, IInitializable where T : MonoBehaviour
{
    protected bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    public virtual void Initialize()
    {
        if (!isInitialized)
        {
            InitializeDefaultData();
        }
    }

    public virtual void InitializeDefaultData()
    {
        try
        {
            Debug.Log($"Starting to initialize default data structure for {GetType().Name}...");
            LoadRuntimeData();
            Debug.Log($"Successfully initialized default data structure for {GetType().Name}");
            isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing default data for {GetType().Name}: {e.Message}\n{e.StackTrace}");
            isInitialized = false;
            throw;
        }
    }

    protected virtual void LoadRuntimeData() { }
    protected virtual void SaveRuntimeData() { }
    protected virtual void DeleteRuntimeData() { }
    protected virtual void ClearRuntimeData() { }
}