using UnityEngine;

public class penetratingbullet : MonoBehaviour
{
    public float lifeTime = 5f; // 5秒後に消える
    [SerializeField] private int damage;

    private Vector2 direction;
    public PlayerStats stats; // プレイヤーステータス

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Playerから呼ばれる
    public void SetDirection(Vector2 dir)
    {
        direction = dir;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            int totalDamage = damage + stats.effectBulletDamage;

            if (enemy != null)
            {
                enemy.TakeDamage(totalDamage);
            }

            // ↓ 消さないので貫通する
            // Destroy(gameObject);
        }
    }

    // ダメージ設定
    public void SetDamage(int value)
    {
        damage = value;
        Debug.Log("ダメージ数 " + damage);
    }
}