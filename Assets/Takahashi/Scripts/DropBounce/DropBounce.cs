using UnityEngine;

public class DropBounce : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 2f;      // 横方向の移動スピード
    public float bounceHeight = 1.5f; // 最初のバウンドの高さ
    public float duration = 0.6f;     // 1回のバウンドにかかる時間

    [Header("バウンド設定")]
    public int bounceCount = 2;       // バウンド回数（2回くらいが自然）

    private Vector3 startPos;         // バウンド開始時の基準位置
    private Vector2 moveDir;          // 横移動の方向
    private float timer;              // 経過時間
    private int currentBounce;        // 現在のバウンド回数

    private Vector3 baseScale;        // 元のサイズ

    void Start()
    {
        // 初期位置を保存（Yの基準になる）
        startPos = transform.position;

        // 元のスケール保存
        baseScale = transform.localScale;

        // ランダムな方向に少し広がる（Xメイン）
        moveDir = Random.insideUnitCircle.normalized;

        // 下に行かないようにYは上方向に補正
        moveDir.y = Mathf.Abs(moveDir.y);
    }

    void Update()
    {
        // 時間経過
        timer += Time.deltaTime;

        // 0〜1に正規化（バウンド1回分の進行度）
        float t = timer / duration;

        // 1回のバウンドが終わったら
        if (t > 1f)
        {
            currentBounce++;
            timer = 0f;

            // バウンドするたびに高さを半分に（減衰）
            bounceHeight *= 0.5f;

            // 指定回数バウンドしたら停止
            if (currentBounce >= bounceCount)
            {
                enabled = false; // Update止める
                return;
            }
        }

        // =========================
        // 横方向の移動
        // =========================
        transform.position += (Vector3)(moveDir * moveSpeed * Time.deltaTime);

        // =========================
        // 縦のバウンド（超重要）
        // Sin波で綺麗なジャンプカーブを作る
        // =========================
        float y = Mathf.Sin(t * Mathf.PI) * bounceHeight;

        transform.position = new Vector3(
            transform.position.x,
            startPos.y + y,
            transform.position.z
        );

        // =========================
        // スカッシュ＆ストレッチ
        // =========================
        // Sinの値を使ってサイズ変化
        float squash = Mathf.Sin(t * Mathf.PI);

        // 空中で縦に伸びる・横に縮む
        float scaleY = 1 + squash * 0.2f;
        float scaleX = 1 - squash * 0.2f;

        transform.localScale = new Vector3(
            baseScale.x * scaleX,
            baseScale.y * scaleY,
            1
        );

        // =========================
        // 回転（ちょっとだけ付けると自然）
        // =========================
        transform.Rotate(0, 0, 180f * Time.deltaTime);
    }
}