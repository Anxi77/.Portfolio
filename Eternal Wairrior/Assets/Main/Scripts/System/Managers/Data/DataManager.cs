using UnityEngine;

public abstract class DataManager<T> : SingletonManager<T>, IInitializable
    where T : MonoBehaviour
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
            LoadRuntimeData();
            isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                $"Error initializing default data for {GetType().Name}: {e.Message}\n{e.StackTrace}"
            );
            isInitialized = false;
            throw;
        }
    }

    protected virtual void ClearRuntimeData() { }

    protected virtual void DeleteRuntimeData() { }

    protected virtual void LoadRuntimeData() { }

    protected virtual void SaveRuntimeData() { }
}
