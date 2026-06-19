using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    enum State
    {
        Enter, // 画面外から侵入
        Float  // 画面内で漂う
    }

    State state = State.Enter;

    Vector2 direction;   // 移動方向
    float changeTime = 2f;
    float timer;

    Vector2 target;       // 中央ターゲット

    [Header("速度")]
    public float enterSpeed = 4f;
    public float floatSpeed = 1.5f;

    [Header("スポーン位置の外側距離")]
    public float spawnOffset = 2f;

    private SpriteRenderer sr; // ★向き反転用

    private EnemyHP enemyHP;

    void Start()
    {
        target = Vector2.zero;
        SetRandomDirection();
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

        if (state == State.Enter)
        {
            MoveToScreen();
        }
        else if (state == State.Float)
        {
            FloatMove();
        }

        // 移動方向に応じて向きを変える
        FlipSprite();
    }

    //========================
    // 画面外→画面内へ侵入
    //========================
    void MoveToScreen()
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;

        transform.Translate(dir * enterSpeed * Time.deltaTime);

        // 画面に入ったら状態変更
        if (IsInsideScreen())
        {
            state = State.Float;
        }
    }

    //========================
    // 画面内で漂う
    //========================
    void FloatMove()
    {
        timer += Time.deltaTime;

        // 一定時間ごとに方向変更
        if (timer > changeTime)
        {
            SetRandomDirection();
            timer = 0f;
        }

        transform.Translate(direction * floatSpeed * Time.deltaTime);

        // 画面外に出ないよう反射
        StayInScreen();
    }

    //========================
    // ランダム方向
    //========================
    void SetRandomDirection()
    {
        direction = Random.insideUnitCircle.normalized;
    }

    //========================
    // スプライト反転処理
    //========================
    void FlipSprite()
    {
        if (sr == null) return;

        // 右に動く → 左向き（反対）
        if (direction.x > 0)
            sr.flipX = true;

        // 左に動く → 右向き（反対）
        else if (direction.x < 0)
            sr.flipX = false;
    }

    /*Vector2 GetSpawnPosition()
    {
        Camera cam = Camera.main;

        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        float halfW = width / 2f;

        int side = Random.Range(0, 2);

        // 0 = 左 / 1 = 右
        switch (side)
        {
            case 0: // 左から出る
                return new Vector2(
                    -halfW - spawnOffset,
                    Random.Range(-height / 2f, height / 2f)
                );

            default: // 右から出る
                return new Vector2(
                    halfW + spawnOffset,
                    Random.Range(-height / 2f, height / 2f)
                );
        }
    }*/

    //========================
    // 画面内判定
    //========================
    bool IsInsideScreen()
    {
        Camera cam = Camera.main;
        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);

        float margin = 0.05f;

        return viewPos.x > margin && viewPos.x < 1 - margin &&
               viewPos.y > margin && viewPos.y < 1 - margin;
    }

    //========================
    // 画面内反射移動
    //========================
    void StayInScreen()
    {
        Camera cam = Camera.main;

        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);

        float margin = 0.05f;

        if (viewPos.x < margin || viewPos.x > 1 - margin)
        {
            direction.x *= -1;
        }

        if (viewPos.y < margin || viewPos.y > 1 - margin)
        {
            direction.y *= -1;
        }
    }
}