using UnityEngine;
using System.Collections;
public class EnemyWarpMove : MonoBehaviour, IHitSlowable
{
    [Header("移動")]
    public float moveSpeed = 3f;
    [Header("ワープ")]
    public float warpInterval = 3f;     // 動いてからワープするまでの時間
    public float warpRange = 6f;
    public float warpAnimTime = 0.3f;   // 消える/出現アニメの時間
    [Header("停止タイミング")]
    public float stopBeforeWarpTime = 0.5f; // ワープする前に止まっている時間
    public float stopAfterWarpTime = 0.5f;  // ワープ後、動き出すまでの待ち時間

    [Header("被弾時の鈍化")]
    public float hitSlowMultiplier = 0.3f; // 鈍化中の速度倍率（1fで鈍化なし、0fで完全停止）
    public float hitSlowDuration = 0.5f;   // 鈍化が続く時間（秒）

    private float slowTimer = 0f;          // 鈍化の残り時間
    private float speedMultiplier = 1f;    // 現在の速度倍率（鈍化中は1未満になる）

    private bool isWarping = false; // true の間は移動しない（停止中もワープ中も含む）
    private Vector3 startScale;

    //========================
    // 拘束
    //========================

    private Coroutine bindCoroutine;
    private bool isBind = false;

    private EnemyHP enemyHP;
    void Start()
    {
        startScale = transform.localScale;
        StartCoroutine(WarpRoutine());
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

        if (isWarping) return;

        // 被弾鈍化の更新
        UpdateHitSlow();

        // 画面中央へ移動
        Vector3 dir = (-transform.position).normalized;
        transform.position += dir * moveSpeed * speedMultiplier * Time.deltaTime;
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

    // 移動→停止→ワープ→停止→移動…のループ全体を管理
    IEnumerator WarpRoutine()
    {
        while (true)
        {
            // ===== 移動 =====
            yield return new WaitForSeconds(warpInterval);
            // ===== 止まる（ワープ前） =====
            isWarping = true;
            yield return new WaitForSeconds(stopBeforeWarpTime);
            // ===== ワープ（消える→移動→出現） =====
            yield return StartCoroutine(Warp());
            // ===== 止まる（ワープ後、少ししてから動き出す） =====
            yield return new WaitForSeconds(stopAfterWarpTime);
            // ===== ここでまた動き出す =====
            isWarping = false;
        }
    }
    // ワープ演出（縮んで消える→座標移動→拡大して出現）
    IEnumerator Warp()
    {
        float timer = 0f;
        // 消える
        while (timer < warpAnimTime)
        {
            timer += Time.deltaTime;
            transform.localScale =
                Vector3.Lerp(startScale, Vector3.zero, timer / warpAnimTime);
            yield return null;
        }
        // ワープ先へ移動
        Vector2 pos = Random.insideUnitCircle.normalized * warpRange;
        transform.position = pos;
        timer = 0f;
        // 出現
        while (timer < warpAnimTime)
        {
            timer += Time.deltaTime;
            transform.localScale =
                Vector3.Lerp(Vector3.zero, startScale, timer / warpAnimTime);
            yield return null;
        }
        transform.localScale = startScale;
    }
}