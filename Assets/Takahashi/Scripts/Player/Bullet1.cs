using UnityEngine;

public class Bullet1 : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    private int damage = 20; // ダメージ量
    private Vector2 direction;

    // Playerから呼ばれる
    public void SetDirection(Vector2 dir)
    {
        direction = dir;
    }

    void Update()
    {
        // 指定方向に移動
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // 画面外で削除
        if (Mathf.Abs(transform.position.x) > 1100f || Mathf.Abs(transform.position.y) > 600f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Enemyスクリプト取得
            EnemyHP enemy = other.GetComponent<EnemyHP>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage); // ダメージを与える
            }

            Destroy(gameObject); // 弾は消える
        }
    }
}
