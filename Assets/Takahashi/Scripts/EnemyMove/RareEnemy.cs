using UnityEngine;

public class RareEnemy : MonoBehaviour
{
    enum State
    {
        Enter,
        Float,
        Flee
    }

    State state = State.Enter;

    [Header("移動")]
    public float enterSpeed = 4f;
    public float floatSpeed = 1.5f;
    public float fleeSpeed = 10f;

    [Header("HP")]
    public int hp = 3;

    [Header("逃走条件")]
    public float detectDistance = 250f;

    [Header("方向変更")]
    public float changeTime = 2f;

    private Vector2 direction;
    private float timer;

    private SpriteRenderer sr;
    private Transform player;

    private bool hasFled = false;

    private Vector2 target;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        target = Vector2.zero;
        SetRandomDirection();
    }

    void Update()
    {
        if (state == State.Flee)
        {
            FleeMove();
            return;
        }

        CheckPlayerDistance();

        if (state == State.Enter)
        {
            MoveToScreen();
        }
        else if (state == State.Float)
        {
            FloatMove();
        }
    }

    // =========================
    // プレイヤー距離チェック
    // =========================
    void CheckPlayerDistance()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= detectDistance)
        {
            StartFlee();
        }
    }

    // =========================
    // ダメージ（必ず1ダメ＋即逃げ）
    // =========================
    public void TakeDamage(int damage)
    {
        if (hasFled) return;

        damage = 1;
        hp -= damage;

        StartFlee();
    }

    // =========================
    // 逃走開始（1回だけ）
    // =========================
    void StartFlee()
    {
        if (hasFled) return;

        hasFled = true;
        state = State.Flee;

        if (player != null)
        {
            direction = ((Vector2)transform.position - (Vector2)player.position).normalized;
        }
        else
        {
            direction = Random.insideUnitCircle.normalized;
        }
    }

    // =========================
    // 逃走
    // =========================
    void FleeMove()
    {
        transform.Translate(direction * fleeSpeed * Time.deltaTime);

        if (IsOutsideScreen())
        {
            Destroy(gameObject);
        }
    }

    // =========================
    // 画面外→画面内
    // =========================
    void MoveToScreen()
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;

        transform.Translate(dir * enterSpeed * Time.deltaTime);

        if (IsInsideScreen())
        {
            state = State.Float;
        }
    }

    // =========================
    // 漂う
    // =========================
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

    // =========================
    // ランダム移動
    // =========================
    void SetRandomDirection()
    {
        direction = Random.insideUnitCircle.normalized;

        if (direction == Vector2.zero)
            direction = Vector2.one.normalized;
    }

    // =========================
    // 画面内判定
    // =========================
    bool IsInsideScreen()
    {
        Camera cam = Camera.main;
        Vector3 v = cam.WorldToViewportPoint(transform.position);

        float m = 0.05f;

        return v.x > m && v.x < 1 - m &&
               v.y > m && v.y < 1 - m;
    }

    // =========================
    // 画面外判定
    // =========================
    bool IsOutsideScreen()
    {
        Camera cam = Camera.main;
        Vector3 v = cam.WorldToViewportPoint(transform.position);

        return v.x < -0.1f || v.x > 1.1f ||
               v.y < -0.1f || v.y > 1.1f;
    }

    // =========================
    // 画面内維持
    // =========================
    void StayInScreen()
    {
        Camera cam = Camera.main;
        Vector3 v = cam.WorldToViewportPoint(transform.position);

        float m = 0.05f;

        if (v.x < m || v.x > 1 - m)
            direction.x *= -1;

        if (v.y < m || v.y > 1 - m)
            direction.y *= -1;
    }
}