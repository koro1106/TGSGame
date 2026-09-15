using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 5f;

    [SerializeField] private int damage;

    // ★追加：クリティカルかどうか
    private bool isCritical = false;

    private Vector2 direction;

    public GameObject ammoDropPrefab;
    public Sprite ammoUISprite;

    private Vector3 defaultScale = new Vector3(40f, 50f, 144.1f);
    public PlayerStats stats;

    void Start()
    {
        transform.localScale = defaultScale + Vector3.one * stats.bulletSize;

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
        // ========================
        // ケアパッケージ
        // ========================
        if (other.CompareTag("CarePackage"))
        {
            CarePackage package =
                other.GetComponent<CarePackage>();

            if (package != null)
            {
                package.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        // ========================
        // EnemyHP取得
        // ========================
        EnemyHP enemy =
            other.GetComponent<EnemyHP>();

        // EnemyHPが無ければ無視
        if (enemy == null)
            return;

        // ダメージ
        // ★変更：クリティカル情報も一緒に渡す
        enemy.TakeDamage(damage, isCritical);

        // 敵着弾SE
        if (SEManager.Instance != null)
        {
            SEManager.Instance.PlayEnemyHitSE();
        }

        Debug.Log(
            enemy.name +
            " に " +
            damage +
            " ダメージ" +
            (isCritical ? "（クリティカル！）" : "")
        );

        // 弾消滅
        Destroy(gameObject);
    }

    //========================
    // ダメージ設定
    //========================

    // ★変更：クリティカルかどうかも受け取れるように引数追加（省略時はfalse）
    public void SetDamage(int value, bool critical = false)
    {
        damage = value;
        isCritical = critical;
    }
}