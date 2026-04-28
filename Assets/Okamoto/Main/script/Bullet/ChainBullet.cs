using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ChainBullet : MonoBehaviour
{
    public float lifeTime = 3f;
    public int damage = 20;

    public int chainCount = 2;
    public float chainRadius = 5f;

    public GameObject lightningPrefab;

    public AudioClip hitSound;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        Enemy firstEnemy = other.GetComponent<Enemy>();
        if (firstEnemy == null) return;

        // 音（安全）
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }

        ChainDamage(firstEnemy);

        Destroy(gameObject);
    }

    void ChainDamage(Enemy startEnemy)
    {
        List<Enemy> hitEnemies = new List<Enemy>();

        // 最初の敵
        hitEnemies.Add(startEnemy);
        startEnemy.TakeDamage(damage);

        // ★最初の雷（これ重要）
        SpawnLightning(transform.position, startEnemy.transform.position);

        Collider2D[] hits = Physics2D.OverlapCircleAll(startEnemy.transform.position, chainRadius);

        var enemies = hits
            .Select(h => h.GetComponent<Enemy>())
            .Where(e => e != null && !hitEnemies.Contains(e))
            .OrderBy(e => Vector2.Distance(startEnemy.transform.position, e.transform.position))
            .ToList();

        Enemy current = startEnemy;
        int count = 0;

        foreach (var enemy in enemies)
        {
            if (count >= chainCount) break;

            enemy.TakeDamage(damage);
            hitEnemies.Add(enemy);

            SpawnLightning(current.transform.position, enemy.transform.position);

            current = enemy;
            count++;
        }
    }

    void SpawnLightning(Vector3 start, Vector3 end)
    {
        if (lightningPrefab == null) return;

        GameObject obj = Instantiate(lightningPrefab);

        LightningDotEffect le = obj.GetComponentInChildren<LightningDotEffect>();

        if (le != null)
        {
            le.Setup(start, end);
        }
        else
        {
            Debug.LogWarning("LightningDotEffectが見つからない");
        }
    }
}