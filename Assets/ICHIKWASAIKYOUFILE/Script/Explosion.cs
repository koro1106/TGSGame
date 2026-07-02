using UnityEngine;

public class Bulletxplosion : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 5f;

    [SerializeField] private int damage;

    private Vector2 direction;

    public GameObject ammoDropPrefab;
    public Sprite ammoUISprite;

    [Header("Hit Effect")]
    public ParticleSystem hitEffectPrefab;

    // ▼追加
    [Header("Explosion Size")]
    public float explosionSize = 3f;
    public float totalExplosionSize = 0f;

    public PlayerStats playerStats;
    private Vector3 defaultScale = new Vector3(210.7f, 95.8f, 144.1f);

    void Start()
    {
        transform.localScale = defaultScale + Vector3.one * playerStats.bulletSize;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(
            direction * speed * Time.deltaTime,
            Space.World
        );
    }

    // 発射方向設定
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    //========================
    // 当たり判定
    //========================

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHP enemy =
            other.GetComponent<EnemyHP>();

        if (enemy == null)
            return;

        // ダメージ
        enemy.TakeDamage(damage);

        Debug.Log(
            enemy.name +
            " に " +
            damage +
            " ダメージ"
        );

        //========================
        // エフェクト生成
        //========================

        if (hitEffectPrefab != null)
        {
            ParticleSystem effect =
                Instantiate(
                    hitEffectPrefab,
                    transform.position,
                    Quaternion.identity
                );

            // 見た目サイズ変更
            totalExplosionSize = explosionSize + playerStats.explosionRangeUP;

            effect.transform.localScale =
                Vector3.one * totalExplosionSize;
        }

        // 弾消滅
        Destroy(gameObject);
    }

    //========================
    // ダメージ設定
    //========================

    public void SetDamage(int value)
    {
        damage = value;
    }
}