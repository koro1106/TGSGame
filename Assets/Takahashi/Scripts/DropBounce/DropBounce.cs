using UnityEngine;

/// <summary>
/// ドロップアイテムを
/// ・ランダム方向へ飛ばす
/// ・バウンドさせる
/// ・回転させる
/// ・地面影を表示する
/// スクリプト
/// </summary>
public class DropBounce : MonoBehaviour
{
    // =========================================================
    // 移動設定
    // =========================================================

    [Header("移動")]

    /// <summary>
    /// 横移動速度
    /// </summary>
    public float moveSpeed = 2f;

    /// <summary>
    /// バウンドの高さ
    /// </summary>
    public float bounceHeight = 1.5f;

    /// <summary>
    /// バウンド回数
    /// </summary>
    public int bounceCount = 2;

    // =========================================================
    // 着地時間ランダム
    // =========================================================

    [Header("着地時間ランダム")]

    /// <summary>
    /// 最小着地時間
    /// </summary>
    public float minDuration = 0.3f;

    /// <summary>
    /// 最大着地時間
    /// </summary>
    public float maxDuration = 1.2f;

    /// <summary>
    /// 現在の着地時間
    /// </summary>
    private float duration;

    // =========================================================
    // 散らばり
    // =========================================================

    [Header("散らばり")]

    /// <summary>
    /// 横方向の散らばり
    /// 0～150
    /// </summary>
    [Range(0f, 150f)]
    public float horizontalSpread = 30f;

    // =========================================================
    // 回転設定
    // =========================================================

    [Header("回転")]

    /// <summary>
    /// 最小回転速度
    /// </summary>
    public float minRotateSpeed = 90f;

    /// <summary>
    /// 最大回転速度
    /// </summary>
    public float maxRotateSpeed = 360f;

    // =========================================================
    // 回転時間設定
    // =========================================================

    [Header("回転時間")]

    /// <summary>
    /// 最小回転時間
    /// </summary>
    public float minRotateTime = 0.3f;

    /// <summary>
    /// 最大回転時間
    /// </summary>
    public float maxRotateTime = 2f;

    // =========================================================
    // 影設定
    // =========================================================

    [Header("影Prefab")]

    /// <summary>
    /// 影Prefab
    /// </summary>
    [SerializeField]
    private GameObject shadowPrefab;

    /// <summary>
    /// 生成した影
    /// </summary>
    private Transform shadow;

    /// <summary>
    /// 影のSpriteRenderer
    /// </summary>
    private SpriteRenderer shadowRenderer;

    // =========================================================
    // 内部変数
    // =========================================================

    /// <summary>
    /// 開始位置
    /// </summary>
    private Vector3 startPos;

    /// <summary>
    /// 元のScale
    /// </summary>
    private Vector3 baseScale;

    /// <summary>
    /// 移動方向
    /// </summary>
    private Vector2 moveDir;

    /// <summary>
    /// バウンドタイマー
    /// </summary>
    private float timer;

    /// <summary>
    /// 現在のバウンド回数
    /// </summary>
    private int bounceIndex;

    /// <summary>
    /// 回転速度
    /// </summary>
    private float rotateSpeed;

    /// <summary>
    /// 回転方向
    /// </summary>
    private float rotateDir;

    /// <summary>
    /// 回転時間
    /// </summary>
    private float rotateTime;

    /// <summary>
    /// 回転タイマー
    /// </summary>
    private float rotateTimer;

    /// <summary>
    /// バウンド終了
    /// </summary>
    private bool finished;

    // =========================================================
    // 初期化
    // =========================================================

    void Start()
    {
        // 初期位置保存
        startPos = transform.position;

        // 元Scale保存
        baseScale = transform.localScale;

        // =====================================================
        // 散らばり設定
        // =====================================================

        // 0～150 を 0～1.5 に変換
        float spread =
            horizontalSpread / 100f;

        // 横方向ランダム
        float randomX =
            UnityEngine.Random.Range(
                -spread,
                spread
            );

        // 上方向へ飛ばす
        moveDir =
            new Vector2(randomX, 1f).normalized;

        // =====================================================
        // ランダム回転速度
        // =====================================================

        rotateSpeed =
            UnityEngine.Random.Range(
                minRotateSpeed,
                maxRotateSpeed
            );

        // =====================================================
        // 回転方向ランダム
        // =====================================================

        rotateDir =
            UnityEngine.Random.Range(-2f, 2f);

        // ほぼ停止防止
        if (Mathf.Abs(rotateDir) < 0.3f)
        {
            rotateDir =
                Mathf.Sign(rotateDir) * 0.3f;
        }

        // =====================================================
        // ランダム回転時間
        // =====================================================

        rotateTime =
            UnityEngine.Random.Range(
                minRotateTime,
                maxRotateTime
            );

        // =====================================================
        // ランダム着地時間
        // =====================================================

        duration =
            UnityEngine.Random.Range(
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
    // 更新処理
    // =========================================================

    void Update()
    {
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
                transform.position = new Vector3(
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

        transform.position = new Vector3(
            transform.position.x,
            startPos.y + height,
            0
        );

        // =====================================================
        // 回転
        // =====================================================

        rotateTimer += Time.deltaTime;

        // 回転時間内だけ回す
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