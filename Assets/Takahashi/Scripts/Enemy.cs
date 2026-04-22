using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;     // 移動速度
    public float rotateSpeed = 3f; // 回転速度
    private Vector2 moveDirection;

    void Start()
    {
        // ランダム方向
        moveDirection = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        // 少しずつ方向をランダムに変える
        moveDirection += Random.insideUnitCircle * 0.06f;
        moveDirection = moveDirection.normalized;

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        // 回転
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }
}
