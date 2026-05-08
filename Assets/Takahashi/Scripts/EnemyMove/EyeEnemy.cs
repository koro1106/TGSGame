using UnityEngine;

public class EyeEnemy : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float angryMoveSpeed = 10f;

    [Header("ぴょんぴょん")]
    public float bounceHeight = 0.2f;
    public float bounceSpeed = 8f;

    [Header("スプライト")]
    public Sprite normalSprite;
    public Sprite angrySprite;

    private Vector2 moveDirection;

    private float screenWidth;
    private float screenHeight;

    private EnemyHP hp;
    private SpriteRenderer sr;

    private bool angryTriggered = false;

    void Start()
    {
        Camera cam = Camera.main;

        screenHeight = cam.orthographicSize;
        screenWidth = screenHeight * cam.aspect;

        SetSpawnAndDirection();

        hp = GetComponent<EnemyHP>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (hp == null) return;

        float hpRate = (float)hp.currentHP / hp.maxHP;

        // ===== HP 1/4以下で暴走 =====
        if (!angryTriggered && hp.currentHP <= hp.maxHP * 1 / 4)
        {
            angryTriggered = true;
            moveSpeed = angryMoveSpeed;

            if (sr != null && angrySprite != null)
            {
                sr.sprite = angrySprite;
            }
        }

        // ===== 移動 =====
        transform.Translate(
            moveDirection * moveSpeed * Time.deltaTime,
            Space.World
        );

        // ===== ぴょんぴょん =====
        float currentBounce = bounceHeight * hpRate;

        Vector3 pos = transform.position;

        pos.y += Mathf.Sin(Time.time * bounceSpeed) * currentBounce;

        transform.position = pos;
    }

    void SetSpawnAndDirection()
    {
        Camera cam = Camera.main;

        float h = cam.orthographicSize;
        float w = h * cam.aspect;

        int side = Random.Range(0, 4);
        Vector2 spawnPos = Vector2.zero;

        // ===== 画面外スポーン =====
        switch (side)
        {
            case 0: // 右
                spawnPos = new Vector2(w + 1, Random.Range(-h, h));
                break;

            case 1: // 左
                spawnPos = new Vector2(-w - 1, Random.Range(-h, h));
                break;

            case 2: // 上
                spawnPos = new Vector2(Random.Range(-w, w), h + 1);
                break;

            case 3: // 下
                spawnPos = new Vector2(Random.Range(-w, w), -h - 1);
                break;
        }

        transform.position = spawnPos;

        // 画面内ランダム地点へ向かう
        Vector2 target = new Vector2(
            Random.Range(-w, w),
            Random.Range(-h, h)
        );

        moveDirection = (target - spawnPos).normalized;
    }
}