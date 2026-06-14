using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 5f;

    [SerializeField] private int damage;

    private Vector2 direction;

    public GameObject ammoDropPrefab;
    public Sprite ammoUISprite;

    void Start()
    {
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
        // EnemyHP取得
        EnemyHP enemy =
            other.GetComponent<EnemyHP>();

        // EnemyHPが無ければ無視
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