using UnityEngine;

public class SlimeEnemy: MonoBehaviour
{
    [Header("移動")]
    public float moveSpeed = 5f;        // 移動速度
    public float rotateSpeed = 180f;    // 回転速度

    [Header("伸縮アニメ")]
    public float animSpeed = 6f;        // 伸縮の速さ
    public float stretchAmount = 0.2f;  // 伸縮の強さ

    private Vector2 moveDirection;      // 移動方向
    private Vector3 baseScale;          // 元のサイズ

    void Start()
    {
        // 画面外スポーン＋進行方向決定
        SetSpawnAndDirection();

        // 元のスケール保存
        baseScale = transform.localScale;
    }

    void Update()
    {
        // =====================
        // 移動
        // =====================
        transform.Translate(
            moveDirection * moveSpeed * Time.deltaTime,
            Space.World
        );

        // =====================
        // 回転
        // =====================
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

        //// =====================
        //// 伸縮アニメ（百足風）
        //// =====================
        //float sin = Mathf.Sin(Time.time * animSpeed);

        //float scaleX = baseScale.x * (1f + sin * stretchAmount);
        //float scaleY = baseScale.y * (1f - sin * stretchAmount);

        //transform.localScale = new Vector3(scaleX, scaleY, 1f);

        // =====================
        // 画面外チェック
        // =====================
        CheckOutOfScreen();
    }

    // =========================
    // 画面外に出たら削除
    // =========================
    void CheckOutOfScreen()
    {
        Camera cam = Camera.main;

        float height = cam.orthographicSize;
        float width = height * cam.aspect;

        Vector2 pos = transform.position;

        float margin = 100f;

        if (pos.x < -width - margin ||
            pos.x > width + margin ||
            pos.y < -height - margin ||
            pos.y > height + margin)
        {
            Destroy(gameObject);
        }
    }

    // =========================
    // スポーン＆方向設定
    // =========================
    void SetSpawnAndDirection()
    {
        Camera cam = Camera.main;

        float height = cam.orthographicSize;
        float width = height * cam.aspect;

        int side = Random.Range(0, 4);
        Vector2 spawnPos = Vector2.zero;

        // 画面外スポーン
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

        // 画面内ランダム地点へ向かう
        Vector2 target = new Vector2(
            Random.Range(-width, width),
            Random.Range(-height, height)
        );

        moveDirection = (target - spawnPos).normalized;
    }
}