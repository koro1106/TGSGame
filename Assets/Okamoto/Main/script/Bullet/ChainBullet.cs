using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ChainBullet : MonoBehaviour
{
    public float lifeTime = 3f;
    public int damage = 20;

    public int chainCount = 2;        // 追加で何体に飛ぶか
    public float chainRadius = 5f;    // 探索範囲

    public GameObject lightningPrefab;

    public AudioClip hitSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy firstEnemy = other.GetComponent<Enemy>();

            if (firstEnemy != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
                ChainDamage(firstEnemy);
            }

            Destroy(gameObject);
        }
    }

    void ChainDamage(Enemy startEnemy)
    {
        List<Enemy> hitEnemies = new List<Enemy>();

        // 最初の敵
        hitEnemies.Add(startEnemy);
        startEnemy.TakeDamage(damage);

        Collider2D[] hits = Physics2D.OverlapCircleAll(startEnemy.transform.position, chainRadius);

        var enemies = hits
            .Select(h => h.GetComponent<Enemy>())
            .Where(e => e != null && !hitEnemies.Contains(e)) // ←ここ重要
            .OrderBy(e => Vector2.Distance(startEnemy.transform.position, e.transform.position))
            .ToList();

        Enemy current = startEnemy;
        int count = 0;

        foreach (var enemy in enemies)
        {
            if (count >= chainCount) break;

            // 念のためもう一回チェック
            if (hitEnemies.Contains(enemy)) continue;

            enemy.TakeDamage(damage);
            hitEnemies.Add(enemy);

            // 雷エフェクト
            GameObject obj = Instantiate(lightningPrefab);
            LightningEffect le = obj.GetComponent<LightningEffect>();
            le.Setup(current.transform.position, enemy.transform.position);

            current = enemy;
            count++;
        }
    }
}