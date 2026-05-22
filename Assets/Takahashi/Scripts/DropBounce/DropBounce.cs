using UnityEngine;

public class DropBounce : MonoBehaviour
{
    [Header("移動")]
    public float moveSpeed = 2f;
    public float bounceHeight = 1.5f;
    public float duration = 0.6f;
    public int bounceCount = 2;

    [Header("回転")]
    public float minRotateSpeed = 90f;
    public float maxRotateSpeed = 360f;

    [Header("影（Quad + テクスチャ）")]
    [SerializeField] Transform shadow;

    private Vector3 startPos;
    private Vector2 moveDir;
    private Vector3 baseScale;

    private float timer;
    private int bounceIndex;

    private float rotateSpeed;
    private float rotateDir;

    private bool finished;

    void Start()
    {
        startPos = transform.position;
        baseScale = transform.localScale;

        moveDir = Random.insideUnitCircle.normalized;
        moveDir.y = Mathf.Abs(moveDir.y);

        rotateSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);
        rotateDir = Random.value < 0.5f ? -1f : 1f;
    }

    void Update()
    {
        if (finished)
        {
            FixShadow();
            return;
        }

        timer += Time.deltaTime;
        float t = timer / duration;

        // =====================
        // バウンド切り替え
        // =====================
        if (t > 1f)
        {
            bounceIndex++;
            timer = 0f;

            bounceHeight *= 0.5f;

            if (bounceIndex >= bounceCount)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    startPos.y,
                    transform.position.z
                );

                transform.localScale = baseScale;

                finished = true;
                return;
            }
        }

        // =====================
        // 横移動
        // =====================
        transform.position += (Vector3)(moveDir * moveSpeed * Time.deltaTime);

        // =====================
        // バウンド（上下）
        // =====================
        float y = Mathf.Sin(t * Mathf.PI) * bounceHeight;

        transform.position = new Vector3(
            transform.position.x,
            startPos.y + y,
            transform.position.z
        );

        // =====================
        // 回転
        // =====================
        transform.Rotate(0, 0, rotateSpeed * rotateDir * Time.deltaTime);

        // =====================
        // 影（テクスチャQuad）
        // =====================
        UpdateShadow(y);
    }

    void UpdateShadow(float height)
    {
        if (shadow == null) return;

        float t = Mathf.Clamp01(height / bounceHeight);

        // 高いほど小さく＆薄く見える
        float scale = Mathf.Lerp(1f, 0.4f, t);

        shadow.position = new Vector3(
            transform.position.x,
            startPos.y,
            transform.position.z
        );

        // QuadなのでXYじゃなくXZで潰す
        shadow.localScale = new Vector3(scale, 1f, scale);
    }

    void FixShadow()
    {
        if (shadow == null) return;

        shadow.position = new Vector3(
            transform.position.x,
            startPos.y,
            transform.position.z
        );

        shadow.localScale = new Vector3(0.7f, 1f, 0.7f);
    }
}