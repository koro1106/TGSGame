using UnityEngine;

[System.Serializable]
public class EnemyData
{
    public GameObject prefab;
    public int weight;
}

public class EnemySpawner : MonoBehaviour
{
    public EnemyData[] enemies;
    public Transform player;

    public float spawnInterval = 2f;
    public int baseHP = 10;

    private float timer;
    private float hpTimer;
    private float hpMultiplier = 1f;

    public PlayerStats playerStats; // Playerのステータス
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }

        hpTimer += Time.deltaTime;

        if (hpTimer >= 5f)
        {
            hpTimer -= 5f;
            hpMultiplier *= 1.5f;
        }
    }

    void SpawnEnemy()
    {
        GameObject prefab = GetRandomEnemy();
        Vector2 spawnPos = GetSpawnPosition();

        GameObject enemy =
            Instantiate(prefab, spawnPos, Quaternion.identity);

        // HP設定
        EnemyHP hp = enemy.GetComponent<EnemyHP>();
        if (hp != null)
        {
            hp.maxHP = Mathf.CeilToInt(baseHP * hpMultiplier);
            hp.currentHP = hp.maxHP;
        }

        // プレイヤー注入
        RushEnemy rush = enemy.GetComponent<RushEnemy>();
        if (rush != null)
        {
            rush.player = player;
        }
    }

    GameObject GetRandomEnemy()
    {
        int total = 0;

        foreach (var e in enemies)
            total += e.weight + playerStats.enemySpawnWeightBonus;

        int r = Random.Range(0, total);

        int sum = 0;

        foreach (var e in enemies)
        {
            sum += e.weight + playerStats.enemySpawnWeightBonus;

            if (r < sum)
                return e.prefab;
        }

        return enemies[0].prefab;
    }

    Vector2 GetSpawnPosition()
    {
        Camera cam = Camera.main;

        float h = cam.orthographicSize;
        float w = h * cam.aspect;

        float halfW = w;
        float halfH = h;

        float offset = 2f; // ←ここが重要（画面外距離）

        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: // 左
                return new Vector2(
                    -halfW - offset,
                    Random.Range(-halfH - offset, halfH + offset)
                );

            case 1: // 右
                return new Vector2(
                    halfW + offset,
                    Random.Range(-halfH - offset, halfH + offset)
                );

            case 2: // 上
                return new Vector2(
                    Random.Range(-halfW - offset, halfW + offset),
                    halfH + offset
                );

            default: // 下
                return new Vector2(
                    Random.Range(-halfW - offset, halfW + offset),
                    -halfH - offset
                );
        }
    }
}