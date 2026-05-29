using UnityEngine;

/// <summary>
/// ・放射状に飛ぶ
/// ・バウンドするたびに散らばり増加
/// ・回転する
/// ・影あり
/// </summary>
public class DropScatter : MonoBehaviour
{
    // =========================
    // 移動
    // =========================
    [Header("移動")]
    public float moveSpeed = 2f;

    [Header("バウンド")]
    public float bounceHeight = 1.5f;
    public float duration = 0.6f;
    public int bounceCount = 2;

    [Header("散らばり強さ")]
    public float spreadPerBounce = 0.6f;

    // =========================
    // 回転
    // =========================
    [Header("回転")]
    public float minRotateSpeed = 90f;
    public float maxRotateSpeed = 360f;

    // =========================
    // 影
    // =========================
    [Header("影")]
    [SerializeField] private GameObject shadowPrefab;

    private Transform shadow;
    private SpriteRenderer shadowRenderer;

    // =========================
    // 内部
    // =========================
    private Vector3 startPos;
    private Vector3 baseScale;

    private Vector2 moveDir;
    private float timer;
    private int bounceIndex;

    private float rotateSpeed;
    private float rotateDir;

    private bool finished;

    // =========================
    // 初期化
    // =========================
    void Start()
    {
        startPos = transform.position;
        baseScale = transform.localScale;

        // ★中心からランダム放射
        float angle = Random.Range(0f, 360f);

        moveDir = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );

        moveDir += Random.insideUnitCircle * 0.3f;
        moveDir = moveDir.normalized;

        // 回転
        rotateSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);
        rotateDir = Random.value < 0.5f ? -1f : 1f;

        // 影生成
        if (shadowPrefab != null)
        {
            GameObject s = Instantiate(shadowPrefab);
            shadow = s.transform;

            shadowRenderer = s.GetComponent<SpriteRenderer>();

            if (shadowRenderer != null)
            {
                Color c = shadowRenderer.color;
                c.a = 0.5f;
                shadowRenderer.color = c;
            }
        }
    }

    // =========================
    // 更新
    // =========================
    void Update()
    {
        if (finished)
        {
            FixShadow();
            return;
        }

        timer += Time.deltaTime;
        float t = timer / duration;

        // =========================
        // バウンド処理
        // =========================
        if (t > 1f)
        {
            bounceIndex++;
            timer = 0f;

            // ★バウンドごとに散らばり増加
            Vector2 noise =
                Random.insideUnitCircle * spreadPerBounce * (bounceIndex + 1);

            moveDir = (moveDir + noise).normalized;

            // 少し速度変化（自然さ）
            moveSpeed *= Random.Range(0.9f, 1.1f);

            // バウンドごとに弱くなる
            bounceHeight *= 0.5f;

            if (bounceIndex >= bounceCount)
            {
                finished = true;
                return;
            }
        }

        // =========================
        // 移動
        // =========================
        transform.position += (Vector3)(moveDir * moveSpeed * Time.deltaTime);

        // =========================
        // バウンド（高さ）
        // =========================
        float height =
            Mathf.Sin(t * Mathf.PI) * bounceHeight;

        transform.position = new Vector3(
            transform.position.x,
            startPos.y + height,
            0
        );

        // =========================
        // 回転
        // =========================
        transform.Rotate(0, 0,
            rotateSpeed * rotateDir * Time.deltaTime
        );

        UpdateShadow(height);
    }

    // =========================
    // 影更新
    // =========================
    void UpdateShadow(float height)
    {
        if (shadow == null) return;

        shadow.position = new Vector3(
            transform.position.x,
            startPos.y - 0.3f,
            0
        );

        float t = Mathf.Clamp01(height / bounceHeight);

        if (shadowRenderer != null)
        {
            Color c = shadowRenderer.color;
            c.a = Mathf.Lerp(0.5f, 0.15f, t);
            shadowRenderer.color = c;
        }

        shadow.rotation = Quaternion.identity;
    }

    // =========================
    // 着地後固定
    // =========================
    void FixShadow()
    {
        if (shadow == null) return;

        shadow.position = new Vector3(
            transform.position.x,
            startPos.y - 30f,
            0
        );

        shadow.rotation = Quaternion.identity;
    }

    // =========================
    // 削除時
    // =========================
    void OnDestroy()
    {
        if (shadow != null)
            Destroy(shadow.gameObject);
    }
}