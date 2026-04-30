using UnityEngine;

public class EnemyHP : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 40;
    public int currentHP;

    [Header("見た目")]
    public float scaleSmooth = 12f;

    [Header("ドロップ")]
    public GameObject dropPrefab;

    private Vector3 baseScale;
    private Vector3 targetScale;

    void Start()
    {
        // HP初期化
        currentHP = maxHP;

        // 元のサイズを保存
        baseScale = transform.localScale;
        targetScale = baseScale;
    }

    void Update()
    {
        // サイズをなめらかに変える
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * scaleSmooth
        );
    }

    // ダメージを受ける
    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            Die(); // 死亡
        }
        else
        {
            UpdateScale(); // HPに応じてサイズ変更
        }
    }

    void UpdateScale()
    {
        // HP割合でサイズを変える
        float ratio = (float)currentHP / maxHP;
        targetScale = baseScale * ratio;
    }

    void Die()
    {
        // ドロップ生成
        if (dropPrefab != null)
        {
            GameObject drop = Instantiate(dropPrefab, transform.position, Quaternion.identity);

            // Rigidbody2D取得
            Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // 重力なしにする（宇宙用）
                rb.gravityScale = 0f;

                // 減速をほぼなくす（漂う感じ）
                rb.linearDamping = 0.1f;

                // ランダムな方向に軽く飛ばす
                Vector2 force = Random.insideUnitCircle.normalized * Random.Range(50f, 150f);
                rb.AddForce(force, ForceMode2D.Impulse);

                // ゆっくり回転させる
                rb.AddTorque(Random.Range(-2f, 2f), ForceMode2D.Impulse);
            }
        }

        // 敵を削除
        Destroy(gameObject);
    }
}