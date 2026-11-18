using UnityEngine;

public class ManagerBase : MonoBehaviour
{
    protected T EnsureManager<T>(ref T manager) where T : ManagerBase
    {
        return EnsureManager(transform, ref manager);
    }

    public static T EnsureManager<T>(Transform parent, ref T manager) where T : ManagerBase
    {
        if (manager != null)
            return manager;

        if (parent == null)
        {
            Debug.LogError($"[ManagerBase] EnsureManager<{typeof(T).Name}> parent가 null입니다.");
            return null;
        }

        manager = parent.GetComponentInChildren<T>(true);
        if (manager != null)
            return manager;

        var go = new GameObject(typeof(T).Name);
        go.transform.SetParent(parent);
        manager = go.AddComponent<T>();
        return manager;
    }
}
