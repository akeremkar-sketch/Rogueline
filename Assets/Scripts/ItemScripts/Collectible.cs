using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Transform player;
    private bool isAttracting = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= PlayerStats.Instance.PickupRadius)
            isAttracting = true;

        if (isAttracting)
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }
}