using UnityEngine;

public class CarePackage : MonoBehaviour
{
    // =========================
    // アイテム
    // =========================

    [System.Serializable]
    public class DropItem
    {
        public GameObject prefab;
        [Range(0, 100)] public int dropRate = 10;
    }

    [Header("ドロップテーブル")]
    [SerializeField] private DropItem[] dropItems;

    // =========================
    // HP
    // =========================

    [Header("HP")]
    [SerializeField] private int maxHP = 50;

    private int currentHP;
    private bool landed;

    // =========================
    // 落下
    // =========================

    [Header("落下")]
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private float groundY = 0f;

    // =========================
    // 着地エフェクト（スカッシュ＆ストレッチ）
    // =========================

    [Header("── 落下中の潰れ ──────────")]
    [Tooltip("落下中、横にどれだけ狭く/広くなるか（1.0が基準）")]
    [SerializeField] private float fallSquashScaleX = 1f;
    [Tooltip("落下中、縦にどれだけ狭くなるか（1.0が基準）")]
    [SerializeField] private float fallSquashScaleY = 0.9f;
    [Tooltip("落下中の潰れの補間速さ")]
    [SerializeField] private float fallSquashSpeed = 4f;

    [Header("── 着地エフェクト ──────────")]
    [Tooltip("着地した瞬間、横にどれだけ伸びるか（1.0が基準）")]
    [SerializeField] private float squashScaleX = 1.3f;
    [Tooltip("着地した瞬間、縦にどれだけ潰れるか（1.0が基準）")]
    [SerializeField] private float squashScaleY = 0.6f;
    [Tooltip("潰れきるまでの速さ")]
    [SerializeField] private float squashSpeed = 18f;
    [Tooltip("元のスケールに戻る速さ")]
    [SerializeField] private float squashRecoverSpeed = 10f;
    [Tooltip("潰れている時間（この後、戻り始める）")]
    [SerializeField] private float squashHoldTime = 0.05f;

    private Vector3 baseScale;
    private bool squashing;
    private bool squashPhaseDone; // true = 潰れ切って戻りフェーズへ
    private float squashTimer;

    // =========================
    // 影
    // =========================

    [Header("影Prefab")]
    [SerializeField] private GameObject shadowPrefab;

    [Header("影の位置オフセット（地面からのずれ）")]
    [SerializeField] private Vector2 shadowOffset = new Vector2(0f, -60f);

    private Transform shadow;
    private SpriteRenderer shadowRenderer;

    // =========================
    // 報酬
    // =========================

    public enum RewardType
    {
        ItemRain,
        AmmoSupply,
        KillAllEnemies
    }

    [System.Serializable]
    public class RewardData
    {
        public RewardType rewardType;
        [Range(0, 100)] public int chance = 10;
    }

    [Header("報酬テーブル")]
    [SerializeField] private RewardData[] rewards;

    // =========================
    // 初期化
    // =========================

    private void Start()
    {
        currentHP = maxHP;
        baseScale = transform.localScale;
        CreateShadow();
    }

    private void Update()
    {
        if (!landed)
            Fall();
        else if (squashing)
            UpdateSquash();

        UpdateShadow();
    }

    // 落下処理の近く（Fallの上とか）
    public void SetGroundY(float y)
    {
        groundY = y;
    }

    // =========================
    // 落下
    // =========================

    private void Fall()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // 落下中は少しずつX/Yを潰す・伸ばす
        Vector3 fallTarget = new Vector3(
            baseScale.x * fallSquashScaleX,
            baseScale.y * fallSquashScaleY,
            baseScale.z);
        transform.localScale = Vector3.MoveTowards(
            transform.localScale, fallTarget, fallSquashSpeed * Time.deltaTime);

        if (transform.position.y <= groundY)
        {
            landed = true;

            transform.position = new Vector3(
                transform.position.x,
                groundY,
                0f
            );

            StartSquash();
        }
    }

    // =========================
    // 着地エフェクト（スカッシュ＆ストレッチ）
    // =========================

    private void StartSquash()
    {
        squashing = true;
        squashPhaseDone = false;
        squashTimer = 0f;
    }

    private void UpdateSquash()
    {
        if (!squashPhaseDone)
        {
            // 潰れフェーズ：横に伸びて縦に潰れる
            Vector3 target = new Vector3(
                baseScale.x * squashScaleX,
                baseScale.y * squashScaleY,
                baseScale.z
            );

            transform.localScale = Vector3.MoveTowards(
                transform.localScale, target, squashSpeed * Time.deltaTime);

            if (transform.localScale == target)
            {
                squashTimer += Time.deltaTime;
                if (squashTimer >= squashHoldTime)
                    squashPhaseDone = true;
            }
        }
        else
        {
            // 戻りフェーズ：元のスケールへ補間
            transform.localScale = Vector3.MoveTowards(
                transform.localScale, baseScale, squashRecoverSpeed * Time.deltaTime);

            if (transform.localScale == baseScale)
                squashing = false;
        }
    }

    // =========================
    // ダメージ
    // =========================

    public void TakeDamage(int damage)
    {
        if (!landed) return;

        currentHP -= damage;

        if (currentHP <= 0)
            BreakBox();
    }

    // =========================
    // 報酬抽選
    // =========================

    private RewardType GetRandomReward()
    {
        int total = 0;

        foreach (var r in rewards)
            total += r.chance;

        int roll = Random.Range(0, total);
        int current = 0;

        foreach (var r in rewards)
        {
            current += r.chance;
            if (roll < current)
                return r.rewardType;
        }

        return rewards[0].rewardType;
    }

    // =========================
    // アイテム雨
    // =========================

    private GameObject GetRandomItem()
    {
        int total = 0;

        foreach (var i in dropItems)
            total += i.dropRate;

        int roll = Random.Range(0, total);
        int current = 0;

        foreach (var i in dropItems)
        {
            current += i.dropRate;
            if (roll < current)
                return i.prefab;
        }

        return dropItems[0].prefab;
    }

    private void SpawnManyItems()
    {
        for (int i = 0; i < 15; i++)
        {
            GameObject item = GetRandomItem();
            if (item == null) continue;

            Vector3 offset = new Vector3(
                Random.Range(-80f, 80f),
                Random.Range(-80f, 80f),
                0f
            );

            Instantiate(item, transform.position + offset, Quaternion.identity);
        }
    }

    // =========================
    // 弾補給
    // =========================

    private void AmmoSupply()
    {
        Debug.Log("弾補給！");
    }

    // =========================
    // 敵全滅
    // =========================

    private void KillAllEnemies()
    {
        EnemyHP[] enemies = FindObjectsOfType<EnemyHP>();
        Camera cam = Camera.main;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            Vector3 viewportPos = cam.WorldToViewportPoint(enemy.transform.position);

            bool onScreen =
                viewportPos.z > 0 && // カメラの前にいる
                viewportPos.x >= 0f && viewportPos.x <= 1f &&
                viewportPos.y >= 0f && viewportPos.y <= 1f;

            if (!onScreen) continue;

            // 100%確実に倒す
            enemy.TakeDamage(999999);
        }
    }

    // =========================
    // 破壊
    // =========================

    private void BreakBox()
    {
        RewardType reward = GetRandomReward();

        switch (reward)
        {
            case RewardType.ItemRain:
                SpawnManyItems();
                break;

            case RewardType.AmmoSupply:
                AmmoSupply();
                break;

            case RewardType.KillAllEnemies:
                KillAllEnemies();
                break;
        }

        Destroy(gameObject);
    }

    // =========================
    // 影
    // =========================

    private void CreateShadow()
    {
        if (shadowPrefab == null) return;

        GameObject obj = Instantiate(shadowPrefab);

        shadow = obj.transform;
        shadowRenderer = obj.GetComponent<SpriteRenderer>();

        shadow.position = new Vector3(
            transform.position.x + shadowOffset.x,
            groundY + shadowOffset.y,
            0f
        );
    }

    private void UpdateShadow()
    {
        if (shadow == null) return;

        shadow.position = new Vector3(
            transform.position.x + shadowOffset.x,
            groundY + shadowOffset.y,
            0f
        );

        float h = Mathf.Max(transform.position.y - groundY, 0f);
        float t = Mathf.Clamp01(h / 550f);

        float scale = Mathf.Lerp(10f, 2f, t);

        shadow.localScale = new Vector3(scale, scale * 0.6f, 1f);

        Color c = shadowRenderer.color;
        c.a = Mathf.Lerp(0.5f, 0.1f, t);
        shadowRenderer.color = c;

        shadow.rotation = Quaternion.identity;
    }

    private void OnDestroy()
    {
        if (shadow != null)
            Destroy(shadow.gameObject);
    }
}