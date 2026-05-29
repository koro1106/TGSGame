using UnityEngine;

public class BossMove : MonoBehaviour
{
    public Transform player;

    public float moveSpeed = 3f;

    void Update()
    {
        if (player == null) return;

        // ƒvƒŒƒCƒ„[•ûŒü
        Vector2 dir =
            (player.position - transform.position).normalized;

        // ˆÚ“®
        transform.position +=
            (Vector3)dir * moveSpeed * Time.deltaTime;
    }
}