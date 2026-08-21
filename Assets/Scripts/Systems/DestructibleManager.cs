using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks all active DestructibleObjects in the scene.
/// Weapons query this to include breakables in their target list.
/// </summary>
public class DestructibleManager : MonoBehaviour
{
    public static DestructibleManager Instance { get; private set; }

    // HashSet for O(1) add/remove; list kept in sync for ordered iteration
    private readonly HashSet<DestructibleObject>  _activeSet  = new HashSet<DestructibleObject>();
    private readonly List<DestructibleObject>     _activeList = new List<DestructibleObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Register(DestructibleObject obj)
    {
        if (_activeSet.Add(obj))
            _activeList.Add(obj);
    }

    public void Unregister(DestructibleObject obj)
    {
        if (_activeSet.Remove(obj))
            _activeList.Remove(obj);
    }

    public List<DestructibleObject> GetActive() => _activeList;
}
