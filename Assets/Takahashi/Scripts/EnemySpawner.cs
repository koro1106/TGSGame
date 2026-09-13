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

    // 時間ではなく「コンボ数」でボスを呼ぶ
    [Header("ボス出現条件（コンボ数）")]
    [Tooltip("このコンボ数に到達したらボス出現の予告演出が始まる")]
    public int bossComboThreshold = 50;

    private bool bossAlive = false;

    [Header("ボスHPバー")]
    public BossHPBar bossHPBar;

    // 予告演出が終わってボスがまだ出ていない待機中かどうかのフラグ
    private bool waitingForBossSpawn = false;

    // ボス出現条件（コンボ）をすでに満たして予告演出を出したかどうかのフラグ
    // → コンボが50を超え続けている間、何度も予告が走らないようにするためのガード
    private bool bossWarningShown = false;

    [Header("スポーンY範囲（赤い床の高さ）")]
    public float spawnAreaTopRatio = 0.45f;
    public float spawnAreaBottomRatio = 1.0f;

    [Header("ゲーム開始時の初期配置")]
    [Tooltip("ゲーム開始時に生成する敵の数（敵の選ばれ方は通常スポーンと同じ。出現位置だけプレイヤーから離れた場所になる）")]
    public int initialEnemyCount = 3;
    [Tooltip("初期配置時、プレイヤーからこの距離以上離れた場所に出す")]
    public float initialEnemyMinDistanceFromPlayer = 3f;

    private float timer;

    void Start()
    {
        SpawnInitialEnemies();
    }

    void Update()
    {
        hpTimer += Time.deltaTime;

        if (hpTimer >= hpGrowInterval)
        {
            hpTimer = 0f;
            hpMultiplier *= hpGrowMultiplier;
        }

        // ===== ボス管理（コンボ数トリガー） =====
        // waitingForBossSpawn 中は判定を止めておく
        // （予告演出→コールバックでの出現待ちの間、二重発火を防ぐ）
        if (!bossAlive && !waitingForBossSpawn)
        {
            // まだ予告を出していない、かつComboManagerが存在し、
            // 現在のコンボ数が閾値（bossComboThreshold）に到達していたら予告演出を開始
            if (!bossWarningShown
                && ComboManager.instance != null
                && ComboManager.instance.Combo >= bossComboThreshold)
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

    // ★変更：ゲーム開始時の初期配置。
    //   出現位置はプレイヤーから離れた場所（GetRandomPositionAwayFromPlayer）に戻し、
    //   敵の選ばれ方（GetRandomEnemy経由）は通常スポーンと完全に同じSpawnEnemyAtを使う。
    void SpawnInitialEnemies()
    {
        for (int i = 0; i < initialEnemyCount; i++)
        {
            Vector2 spawnPos = GetRandomPositionAwayFromPlayer(initialEnemyMinDistanceFromPlayer);
            SpawnEnemyAt(spawnPos);
        }
    }

    void SpawnEnemy()
    {
        Vector2 spawnPos = GetSpawnPosition();
        SpawnEnemyAt(spawnPos);
    }

    // 指定した座標に敵を1体生成する（通常スポーン・初期配置の両方から呼ばれる共通処理）
    // ★ここで使われるGetRandomEnemy()は初期配置・通常スポーンどちらも完全に同じロジック
    void SpawnEnemyAt(Vector2 spawnPos)
    {
        GameObject prefab = GetRandomEnemy();

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

        // WarpEnemy（プレイヤーへ向かって移動する敵）にもプレイヤーを渡す
        WarpEnemyMove warp = enemy.GetComponent<WarpEnemyMove>();
        if (warp != null)
        {
            warp.player = player;
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
        {
            move.player = player;
            move.spawner = this; // ボス撃破の通知を受け取れるようにする
        }

        BossEnemy bossScript = boss.GetComponent<BossEnemy>();
        if (bossScript != null)
            bossScript.spawner = this;

        EnemyHP bossHP = boss.GetComponent<EnemyHP>();
        if (bossHPBar != null && bossHP != null)
        {
            // HPバーは既に満タン表示済み・待機中なので、HPを紐付けて追従を始めるだけ
            bossHPBar.AttachBoss(bossHP);
        }

        // 次回のボス出現判定に備えてフラグをリセット
        bossWarningShown = false;

        Debug.Log("ボス出現！（コンボ" + bossComboThreshold + "到達）");
    }

    public void BossDefeated()
    {
        bossAlive = false;

        if (bossHPBar != null)
            bossHPBar.Hide();

        // ボス撃破時にコンボをリセットし、次のボスまた0からコンボを貯める形にする
        if (ComboManager.instance != null)
            ComboManager.instance.ResetCombo();

        Debug.Log("ボス撃破！");
    }


    // =========================================
    // 敵HP増加をリセット
    // =========================================
    public void ResetEnemyHPGrowth()
    {
        hpTimer = 0f;
        hpMultiplier = 1f;

        Debug.Log("敵のHP増加をリセットしました");
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
            total += e.weight - playerStats.enemySpawnWeightBonus;

        float r = Random.Range(0, total);
        float sum = 0;

        foreach (var e in spawnList)
        {
            sum += e.weight - playerStats.enemySpawnWeightBonus;
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

    // ゲーム開始時の初期配置用：画面内の「赤い床」エリアからランダムな座標を選び、
    // プレイヤーから一定距離以上離れるまで（最大20回まで）再抽選する。
    // 全部失敗しても進行が止まらないよう、最後に選んだ座標をそのまま返す。
    Vector2 GetRandomPositionAwayFromPlayer(float minDistance)
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

        float areaTop = top - fullH * spawnAreaTopRatio;
        float areaBottom = top - fullH * spawnAreaBottomRatio;

        const int maxAttempts = 20;
        Vector2 candidate = Vector2.zero;

        for (int i = 0; i < maxAttempts; i++)
        {
            candidate = new Vector2(Random.Range(left, right), Random.Range(areaBottom, areaTop));

            if (player == null || Vector2.Distance(candidate, player.position) >= minDistance)
            {
                return candidate;
            }
        }

        // 何度試してもプレイヤーの近くにしかならなかった場合は、最後の候補をそのまま使う
        return candidate;
    }

    // =====================================================
    // リザルト後に敵の強さをリセット
    // =====================================================

    public void ResetEnemyGrowth()
    {
        // =========================================
        // 敵HP成長を完全リセット
        // =========================================

        hpTimer = 0f;
        hpMultiplier = 1f;

        // 通常敵スポーンタイマー
        timer = 0f;

        // ボス状態
        bossAlive = false;
        bossWarningShown = false;
        waitingForBossSpawn = false;

        // コンボもリセット（次周回でまた0から50を目指す形にする）
        if (ComboManager.instance != null)
            ComboManager.instance.ResetCombo();

        Debug.Log("敵のHP成長をリセットしました");
    }
}