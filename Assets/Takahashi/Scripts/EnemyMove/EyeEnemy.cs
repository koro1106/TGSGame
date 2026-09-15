using UnityEngine;

public class EyeEnemy : MonoBehaviour
{
    public float moveSpeed = 4f;

    [Header("見た目")]
    public Transform graphics;

    [Header("ぴょんぴょん")]
    public float bounceHeight = 0.2f;
    public float bounceSpeed = 8f;

    [Header("頭パーツ(まばたき)")]
    public SpriteRenderer head;
    public Sprite headOpenSprite;   // 頭.png
    public Sprite headClosedSprite; // 頭_目閉じ.png
    public float blinkMinInterval = 2f; // 2?5秒でランダムに瞬き
    public float blinkMaxInterval = 5f;
    public float blinkDuration = 0.15f;

    [Header("手パーツ(Z回転で開閉)")]
    public Transform leftHand;
    public Transform rightHand;
    public float rightHandUpAngle = -25f;   // 右手: 上に行っている時
    public float rightHandDownAngle = 25f;  // 右手: 下に行っている時
    public float leftHandUpAngle = 25f;     // 左手: 上に行っている時(右手と逆)
    public float leftHandDownAngle = -25f;  // 左手: 下に行っている時(右手と逆)

    // ===== スポーン範囲（赤い床の高さ）を他の敵と揃える =====
    [Header("スポーンY範囲（赤い床の高さ）")]
    public float spawnAreaTopRatio = 0.45f;
    public float spawnAreaBottomRatio = 1.0f;

    // ★画面外消滅
    [Header("画面外消滅")]
    [Tooltip("画面端からこの距離だけ離れたら自動的に消える")]
    public float despawnDistance = 5f;

    // ★追加：追尾(ふわふわ迷走)
    [Header("追尾(ふわふわ迷走)")]
    [Tooltip("trueならプレイヤーを狙いつつ漂うように動く")]
    public bool trackPlayer = true;
    [Tooltip("この間隔で狙いをカクッと更新し直す")]
    public float retargetInterval = 1.2f;
    [Tooltip("プレイヤー周辺のどのくらいブレて狙うか")]
    public float aimRandomness = 1.5f;
    [Tooltip("進行方向に加える揺らぎの強さ(度)")]
    public float wanderNoiseAngle = 45f;
    [Tooltip("揺らぎの速さ")]
    public float noiseSpeed = 0.4f;

    private float retargetTimer;
    private float noiseSeed;
    private Vector2 currentAimDir;

    private Vector2 moveDirection;

    private EnemyHP hp;

    private float blinkTimer;
    private bool isBlinking;
    private float blinkTimeLeft;

    private SpriteRenderer bodySR;
    private SpriteRenderer tailSR;
    private SpriteRenderer headSR;
    private SpriteRenderer leftHandSR;
    private SpriteRenderer rightHandSR;

    void Start()
    {
        hp = GetComponent<EnemyHP>();

        if (head != null) headSR = head;

        Transform body = transform.Find("身体");
        if (body != null) bodySR = body.GetComponent<SpriteRenderer>();

        Transform tail = transform.Find("しっぽ");
        if (tail != null) tailSR = tail.GetComponent<SpriteRenderer>();

        if (leftHand != null) leftHandSR = leftHand.GetComponent<SpriteRenderer>();
        if (rightHand != null) rightHandSR = rightHand.GetComponent<SpriteRenderer>();

        noiseSeed = Random.Range(0f, 100f);
        retargetTimer = 0f; // 出現直後に1回すぐ狙いを決める

        // ← 一番最後に呼ぶ
        SetSpawnAndDirection();

        if (head != null && headOpenSprite != null)
            head.sprite = headOpenSprite;

        ResetBlinkTimer();
    }

    void Update()
    {
        // =====================================================
        // ポーズ中・リザルト中は完全停止
        // =====================================================
        if (PauseMenu.IsPaused || ResultManager.IsResultActive)
        {
            return;
        }

        if (hp != null && hp.IsBind())
        {
            return;
        }

        if (hp == null) return;

        float hpRate = (float)hp.currentHP / hp.maxHP;

        // ★追尾(ふわふわ迷走)の方向更新
        UpdateWanderDirection();

        // 移動
        transform.Translate(
            moveDirection * moveSpeed * Time.deltaTime,
            Space.World
        );

        // ぴょんぴょん
        float currentBounce = bounceHeight * hpRate;
        Vector3 pos = transform.position;
        pos.y += Mathf.Sin(Time.time * bounceSpeed) * currentBounce;
        transform.position = pos;

        // まばたき
        UpdateBlink();

        // 手の開閉
        UpdateHandSway();

        // ★画面外へ一定距離離れたら消える
        CheckDespawn();
    }


    void ResetBlinkTimer()
    {
        blinkTimer = Random.Range(blinkMinInterval, blinkMaxInterval);
    }

    void UpdateBlink()
    {
        if (head == null) return;

        if (isBlinking)
        {
            blinkTimeLeft -= Time.deltaTime;
            if (blinkTimeLeft <= 0f)
            {
                isBlinking = false;
                head.sprite = headOpenSprite;
                ResetBlinkTimer();
            }
            return;
        }

        blinkTimer -= Time.deltaTime;
        if (blinkTimer <= 0f)
        {
            isBlinking = true;
            blinkTimeLeft = blinkDuration;
            head.sprite = headClosedSprite;
        }
    }

    void UpdateHandSway()
    {
        float wave = Mathf.Sin(Time.time * bounceSpeed);
        float t = (wave + 1f) * 0.5f; // 0(下) ? 1(上)

        if (rightHand != null)
        {
            float angle = Mathf.Lerp(rightHandDownAngle, rightHandUpAngle, t);
            rightHand.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (leftHand != null)
        {
            float angle = Mathf.Lerp(leftHandDownAngle, leftHandUpAngle, t);
            leftHand.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    // =========================================================
    // ★追加：プレイヤーを狙いつつ、間隔を空けてカクッと狙い直す
    // ノイズで常時ふらつかせることで「漂いながら迫る」動きにする
    // =========================================================
    void UpdateWanderDirection()
    {
        if (!trackPlayer)
        {
            // 追尾しない場合はノイズだけ加えて緩やかに揺らす
            float n = (Mathf.PerlinNoise(Time.time * noiseSpeed, noiseSeed) - 0.5f) * 2f * wanderNoiseAngle;
            moveDirection = Quaternion.Euler(0f, 0f, n) * moveDirection;
            UpdateFacing();
            return;
        }

        retargetTimer -= Time.deltaTime;
        if (retargetTimer <= 0f)
        {
            retargetTimer = retargetInterval;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector2 aimPoint = (Vector2)player.transform.position
                    + Random.insideUnitCircle * aimRandomness;
                currentAimDir = (aimPoint - (Vector2)transform.position).normalized;
            }
        }

        // 直進方向(currentAimDir)に対してノイズで揺らぎを加える
        float noise = (Mathf.PerlinNoise(Time.time * noiseSpeed, noiseSeed) - 0.5f) * 2f * wanderNoiseAngle;
        moveDirection = Quaternion.Euler(0f, 0f, noise) * currentAimDir;

        UpdateFacing();
    }

    void UpdateFacing()
    {
        bool flip = moveDirection.x > 0f;
        if (bodySR != null) bodySR.flipX = flip;
        if (tailSR != null) tailSR.flipX = flip;
        if (headSR != null) headSR.flipX = flip;
        if (leftHandSR != null) leftHandSR.flipX = flip;
        if (rightHandSR != null) rightHandSR.flipX = flip;
    }

    // =========================================================
    // スポーン位置決定
    // EnemySpawner.GetSpawnPosition() / EnemyMove.CalcAreaBounds() と
    // 同じ考え方（カメラ位置基準＋赤い床エリア比率で範囲を絞る）に統一
    // 進行方向は UpdateWanderDirection() に任せるため、ここでは
    // スポーン位置だけ決めて初回の狙いをセットする
    // =========================================================
    void SetSpawnAndDirection()
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

        float offset = 3f; // 画面外距離（EnemySpawnerは2fだが、元の値を踏襲）

        // 赤い床エリア（Y範囲）
        float areaTop = top - fullH * spawnAreaTopRatio;
        float areaBottom = top - fullH * spawnAreaBottomRatio;

        // 右・左・下のみ（上からのスポーンは無し）
        int side = Random.Range(0, 3);
        Vector2 spawnPos;

        switch (side)
        {
            case 0: // 右
                spawnPos = new Vector2(right + offset, Random.Range(areaBottom, areaTop));
                break;
            case 1: // 左
                spawnPos = new Vector2(left - offset, Random.Range(areaBottom, areaTop));
                break;
            default: // 下
                spawnPos = new Vector2(Random.Range(left, right), areaBottom - offset);
                break;
        }

        transform.position = spawnPos;

        // 初回の狙い方向を決める（追尾ONならプレイヤー、OFFなら赤い床のランダム点）
        GameObject player = trackPlayer ? GameObject.FindGameObjectWithTag("Player") : null;
        Vector2 target;
        if (player != null)
        {
            target = (Vector2)player.transform.position
                + Random.insideUnitCircle * aimRandomness;
        }
        else
        {
            target = new Vector2(
                Random.Range(left, right),
                Random.Range(areaBottom, areaTop)
            );
        }

        currentAimDir = (target - spawnPos).normalized;
        moveDirection = currentAimDir;

        UpdateFacing();
    }

    // =========================================================
    // ★画面外へ一定距離離れたら自動で消える
    // =========================================================
    void CheckDespawn()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float h = cam.orthographicSize;
        float w = h * cam.aspect;

        float camX = cam.transform.position.x;
        float camY = cam.transform.position.y;

        float left = camX - w - despawnDistance;
        float right = camX + w + despawnDistance;
        float top = camY + h + despawnDistance;
        float bottom = camY - h - despawnDistance;

        Vector3 pos = transform.position;

        bool outside =
            pos.x < left || pos.x > right ||
            pos.y < bottom || pos.y > top;

        if (outside)
        {
            Destroy(gameObject);
        }
    }
}