using UnityEngine;

public class SquareRedMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 180f;

    private Vector2 moveDirection;

    void Start()
    {
        SetSpawnAndDirection();
    }

    void Update()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }

    void SetSpawnAndDirection()
    {
        Camera cam = Camera.main;

        float height = cam.orthographicSize;
        float width = height * cam.aspect;

        int side = Random.Range(0, 4);
        Vector2 spawnPos = Vector2.zero;

        // 画面外にスポーン
        switch (side)
        {
            case 0: // 右
                spawnPos = new Vector2(width + 1, Random.Range(-height, height));
                break;

            case 1: // 左
                spawnPos = new Vector2(-width - 1, Random.Range(-height, height));
                break;

            case 2: // 上
                spawnPos = new Vector2(Random.Range(-width, width), height + 1);
                break;

            case 3: // 下
                spawnPos = new Vector2(Random.Range(-width, width), -height - 1);
                break;
        }

        transform.position = spawnPos;

        // 画面内のランダムな場所をターゲットにする
        Vector2 target = new Vector2(
            Random.Range(-width, width),
            Random.Range(-height, height)
        );

        // ターゲットに向かう方向
        moveDirection = (target - spawnPos).normalized;
    }
}