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

    // =========================
    // 実際に生成した毒エフェクト
    // =========================

    private GameObject poisonEffect;

    // =========================
    // 毒エフェクト
    // =========================

    [Header("毒エフェクト")]

    public GameObject poisonEffectPrefab;

    // エフェクトの大きさ
    public float effectSize = 2f;

    // =========================
    // エフェクト位置調整
    // =========================

    [Header("エフェクト位置調整")]

    // 敵に当たった位置からの調整
    public Vector3 effectOffset =
        new Vector3(0f, -0.5f, 0f);

    // =========================
    // エフェクト速度
    // =========================

    [Header("エフェクトアニメーション速度")]

    // 1 = 通常
    // 0.5 = 半分の速度
    // 0.25 = かなりゆっくり
    public float effectSimulationSpeed = 0.5f;

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
        int totalDamage =
            hitDamage +
            stats.bulletDamage +
            stats.effectBulletDamage;

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
            PoisonArea(other)
        );
    }

    // =========================
    // 毒エリア
    // =========================

    IEnumerator PoisonArea(Collider2D hitCollider)
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
            // 当たったObjectの中心位置
            Vector3 effectPosition =
                hitCollider.bounds.center +
                effectOffset;

            poisonEffect =
                Instantiate(
                    poisonEffectPrefab,
                    effectPosition,
                    Quaternion.identity
                );

            // エフェクトを大きくする
            poisonEffect.transform.localScale =
                Vector3.one * effectSize;

            // エフェクトをゆっくり再生
            SetEffectSpeed(poisonEffect);
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
    // エフェクト再生速度変更
    // =========================

    void SetEffectSpeed(GameObject effectObject)
    {
        if (effectObject == null)
            return;

        ParticleSystem[] particles =
            effectObject.GetComponentsInChildren<ParticleSystem>(
                true
            );

        foreach (ParticleSystem particle in particles)
        {
            var main =
                particle.main;

            main.simulationSpeed =
                effectSimulationSpeed;
        }
    }

    // =========================
    // リザルト時などに
    // 毒エフェクトを強制削除
    // =========================

    public void ClearEffect()
    {
        // 毒エフェクト削除
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