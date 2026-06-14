using UnityEngine;
using System.Collections;

public class PoisonBullet : Bullet
{
    // =========================
    // 着弾時ダメージ
    // =========================

    [Header("着弾ダメージ")]
    public int hitDamage = 10;

    // =========================
    // 毒設定
    // =========================

    [Header("毒設定")]

    public float poisonRadius = 3f;

    public int poisonDamage = 3;

    public float poisonInterval = 1f;

    public float poisonDuration = 5f;

    // =========================
    // 毒エフェクト
    // =========================

    [Header("毒エフェクト")]

    public GameObject poisonEffectPrefab;

    public float effectSize = 1f;

    // 着弾済み判定
    private bool exploded = false;

    public PlayerStats stats;

    // =========================
    // 敵に当たった
    // =========================

    protected new void OnTriggerEnter2D(Collider2D other)
    {
        // 多重発動防止
        if (exploded)
            return;

        // EnemyHP取得
        EnemyHP enemy =
            other.GetComponent<EnemyHP>();

        // EnemyHP無ければ無視
        if (enemy == null)
            return;

        int totalDamage =
            hitDamage +
            stats.effectBulletDamage;

        // 着弾ダメージ
        enemy.TakeDamage(totalDamage);

        // 毒エリア開始
        StartCoroutine(PoisonArea());

        exploded = true;
    }

    // =========================
    // 毒エリア
    // =========================

    IEnumerator PoisonArea()
    {
        // Rigidbody取得
        Rigidbody2D rb =
            GetComponent<Rigidbody2D>();

        // 停止
        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.simulated = false;
        }

        // Sprite消す
        SpriteRenderer sr =
            GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.enabled = false;
        }

        // Collider消す
        Collider2D col =
            GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }

        // =========================
        // 毒エフェクト
        // =========================

        GameObject effect = null;

        if (poisonEffectPrefab != null)
        {
            effect = Instantiate(
                poisonEffectPrefab,
                transform.position,
                Quaternion.identity
            );

            effect.transform.localScale =
                Vector3.one *
                effectSize;
        }

        // =========================
        // 毒継続
        // =========================

        float timer = 0f;

        while (timer < poisonDuration)
        {
            Collider2D[] hits =
                Physics2D.OverlapCircleAll(
                    transform.position,
                    poisonRadius
                );

            foreach (Collider2D hit in hits)
            {
                EnemyHP enemy =
                    hit.GetComponent<EnemyHP>();

                if (enemy == null)
                    continue;

                enemy.TakeDamage(poisonDamage);
            }

            yield return new WaitForSeconds(
                poisonInterval
            );

            timer += poisonInterval;
        }

        // 毒終了
        Destroy(gameObject);
    }

    // =========================
    // 範囲表示
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