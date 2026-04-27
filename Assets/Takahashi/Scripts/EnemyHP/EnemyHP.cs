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
        currentHP = maxHP;

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

    // 外から呼ぶダメージ関数
    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            UpdateScale();
        }
    }

    void UpdateScale()
    {
        float ratio = (float)currentHP / maxHP;
        targetScale = baseScale * ratio;
    }

    void Die()
    {
        //  死んだ位置に生成
        if (dropPrefab != null)
        {
            Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
 
    }
}