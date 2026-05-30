using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f; // 弾速
    public float lifeTime = 5f; // 5秒後に消える

    [SerializeField] private int damage; // ダメージ

    private Vector2 direction; // 発射方向

    public GameObject ammoDropPrefab;
    public Sprite ammoUISprite;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 弾を移動
        transform.Translate(
            direction * speed * Time.deltaTime,
            Space.World
        );
    }

    // Playerから呼ばれる
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                Debug.Log("敵に " + damage + " ダメージ");
            }

            Destroy(gameObject);
        }
    }

    // ダメージ設定
    public void SetDamage(int value)
    {
        damage = value;
    }
}