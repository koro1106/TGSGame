using UnityEngine;

/// <summary>
/// ドロップアイテムを
/// ・ランダム方向へ飛ばす
/// ・バウンドさせる
/// ・回転させる
/// ・クロスヘアで回収する
/// スクリプト
/// </summary>
public class DropBounce : MonoBehaviour
{
    // =========================================================
    // Exp設定
    // =========================================================

    [Header("経験値")]

    public PlayerData playerData;

    public int addExp1 = 1;
    public int addExp2 = 0;
    public int addExp3 = 0;

    // =========================================================
    // 回収設定
    // =========================================================

    [Header("回収")]

    public float collectDistance = 1f;

    private RectTransform crosshair;

    private Camera cam;

    [Header("回収演出")]

    public float collectMoveSpeed = 15f;

    private bool isCollecting;

    // =========================================================
    // 移動設定
    // =========================================================

    [Header("移動")]

    public float moveSpeed = 2f;

    public float bounceHeight = 1.5f;

    public int bounceCount = 2;

    // =========================================================
    // 着地時間ランダム
    // =========================================================

    [Header("着地時間ランダム")]

    public float minDuration = 0.3f;

    public float maxDuration = 1.2f;

    private float duration;

    // =========================================================
    // 散らばり
    // =========================================================

    [Header("散らばり")]

    [Range(0f, 150f)]
    public float horizontalSpread = 30f;

    // =========================================================
    // 回転設定
    // =========================================================

    [Header("回転")]

    public float minRotateSpeed = 90f;

    public float maxRotateSpeed = 360f;

    // =========================================================
    // 回転時間設定
    // =========================================================

    [Header("回転時間")]

    public float minRotateTime = 0.3f;

    public float maxRotateTime = 2f;

    // =========================================================
    // 影設定
    // =========================================================

    [Header("影Prefab")]

    [SerializeField]
    private GameObject shadowPrefab;

    private Transform shadow;

    private SpriteRenderer shadowRenderer;

    // =========================================================
    // 内部変数
    // =========================================================

    private Vector3 startPos;

    private Vector3 baseScale;

    private Vector2 moveDir;

    private float timer;

    private int bounceIndex;

    private float rotateSpeed;

    private float rotateDir;

    private float rotateTime;

    private float rotateTimer;

    private bool finished;

    // =========================================================
    // Start
    // =========================================================

    void Start()
    {
        // カメラ取得
        cam = Camera.main;

        // GunController取得
        GunController gun =
            FindObjectOfType<GunController>();

        // クロスヘア取得
        if (gun != null)
        {
            crosshair = gun.crosshair;
        }

        // 初期位置保存
        startPos = transform.position;

        // 元Scale保存
        baseScale = transform.localScale;

        // =====================================================
        // 散らばり設定
        // =====================================================

        float spread =
            horizontalSpread / 100f;

        float randomX =
            Random.Range(
                -spread,
                spread
            );

        moveDir =
            new Vector2(randomX, 1f).normalized;

        // =====================================================
        // 回転設定
        // =====================================================

        rotateSpeed =
            Random.Range(
                minRotateSpeed,
                maxRotateSpeed
            );

        rotateDir =
            Random.Range(-2f, 2f);

        // ほぼ停止防止
        if (Mathf.Abs(rotateDir) < 0.3f)
        {
            rotateDir =
                Mathf.Sign(rotateDir) * 0.3f;
        }

        rotateTime =
            Random.Range(
                minRotateTime,
                maxRotateTime
            );

        // =====================================================
        // 着地時間ランダム
        // =====================================================

        duration =
            Random.Range(
                minDuration,
                maxDuration
            );

        // =====================================================
        // 影生成
        // =====================================================

        if (shadowPrefab != null)
        {
            GameObject s =
                Instantiate(shadowPrefab);

            shadow = s.transform;

            shadowRenderer =
                s.GetComponent<SpriteRenderer>();

            shadow.position = new Vector3(
                transform.position.x,
                startPos.y - 0.1f,
                0
            );

            if (shadowRenderer != null)
            {
                Color color =
                    shadowRenderer.color;

                color.a = 0.5f;

                shadowRenderer.color = color;
            }
        }
    }

    // =========================================================
    // Update
    // =========================================================

    void Update()
    {
        // 回収中
        if (isCollecting)
        {
            MoveCollect();
            return;
        }

        // 回収判定
        CheckCollect();

        // バウンド終了後
        if (finished)
        {
            FixShadow();
            return;
        }

        // =====================================================
        // タイマー更新
        // =====================================================

        timer += Time.deltaTime;

        float t =
            timer / duration;

        // =====================================================
        // バウンド終了判定
        // =====================================================

        if (t > 1f)
        {
            bounceIndex++;

            timer = 0f;

            // 跳ねるたび高さ減少
            bounceHeight *= 0.5f;

            // 全バウンド終了
            if (bounceIndex >= bounceCount)
            {
                transform.position =
                    new Vector3(
                        transform.position.x,
                        startPos.y,
                        0
                    );

                transform.localScale =
                    baseScale;

                finished = true;

                return;
            }
        }

        // =====================================================
        // 横移動
        // =====================================================

        transform.position +=
            (Vector3)(
                moveDir *
                moveSpeed *
                Time.deltaTime
            );

        // =====================================================
        // バウンド
        // =====================================================

        float height =
            Mathf.Sin(t * Mathf.PI)
            * bounceHeight;

        transform.position =
            new Vector3(
                transform.position.x,
                startPos.y + height,
                0
            );

        // =====================================================
        // 回転
        // =====================================================

        rotateTimer += Time.deltaTime;

        if (rotateTimer < rotateTime)
        {
            transform.Rotate(
                0,
                0,
                rotateSpeed *
                rotateDir *
                Time.deltaTime
            );
        }

        // =====================================================
        // 影更新
        // =====================================================

        UpdateShadow(height);
    }

    // =========================================================
    // 回収判定
    // =========================================================

    void CheckCollect()
    {
        if (crosshair == null) return;

        Vector3 crosshairWorld =
            cam.ScreenToWorldPoint(
                crosshair.position);

        crosshairWorld.z =
            transform.position.z;

        // 範囲内のCollider取得
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                crosshairWorld,
                collectDistance);

        foreach (Collider2D hit in hits)
        {
            DropBounce drop =
                hit.GetComponent<DropBounce>();

            if (drop != null)
            {
                drop.Collect();
            }
        }
    }

    // =========================================================
    // 回収開始
    // =========================================================

    public void Collect()
    {
        if (isCollecting) return;

        isCollecting = true;
    }

    // =========================================================
    // 回収演出
    // =========================================================

    void MoveCollect()
    {
        // 下方向へ移動
        transform.position +=
            Vector3.down *
            collectMoveSpeed *
            Time.deltaTime;

        // 少し縮小
        transform.localScale =
            Vector3.Lerp(
                transform.localScale,
                Vector3.zero,
                10f * Time.deltaTime
            );

        // 回転
        transform.Rotate(
            0,
            0,
            720f * Time.deltaTime
        );

        // 一定位置で回収完了
        if (transform.position.y < -10f)
        {
            FinishCollect();
        }
    }

    // =========================================================
    // 回収完了
    // =========================================================

    void FinishCollect()
    {
        if (playerData != null)
        {
            playerData.currentExp_1 += addExp1;
            playerData.currentExp_2 += addExp2;
            playerData.currentExp_3 += addExp3;
        }

        Destroy(gameObject);
    }

    // =========================================================
    // 影更新
    // =========================================================

    void UpdateShadow(float height)
    {
        if (shadow == null) return;

        shadow.position = new Vector3(
            transform.position.x,
            startPos.y - 15f,
            0
        );

        float t =
            Mathf.Clamp01(
                height / bounceHeight
            );

        // 高いほど薄くする
        if (shadowRenderer != null)
        {
            Color color =
                shadowRenderer.color;

            color.a =
                Mathf.Lerp(
                    0.5f,
                    0.15f,
                    t
                );

            shadowRenderer.color =
                color;
        }

        shadow.rotation =
            Quaternion.identity;
    }

    // =========================================================
    // 着地後影固定
    // =========================================================

    void FixShadow()
    {
        if (shadow == null) return;

        shadow.position = new Vector3(
            transform.position.x,
            startPos.y - 15f,
            0
        );

        shadow.rotation =
            Quaternion.identity;

        if (shadowRenderer != null)
        {
            Color color =
                shadowRenderer.color;

            color.a = 0.5f;

            shadowRenderer.color =
                color;
        }
    }

    // =========================================================
    // 削除時
    // =========================================================

    void OnDestroy()
    {
        if (shadow != null)
        {
            Destroy(shadow.gameObject);
        }
    }
}