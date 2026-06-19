using UnityEngine;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// 雷属性弾
/// </summary>
public class ChainBullet : MonoBehaviour
{
    public float lifeTime = 3f;
    public int damage = 20;

    public int chainCount = 2;
    public float chainRadius = 5f;

    public GameObject lightningPrefab;
    public GameObject hitEffectPrefab;

    public AudioClip hitSound;

    private LineRenderer lr;

    public GameObject ammoDropPrefab;
    public Sprite ammoUISprite;

    public PlayerStats stats;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();

        if (lr == null)
        {
            Debug.LogError("LineRendererが付いてない！");
        }
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
        chainCount += stats.lightningBulletUP; // 性能UP分加算
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // EnemyHP取得
        EnemyHP firstEnemy =
            other.GetComponent<EnemyHP>();

        // EnemyHPが無ければ無視
        if (firstEnemy == null)
            return;

        // 音
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(
                hitSound,
                transform.position
            );
        }

        ChainDamage(firstEnemy);

        Destroy(gameObject);
    }

    void ChainDamage(EnemyHP startEnemy)
    {
        List<EnemyHP> hitEnemies =
            new List<EnemyHP>();

        int totalDamage =
            damage +
            stats.effectBulletDamage;

        // 最初の敵
        hitEnemies.Add(startEnemy);

        startEnemy.TakeDamage(totalDamage);

        // 最初の雷
        SpawnLightning(
            transform.position,
            startEnemy.transform.position
        );

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                startEnemy.transform.position,
                chainRadius
            );

        var enemies = hits
            .Select(h => h.GetComponent<EnemyHP>())
            .Where(e =>
                e != null &&
                !hitEnemies.Contains(e)
            )
            .OrderBy(e =>
                Vector2.Distance(
                    startEnemy.transform.position,
                    e.transform.position
                )
            )
            .ToList();

        EnemyHP current =
            startEnemy;

        int count = 0;

        foreach (var enemy in enemies)
        {
            if (count >= chainCount)
                break;

            enemy.TakeDamage(totalDamage);

            hitEnemies.Add(enemy);

            SpawnLightning(
                current.transform.position,
                enemy.transform.position
            );

            current = enemy;

            count++;
        }
    }

    void SpawnLightning(Vector3 start, Vector3 end)
    {
        if (lightningPrefab == null)
            return;

        // 雷本体
        GameObject obj =
            Instantiate(lightningPrefab);

        LightningLineEffect le =
            obj.GetComponent<LightningLineEffect>();

        if (le != null)
        {
            le.Setup(start, end);
        }

        // 着弾エフェクト
        if (hitEffectPrefab != null)
        {
            Instantiate(
                hitEffectPrefab,
                end,
                Quaternion.identity
            );
        }
    }
}