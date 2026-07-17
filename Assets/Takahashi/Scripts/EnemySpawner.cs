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

    [Header("ボスHPバー")]
    public BossHPBar bossHPBar;

    [Header("ボス予告演出")]
    public float bossWarningTime = 3f;   // ボス出現サイクル中、予告演出を開始するタイミング
    private bool bossWarningShown = false;

    // 予告演出が終わってボスがまだ出ていない待機中かどうかのフラグ
    private bool waitingForBossSpawn = false;

    [Header("スポーンY範囲（赤い床の高さ）")]
    public float spawnAreaTopRatio = 0.45f;
    public float spawnAreaBottomRatio = 1.0f;

    private float timer;

    void Update()
    {
        hpTimer += Time.deltaTime;

        if (hpTimer >= hpGrowInterval)
        {
            hpTimer = 0f;
            hpMultiplier *= hpGrowMultiplier;
        }

        // ===== ボス管理 =====
        // waitingForBossSpawn 中はタイマーを進めない
        // （予告演出→コールバックでの出現待ちの間、時間経過処理を止めておく）
        if (!bossAlive && !waitingForBossSpawn)
        {
            bossTimer += Time.deltaTime;

            // ボス出現の bossWarningTime 秒前になったら予告表示を開始
            if (!bossWarningShown && bossTimer >= bossSpawnTime - bossWarningTime)
            {
                bossWarningShown = true;
                waitingForBossSpawn = true; // 演出完了待ち状態に入る

                if (bossHPBar != null)
                {
                    // 演出完了時に SpawnBoss を呼ぶようコールバックを渡す
                    bossHPBar.ShowWarning("BOSS", SpawnBoss);
                }
                else
                {
                    // 万が一bossHPBarが未設定でも進行が止まらないよう、
                    // 参照がない場合は即座にボスを出す。
                    SpawnBoss();
                }
            }

            // 以前あった「bossTimer >= bossSpawnTime で SpawnBoss」の直接呼び出しは撤去済み。
            // 今は ShowWarning の演出完了コールバック経由でのみ SpawnBoss が呼ばれる。
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
    }

    void SpawnEnemy()
    {
        GameObject prefab = GetRandomEnemy();
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

    // HPバーの予告演出が終わった瞬間に呼ばれるメソッド
    void SpawnBoss()
    {
        waitingForBossSpawn = false; // 待機状態を解除

        Vector2 spawnPos = GetSpawnPosition();

        GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        bossAlive = true;

        BossMove move = boss.GetComponent<BossMove>();
        if (move != null)
            move.player = player;

        BossEnemy bossScript = boss.GetComponent<BossEnemy>();
        if (bossScript != null)
            bossScript.spawner = this;

        EnemyHP bossHP = boss.GetComponent<EnemyHP>();
        if (bossHPBar != null && bossHP != null)
        {
            // HPバーは既に満タン表示済み・待機中なので、HPを紐付けて追従を始めるだけ
            bossHPBar.AttachBoss(bossHP);
        }

        bossTimer = 0f;
        bossWarningShown = false;

        Debug.Log("ボス出現！");
    }

    public void BossDefeated()
    {
        bossAlive = false;

        if (bossHPBar != null)
            bossHPBar.Hide();

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

        float offset = 2f;

        float areaTop = top - fullH * spawnAreaTopRatio;
        float areaBottom = top - fullH * spawnAreaBottomRatio;

        int side = Random.Range(0, 3);

        switch (side)
        {
            case 0:
                return new Vector2(left - offset, Random.Range(areaBottom, areaTop));
            case 1:
                return new Vector2(right + offset, Random.Range(areaBottom, areaTop));
            default:
                return new Vector2(Random.Range(left, right), areaBottom - offset);
        }
    }
}