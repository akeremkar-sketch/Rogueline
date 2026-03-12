using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sahnedeki aktif düşmanları takip eder.
/// WeaponBase her frame FindGameObjectsWithTag çağırmak yerine bu listeyi kullanır.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    private readonly List<EnemyBase> activeEnemies = new List<EnemyBase>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Register(EnemyBase enemy)
    {
        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    public void Unregister(EnemyBase enemy)
    {
        activeEnemies.Remove(enemy);
    }

    public List<EnemyBase> GetActiveEnemies() => activeEnemies;
}
