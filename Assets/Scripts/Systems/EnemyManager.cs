using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    private List<EnemyBase> activeEnemies = new List<EnemyBase>();

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

    public List<EnemyBase> GetActiveEnemies()
    {
        return activeEnemies;
    }

    public EnemyBase GetClosestEnemy(Vector2 position, float range)
    {
        EnemyBase closest = null;
        float closestDistance = range;

        foreach (EnemyBase enemy in activeEnemies)
        {
            if (enemy == null) continue;
            float distance = Vector2.Distance(position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }
}