using UnityEngine;

public class Bullet1 : MonoBehaviour
{
    [Header("移動速度")]
    public float speed = 8f;

    [Header("通常ダメージ")]
    public int damage = 10;

    [Header("クリティカル設定")]
    [Range(0f, 100f)]
    public float criticalChance = 50f;

    public float criticalMultiplier = 2f;

    // 弾の進行方向
    private Vector2 direction;

    // プレイヤーから設定される
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        // 直進移動
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // 画面外削除（雑な範囲制限）
        if (Mathf.Abs(transform.position.x) > 1100f ||
            Mathf.Abs(transform.position.y) > 600f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 敵に当たったとき
        if (other.CompareTag("Enemy"))
        {
            EnemyHP enemy = other.GetComponent<EnemyHP>();

            if (enemy == null) return;

            // クリティカル判定
            bool isCritical =
                Random.Range(0f, 100f) < criticalChance;

            // 最終ダメージ
            int finalDamage = damage;

            // クリティカルなら倍率
            if (isCritical)
            {
                finalDamage = Mathf.RoundToInt(damage * criticalMultiplier);
                Debug.Log("クリティカル！");
            }

            // ダメージ適用
            enemy.TakeDamage(finalDamage, isCritical);

            // 弾消滅
            Destroy(gameObject);
        }
    }
}