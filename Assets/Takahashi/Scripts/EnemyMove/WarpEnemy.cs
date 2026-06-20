using UnityEngine;
using System.Collections;

public class EnemyWarpMove : MonoBehaviour
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

    private bool isWarping = false; // true の間は移動しない（停止中もワープ中も含む）
    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
        StartCoroutine(WarpRoutine());
    }

    void Update()
    {
        if (isWarping) return;

        // 画面中央へ移動
        Vector3 dir = (-transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
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