using UnityEngine;

public class EnemyMove : MonoBehaviour, IHitSlowable

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

    [Header("被弾時の鈍化")]

    public float hitSlowMultiplier = 0.3f; // 鈍化中の速度倍率（1fで鈍化なし、0fで完全停止）

    public float hitSlowDuration = 0.5f;   // 鈍化が続く時間（秒）

    private float slowTimer = 0f;       // 鈍化の残り時間

    private float speedMultiplier = 1f; // 現在の速度倍率（鈍化中は1未満になる）

    private SpriteRenderer sr; // 向き反転用

    //========================
    // 拘束
    //========================

    private Coroutine bindCoroutine;
    private bool isBind = false;

    private EnemyHP enemyHP;

    void Start()

    {

        target = Vector2.zero;

        SetRandomDirection();

        sr = GetComponent<SpriteRenderer>();

    }

    void Update()

    {

        //========================
        // 拘束中停止
        //========================

        if (enemyHP != null &&
   enemyHP.IsBind())
        {
            return;
        }

        // 被弾鈍化の更新

        UpdateHitSlow();

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

        transform.Translate(dir * enterSpeed * speedMultiplier * Time.deltaTime);

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

        transform.Translate(direction * floatSpeed * speedMultiplier * Time.deltaTime);

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

    // 被弾時に呼ぶ（EnemyHP側からの呼び出し用）

    public void ApplyHitSlow()

    {

        slowTimer = hitSlowDuration;

        speedMultiplier = hitSlowMultiplier;

    }

    // 鈍化タイマーの経過処理

    void UpdateHitSlow()

    {

        if (slowTimer <= 0f)

        {

            speedMultiplier = 1f;

            return;

        }

        slowTimer -= Time.deltaTime;

        if (slowTimer <= 0f)

        {

            speedMultiplier = 1f; // 鈍化終了、通常速度に戻す

        }

        else

        {

            speedMultiplier = hitSlowMultiplier; // 鈍化継続中

        }

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
