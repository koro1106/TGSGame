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

    public float totalPoisonRadius = 0f;

    public int poisonDamage = 3;

    public float poisonInterval = 1f;

    public float poisonDuration = 5f;

    // 実際に生成した毒エフェクト
    private GameObject poisonEffect;

    // =========================
    // 毒エフェクト
    // =========================

    [Header("毒エフェクト")]

    public GameObject poisonEffectPrefab;

    public float effectSize = 1f;

    // 着弾済み判定
    private bool exploded = false;

    public PlayerStats stats;

    private Vector3 defaultScale =
        new Vector3(210.7f, 95.8f, 144.1f);

    // =========================
    // 開始
    // =========================

    private void Start()
    {
        transform.localScale =
            defaultScale +
            Vector3.one * stats.bulletSize;
    }

    // =========================
    // 敵に当たった
    // =========================

    protected new void OnTriggerEnter2D(
        Collider2D other)
    {
        // 多重発動防止
        if (exploded)
            return;

        // ダメージ
        int totalDamage =(hitDamage +stats.bulletDamage + stats.effectBulletDamage);

        // ========================
        // ケアパッケージ
        // ========================

        if (other.CompareTag("CarePackage"))
        {
            CarePackage package =
                other.GetComponent<CarePackage>();

            if (package != null)
            {
                package.TakeDamage(totalDamage);
            }

            exploded = true;

            Destroy(gameObject);

            return;
        }

        // ========================
        // EnemyHP取得
        // ========================

        EnemyHP enemy =
            other.GetComponent<EnemyHP>();

        if (enemy == null)
            return;

        // ========================
        // 着弾ダメージ
        // ========================

        enemy.TakeDamage(totalDamage);

        // ========================
        // 毒エリア開始
        // ========================

        exploded = true;

        StartCoroutine(
            PoisonArea()
        );
    }

    // =========================
    // 毒エリア
    // =========================

    IEnumerator PoisonArea()
    {
        // =========================
        // Rigidbody取得
        // =========================

        Rigidbody2D rb =
            GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.simulated = false;
        }

        // =========================
        // Sprite消す
        // =========================

        SpriteRenderer sr =
            GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.enabled = false;
        }

        // =========================
        // Collider消す
        // =========================

        Collider2D col =
            GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }

        // =========================
        // 毒エフェクト生成
        // =========================

        if (poisonEffectPrefab != null)
        {
            poisonEffect =
                Instantiate(
                    poisonEffectPrefab,
                    transform.position,
                    Quaternion.identity
                );

            poisonEffect.transform.localScale =
                Vector3.one * effectSize;
        }

        // =========================
        // 毒範囲
        // =========================

        totalPoisonRadius =
            poisonRadius +
            stats.poisonRangeUP;

        Debug.Log(
            "合計" +
            totalPoisonRadius
        );

        // =========================
        // 毒継続
        // =========================

        float timer = 0f;

        while (timer < poisonDuration)
        {
            Collider2D[] hits =
                Physics2D.OverlapCircleAll(
                    transform.position,
                    totalPoisonRadius
                );

            foreach (Collider2D hit in hits)
            {
                EnemyHP enemy =
                    hit.GetComponent<EnemyHP>();

                if (enemy == null)
                    continue;

                enemy.TakeDamage(
                    poisonDamage
                );
            }

            yield return new WaitForSeconds(
                poisonInterval
            );

            timer += poisonInterval;
        }

        // =========================
        // 毒終了
        // =========================

        ClearEffect();

        // =========================
        // 毒弾本体削除
        // =========================

        Destroy(gameObject);
    }

    // =========================
    // リザルト時などに
    // 毒エフェクトを強制削除
    // =========================

    public void ClearEffect()
    {
        if (poisonEffect != null)
        {
            Destroy(poisonEffect);
            poisonEffect = null;
        }
        Destroy(gameObject);
    }

    // =========================
    // 範囲表示
    // =========================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        if (stats != null)
        {
            totalPoisonRadius =
                poisonRadius +
                stats.poisonRangeUP;
        }
        else
        {
            totalPoisonRadius =
                poisonRadius;
        }

        Gizmos.DrawWireSphere(
            transform.position,
            totalPoisonRadius
        );
    }
}