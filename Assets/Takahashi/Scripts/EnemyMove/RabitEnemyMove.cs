using UnityEngine;

/// <summary>
/// Rabit（ゴミ箱敵・連番スプライト版）
///
/// 本格版 EnemyMove.cs の状態管理（侵入 → 着地 → 待機 → ジャンプ徘徊 →
/// 検知 → 溜め → 突進）をそのまま移植し、見た目の演出だけを
/// 「箱・蓋・耳・ウサギを個別に動かすリグアニメーション」から
/// 「40枚の連番スプライトをコマ送りする方式」に置き換えたバージョンです。
///
/// 【動きの流れ】（EnemyMoveと同じ）
///   ① 画面外から画面内へアーチ状ジャンプで侵入（Enter）
///   ② 着地（Land）→ 待機（Wait）でその場バウンス
///      → プレイヤーが検知円（detectRadius）内に入ったら突進へ
///   ③ 待機中はランダム方向へアーチ状ジャンプで徘徊（Jump）
///   ④ 検知したら溜め（Telegraph）→ 突進（Charge）→ 着地 → ②へ戻る
///
/// 【見た目】
///   全状態共通で Frames（001〜040の連番）をループ再生。
///   状態ごとに再生速度（fps）だけ変えることで、
///   「待機中はゆっくり」「突進中は速く」といった緩急をつけています。
///   さらに移動中は進行方向へわずかに傾ける演出（bodyTilt）を
///   Transform の回転だけで簡易的に再現しています。
///
/// 【使い方】
/// 1. 敵オブジェクトに SpriteRenderer を追加
/// 2. このスクリプト (Rabit.cs) をアタッチ
/// 3. Inspector の "Frames" に enemy_jump_001〜040 を順番通りにドラッグ
/// 4. Player Transform（未設定なら PlayerMovement / Player から自動取得）
/// 5. 各種パラメータ（速度・検知半径・突進など）を調整
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class RabitEnemyMove : MonoBehaviour, IHitSlowable
{
    // =========================================================
    // 状態管理
    // =========================================================
    enum State { Enter, Land, Wait, Jump, Charge, Telegraph }
    State state = State.Enter;
    Vector2 direction;

    // =========================================================
    // アニメーション（連番スプライト）
    // =========================================================
    [Header("── アニメーション用スプライト（001〜040を順番に） ──")]
    public Sprite[] frames;

    [Header("── 状態ごとの再生速度（fps） ──")]
    public float idleFps = 18f;
    public float jumpFps = 24f;
    public float chargeFps = 30f;

    [Header("── 移動中の傾き演出（Transform回転で簡易再現） ──")]
    public float bodyTiltAngle = 12f;
    public float bodyTiltSpeed = 12f;
    private float currentBodyTilt = 0f;

    [Header("── 重なり順（点滅対策） ──────────")]
    [Tooltip("ONにするとY座標に応じてSorting Orderを自動計算し、敵同士が重なった時の点滅（描画順の入れ替わり）を防ぎます")]
    public bool autoSortByY = true;
    [Tooltip("Y座標をSorting Orderに変換する際の倍率。値が大きいほど細かい差でも順序が変わります")]
    public float sortingOrderMultiplier = 100f;
    [Tooltip("この値を基準の順序に足す（他の描画物との前後関係を調整したい時に使用）")]
    public int sortingOrderOffset = 0;

    private SpriteRenderer sr;
    private int frameIndex;
    private float frameTimer;

    // =========================================================
    // 移動パラメータ
    // =========================================================
    [Header("── 移動 ──────────────────")]
    public float jumpMoveSpeed = 75f;
    public float jumpHeightVisualScale = 1f; // 見た目のジャンプ感を出すためのY方向の軽い揺れ（任意）

    [Header("着地後の待機時間")]
    public float waitTimeMin = 0.3f;
    public float waitTimeMax = 0.8f;
    private float waitTimer;
    private float waitDuration;

    [Header("ジャンプ／突進1回あたりの時間")]
    public float jumpDuration = 0.5f;

    // =========================================================
    // 突進攻撃
    // =========================================================
    [Header("── 突進攻撃 ──────────────────")]
    [Tooltip("未設定ならStartでPlayerMovement(優先)/Playerのシングルトンから自動取得")]
    public Transform player;
    [Tooltip("この距離までプレイヤーが近づいたら突進する（＝検知円の半径）")]
    public float detectRadius = 90f;
    [Tooltip("突進中の移動速度")]
    public float chargeSpeed = 150f;
    [Tooltip("検知してから実際に突進するまでの予備動作（溜め）時間（秒）")]
    public float chargeTelegraphTime = 0.3f;
    [Tooltip("突進を続ける時間（秒）")]
    public float chargeDuration = 0.4f;
    [Tooltip("突進後、再び突進判定を行えるようになるまでのクールダウン（秒）")]
    public float chargeCooldown = 1.2f;
    [Tooltip("突進がプレイヤーに当たった時のダメージ（プレイヤー側の受け取り実装が必要）")]
    public int attackDamage = 1;

    private float chargeTimer = 0f;
    private float chargeCooldownTimer = 0f;
    private Vector2 chargeDirection;
    private float telegraphTimer = 0f;

    [Header("── 突進の狙い演出（ビーム） ──────────")]
    [Tooltip("狙いを示す赤いビームのPrefab（未設定でも動作します）")]
    public GameObject telegraphPrefab;
    [Tooltip("ビームが伸びきったときの長さ")]
    public float telegraphLength = 200f;

    private Transform telegraphVisual;
    private Vector2 telegraphDirection;

    // =========================================================
    // 検知円の表示
    // =========================================================
    [Header("── 検知円の表示 ──────────────")]
    public bool showDetectCircle = true;
    public Color detectCircleColor = new Color(1f, 0.3f, 0.3f, 0.6f);
    [Range(8, 64)]
    public int circleSegments = 40;
    public float circleLineWidth = 1.5f;

    private LineRenderer detectCircleRenderer;

    // =========================================================
    // 移動エリア制限（赤い床）
    // =========================================================
    [Header("── 移動エリア（赤い床） ─────────")]
    public float moveAreaTopRatio = 0.3f;
    public float moveAreaBottomRatio = 1.0f;

    private float areaLeft, areaRight, areaTop, areaBottom;

    // =========================================================
    // 被弾鈍化
    // =========================================================
    [Header("被弾時の鈍化")]
    public float hitSlowMultiplier = 0.3f;
    public float hitSlowDuration = 0.5f;
    private float slowTimer = 0f;
    private float speedMultiplier = 1f;
    private Vector2 target;

    [Header("── デバッグ ──────────────────")]
    [Tooltip("Playモード中にこのキーを押すと、検知円やクールダウンを無視して即座に突進攻撃を1回実行")]
    public KeyCode debugTriggerKey = KeyCode.L;

    private float jumpTimer = 0f;
    private float landTimer = 0f;
    public float landDuration = 0.2f;

    private EnemyHP enemyHP;

    [Header("── 影 ──────────────────────")]
    public GameObject shadowPrefab;
    public Vector2 shadowOffset = new Vector2(0f, -0.1f);
    public Vector2 shadowBaseScale = new Vector2(1f, 0.3f);
    public Vector2 shadowAirScale = new Vector2(0.5f, 0.15f);

    private Transform shadow;
    private SpriteRenderer shadowSR;

    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        enemyHP = GetComponent<EnemyHP>();
        target = Vector2.zero;

        if (frames != null && frames.Length > 0)
        {
            sr.sprite = frames[0];
        }

        CalcAreaBounds();
        TryGetPlayer();

        if (showDetectCircle) SetupDetectCircle();

        if (telegraphPrefab != null)
        {
            GameObject tg = Instantiate(telegraphPrefab, transform.position, Quaternion.identity);
            telegraphVisual = tg.transform;
            telegraphVisual.gameObject.SetActive(false);
        }

        if (shadowPrefab != null)
        {
            GameObject s = Instantiate(shadowPrefab);
            shadow = s.transform;
            shadowSR = s.GetComponent<SpriteRenderer>();
            FixShadow();
        }

        direction = ((Vector2)target - (Vector2)transform.position).normalized;

        StartJump();
        state = State.Enter;
    }

    void LateUpdate()
    {
        // Y座標が低い（画面下＝手前）ほど大きいSorting Orderになるようにして、
        // 敵同士が重なった時にどちらが手前か毎フレーム安定させる（点滅防止）
        if (autoSortByY && sr != null)
        {
            sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * sortingOrderMultiplier) + sortingOrderOffset;
        }
    }

    // =========================================================
    // プレイヤー取得
    // =========================================================
    void TryGetPlayer()
    {
        if (player != null) return;

        if (PlayerMovement.Instance != null)
        {
            player = PlayerMovement.Instance.transform;
        }
        else if (Player.Instance != null)
        {
            player = Player.Instance.transform;
        }
    }

    // =========================================================
    // 検知円セットアップ
    // =========================================================
    void SetupDetectCircle()
    {
        GameObject circleObj = new GameObject("DetectCircle");
        circleObj.transform.SetParent(transform);
        circleObj.transform.localPosition = Vector3.zero;
        circleObj.transform.localRotation = Quaternion.identity;

        detectCircleRenderer = circleObj.AddComponent<LineRenderer>();
        detectCircleRenderer.useWorldSpace = false;
        detectCircleRenderer.loop = true;
        detectCircleRenderer.positionCount = circleSegments;
        detectCircleRenderer.widthMultiplier = circleLineWidth;
        detectCircleRenderer.material = new Material(Shader.Find("Sprites/Default"));
        detectCircleRenderer.startColor = detectCircleColor;
        detectCircleRenderer.endColor = detectCircleColor;
        detectCircleRenderer.sortingOrder = 10;

        DrawDetectCircle();
    }

    void DrawDetectCircle()
    {
        if (detectCircleRenderer == null) return;
        detectCircleRenderer.positionCount = circleSegments;

        for (int i = 0; i < circleSegments; i++)
        {
            float angle = (float)i / circleSegments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * detectRadius;
            float y = Mathf.Sin(angle) * detectRadius;
            detectCircleRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }
    }

    // =========================================================
    // エリア境界
    // =========================================================
    void CalcAreaBounds()
    {
        Camera cam = Camera.main;
        float h = cam.orthographicSize;
        float w = h * cam.aspect;
        float camX = cam.transform.position.x;
        float camY = cam.transform.position.y;
        float fullH = h * 2f;

        areaLeft = camX - w;
        areaRight = camX + w;
        areaTop = (camY + h) - fullH * moveAreaTopRatio;
        areaBottom = (camY + h) - fullH * moveAreaBottomRatio;
    }

    // =========================================================
    // Update
    // =========================================================
    void Update()
    {
        if (enemyHP != null && enemyHP.IsBind()) return;
        if (enemyHP != null && enemyHP.IsDying()) return; // ★追加：死亡演出中は本体を一切動かさない（回転上書き防止）
        UpdateHitSlow();

        if (debugTriggerKey != KeyCode.None && Input.GetKeyDown(debugTriggerKey))
        {
            DebugTriggerChargeAttack();
        }

        if (chargeCooldownTimer > 0f) chargeCooldownTimer -= Time.deltaTime;

        if ((state == State.Wait || state == State.Jump) && chargeCooldownTimer <= 0f)
        {
            TryGetPlayer();
            if (player != null)
            {
                float dist = Vector2.Distance(transform.position, player.position);
                if (dist <= detectRadius)
                {
                    EnterTelegraph();
                    return;
                }
            }
        }

        switch (state)
        {
            case State.Enter: UpdateEnter(); break;
            case State.Land: UpdateLand(); break;
            case State.Wait: UpdateWait(); break;
            case State.Jump: UpdateJump(); break;
            case State.Charge: UpdateCharge(); break;
            case State.Telegraph: UpdateTelegraph(); break;
        }
    }

    [ContextMenu("デバッグ：突進攻撃を今すぐ実行")]
    public void DebugTriggerChargeAttack()
    {
        if (!Application.isPlaying) return;
        TryGetPlayer();
        if (player == null) return;

        if (state == State.Wait || state == State.Jump || state == State.Telegraph)
        {
            chargeCooldownTimer = 0f;
            EnterTelegraph();
        }
    }

    // =========================================================
    // 【Enter】侵入
    // =========================================================
    void UpdateEnter()
    {
        jumpTimer += Time.deltaTime;
        transform.Translate((Vector3)direction * jumpMoveSpeed * speedMultiplier * Time.deltaTime);

        AnimateFrames(jumpFps);
        ApplyTilt();
        FlipSprite();
        UpdateShadow();

        if (IsInsideArea()) EnterLand();
    }

    // =========================================================
    // 【Land】着地
    // =========================================================
    void UpdateLand()
    {
        landTimer += Time.deltaTime;

        AnimateFrames(idleFps);
        currentBodyTilt = Mathf.LerpAngle(currentBodyTilt, 0f, Time.deltaTime * bodyTiltSpeed);
        transform.eulerAngles = new Vector3(0f, 0f, currentBodyTilt);

        FixShadow();

        if (landTimer >= landDuration) EnterWait();
    }

    // =========================================================
    // 【Wait】待機
    // =========================================================
    void UpdateWait()
    {
        TryGetPlayer();

        waitTimer += Time.deltaTime;
        AnimateFrames(idleFps);

        if (waitTimer >= waitDuration)
        {
            SetRandomDirection();
            StartJump();
            FixShadow();
        }
    }

    // =========================================================
    // 【Telegraph】突進前の溜め
    // =========================================================
    void EnterTelegraph()
    {
        state = State.Telegraph;
        telegraphTimer = 0f;

        if (player != null)
        {
            telegraphDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }

        if (telegraphVisual != null)
        {
            telegraphVisual.gameObject.SetActive(true);
            float angle = Mathf.Atan2(telegraphDirection.y, telegraphDirection.x) * Mathf.Rad2Deg;
            telegraphVisual.rotation = Quaternion.Euler(0f, 0f, angle);
            Vector3 baseScale = telegraphVisual.localScale;
            telegraphVisual.localScale = new Vector3(0f, baseScale.y, baseScale.z);
            telegraphVisual.position = transform.position;
        }
    }

    void UpdateTelegraph()
    {
        telegraphTimer += Time.deltaTime;
        AnimateFrames(idleFps);

        if (player != null)
        {
            telegraphDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }

        if (telegraphVisual != null)
        {
            float ratio = Mathf.Clamp01(telegraphTimer / chargeTelegraphTime);
            float currentLength = telegraphLength * ratio;

            float angle = Mathf.Atan2(telegraphDirection.y, telegraphDirection.x) * Mathf.Rad2Deg;
            telegraphVisual.rotation = Quaternion.Euler(0f, 0f, angle);

            Vector3 baseScale = telegraphVisual.localScale;
            telegraphVisual.localScale = new Vector3(currentLength, baseScale.y, baseScale.z);
            telegraphVisual.position = transform.position + (Vector3)(telegraphDirection * currentLength * 0.5f);
        }

        if (telegraphTimer >= chargeTelegraphTime)
        {
            if (telegraphVisual != null) telegraphVisual.gameObject.SetActive(false);

            if (player != null)
            {
                telegraphDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
                StartCharge();
            }
            else
            {
                EnterWait();
            }
        }
    }

    // =========================================================
    // 【Jump】徘徊
    // =========================================================
    void UpdateJump()
    {
        jumpTimer += Time.deltaTime;
        transform.Translate((Vector3)direction * jumpMoveSpeed * speedMultiplier * Time.deltaTime);

        ClampToArea();
        AnimateFrames(jumpFps);
        ApplyTilt();
        FlipSprite();
        UpdateShadow();

        if (jumpTimer >= jumpDuration) EndJump();
    }

    // =========================================================
    // 【Charge】突進
    // =========================================================
    void StartCharge()
    {
        state = State.Charge;
        chargeTimer = 0f;

        chargeDirection = (telegraphDirection != Vector2.zero)
            ? telegraphDirection
            : ((Vector2)player.position - (Vector2)transform.position).normalized;
        direction = chargeDirection;
    }

    void UpdateCharge()
    {
        chargeTimer += Time.deltaTime;
        transform.Translate((Vector3)chargeDirection * chargeSpeed * speedMultiplier * Time.deltaTime);

        ClampToArea();
        AnimateFrames(chargeFps);
        ApplyTilt();
        FlipSprite();
        UpdateShadow();

        if (chargeTimer >= chargeDuration)
        {
            chargeCooldownTimer = chargeCooldown;
            EnterLand();
        }
    }

    // 突進中の当たり判定（プレイヤー側のダメージ受け取り実装が必要）
    void OnTriggerEnter2D(Collider2D other)
    {
        if (state != State.Charge) return;
        if (!other.CompareTag("Player")) return;

        // 例）other.GetComponent<PlayerHP>()?.TakeDamage(attackDamage);
    }

    // =========================================================
    // アニメーション（連番スプライトのコマ送り）
    // =========================================================
    void AnimateFrames(float fps)
    {
        if (frames == null || frames.Length == 0) return;

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(1f, fps);

        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % frames.Length;
            sr.sprite = frames[frameIndex];
        }
    }

    // 移動方向へわずかに傾ける簡易演出（元のbodyTiltを箱パーツ無しで再現）
    void ApplyTilt()
    {
        float tiltDir = (direction.x > 0f) ? -1f : 1f;
        float targetTilt = tiltDir * bodyTiltAngle;
        currentBodyTilt = Mathf.LerpAngle(currentBodyTilt, targetTilt, Time.deltaTime * bodyTiltSpeed);
        transform.eulerAngles = new Vector3(0f, 0f, currentBodyTilt);
    }

    // =========================================================
    // 状態遷移ヘルパー
    // =========================================================
    void EnterLand()
    {
        state = State.Land;
        landTimer = 0f;
        FixShadow();
    }

    void EnterWait()
    {
        state = State.Wait;
        waitTimer = 0f;
        waitDuration = Random.Range(waitTimeMin, waitTimeMax);
    }

    void StartJump()
    {
        state = State.Jump;
        jumpTimer = 0f;
    }

    void EndJump() => EnterLand();

    // =========================================================
    // 左右反転
    // =========================================================
    void FlipSprite()
    {
        if (direction == Vector2.zero) return;
        sr.flipX = direction.x > 0f;
    }

    // =========================================================
    // エリア内判定・押し戻し
    // =========================================================
    bool IsInsideArea()
    {
        Vector2 pos = transform.position;
        float enterMargin = 125f;
        float innerLeft = areaLeft + enterMargin;
        float innerRight = areaRight - enterMargin;
        float innerBottom = areaBottom + enterMargin;

        return pos.x > innerLeft && pos.x < innerRight
            && pos.y > innerBottom && pos.y < areaTop;
    }

    void ClampToArea()
    {
        Vector2 pos = transform.position;
        bool reflected = false;

        if (pos.x <= areaLeft)
        {
            pos.x = areaLeft + 0.01f;
            direction.x = Mathf.Abs(direction.x);
            reflected = true;
        }
        else if (pos.x >= areaRight)
        {
            pos.x = areaRight - 0.01f;
            direction.x = -Mathf.Abs(direction.x);
            reflected = true;
        }

        if (pos.y <= areaBottom)
        {
            pos.y = areaBottom + 0.01f;
            direction.y = Mathf.Abs(direction.y);
            reflected = true;
        }
        else if (pos.y >= areaTop)
        {
            pos.y = areaTop - 0.01f;
            direction.y = -Mathf.Abs(direction.y);
            reflected = true;
        }

        if (reflected)
        {
            transform.position = pos;
            direction = direction.normalized;
        }
    }

    // =========================================================
    // IHitSlowable
    // =========================================================
    public void ApplyHitSlow()
    {
        slowTimer = hitSlowDuration;
        speedMultiplier = hitSlowMultiplier;
    }

    void UpdateHitSlow()
    {
        if (slowTimer <= 0f) { speedMultiplier = 1f; return; }
        slowTimer -= Time.deltaTime;
        speedMultiplier = (slowTimer <= 0f) ? 1f : hitSlowMultiplier;
    }

    void SetRandomDirection()
    {
        if (player != null)
        {
            direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
            return;
        }

        Vector2 random = Random.insideUnitCircle.normalized;
        Vector2 center = new Vector2((areaLeft + areaRight) * 0.5f, (areaTop + areaBottom) * 0.5f);
        Vector2 toCenter = (center - (Vector2)transform.position).normalized;
        direction = (random + toCenter * 0.15f).normalized;
    }

    // =========================================================
    // 影
    // =========================================================
    void UpdateShadow()
    {
        if (shadow == null) return;

        shadow.position = new Vector3(transform.position.x + shadowOffset.x, transform.position.y + shadowOffset.y, 0f);
        shadow.rotation = Quaternion.identity;

        if (shadowSR == null) return;

        Color c = shadowSR.color;
        c.a = 0.35f;
        shadowSR.color = c;
        shadow.localScale = new Vector3(shadowAirScale.x, shadowAirScale.y, 1f);
    }

    void FixShadow()
    {
        if (shadow == null) return;

        shadow.position = new Vector3(transform.position.x + shadowOffset.x, transform.position.y + shadowOffset.y, 0f);
        shadow.rotation = Quaternion.identity;
        shadow.localScale = new Vector3(shadowBaseScale.x, shadowBaseScale.y, 1f);

        if (shadowSR != null)
        {
            Color c = shadowSR.color;
            c.a = 0.5f;
            shadowSR.color = c;
        }
    }

    public void HideShadow()
    {
        if (shadow != null)
        {
            Destroy(shadow.gameObject);
            shadow = null;
        }

        if (telegraphVisual != null)
        {
            telegraphVisual.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (shadow != null) Destroy(shadow.gameObject);
        if (telegraphVisual != null) Destroy(telegraphVisual.gameObject);
    }
}