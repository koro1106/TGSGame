using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemyData
{
    public GameObject prefab;
    public int weight;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("通常敵")]
    public EnemyData[] enemies;
    [Header("解放敵")]
    public EnemyData enemyA;
    public EnemyData enemyB;
    public EnemyData enemyC;
    public List<EnemyData> spawnList = new List<EnemyData>();

    public Transform player;

    public float spawnInterval = 2f;
    public float s = 0;

    [Header("時間経過でスポーン時HPが増える設定")]
    public float hpGrowInterval = 15f;
    public float hpGrowMultiplier = 1.2f;
    private float hpTimer;
    private float hpMultiplier = 1f;

    public PlayerStats playerStats;

    public GameObject bossPrefab;

    public float bossSpawnTime = 10f;

    private float bossTimer;

    private bool bossAlive = false;

    [Header("スポーンY範囲（赤い床の高さ）")]
    // 画面上端を0、画面下端を1とした割合で指定

    // 上端（0=画面上端, 1=画面下端）
    public float spawnAreaTopRatio = 0.45f;
    // 下端（0=画面上端, 1=画面下端）
    public float spawnAreaBottomRatio = 1.0f;

    void Update()
    {
        // ===== 経過時間でスポーン時HP倍率を上げる =====
        hpTimer += Time.deltaTime;

        if (hpTimer >= hpGrowInterval)
        {
            hpTimer = 0f;
            hpMultiplier *= hpGrowMultiplier;
        }

        // ===== ボス管理 =====
        if (!bossAlive)
        {
            bossTimer += Time.deltaTime;

            if (bossTimer >= bossSpawnTime)
            {
                SpawnBoss();
                bossTimer = 0f;
            }
        }

        // ボス中は通常敵を止める
        if (bossAlive)
            return;

        timer += Time.deltaTime;
        s = spawnInterval - playerStats.enemySpawnWeightBonus;
        if (timer >= s)
        {
            SpawnEnemy();
            timer = 0f;
        }

        void SpawnEnemy()
        {
            GameObject prefab = GetRandomEnemy();
            // 赤いエリア内にスポーン
            Vector2 spawnPos = GetSpawnPosition();

            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

            EnemyHP hp = enemy.GetComponent<EnemyHP>();
            if (hp != null)
            {
                hp.maxHP = Mathf.CeilToInt(hp.maxHP * hpMultiplier);
                hp.currentHP = hp.maxHP;
            }

            RushEnemy rush = enemy.GetComponent<RushEnemy>();
            if (rush != null)
            {
                rush.player = player;
            }
        }

        void SpawnBoss()
        {
            Vector2 spawnPos = GetSpawnPosition();

            GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

            bossAlive = true;

            BossMove move = boss.GetComponent<BossMove>();
            if (move != null)
                move.player = player;

            BossEnemy bossScript = boss.GetComponent<BossEnemy>();
            if (bossScript != null)
                bossScript.spawner = this;

            Debug.Log("ボス出現！");
        }
    }

    private float timer;

    public void BossDefeated()
    {
        bossAlive = false;
        Debug.Log("ボス撃破！");
    }

    GameObject GetRandomEnemy()
    {
        spawnList.Clear();

        foreach (var e in enemies)
            spawnList.Add(e);

        if (playerStats.enemyAUnlocked) spawnList.Add(enemyA);
        if (playerStats.enemyBUnlocked) spawnList.Add(enemyB);
        if (playerStats.enemyCUnlocked) spawnList.Add(enemyC);

        float total = 0;
        foreach (var e in spawnList)
            total += e.weight + playerStats.enemySpawnWeightBonus;

        float r = Random.Range(0, total);
        float sum = 0;

        foreach (var e in spawnList)
        {
            sum += e.weight + playerStats.enemySpawnWeightBonus;
            if (r < sum) return e.prefab;
        }

        return spawnList[0].prefab;
    }

    // 画面外からスポーン、ただしY座標は赤いエリアの範囲に限定
    Vector2 GetSpawnPosition()
    {
        Camera cam = Camera.main;

        float h = cam.orthographicSize;
        float w = h * cam.aspect;

        float camX = cam.transform.position.x;
        float camY = cam.transform.position.y;

        float left = camX - w;
        float right = camX + w;
        float top = camY + h;
        float bottom = camY - h;
        float fullH = top - bottom;

        float offset = 2f; // 画面外にはみ出す距離

        // 赤いエリアのY範囲（Ratioで指定）
        float areaTop = top - fullH * spawnAreaTopRatio;
        float areaBottom = top - fullH * spawnAreaBottomRatio;

        // 左・右・下の3辺からランダムにスポーン
        int side = Random.Range(0, 3);

        switch (side)
        {
            case 0: // 左
                return new Vector2(left - offset, Random.Range(areaBottom, areaTop));
            case 1: // 右
                return new Vector2(right + offset, Random.Range(areaBottom, areaTop));
            default: // 下
                return new Vector2(Random.Range(left, right), areaBottom - offset);
        }
    }
}
