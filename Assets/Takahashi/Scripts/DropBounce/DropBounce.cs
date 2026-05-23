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
    /// 1回のバウンド時間
    /// </summary>
    public float duration = 0.6f;

    /// <summary>
    /// バウンド回数
    /// </summary>
    public int bounceCount = 2;

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
    // 影設定
    // =========================================================

    [Header("影Prefab")]

    /// <summary>
    /// 影Prefab
    /// SpriteRenderer付きの黒丸を設定
    /// </summary>
    [SerializeField]
    private GameObject shadowPrefab;

    /// <summary>
    /// 生成した影
    /// </summary>
    private Transform shadow;

    /// <summary>
    /// 影のSpriteRenderer
    /// 透明度変更用
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
    /// 1 or -1
    /// </summary>
    private float rotateDir;

    /// <summary>
    /// バウンド終了フラグ
    /// </summary>
    private bool finished;

    // =========================================================
    // 初期化
    // =========================================================

    /// <summary>
    /// 初期化処理
    /// </summary>
    void Start()
    {
        // 初期位置保存
        startPos = transform.position;

        // 元Scale保存
        baseScale = transform.localScale;

        // ランダム方向
        moveDir = Random.insideUnitCircle.normalized;

        // 上方向へ飛ばす
        moveDir.y = Mathf.Abs(moveDir.y);

        // ランダム回転速度
        rotateSpeed =
            Random.Range(minRotateSpeed, maxRotateSpeed);

        // 回転方向ランダム
        rotateDir =
            Random.value < 0.5f ? -1f : 1f;

        // =====================================================
        // 影生成
        // =====================================================

        if (shadowPrefab != null)
        {
            // 影生成
            GameObject s =
                Instantiate(shadowPrefab);

            shadow = s.transform;

            // SpriteRenderer取得
            shadowRenderer =
                s.GetComponent<SpriteRenderer>();

            // 初期位置
            shadow.position = new Vector3(
                transform.position.x,
                startPos.y - 0.1f,
                0
            );

            // 初期透明度
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

    /// <summary>
    /// 毎フレーム更新
    /// </summary>
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

        transform.Rotate(
            0,
            0,
            rotateSpeed *
            rotateDir *
            Time.deltaTime
        );

        // =====================================================
        // 影更新
        // =====================================================

        UpdateShadow(height);
    }

    // =========================================================
    // 影更新
    // =========================================================

    /// <summary>
    /// 影を更新する
    /// </summary>
    /// <param name="height">
    /// 現在のジャンプ高さ
    /// </param>
    void UpdateShadow(float height)
    {
        if (shadow == null) return;

        // =====================================================
        // 影位置
        // =====================================================

        shadow.position = new Vector3(
            transform.position.x,
            startPos.y - 30f,
            0
        );

        // =====================================================
        // 高さ割合
        // =====================================================

        float t =
            Mathf.Clamp01(
                height / bounceHeight
            );

        // =====================================================
        // 透明度変更
        // 高いほど薄くする
        // =====================================================

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

        // =====================================================
        // 回転固定
        // =====================================================

        shadow.rotation =
            Quaternion.identity;
    }

    // =========================================================
    // 着地後影固定
    // =========================================================

    /// <summary>
    /// 着地後の影固定
    /// </summary>
    void FixShadow()
    {
        if (shadow == null) return;

        shadow.position = new Vector3(
            transform.position.x,
            startPos.y - 30f,
            0
        );

        shadow.rotation =
            Quaternion.identity;

        // 着地後は少し濃く
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

    /// <summary>
    /// オブジェクト削除時
    /// 影も一緒に削除
    /// </summary>
    void OnDestroy()
    {
        if (shadow != null)
        {
            Destroy(shadow.gameObject);
        }
    }
}