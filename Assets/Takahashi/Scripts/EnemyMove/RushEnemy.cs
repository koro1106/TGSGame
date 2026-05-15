using UnityEngine;

public class RushEnemy : MonoBehaviour
{
    [Header("プレイヤー")]
    public Transform player;

    [Header("移動")]
    public float moveSpeed = 3f;      // 通常移動速度
    public float rushSpeed = 12f;     // 突進速度
    public float rushDistance = 300f; // これ以下でため開始

    [Header("ため時間")]
    public float chargeTime = 1f;     // ためにかかる時間

    [Header("アニメーション（伸縮）")]
    public float animSpeed = 6f;       // 伸縮の速さ
    public float stretchAmount = 0.2f; // 伸縮の強さ

    private bool charging = false;     // ため中フラグ
    private bool rushing = false;      // 突進中フラグ

    private float chargeTimer = 0f;    // ため時間計測用

    private Vector3 baseScale;         // 元のサイズ保存

    void Start()
    {
        // 初期サイズを保存
        baseScale = transform.localScale;

        // 画面外にスポーン
        SpawnOutsideScreen();
    }

    void Update()
    {
        // プレイヤーがいなければ何もしない
        if (player == null) return;

        // プレイヤー方向ベクトル
        Vector2 dir =
            (player.position - transform.position).normalized;

        // プレイヤーとの距離
        float distance =
            Vector2.Distance(transform.position, player.position);

        // =====================
        // 通常移動 → ため開始
        // =====================
        if (!charging && !rushing)
        {
            // プレイヤーへ向かって移動
            transform.Translate(
                dir * moveSpeed * Time.deltaTime,
                Space.World
            );

            // 距離が一定以下でため開始
            if (distance <= rushDistance)
            {
                charging = true;
                chargeTimer = 0f;
            }
        }

        // =====================
        // ため中処理
        // =====================
        if (charging)
        {
            // 時間加算
            chargeTimer += Time.deltaTime;

            // 百足っぽい揺れ（小さく震える）
            transform.position +=
                (Vector3)Random.insideUnitCircle * 0.01f;

            // ため時間終了で突進へ
            if (chargeTimer >= chargeTime)
            {
                charging = false;
                rushing = true;
            }
        }

        // =====================
        // 突進処理
        // =====================
        if (rushing)
        {
            // プレイヤー方向へ高速移動
            transform.Translate(
                dir * rushSpeed * Time.deltaTime,
                Space.World
            );
        }

        // =====================
        // 伸縮アニメーション（常時）
        // =====================
        float sin = Mathf.Sin(Time.time * animSpeed);

        // 横に伸びて縦に縮む（百足っぽい動き）
        float scaleX = baseScale.x * (1f + sin * stretchAmount);
        float scaleY = baseScale.y * (1f - sin * stretchAmount);

        transform.localScale =
            new Vector3(scaleX, scaleY, 1f);
    }

    void SpawnOutsideScreen()
    {
        // メインカメラ取得
        Camera cam = Camera.main;

        // 画面高さ
        float h = cam.orthographicSize;

        // 画面幅
        float w = h * cam.aspect;

        // ランダムで上下左右どこから出すか決定
        int side = Random.Range(0, 4);

        Vector2 spawnPos = Vector2.zero;

        // 画面外にスポーン位置を設定
        switch (side)
        {
            case 0: // 右
                spawnPos =
                    new Vector2(
                        w + 1,
                        Random.Range(-h, h)
                    );
                break;

            case 1: // 左
                spawnPos =
                    new Vector2(
                        -w - 1,
                        Random.Range(-h, h)
                    );
                break;

            case 2: // 上
                spawnPos =
                    new Vector2(
                        Random.Range(-w, w),
                        h + 1
                    );
                break;

            case 3: // 下
                spawnPos =
                    new Vector2(
                        Random.Range(-w, w),
                        -h - 1
                    );
                break;
        }

        // 位置を設定
        transform.position = spawnPos;
    }
}