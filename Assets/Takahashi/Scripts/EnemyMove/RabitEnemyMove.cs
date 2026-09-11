using UnityEngine;

/// <summary>
/// Rabit（ゴミ箱敵・連番スプライト版）
///
/// 本格版 EnemyMove.cs の状態管理（侵入 → 着地 → 待機 → ジャンプ徘徊 →
/// 検知 → 突進）をそのまま移植し、見た目の演出だけを
/// 「箱・蓋・耳・ウサギを個別に動かすリグアニメーション」から
/// 「40枚の連番スプライトをコマ送りする方式」に置き換えたバージョンです。
///
/// 【動きの流れ】
///   ① 画面外から画面内へアーチ状ジャンプで侵入（Enter）
///   ② 着地（Land）→ 待機（Wait）でその場バウンス
///      → プレイヤーが検知円（detectRadius）内に入ったらビックリマーク演出へ
///   ③ 待機中はランダム方向へアーチ状ジャンプで徘徊（Jump）
///   ④ 検知したら頭上に「！」を出して少し待つ（Alert）→ 突進（Charge）→ 着地 → ②へ戻る
///
/// 【2026/09 修正メモ】
///   ・狙いを示す赤いビーム演出（Telegraph）を削除し、代わりに
///     頭上に「！」画像を表示してから少し待って突進する演出（Alert）を追加。
///     「！」はPrefab不要。Exclamation Framesに画像をドラッグするだけで
///     表示用オブジェクトを自動生成する（Prefabで見た目をカスタムしたい場合のみ
///     Exclamation Prefabを設定すればそちらが優先される）。
///   ・Sorting Order の自動計算方式を変更。
///     以前は「Y座標 × 倍率」で直接計算していたため、ワールド座標の値次第で
///     マイナスになったり数万という異常な値になったりして安定しなかった。
///     → 移動エリア(areaTop〜areaBottom)内でのY位置を0〜1に正規化してから
///        sortingOrderBase〜sortingOrderBase+sortingOrderRange の範囲に
///        収めるように変更。これで常に一定範囲（デフォルト100〜200）に収まり、
///        マイナスや異常値には絶対にならない。
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class RabitEnemyMove : MonoBehaviour, IHitSlowable
{
    // =========================================================
    // 状態管理
    // =========================================================
    enum State { Enter, Land, Wait, Jump, Alert, Charge }
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

    // =========================================================
    // 重なり順（点滅対策）
    // =========================================================
    [Header("── 重なり順（点滅対策） ──────────")]
    [Tooltip("ONにすると移動エリア内のY位置に応じてSorting Orderを自動計算し、敵同士が重なった時の点滅（描画順の入れ替わり）を防ぎます")]
    public bool autoSortByY = true;

    [Tooltip("Sorting Orderの基準値。常にこの値以上になります（例：100）")]
    public int sortingOrderBase = 100;

    [Tooltip("Sorting Orderの変動幅。sortingOrderBase 〜 sortingOrderBase+sortingOrderRange の範囲に収まります（例：range=100なら100〜200）。ほぼ固定にしたい場合はこの値を小さく（例：10〜20）してください")]
    public int sortingOrderRange = 100;

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
    // 突進前のビックリマーク演出
    // =========================================================
    [Header("── 突進前のビックリマーク演出 ──────────")]
    [Tooltip("（任意）頭上に出す「！」用のPrefab。未設定でも下のExclamation Framesに画像をドラッグするだけで自動的に表示用オブジェクトが作られます。演出をカスタムしたい場合だけ設定してください")]
    public GameObject exclamationPrefab;
    [Tooltip("「！」の連番スプライト（画像をここに順番通りドラッグするだけでOK。Prefab不要。1枚だけでも可）")]
    public Sprite[] exclamationFrames;
    [Tooltip("「！」アニメーションの再生速度（fps）")]
    public float exclamationFps = 12f;
    [Tooltip("5枚を1回再生し終えたあと、最後のコマのままどれだけ静止させておくか（秒）")]
    public float exclamationHoldDuration = 0.3f;
    [Tooltip("exclamationPrefabを使わない場合、自動生成される表示オブジェクトのSorting Order（敵本体より手前に出したい場合は大きめの値に）")]
    public int exclamationSortingOrder = 50;
    [Tooltip("「！」の表示サイズ（Prefabを使わない場合はこの値がそのままscaleになります。Prefab使用時は、そのPrefab本来のscaleに、この値を掛け合わせたサイズになります）")]
    public Vector3 exclamationSize = Vector3.one;
    [Tooltip("本体からの表示オフセット（頭上に出したい場合はYをプラスに）")]
    public Vector2 exclamationOffset = new Vector2(0f, 0.6f);
    [Tooltip("Exclamation Framesが未設定の場合に使う、突進までの待ち時間（秒）。Frames設定時はコマ送り+保持時間から自動計算されるため使われません")]
    public float alertDuration = 0.4f;
    [Tooltip("表示された瞬間、一瞬これだけ拡大してから元のサイズに戻る（1=拡大なし）")]
    public float exclamationPopScale = 1.3f;
    [Tooltip("拡大→元のサイズに戻るまでの時間（秒）。alertDurationより短くしてください")]
    public float exclamationPopDuration = 0.12f;

    private Transform exclamationVisual;
    private SpriteRenderer exclamationSR;
    private int exclamationFrameIndex;
    private float exclamationFrameTimer;
    private Vector3 exclamationBaseScale = Vector3.one;
    private float alertTimer = 0f;
    private float currentAlertDuration = 0f; // Enter Alert時に計算される、今回実際に使う待ち時間

    // =========================================================
    // 突進攻撃（狙い演出なし・検知した瞬間に即突進）
    // =========================================================
    [Header("── 突進攻撃（検知円に入ったらビックリマーク→突進） ──")]
    [Tooltip("未設定ならStartでPlayerMovement(優先)/Playerのシングルトンから自動取得")]
    public Transform player;
    [Tooltip("この距離までプレイヤーが近づいたら突進する（＝検知円の半径）")]
    public float detectRadius = 90f;
    [Tooltip("突進中の移動速度")]
    public float chargeSpeed = 150f;
    [Tooltip("突進を続ける時間（秒）")]
    public float chargeDuration = 0.4f;
    [Tooltip("突進後、再び突進判定を行えるようになるまでのクールダウン（秒）")]
    public float chargeCooldown = 1.2f;
    [Tooltip("突進がプレイヤーに当たった時のダメージ（プレイヤー側の受け取り実装が必要）")]
    public int attackDamage = 1;

    private float chargeTimer = 0f;
    private float chargeCooldownTimer = 0f;
    private Vector2 chargeDirection;

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

        if (shadowPrefab != null)
        {
            GameObject s = Instantiate(shadowPrefab);
            shadow = s.transform;
            shadowSR = s.GetComponent<SpriteRenderer>();
            FixShadow();
        }

        // ★変更：exclamationPrefabが未設定でも、exclamationFramesに画像さえ入っていれば
        //   表示用のGameObject(SpriteRenderer付き)をここで自動生成する。
        //   Prefabを用意する必要がなくなり、画像をドラッグするだけで使えるようになる。
        if (exclamationPrefab != null)
        {
            GameObject ex = Instantiate(exclamationPrefab, transform.position, Quaternion.identity);
            exclamationVisual = ex.transform;
            exclamationSR = ex.GetComponent<SpriteRenderer>();
            // PrefabのscaleにexclamationSizeを掛け合わせたものを基準サイズにする
            exclamationBaseScale = Vector3.Scale(exclamationVisual.localScale, exclamationSize);

            if (exclamationSR != null && exclamationFrames != null && exclamationFrames.Length > 0)
            {
                exclamationSR.sprite = exclamationFrames[0];
            }

            exclamationVisual.gameObject.SetActive(false);
        }
        else if (exclamationFrames != null && exclamationFrames.Length > 0)
        {
            GameObject ex = new GameObject("ExclamationMark");
            ex.transform.position = transform.position;

            exclamationSR = ex.AddComponent<SpriteRenderer>();
            exclamationSR.sprite = exclamationFrames[0];
            exclamationSR.sortingOrder = exclamationSortingOrder;

            exclamationVisual = ex.transform;
            exclamationBaseScale = exclamationSize; // 自動生成時はexclamationSizeがそのまま基準サイズになる

            exclamationVisual.gameObject.SetActive(false);
        }

        direction = ((Vector2)target - (Vector2)transform.position).normalized;

        StartJump();
        state = State.Enter;
    }

    void LateUpdate()
    {
        // ワールド座標のYに倍率を掛ける方式（-∞〜+∞になり得て不安定）をやめ、
        //   移動エリア(areaTop〜areaBottom)内でのY位置を0〜1に正規化してから
        //   sortingOrderBase〜sortingOrderBase+sortingOrderRange の範囲に収める方式に変更。
        //   これにより sortingOrder は必ず一定範囲に収まり、
        //   マイナスや数万といった異常な値には絶対にならない。
        if (autoSortByY && sr != null)
        {
            // areaTop（画面奥側）のとき0、areaBottom（画面手前側）のとき1になるように正規化
            // ※範囲外の値が来ても InverseLerp は自動で0〜1にクランプしてくれる
            float normalizedY = Mathf.InverseLerp(areaTop, areaBottom, transform.position.y);

            // 手前（画面下＝normalizedYが大きい）ほどOrderが大きくなる＝手前に描画される
            int order = sortingOrderBase + Mathf.RoundToInt(normalizedY * sortingOrderRange);
            sr.sortingOrder = order;
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
        if (enemyHP != null && enemyHP.IsDying()) return; // 死亡演出中は本体を一切動かさない（回転上書き防止）
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
                    EnterAlert();
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
            case State.Alert: UpdateAlert(); break;
            case State.Charge: UpdateCharge(); break;
        }
    }

    [ContextMenu("デバッグ：突進攻撃を今すぐ実行")]
    public void DebugTriggerChargeAttack()
    {
        if (!Application.isPlaying) return;
        TryGetPlayer();
        if (player == null) return;

        if (state == State.Wait || state == State.Jump)
        {
            chargeCooldownTimer = 0f;
            EnterAlert();
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
    // 【Alert】突進前にビックリマークを出して少し待つ
    // =========================================================
    void EnterAlert()
    {
        state = State.Alert;
        alertTimer = 0f;
        exclamationFrameIndex = 0;
        exclamationFrameTimer = 0f;

        // ★変更：Exclamation Framesが設定されている場合は、
        //   「5枚を1回再生し終えるのにかかる時間」＋「最後のコマを保持する時間」を
        //   自動計算して、それを今回の待ち時間として使う。
        //   未設定の場合は従来通りalertDurationをそのまま使う。
        if (exclamationFrames != null && exclamationFrames.Length > 0)
        {
            float frameDuration = 1f / Mathf.Max(1f, exclamationFps);
            currentAlertDuration = frameDuration * exclamationFrames.Length + exclamationHoldDuration;
        }
        else
        {
            currentAlertDuration = alertDuration;
        }

        if (exclamationVisual != null)
        {
            exclamationVisual.gameObject.SetActive(true);
            exclamationVisual.rotation = Quaternion.identity;
            exclamationVisual.position = transform.position + (Vector3)exclamationOffset;
            exclamationVisual.localScale = exclamationBaseScale;

            if (exclamationSR != null && exclamationFrames != null && exclamationFrames.Length > 0)
            {
                exclamationSR.sprite = exclamationFrames[0];
            }
        }
    }

    void UpdateAlert()
    {
        alertTimer += Time.deltaTime;
        AnimateFrames(idleFps);
        AnimateExclamation();

        if (exclamationVisual != null)
        {
            // 本体に追従（回転は継承させず常に真っ直ぐ表示）
            exclamationVisual.position = transform.position + (Vector3)exclamationOffset;
            exclamationVisual.rotation = Quaternion.identity;

            // 出た瞬間、一瞬拡大してから元のサイズへ戻る「ポン」という演出
            float popHalf = Mathf.Max(exclamationPopDuration * 0.5f, 0.0001f);
            if (alertTimer < popHalf)
            {
                float ratio = alertTimer / popHalf;
                exclamationVisual.localScale = Vector3.Lerp(exclamationBaseScale, exclamationBaseScale * exclamationPopScale, ratio);
            }
            else if (alertTimer < popHalf * 2f)
            {
                float ratio = (alertTimer - popHalf) / popHalf;
                exclamationVisual.localScale = Vector3.Lerp(exclamationBaseScale * exclamationPopScale, exclamationBaseScale, ratio);
            }
            else
            {
                exclamationVisual.localScale = exclamationBaseScale;
            }
        }

        if (alertTimer >= currentAlertDuration)
        {
            if (exclamationVisual != null) exclamationVisual.gameObject.SetActive(false);

            if (player != null)
            {
                StartCharge();
            }
            else
            {
                EnterWait();
            }
        }
    }

    // =========================================================
    // 【Charge】突進（狙い演出なし・検知した瞬間に即突進）
    // =========================================================
    void StartCharge()
    {
        state = State.Charge;
        chargeTimer = 0f;

        chargeDirection = (player != null)
            ? ((Vector2)player.position - (Vector2)transform.position).normalized
            : direction;
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

    // ビックリマーク（連番スプライト）のコマ送り。
    // ★変更：ループせず、最後のコマまで進んだらそこで止まる（静止して保持される）
    void AnimateExclamation()
    {
        if (exclamationSR == null || exclamationFrames == null || exclamationFrames.Length == 0) return;
        if (exclamationFrameIndex >= exclamationFrames.Length - 1) return; // 既に最後のコマ→これ以上進めない

        exclamationFrameTimer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(1f, exclamationFps);

        if (exclamationFrameTimer >= frameDuration)
        {
            exclamationFrameTimer -= frameDuration;
            exclamationFrameIndex++;
            exclamationSR.sprite = exclamationFrames[exclamationFrameIndex];
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

        if (exclamationVisual != null)
        {
            exclamationVisual.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (shadow != null) Destroy(shadow.gameObject);
        if (exclamationVisual != null) Destroy(exclamationVisual.gameObject);
    }
}