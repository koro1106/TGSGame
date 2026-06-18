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

    private EnemyHP enemyHP;

    void Start()
    {
        Camera cam = Camera.main;

        screenHeight = cam.orthographicSize;
        screenWidth = screenHeight * cam.aspect;

        SetSpawnAndDirection();

        hp = GetComponent<EnemyHP>();
        sr = GetComponent<SpriteRenderer>();

        enemyHP = GetComponent<EnemyHP>();
    }

    void Update()
    {
        if (enemyHP != null &&
   enemyHP.IsBind())
        {
            return;
        }

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

        float halfW = w;
        float halfH = h;

        float offset = 3f; //（画面外距離）

        int side = Random.Range(0, 4);
        Vector2 spawnPos;

        switch (side)
        {
            case 0: // 右
                spawnPos = new Vector2(halfW + offset, Random.Range(-halfH, halfH));
                break;

            case 1: // 左
                spawnPos = new Vector2(-halfW - offset, Random.Range(-halfH, halfH));
                break;

            case 2: // 上
                spawnPos = new Vector2(Random.Range(-halfW, halfW), halfH + offset);
                break;

            default: // 下
                spawnPos = new Vector2(Random.Range(-halfW, halfW), -halfH - offset);
                break;
        }

        transform.position = spawnPos;

        // 画面内ランダム地点へ向かう
        Vector2 target = new Vector2(
            Random.Range(-halfW, halfW),
            Random.Range(-halfH, halfH)
        );

        moveDirection = (target - spawnPos).normalized;
    }
}