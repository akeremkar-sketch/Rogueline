using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Limits")]
    [Tooltip("Maximum number of enemies active in the scene at once")]
    [SerializeField] private int maxActiveEnemies = 60;

    private readonly HashSet<EnemyBase> activeEnemiesSet  = new HashSet<EnemyBase>();
    private readonly List<EnemyBase>    activeEnemiesList = new List<EnemyBase>();

    private CameraController _cam;

    public int  ActiveEnemyCount => activeEnemiesList.Count;
    public bool CanSpawnMore     => activeEnemiesList.Count < maxActiveEnemies;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        _cam = CameraController.Instance;
    }

    public void Register(EnemyBase enemy)
    {
        if (activeEnemiesSet.Add(enemy))
            activeEnemiesList.Add(enemy);
    }

    public void Unregister(EnemyBase enemy)
    {
        if (enemy == null) return;
        if (activeEnemiesSet.Remove(enemy))
            activeEnemiesList.Remove(enemy);
    }

    public void ClearAllEnemies()
    {
        for (int i = activeEnemiesList.Count - 1; i >= 0; i--)
        {
            if (activeEnemiesList[i] != null)
                Destroy(activeEnemiesList[i].gameObject);
        }
        activeEnemiesList.Clear();
        activeEnemiesSet.Clear();
    }

    public List<EnemyBase> GetActiveEnemies()
    {
        return activeEnemiesList;
    }

    private void Update()
    {
        if (_cam == null) _cam = CameraController.Instance;
        if (_cam == null) return;

        float killLine = _cam.CameraBottomY - 2f;

        for (int i = activeEnemiesList.Count - 1; i >= 0; i--)
        {
            if (activeEnemiesList[i] == null)
            {
                activeEnemiesList.RemoveAt(i);
                continue;
            }

            // Inside a boss room nothing is culled — boss chunk handles cleanup on exit
            if (!BossChunk.IsActive && activeEnemiesList[i].transform.position.y < killLine)
            {
                activeEnemiesSet.Remove(activeEnemiesList[i]);
                Destroy(activeEnemiesList[i].gameObject);
                activeEnemiesList.RemoveAt(i);
            }
        }
    }

    public EnemyBase GetClosestEnemy(Vector2 position, float range)
    {
        EnemyBase closest    = null;
        float closestSqrDist = range * range;

        foreach (EnemyBase enemy in activeEnemiesList)
        {
            if (enemy == null) continue;
            float sqrDist = ((Vector2)enemy.transform.position - position).sqrMagnitude;
            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = enemy;
            }
        }

        return closest;
    }
}
