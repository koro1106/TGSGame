using UnityEngine;
using System.Collections;

public class PoisonBullet : Bullet
{
    // =========================
    // 着弾時のダメージ
    // =========================
    [Header("着弾ダメージ")]
    public int hitDamage = 10;

    // =========================
    // 毒エリア設定
    // =========================
    [Header("毒設定")]

    public float poisonRadius = 3f;
    // 毒の範囲

    public int poisonDamage = 3;
    // 毒の継続ダメージ

    public float poisonInterval = 1f;
    // 何秒ごとにダメージを与えるか

    public float poisonDuration = 5f;
    // 毒エリアが存在する時間

    // =========================
    // 毒エフェクト
    // =========================
    [Header("毒エフェクト")]

    public GameObject poisonEffectPrefab;
    // 毒の見た目用エフェクト

    public float effectSize = 1f;
    // エフェクトの大きさ

    // 既に着弾したか判定
    private bool exploded = false;

    // =========================
    // 敵に当たった時
    // =========================
    protected new void OnTriggerEnter2D(Collider2D other)
    {
        // 既に着弾済みなら処理しない
        if (exploded) return;

        // Enemyタグに当たったか
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            // 着弾ダメージ
            if (enemy != null)
            {
                enemy.TakeDamage(hitDamage);
            }

            // 毒エリア開始
            StartCoroutine(PoisonArea());

            exploded = true;
        }
    }

    // =========================
    // 毒エリア処理
    // =========================
    IEnumerator PoisonArea()
    {
        // Rigidbody取得
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // 弾を停止
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // SpriteRenderer取得
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // 弾画像を消す
        if (sr != null)
        {
            sr.enabled = false;
        }

        // Collider取得
        Collider2D col = GetComponent<Collider2D>();

        // 当たり判定を消す
        if (col != null)
        {
            col.enabled = false;
        }

        // =========================
        // 毒エフェクト生成
        // =========================

        GameObject effect = null;

        if (poisonEffectPrefab != null)
        {
            effect = Instantiate(
                poisonEffectPrefab,
                transform.position,
                Quaternion.identity
            );

            // Inspectorで設定したサイズ適用
            effect.transform.localScale =
                Vector3.one * effectSize;
        }

        // 毒時間カウント
        float timer = 0f;

        // 指定時間まで継続
        while (timer < poisonDuration)
        {
            // 範囲内のCollider取得
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position,
                poisonRadius
            );

            // 範囲内の敵へダメージ
            foreach (Collider2D hit in hits)
            {
                // Enemyタグのみ
                if (hit.CompareTag("Enemy"))
                {
                    Enemy enemy = hit.GetComponent<Enemy>();

                    if (enemy != null)
                    {
                        enemy.TakeDamage(poisonDamage);
                    }
                }
            }

            // 次のダメージまで待機
            yield return new WaitForSeconds(poisonInterval);

            // 時間加算
            timer += poisonInterval;
        }

        // 毒終了後削除
        Destroy(gameObject);
    }

    // =========================
    // Sceneビューで毒範囲表示
    // =========================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            poisonRadius
        );
    }
}