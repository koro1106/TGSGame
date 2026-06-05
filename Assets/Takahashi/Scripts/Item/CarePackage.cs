using UnityEngine;

/// <summary>
/// ケアパッケージ本体
/// ・上から降ってくる
/// ・着地後に攻撃可能
/// ・HPが0になるとアイテムをドロップ
/// </summary>
public class CarePackage : MonoBehaviour
{
    // =====================================================
    // HP設定
    // =====================================================

    [Header("HP")]
    [SerializeField] private int maxHP = 50;

    /// <summary>
    /// 現在HP
    /// </summary>
    private int currentHP;

    // =====================================================
    // 落下設定
    // =====================================================

    [Header("落下")]
    [SerializeField] private float fallSpeed = 5f;

    /// <summary>
    /// 地面のY座標
    /// </summary>
    [SerializeField] private float groundY = 0f;

    /// <summary>
    /// 着地済みか
    /// </summary>
    private bool landed;

    // =====================================================
    // ドロップ設定
    // =====================================================

    [Header("ドロップアイテム")]
    [SerializeField] private GameObject itemPrefab;

    [Header("ドロップ個数")]
    [SerializeField] private int minDrop = 3;

    [SerializeField] private int maxDrop = 6;

    // =====================================================
    // 影設定
    // =====================================================

    [Header("影Prefab")]
    [SerializeField] private GameObject shadowPrefab;

    [Header("影サイズ")]
    [SerializeField]
    public float shadowMaxScale = 8f; // 着地時

    [SerializeField]
    public float shadowMinScale = 2f; // 上空

    /// <summary>
    /// 生成した影
    /// </summary>
    private Transform shadow;

    /// <summary>
    /// 影のSpriteRenderer
    /// </summary>
    private SpriteRenderer shadowRenderer;

    // =====================================================
    // 初期化
    // =====================================================

    private void Start()
    {
        // HP初期化
        currentHP = maxHP;

        // 影生成
        CreateShadow();
    }

    // =====================================================
    // 更新
    // =====================================================

    private void Update()
    {
        // 着地前なら落下
        if (!landed)
        {
            Fall();
        }

        // 影更新
        UpdateShadow();
    }

    public void SetGroundY(float y)
    {
        groundY = y;
    }

    // =====================================================
    // 落下処理
    // =====================================================

    private void Fall()
    {
        // 下方向へ移動
        transform.position +=
            Vector3.down *
            fallSpeed *
            Time.deltaTime;

        // 地面到達
        if (transform.position.y <= groundY)
        {
            landed = true;

            transform.position =
                new Vector3(
                    transform.position.x,
                    groundY,
                    0
                );
        }
    }

    // =====================================================
    // ダメージ
    // =====================================================

    public void TakeDamage(int damage)
    {
        // 着地前は無敵
        if (!landed)
            return;

        currentHP -= damage;

        Debug.Log("箱HP : " + currentHP);

        if (currentHP <= 0)
        {
            BreakBox();
        }
    }

    // =====================================================
    // 箱破壊
    // =====================================================

    private void BreakBox()
    {
        // ランダム個数
        int dropCount =
            Random.Range(
                minDrop,
                maxDrop + 1
            );

        // アイテム生成
        for (int i = 0; i < dropCount; i++)
        {
            Instantiate(
                itemPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        // 箱削除
        Destroy(gameObject);
    }

    // =====================================================
    // 影生成
    // =====================================================

    private void CreateShadow()
    {
        if (shadowPrefab == null)
            return;

        GameObject obj =
            Instantiate(shadowPrefab);

        shadow = obj.transform;

        shadowRenderer =
            obj.GetComponent<SpriteRenderer>();

        // 初期透明度
        if (shadowRenderer != null)
        {
            Color color =
                shadowRenderer.color;

            color.a = 0.5f;

            shadowRenderer.color = color;
        }
    }

    // =====================================================
    // 影更新
    // =====================================================

    private void UpdateShadow()
    {
        if (shadow == null)
            return;

        // -------------------------------------
        // 影は地面に固定
        // -------------------------------------

        shadow.position =
            new Vector3(
                transform.position.x,
                groundY - 60f,
                0f
            );

        // -------------------------------------
        // 現在の高さ
        // -------------------------------------

        float height =
            Mathf.Max(
                transform.position.y - groundY,
                0f
            );

        // -------------------------------------
        // 0～1に変換
        // 550が最高高度
        // -------------------------------------

        float t =
            Mathf.Clamp01(
                height / 550f
            );

        // -------------------------------------
        // 影サイズ
        //
        // 地面付近 → 大
        // 上空     → 小
        // -------------------------------------

        float scale =
            Mathf.Lerp(
                10f,   // 着地時
                2f, // 上空
                t
            );

        // 楕円影
        shadow.localScale =
            new Vector3(
                scale,
                scale * 0.6f,
                1f
            );

        // -------------------------------------
        // 透明度
        //
        // 高いほど薄く
        // -------------------------------------

        if (shadowRenderer != null)
        {
            Color color =
                shadowRenderer.color;

            color.a =
                Mathf.Lerp(
                    0.5f, // 着地時
                    0.1f, // 上空
                    t
                );

            shadowRenderer.color = color;
        }

        // -------------------------------------
        // 回転させない
        // -------------------------------------

        shadow.rotation =
            Quaternion.identity;
    }

    // =====================================================
    // 削除時
    // =====================================================

    private void OnDestroy()
    {
        if (shadow != null)
        {
            Destroy(shadow.gameObject);
        }
    }
}