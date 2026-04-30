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
            Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }

        // 敵を削除
        Destroy(gameObject);
    }
}