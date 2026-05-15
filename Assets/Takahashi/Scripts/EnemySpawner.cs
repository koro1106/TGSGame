using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;   // 敵Prefab
    public Transform player;         // プレイヤー

    public float spawnInterval = 2f; // 出現間隔
    public int baseHP = 10;          // 基本HP

    private float timer;             // スポーン用タイマー
    private float hpTimer;           // HP倍率用タイマー

    private float hpMultiplier = 1f; // HP倍率

    void Update()
    {

        // 敵生成タイマー

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }


        // HP倍率タイマー（5秒ごと）

        hpTimer += Time.deltaTime;

        if (hpTimer >= 5f)
        {
            hpTimer -= 5f;

            // 1.5倍にする
            hpMultiplier *= 1.5f;
        }
    }

    void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab);

        EnemyHP hp = enemy.GetComponent<EnemyHP>();

        if (hp != null)
        {
            // その時点のHPを設定（繰り上げ）
            hp.maxHP = Mathf.CeilToInt(baseHP * hpMultiplier);
            hp.currentHP = hp.maxHP;
        }

        // プレイヤー渡す（必要なら）
        RushEnemy rush = enemy.GetComponent<RushEnemy>();

        if (rush != null)
        {
            rush.player = player;
        }
    }
}