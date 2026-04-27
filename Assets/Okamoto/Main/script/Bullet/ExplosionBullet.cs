using UnityEngine;

public class ExplosionBullet : MonoBehaviour
{
    public float lifeTime = 3f;
    public int damage = 20;

    public float explosionRadius = 3f;   // 爆発範囲
    public GameObject explosionEffect;   // エフェクトPrefab
    public AudioClip explosionSound;     // 爆発音

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Explode(transform.position);
        }
    }

    void Explode(Vector3 pos)
    {
        Debug.Log("爆発した！");

        // ★ダメージ処理（これ消えてると当たらない）
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, explosionRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }

        // エフェクト
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, pos, Quaternion.identity);
        }

        // 音
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, pos);
        }

        Destroy(gameObject);
    }

    // デバッグ用（範囲見える）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}