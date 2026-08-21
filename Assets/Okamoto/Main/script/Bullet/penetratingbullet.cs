using UnityEngine;

public class penetratingbullet : MonoBehaviour
{
    public float lifeTime = 5f; // 5秒後に消える
    [SerializeField] private int damage;

    private Vector2 direction;
    public PlayerStats stats; // プレイヤーステータス
    private Vector3 defaultScale = new Vector3(210.7f, 95.8f, 144.1f);
    void Start()
    {
        transform.localScale = defaultScale + Vector3.one * stats.bulletSize;

        Destroy(gameObject, lifeTime);
    }

    // Playerから呼ばれる
    public void SetDirection(Vector2 dir)
    {
        direction = dir;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ダメージを最初に1回だけ計算
        int totalDamage =
            damage +
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

            // 貫通弾なので消さない
            return;
        }

        // ========================
        // EnemyHP取得
        // ========================
        EnemyHP enemy =
            other.GetComponent<EnemyHP>();

        if (enemy == null)
            return;

        enemy.TakeDamage(totalDamage);

        Debug.Log(
            enemy.name +
            " に " +
            totalDamage +
            " ダメージ"
        );

        // 貫通なので消さない
    }

    // ダメージ設定
    public void SetDamage(int value)
    {
        damage = value;
        Debug.Log("ダメージ数 " + damage);
    }
}