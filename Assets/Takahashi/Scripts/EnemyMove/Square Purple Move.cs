using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    enum State
    {
        Enter, // 画面に入るまで
        Float  // 画面内で漂う
    }

    State state = State.Enter;

    Vector2 direction;
    float changeTime = 2f;
    float timer;

    Vector2 target;

    [Header("速度")]
    public float enterSpeed = 4f;
    public float floatSpeed = 1.5f;

    [Header("スポーン位置の外側距離")]
    public float spawnOffset = 2f;

    void Start()
    {
        // ① 画面外にスポーン
        transform.position = GetSpawnPosition();

        // ② 中央へ向かう（侵入用）
        target = Vector2.zero;

        // ③ 漂い初期方向
        SetRandomDirection();
    }

    void Update()
    {
        if (state == State.Enter)
        {
            MoveToScreen();
        }
        else if (state == State.Float)
        {
            FloatMove();
        }
    }

    //========================
    // 外から画面に入る
    //========================
    void MoveToScreen()
    {
        // 強制的に中央方向へ（壁や方向無視）
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        transform.Translate(dir * enterSpeed * Time.deltaTime);

        // 画面内に入ったら漂いへ
        if (IsInsideScreen())
        {
            state = State.Float;
        }
    }

    //========================
    // 漂い移動
    //========================
    void FloatMove()
    {
        timer += Time.deltaTime;

        if (timer > changeTime)
        {
            SetRandomDirection();
            timer = 0f;
        }

        transform.Translate(direction * floatSpeed * Time.deltaTime);

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
    // 画面外スポーン
    //========================
    Vector2 GetSpawnPosition()
    {
        Camera cam = Camera.main;

        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: // 上
                return new Vector2(Random.Range(-width / 2, width / 2), height / 2 + spawnOffset);
            case 1: // 下
                return new Vector2(Random.Range(-width / 2, width / 2), -height / 2 - spawnOffset);
            case 2: // 右
                return new Vector2(width / 2 + spawnOffset, Random.Range(-height / 2, height / 2));
            default: // 左
                return new Vector2(-width / 2 - spawnOffset, Random.Range(-height / 2, height / 2));
        }
    }

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
    // 画面内に留める（反射）
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