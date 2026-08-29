using UnityEngine;

/// <summary>
/// ゴミ箱敵：アーチ移動・箱傾き・着地埋まり・待機モーション・突進攻撃対応版
///
/// 【動きの流れ】
///   ① 画面外から画面内へアーチ状ジャンプで侵入（侵入方向へ飛ぶ）
///   ② 着地 → ウサギ・蓋が少し埋まってから戻る（着地エフェクト）
///   ③ 待機モーション（ウサギ・蓋だけバウンス、箱は固定）
///      → プレイヤーが検知円（detectRadius）内に入ったら突進攻撃へ
///   ④ 突進攻撃：プレイヤー方向へ高速直進 → 着地演出 → ②へ戻る
///      （検知円外、またはクールダウン中はランダム方向へアーチ状ジャンプ）
///
/// 【箱の傾き】
///   ジャンプ・突進前半：進行方向へ傾く（前のめり感）
///   ジャンプ・突進後半：徐々に元に戻る
///   着地・待機中：完全に垂直へリセット
///
/// 【スプライト切り替え】
///   待機中 → ゴミ箱_前 / 蓋_前 / 蓋_後ろ / 耳_前 を表示（ゴミ箱_後ろは使わない）
///   ジャンプ・突進中 → ゴミ箱_前 / 蓋_前 / 蓋_後ろ / 耳_後ろ を表示（ゴミ箱_後ろは使わない）
///   ウサギ・手は常に表示
///
/// 【手・耳の挙動】
///   左右の手、および耳はウサギと完全連動
///   （ウサギの基準位置からのY方向のズレ量をそのまま適用する）
///
/// 【耳の待機モーション】
///   Yスケールは一切変更しない。回転（パタパタ）のみで揺れを表現する
///   （Y位置はウサギのバウンスに連動）
///
/// 【プレイヤー参照】
///   PlayerMovement（本番）を優先し、無ければ Player（仮）を使う
///   （TryGetPlayer() で自動取得。Instanceが立っていればどちらのシーンでも動く）
///
/// 【オブジェクト構成】
///   TrashEnemy (親)
///     ├ Body_Back  (ゴミ箱_後ろ) ※未使用
///     ├ Lid_Back   (蓋_後ろ)
///     ├ Ear_Back   (耳_後ろ)
///     ├ Rabbit     (ウサギ)
///     ├ HandRight  (右手)
///     ├ HandLeft   (左手)
///     ├ Body_Front (ゴミ箱_前)
///     ├ Ear_Front  (耳_前)
///     ├ Lid_Front  (蓋_前)
///     └ DetectCircle (検知円。未設定なら自動生成)
///
/// 【2026/08 修正メモ】
///   ・突進判定(検知円チェック)を UpdateWait() 内だけでなく Update() 全体で
///     毎フレーム行うように変更。→ Jump(徘徊)中や着地演出待ちで反応が遅れる/
///     反応しない問題を解消。
///   ・Telegraph(溜め)中も毎フレーム狙い方向を更新し、突進開始の瞬間に
///     最新のプレイヤー位置で方向を再確定するように変更。
///     → 「プレイヤーがいた位置に向かって突進してしまう」問題を軽減。
/// </summary>
public class EnemyMove : MonoBehaviour, IHitSlowable
{
    // =========================================================
    // 状態管理
    // =========================================================
    enum State { Enter, Land, Wait, Jump, Charge, Telegraph }
    State state = State.Enter;
    Vector2 direction;

    // =========================================================
    // 移動パラメータ
    // =========================================================
    [Header("── 移動 ──────────────────")]
    public float jumpMoveSpeed = 75f;

    [Header("着地後の待機時間")]
    public float waitTimeMin = 0.3f;
    public float waitTimeMax = 0.8f;
    private float waitTimer;
    private float waitDuration;

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
    [Tooltip("突進中のジャンプの高さ（通常のjumpHeightとは別に設定可）")]
    public float chargeJumpHeight = 70f;
    [Tooltip("突進中の蓋の開閉スピード（通常のlidOpenSpeedとは別に設定可。突進は時間が短いので大きめ推奨）")]
    public float chargeLidOpenSpeed = 20f;
    [Tooltip("突進中の蓋の上下移動スピード（通常のlidMoveSpeedとは別に設定可。突進は時間が短いので大きめ推奨）")]
    public float chargeLidMoveSpeed = 24f;
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
    [Tooltip("狙いを示す赤いビームのPrefab（未設定でも動作します。その場合は見た目の予告なしでただ待つだけになります）")]
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
    // 画面上端を0、画面下端を1とした割合で指定
    // 上端（0=画面上端, 1=画面下端）
    public float moveAreaTopRatio = 0.45f;
    // 下端（0=画面上端, 1=画面下端）
    public float moveAreaBottomRatio = 1.0f;

    // =========================================================
    // 被弾鈍化
    // =========================================================
    [Header("被弾時の鈍化")]
    public float hitSlowMultiplier = 0.3f;
    public float hitSlowDuration = 0.5f;
    private float slowTimer = 0f;
    private float speedMultiplier = 1f;
    private Vector2 target;

    // =========================================================
    // 子オブジェクト参照
    // =========================================================
    [Header("── ジャンプ中に表示 ──────────")]
    public Transform bodyBack;
    public Transform lidBack;

    [Header("── 待機中に表示 ────────────")]
    public Transform bodyFront;
    public Transform lidFront;

    [Header("── 常に表示 ──────────────")]
    public Transform rabbit;

    [Header("── 手（ウサギと連動） ──────────")]
    public Transform handRight;
    public Transform handLeft;

    [Header("── 耳（何個でも登録可） ────────")]
    public Transform[] ears;

    // =========================================================
    // ジャンプアニメーションパラメータ
    // =========================================================
    [Header("── ジャンプアニメーション ──────")]
    public float jumpHeight = 50f;
    public float jumpDuration = 0.5f;

    [Header("── 蓋 ──────────────────────")]
    public float lidOpenAngle = -20f;
    public float lidHeight = 0f;
    public float lidOpenSpeed = 10f;
    public float lidMoveSpeed = 12f;
    private float lidAngle;

    [Header("── ウサギ ───────────────────")]
    public float rabbitRiseHeight = 250f;
    public float rabbitRiseSpeed = 150f;
    [Tooltip("突進中のウサギの飛び出し高さ（通常のrabbitRiseHeightとは別に設定可）")]
    public float chargeRabbitRiseHeight = 350f;
    [Tooltip("突進中のウサギが飛び出す速さ（通常のrabbitRiseSpeedとは別に設定可。突進は時間が短いので大きめ推奨）")]
    public float chargeRabbitRiseSpeed = 500f;

    [Header("── 耳の揺れ ──────────────────")]
    public float earSwingAngle = 25f;
    public float earSwingSpeed = 4.2f;
    public float earPhaseOffset = 1.5f;
    [Tooltip("待機中の耳パタパタ角度（ジャンプ中より控えめにするのが目安）")]
    public float earIdleSwingAngle = 8f;
    [Tooltip("待機中の耳パタパタ速度")]
    public float earIdleSwingSpeed = 3f;

    // =========================================================
    // 着地エフェクトパラメータ
    // =========================================================
    [Header("── 着地エフェクト ───────────────")]
    [Tooltip("着地時にウサギ・蓋が沈む深さ")]
    public float landSinkDepth = 5f;
    [Tooltip("沈み込みの速さ")]
    public float landSinkSpeed = 600f;
    [Tooltip("戻りの速さ")]
    public float landRiseSpeed = 240f;
    [Tooltip("着地エフェクト全体の時間")]
    public float landDuration = 0.35f;

    private float landTimer = 0f;
    private bool landSinking = true;
    private float rabbitLandOffset = 0f;
    private float lidLandOffset = 0f;

    // =========================================================
    // 待機モーションパラメータ（ウサギ・蓋のみ動く、箱は固定）
    // =========================================================
    [Header("── 待機モーション（ウサギ・蓋のみ動く） ──")]
    [Tooltip("ウサギ・蓋のバウンス量")]
    public float idleBobHeight = -20f;
    [Tooltip("バウンス速さ")]
    public float idleBobSpeed = 10f;

    private float idleTimer = 0f;

    // =========================================================
    // 箱傾きパラメータ
    // =========================================================
    [Header("── ジャンプ中の箱傾き ─────────────")]
    [Tooltip("最大傾き角度（度）。進行方向に傾く")]
    public float bodyTiltAngle = 15f;
    [Tooltip("傾きの補間速さ")]
    public float bodyTiltSpeed = 15f;

    private float currentBodyTilt = 0f;

    [Header("── デバッグ ──────────────────")]
    [Tooltip("Playモード中にこのキーを押すと、検知円やクールダウンを無視して即座に突進攻撃を1回実行")]
    public KeyCode debugTriggerKey = KeyCode.L;

    // =========================================================
    // 内部変数
    // =========================================================
    private float jumpTimer = 0f;
    private float earSwingTimer = 0f;

    private Vector3 bodyBaseLocalPos;
    private Vector3 lidBackBaseLocalPos;
    private Vector3 lidFrontBaseLocalPos;
    private Vector3 rabbitHideLocalPos;
    private Vector3 handRightBaseLocalPos;
    private Vector3 handLeftBaseLocalPos;

    // 耳の初期値
    private float[] earBaseLocalX;
    private float[] earBaseLocalY;
    private Quaternion[] earBaseRot;
    private SpriteRenderer[] earSRs;

    // SpriteRenderer
    private SpriteRenderer bodyBackSR;
    private SpriteRenderer lidBackSR;
    private SpriteRenderer bodyFrontSR;
    private SpriteRenderer lidFrontSR;
    private SpriteRenderer rabbitSR;
    private SpriteRenderer handRightSR;
    private SpriteRenderer handLeftSR;

    private EnemyHP enemyHP;

    [Header("── 影 ──────────────────────")]
    public GameObject shadowPrefab;

    [Header("影の位置オフセット（地面からのずれ）")]
    public Vector2 shadowOffset = new Vector2(0f, -0.1f);

    [Header("影のスケール")]
    public Vector2 shadowBaseScale = new Vector2(1f, 0.3f);
    public Vector2 shadowAirScale = new Vector2(0.5f, 0.15f);

    private Transform shadow;
    private SpriteRenderer shadowSR;

    // =========================================================
    // エリア境界キャッシュ（毎フレーム計算しないよう Start で確定）
    // =========================================================
    private float areaLeft;
    private float areaRight;
    private float areaTop;
    private float areaBottom;

    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        enemyHP = GetComponent<EnemyHP>();
        target = Vector2.zero;

        // 移動エリアのワールド座標を計算
        CalcAreaBounds();

        // プレイヤー自動取得（PlayerMovement優先、無ければPlayer）
        TryGetPlayer();

        // 検知円セットアップ
        if (showDetectCircle)
        {
            SetupDetectCircle();
        }

        // 狙いビームのセットアップ（普段は非表示にしておき、Telegraph中だけ表示・伸縮させる）
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

            shadow.position = new Vector3(
                transform.position.x + shadowOffset.x,
                transform.position.y + shadowOffset.y,
                0f
            );
            shadow.localScale = new Vector3(shadowBaseScale.x, shadowBaseScale.y, 1f);

            if (shadowSR != null)
            {
                Color c = shadowSR.color;
                c.a = 0.5f;
                shadowSR.color = c;
            }
        }

        // SpriteRenderer 取得
        if (bodyBack != null) bodyBackSR = bodyBack.GetComponent<SpriteRenderer>();
        if (lidBack != null) lidBackSR = lidBack.GetComponent<SpriteRenderer>();
        if (bodyFront != null) bodyFrontSR = bodyFront.GetComponent<SpriteRenderer>();
        if (lidFront != null) lidFrontSR = lidFront.GetComponent<SpriteRenderer>();
        if (rabbit != null) rabbitSR = rabbit.GetComponent<SpriteRenderer>();
        if (handRight != null) handRightSR = handRight.GetComponent<SpriteRenderer>();
        if (handLeft != null) handLeftSR = handLeft.GetComponent<SpriteRenderer>();

        // 耳の初期値を記憶
        earBaseLocalX = new float[ears.Length];
        earBaseLocalY = new float[ears.Length];
        earBaseRot = new Quaternion[ears.Length];
        earSRs = new SpriteRenderer[ears.Length];
        for (int i = 0; i < ears.Length; i++)
        {
            if (ears[i] == null) continue;
            earBaseLocalX[i] = ears[i].localPosition.x;
            earBaseLocalY[i] = ears[i].localPosition.y;
            earBaseRot[i] = ears[i].localRotation;
            earSRs[i] = ears[i].GetComponent<SpriteRenderer>();
        }

        // 初期位置を記憶
        if (bodyBack != null) bodyBaseLocalPos = bodyBack.localPosition;
        if (lidBack != null) lidBackBaseLocalPos = lidBack.localPosition;
        if (lidFront != null) lidFrontBaseLocalPos = lidFront.localPosition;
        if (rabbit != null) rabbitHideLocalPos = rabbit.localPosition;
        if (handRight != null) handRightBaseLocalPos = handRight.localPosition;
        if (handLeft != null) handLeftBaseLocalPos = handLeft.localPosition;

        direction = ((Vector2)target - (Vector2)transform.position).normalized;

        StartJump();
        state = State.Enter;
        SetSpritesForJump();

        if (rabbitSR != null) rabbitSR.enabled = true;
        if (handRightSR != null) handRightSR.enabled = true;
        if (handLeftSR != null) handLeftSR.enabled = true;
    }

    // =========================================================
    // プレイヤー取得（PlayerMovementがあれば優先、無ければPlayerを使う）
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

        if (Player.Instance != null)
        {
            player = Player.Instance.transform;
        }
    }

    // =========================================================
    // 検知円のセットアップ（LineRendererをローカル座標で子として生成）
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

    // 検知円の頂点を計算して反映する（半径を変えた時のためメソッド化）
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
    // エリア境界をワールド座標で計算
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
        // topRatio=0 → 画面上端、topRatio=1 → 画面下端
        areaTop = (camY + h) - fullH * moveAreaTopRatio;
        areaBottom = (camY + h) - fullH * moveAreaBottomRatio;
    }

    // =========================================================
    // Update
    // =========================================================
    void Update()
    {
        if (enemyHP != null && enemyHP.IsBind()) return;
        UpdateHitSlow();

        // デバッグ：指定キーで検知円やクールダウンを無視して即座に突進攻撃を1回実行
        if (debugTriggerKey != KeyCode.None && Input.GetKeyDown(debugTriggerKey))
        {
            DebugTriggerChargeAttack();
        }

        if (chargeCooldownTimer > 0f)
        {
            chargeCooldownTimer -= Time.deltaTime;
        }

        // ▼修正：距離チェックを Wait/Jump どちらの状態でも毎フレーム行う
        //   （UpdateWait内だけだと、Jump中や着地演出中にプレイヤーが検知円に
        //    入っても反応が遅れる・気づかないまま出ていってしまう問題があった）
        if ((state == State.Wait || state == State.Jump) && chargeCooldownTimer <= 0f)
        {
            TryGetPlayer();
            if (player != null)
            {
                float dist = Vector2.Distance(transform.position, player.position);
                if (dist <= detectRadius)
                {
                    EnterTelegraph();
                    return; // Telegraphへ切り替えたので今フレームのswitch処理は行わない
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

    // インスペクターの ⋮（右上の3点）または右クリックから「デバッグ：突進攻撃を今すぐ実行」でも呼べます
    // ※Playモード中のみ動作します
    [ContextMenu("デバッグ：突進攻撃を今すぐ実行")]
    public void DebugTriggerChargeAttack()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("EnemyMove: Playモード中のみデバッグ実行できます。");
            return;
        }

        TryGetPlayer();

        if (player == null)
        {
            Debug.LogWarning("EnemyMove: playerが未設定のため、デバッグ実行できません。");
            return;
        }

        // Enter/Land/Charge中の演出を壊さないよう、待機・徘徊・溜め中のみ割り込みを許可
        if (state == State.Wait || state == State.Jump || state == State.Telegraph)
        {
            chargeCooldownTimer = 0f;
            EnterTelegraph();
            Debug.Log("EnemyMove: デバッグキーで突進をトリガーしました。");
        }
        else
        {
            Debug.Log("EnemyMove: 現在の状態(" + state + ")では割り込めないためスキップしました。");
        }
    }

    // =========================================================
    // 【Enter】侵入
    // =========================================================
    void UpdateEnter()
    {
        jumpTimer += Time.deltaTime;
        earSwingTimer += Time.deltaTime;

        float t = Mathf.Clamp01(jumpTimer / jumpDuration);
        float halfDur = jumpDuration * 0.5f;

        transform.Translate((Vector3)direction * jumpMoveSpeed * speedMultiplier * Time.deltaTime);

        AnimateBodyLid(t, halfDur, jumpHeight, lidOpenSpeed, lidMoveSpeed);
        AnimateEar(earSwingTimer);
        AnimateRabbit(jumpTimer, halfDur, rabbitRiseHeight, rabbitRiseSpeed);

        if (jumpTimer >= jumpDuration) jumpTimer = 0f;

        FlipSprite();

        if (IsInsideArea()) EnterLand();

        UpdateShadow();
    }

    // =========================================================
    // 【Land】着地エフェクト
    // =========================================================
    void UpdateLand()
    {
        landTimer += Time.deltaTime;

        float progress = Mathf.Clamp01(landTimer / landDuration);

        if (progress < 0.4f)
        {
            float sinkTarget = -landSinkDepth;
            rabbitLandOffset = Mathf.MoveTowards(rabbitLandOffset, sinkTarget, landSinkSpeed * Time.deltaTime);
            lidLandOffset = Mathf.MoveTowards(lidLandOffset, sinkTarget, landSinkSpeed * Time.deltaTime);
        }
        else
        {
            rabbitLandOffset = Mathf.MoveTowards(rabbitLandOffset, 0f, landRiseSpeed * Time.deltaTime);
            lidLandOffset = Mathf.MoveTowards(lidLandOffset, 0f, landRiseSpeed * Time.deltaTime);
        }

        // ウサギ本体
        if (rabbit != null) rabbit.localPosition = rabbitHideLocalPos + Vector3.up * rabbitLandOffset;
        // 手はウサギの沈み込みにそのまま連動
        if (handRight != null) handRight.localPosition = handRightBaseLocalPos + Vector3.up * rabbitLandOffset;
        if (handLeft != null) handLeft.localPosition = handLeftBaseLocalPos + Vector3.up * rabbitLandOffset;

        // 耳もウサギの沈み込みに連動（置き去り防止）
        for (int i = 0; i < ears.Length; i++)
        {
            if (ears[i] == null) continue;
            Vector3 earPos = ears[i].localPosition;
            earPos.y = earBaseLocalY[i] + rabbitLandOffset;
            ears[i].localPosition = earPos;
        }

        if (lidFront != null) lidFront.localPosition = lidFrontBaseLocalPos + Vector3.up * lidLandOffset;
        if (lidBack != null) lidBack.localPosition = lidBackBaseLocalPos + Vector3.up * lidLandOffset;

        if (landTimer >= landDuration)
        {
            rabbitLandOffset = 0f;
            lidLandOffset = 0f;
            ResetParts();
            EnterWait();
        }

        FixShadow();
    }

    // =========================================================
    // 【Wait】待機
    // =========================================================
    void UpdateWait()
    {
        // プレイヤー未取得ならここでも再試行（Enemyの生成タイミング対策）
        TryGetPlayer();

        // ▼修正：距離チェックは Update() 側に一本化したのでここでは行わない
        //   （以前は EnterTelegraph() の呼び出しがここにもあり、Update側と
        //    重複していたため削除。バウンス演出のみ継続する）

        waitTimer += Time.deltaTime;
        PlayIdleBounce();

        if (waitTimer >= waitDuration)
        {
            SetRandomDirection();
            StartJump();
            FixShadow();
        }
    }

    // =========================================================
    // 待機中・予備動作中に共通のウサギ・蓋・耳バウンス演出
    // =========================================================
    void PlayIdleBounce()
    {
        idleTimer += Time.deltaTime;

        float bob = Mathf.Sin(idleTimer * idleBobSpeed) * idleBobHeight;

        if (lidFront != null) lidFront.localPosition = lidFrontBaseLocalPos + Vector3.up * bob;
        if (lidBack != null) lidBack.localPosition = lidBackBaseLocalPos + Vector3.up * bob;

        // ウサギの待機バウンス
        float rabbitBob = bob * 0.5f;
        if (rabbit != null) rabbit.localPosition = rabbitHideLocalPos + Vector3.up * rabbitBob;
        // 手はウサギと完全連動（同じ量だけ動く）
        if (handRight != null) handRight.localPosition = handRightBaseLocalPos + Vector3.up * rabbitBob;
        if (handLeft != null) handLeft.localPosition = handLeftBaseLocalPos + Vector3.up * rabbitBob;

        if (bodyFront != null) bodyFront.localPosition = bodyBaseLocalPos;
        if (bodyBack != null) bodyBack.localPosition = bodyBaseLocalPos;

        // 耳のパタパタ：Yスケールは変更せず、回転のみで表現する
        // ＋ ウサギの待機バウンスにY位置も連動させる（置き去り防止）
        for (int i = 0; i < ears.Length; i++)
        {
            if (ears[i] == null) continue;
            float swing = Mathf.Sin(idleTimer * earIdleSwingSpeed + i * earPhaseOffset) * earIdleSwingAngle;
            ears[i].localRotation = earBaseRot[i] * Quaternion.Euler(0f, 0f, swing);

            Vector3 earPos = ears[i].localPosition;
            earPos.y = earBaseLocalY[i] + rabbitBob;
            ears[i].localPosition = earPos;
        }
    }

    // =========================================================
    // 【Telegraph】突進前の予備動作（溜め）
    // =========================================================
    void EnterTelegraph()
    {
        state = State.Telegraph;
        telegraphTimer = 0f;

        // 狙いの初期方向（以降 UpdateTelegraph 内で毎フレーム更新される）
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

        // 待機中と同じバウンス演出で「溜めている感」を出す
        PlayIdleBounce();

        // ▼修正：溜めている間もプレイヤーを追い続け、狙い方向を更新する
        //   （以前は EnterTelegraph の瞬間の方向で固定していたため、
        //    プレイヤーが動くと「いた位置」に突進する原因になっていた）
        if (player != null)
        {
            telegraphDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }

        // ビームを0から目標の長さまでだんだん伸ばす
        // ※Spriteのpivotが中央の場合、scaleだけ伸ばすと前後に均等に伸びてしまうため、
        //   自分の位置(origin)を起点に、伸びた分の半分だけ狙った方向へpositionもずらしている。
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
            if (telegraphVisual != null)
            {
                telegraphVisual.gameObject.SetActive(false);
            }

            if (player != null)
            {
                // ▼修正：突進開始の瞬間、最新のプレイヤー位置でもう一度方向を確定する
                telegraphDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
                StartCharge();
            }
            else
            {
                // 溜めている間にプレイヤーがいなくなった場合は待機へ戻る
                EnterWait();
            }
        }
    }

    // =========================================================
    // 【Jump】ジャンプ移動（アーチ状）
    // =========================================================
    void UpdateJump()
    {
        jumpTimer += Time.deltaTime;
        earSwingTimer += Time.deltaTime;

        float t = Mathf.Clamp01(jumpTimer / jumpDuration);
        float halfDur = jumpDuration * 0.5f;

        transform.Translate((Vector3)direction * jumpMoveSpeed * speedMultiplier * Time.deltaTime);

        // エリア内に収める（反射 + 押し戻し）
        ClampToArea();

        AnimateBodyLid(t, halfDur, jumpHeight, lidOpenSpeed, lidMoveSpeed);
        AnimateEar(earSwingTimer);
        AnimateRabbit(jumpTimer, halfDur, rabbitRiseHeight, rabbitRiseSpeed);

        FlipSprite();

        if (jumpTimer >= jumpDuration) EndJump();
        UpdateShadow();
    }

    // =========================================================
    // 【Charge】突進攻撃
    // =========================================================
    void StartCharge()
    {
        state = State.Charge;
        chargeTimer = 0f;
        earSwingTimer = 0f;

        // Telegraph終了時点（＝直前のUpdateTelegraphで再確定済み）の方向を使う。
        // 万が一未設定なら念のため現在のプレイヤー位置から再計算する
        chargeDirection = (telegraphDirection != Vector2.zero)
            ? telegraphDirection
            : ((Vector2)player.position - (Vector2)transform.position).normalized;
        direction = chargeDirection; // 傾き演出・スプライト反転にも同じdirectionを使い回す

        if (lidBack != null) lidBack.localRotation = Quaternion.identity;
        if (lidFront != null) lidFront.localRotation = Quaternion.identity;

        SetSpritesForJump();
    }

    void UpdateCharge()
    {
        chargeTimer += Time.deltaTime;
        earSwingTimer += Time.deltaTime;

        float t = Mathf.Clamp01(chargeTimer / chargeDuration);
        float halfDur = chargeDuration * 0.5f;

        transform.Translate((Vector3)chargeDirection * chargeSpeed * speedMultiplier * Time.deltaTime);

        // エリア内に収める（反射 + 押し戻し）
        ClampToArea();

        // ジャンプ用の演出（箱の傾き・蓋・耳・ウサギ）をそのまま流用（高さ・速さだけ突進用に差し替え）
        AnimateBodyLid(t, halfDur, chargeJumpHeight, chargeLidOpenSpeed, chargeLidMoveSpeed);
        AnimateEar(earSwingTimer);
        AnimateRabbit(chargeTimer, halfDur, chargeRabbitRiseHeight, chargeRabbitRiseSpeed);

        FlipSprite();
        UpdateShadow();

        if (chargeTimer >= chargeDuration)
        {
            chargeCooldownTimer = chargeCooldown;
            EnterLand(); // 突進後は通常の着地演出を経て待機へ戻る
        }
    }

    // 突進中にプレイヤーへ接触した時の処理
    // ※プレイヤー側のダメージ受け取り方法が分からないため仮実装です。
    //   プレイヤーのコライダーがトリガーでない場合はOnCollisionEnter2Dに変更してください。
    void OnTriggerEnter2D(Collider2D other)
    {
        if (state != State.Charge) return;
        if (!other.CompareTag("Player")) return;

        // TODO: プレイヤー側のダメージ処理を呼び出す
        // 例）other.GetComponent<PlayerHP>()?.TakeDamage(attackDamage);
    }

    // =========================================================
    // Body + Lid アニメーション
    // =========================================================
    void AnimateBodyLid(float t, float halfDur, float heightOverride, float openSpeed, float moveSpeed)
    {
        float bodyY = Mathf.Sin(t * Mathf.PI) * heightOverride;

        float tiltDir = (direction.x > 0f) ? 1f : -1f;
        float tiltCurve = Mathf.Sin(t * Mathf.PI);
        float targetTilt = tiltDir * bodyTiltAngle * tiltCurve;
        currentBodyTilt = Mathf.LerpAngle(currentBodyTilt, targetTilt, Time.deltaTime * bodyTiltSpeed);

        Vector3 bodyRot = new Vector3(0f, 0f, currentBodyTilt);
        if (bodyBack != null)
        {
            bodyBack.localPosition = bodyBaseLocalPos + Vector3.up * bodyY;
            bodyBack.localEulerAngles = bodyRot;
        }
        if (bodyFront != null)
        {
            bodyFront.localPosition = bodyBaseLocalPos + Vector3.up * bodyY;
            bodyFront.localEulerAngles = bodyRot;
        }

        if (rabbit != null) rabbit.localEulerAngles = bodyRot;
        // 手もウサギ・箱と同じ傾きに連動させる
        if (handRight != null) handRight.localEulerAngles = bodyRot;
        if (handLeft != null) handLeft.localEulerAngles = bodyRot;

        float openAngle = (direction.x < 0f) ? lidOpenAngle : -lidOpenAngle;
        float targetAngle = (jumpTimer < halfDur) ? openAngle : 0f;
        lidAngle = Mathf.LerpAngle(lidAngle, targetAngle, Time.deltaTime * openSpeed);

        AnimateSingleLid(lidBack, lidBackBaseLocalPos, bodyY, moveSpeed);
        AnimateSingleLid(lidFront, lidFrontBaseLocalPos, bodyY, moveSpeed);
    }

    void AnimateSingleLid(Transform lid, Vector3 basePos, float bodyY, float moveSpeed)
    {
        if (lid == null) return;
        Vector3 targetPos = basePos + Vector3.up * (bodyY + lidHeight);
        lid.localPosition = Vector3.Lerp(lid.localPosition, targetPos, moveSpeed * Time.deltaTime);
        lid.localEulerAngles = new Vector3(0f, 0f, lidAngle);
    }

    // =========================================================
    // 耳のパタパタ（ジャンプ・突進中）
    // =========================================================
    void AnimateEar(float timer)
    {
        for (int i = 0; i < ears.Length; i++)
        {
            if (ears[i] == null) continue;
            float dir = (direction.x >= 0f) ? -1f : 1f;
            float swing = Mathf.Sin(timer * earSwingSpeed + i * earPhaseOffset) * earSwingAngle * dir;
            ears[i].localRotation = Quaternion.Euler(0f, 0f, swing);
        }
    }

    // =========================================================
    // ウサギ・手・耳の飛び出し（完全連動）
    // =========================================================
    void AnimateRabbit(float currentTimer, float halfDur, float riseHeight, float riseSpeed)
    {
        if (rabbit == null) return;

        Vector3 showPos = rabbitHideLocalPos + Vector3.up * riseHeight;
        Vector3 rabbitTarget = (currentTimer < halfDur) ? showPos : rabbitHideLocalPos;
        rabbit.localPosition = Vector3.MoveTowards(
            rabbit.localPosition, rabbitTarget, riseSpeed * Time.deltaTime);

        // ウサギが基準位置からどれだけズレたかを計算し、
        // そのズレ量をそのまま手・耳にも適用する（＝完全連動）
        Vector3 rabbitOffset = rabbit.localPosition - rabbitHideLocalPos;

        if (handRight != null) handRight.localPosition = handRightBaseLocalPos + rabbitOffset;
        if (handLeft != null) handLeft.localPosition = handLeftBaseLocalPos + rabbitOffset;

        // 耳もウサギのY方向の動きに連動（置き去り防止）
        // ※X位置はFlipSpriteで別途左右反転されるため、Yのみ更新する
        for (int i = 0; i < ears.Length; i++)
        {
            if (ears[i] == null) continue;
            Vector3 earPos = ears[i].localPosition;
            earPos.y = earBaseLocalY[i] + rabbitOffset.y;
            ears[i].localPosition = earPos;
        }
    }

    // =========================================================
    // 着地エフェクト開始
    // =========================================================
    void EnterLand()
    {
        state = State.Land;
        landTimer = 0f;
        landSinking = true;
        rabbitLandOffset = 0f;
        lidLandOffset = 0f;

        ResetParts();
        SetSpritesForWait();
        FixShadow();
    }

    // =========================================================
    // 待機状態へ移行
    // =========================================================
    void EnterWait()
    {
        state = State.Wait;
        waitTimer = 0f;
        idleTimer = 0f;
        waitDuration = Random.Range(waitTimeMin, waitTimeMax);
        earSwingTimer = 0f;
        SetSpritesForWait();
    }

    // =========================================================
    // ジャンプ開始
    // =========================================================
    void StartJump()
    {
        state = State.Jump;
        jumpTimer = 0f;
        earSwingTimer = 0f;

        if (lidBack != null) lidBack.localRotation = Quaternion.identity;
        if (lidFront != null) lidFront.localRotation = Quaternion.identity;

        SetSpritesForJump();
    }

    void EndJump() => EnterLand();

    // =========================================================
    // パーツリセット
    // =========================================================
    void ResetParts()
    {
        if (bodyBack != null)
        {
            bodyBack.localPosition = bodyBaseLocalPos;
            bodyBack.localEulerAngles = Vector3.zero;
        }
        if (bodyFront != null)
        {
            bodyFront.localPosition = bodyBaseLocalPos;
            bodyFront.localEulerAngles = Vector3.zero;
        }
        if (lidBack != null) lidBack.localPosition = lidBackBaseLocalPos;
        if (lidFront != null) lidFront.localPosition = lidFrontBaseLocalPos;
        if (rabbit != null)
        {
            rabbit.localPosition = rabbitHideLocalPos;
            rabbit.localEulerAngles = Vector3.zero;
        }
        if (handRight != null)
        {
            handRight.localPosition = handRightBaseLocalPos;
            handRight.localEulerAngles = Vector3.zero;
        }
        if (handLeft != null)
        {
            handLeft.localPosition = handLeftBaseLocalPos;
            handLeft.localEulerAngles = Vector3.zero;
        }

        currentBodyTilt = 0f;

        // 耳も基準位置・基準回転へリセット（置き去り防止）
        for (int i = 0; i < ears.Length; i++)
        {
            if (ears[i] == null) continue;
            ears[i].localRotation = earBaseRot[i];
            Vector3 earPos = ears[i].localPosition;
            earPos.y = earBaseLocalY[i];
            ears[i].localPosition = earPos;
        }
    }

    // =========================================================
    // スプライト表示切り替え
    // 待機中・ジャンプ中・突進中とも「箱前・蓋前・蓋後ろ」を表示する（箱後ろは未使用）
    // =========================================================
    void SetSpritesForJump()
    {
        SetActive(bodyBackSR, false);
        SetActive(lidBackSR, true);
        SetActive(bodyFrontSR, true);
        SetActive(lidFrontSR, true);
        for (int i = 0; i < earSRs.Length; i++) SetActive(earSRs[i], true);
    }

    void SetSpritesForWait()
    {
        SetActive(bodyBackSR, false);
        SetActive(lidBackSR, true);
        SetActive(bodyFrontSR, true);
        SetActive(lidFrontSR, true);
        for (int i = 0; i < earSRs.Length; i++) SetActive(earSRs[i], true);
    }

    void SetActive(SpriteRenderer sr, bool active)
    {
        if (sr != null) sr.enabled = active;
    }

    // =========================================================
    // 左右反転
    // =========================================================
    void FlipSprite()
    {
        if (direction == Vector2.zero) return;
        bool facingLeft = direction.x > 0f;

        Flip(bodyBackSR, facingLeft);
        Flip(lidBackSR, facingLeft);
        Flip(bodyFrontSR, facingLeft);
        Flip(lidFrontSR, facingLeft);
        Flip(rabbitSR, facingLeft);
        Flip(handRightSR, facingLeft);
        Flip(handLeftSR, facingLeft);

        for (int i = 0; i < ears.Length; i++)
        {
            Flip(earSRs[i], facingLeft);
            if (ears[i] == null) continue;
            Vector3 pos = ears[i].localPosition;
            // 各耳の元々の左右位置（符号）を保ったまま、向きが変わったら符号だけ反転
            pos.x = facingLeft ? -earBaseLocalX[i] : earBaseLocalX[i];
            ears[i].localPosition = pos;
        }
    }

    void Flip(SpriteRenderer sr, bool flip)
    {
        if (sr != null) sr.flipX = flip;
    }

    // =========================================================
    // エリア内判定（侵入完了チェック用）
    // =========================================================
    bool IsInsideArea()
    {
        Vector2 pos = transform.position;

        // 左右は「画面端より enterMargin だけ内側」まで入ったら着地とみなす
        // areaLeft/areaRight は画面端ぴったりなので、
        // スポーン直後（画面外2f）はここに引っかからない
        float enterMargin = 125f;
        float innerLeft = areaLeft + enterMargin;
        float innerRight = areaRight - enterMargin;

        // 下からスポーンした場合も areaBottom より少し内側まで入ってから着地
        float innerBottom = areaBottom + enterMargin;

        return pos.x > innerLeft && pos.x < innerRight
            && pos.y > innerBottom && pos.y < areaTop;
    }

    // =========================================================
    // エリア内に収める（反射 + 押し戻し）
    // 高速回転対策：反射後に必ずエリア内へ押し戻す
    // =========================================================
    void ClampToArea()
    {
        Vector2 pos = transform.position;
        bool reflected = false;

        // 左右
        if (pos.x <= areaLeft)
        {
            pos.x = areaLeft + 0.01f; // 押し戻し
            direction.x = Mathf.Abs(direction.x);  // 必ず右向きに
            reflected = true;
        }
        else if (pos.x >= areaRight)
        {
            pos.x = areaRight - 0.01f;
            direction.x = -Mathf.Abs(direction.x); // 必ず左向きに
            reflected = true;
        }

        // 上下（赤いエリアの範囲）
        if (pos.y <= areaBottom)
        {
            pos.y = areaBottom + 0.01f;
            direction.y = Mathf.Abs(direction.y);  // 必ず上向きに
            reflected = true;
        }
        else if (pos.y >= areaTop)
        {
            pos.y = areaTop - 0.01f;
            direction.y = -Mathf.Abs(direction.y); // 必ず下向きに
            reflected = true;
        }

        if (reflected)
        {
            transform.position = pos;
            // 反射後に direction を正規化して速度の変化を防ぐ
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
        // ▼▼▼ デバッグ用：一旦「必ずプレイヤー方向へ向かう」動きにしています ▼▼▼
        if (player != null)
        {
            direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
            Debug.Log("[EnemyMove] プレイヤー方向へ移動: " + direction);
            return;
        }
        else
        {
            Debug.Log("[EnemyMove] player が null のためランダム方向へ移動");
        }
        // ▲▲▲ ここまでデバッグ用 ▲▲▲

        // ランダム方向にほんの少しだけ中央寄りのバイアスをかける
        Vector2 random = Random.insideUnitCircle.normalized;

        // エリア中央のワールド座標
        Vector2 center = new Vector2(
            (areaLeft + areaRight) * 0.5f,
            (areaTop + areaBottom) * 0.5f
        );
        // 自分から中央への方向
        Vector2 toCenter = (center - (Vector2)transform.position).normalized;

        // 0.15 = バイアス強さ（0=完全ランダム、1=常に中央へ）
        direction = (random + toCenter * 0.15f).normalized;
    }

    // =========================================================
    // 影更新
    // =========================================================
    void UpdateShadow()
    {
        if (shadow == null) return;

        shadow.position = new Vector3(
            transform.position.x + shadowOffset.x,
            transform.position.y + shadowOffset.y,
            0f
        );
        shadow.rotation = Quaternion.identity;

        if (shadowSR == null) return;

        float bodyY = 0f;
        if (bodyBack != null)
            bodyY = bodyBack.localPosition.y - bodyBaseLocalPos.y;

        float t = Mathf.Clamp01(bodyY / Mathf.Max(jumpHeight, 0.001f));
        Vector2 scale = Vector2.Lerp(shadowBaseScale, shadowAirScale, t);
        shadow.localScale = new Vector3(scale.x, scale.y, 1f);

        Color c = shadowSR.color;
        c.a = Mathf.Lerp(0.5f, 0.1f, t);
        shadowSR.color = c;
    }

    void FixShadow()
    {
        if (shadow == null) return;

        shadow.position = new Vector3(
            transform.position.x + shadowOffset.x,
            transform.position.y + shadowOffset.y,
            0f
        );
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
    }

    void OnDestroy()
    {
        if (shadow != null)
        {
            Destroy(shadow.gameObject);
            shadow = null;
        }

        if (telegraphVisual != null)
        {
            Destroy(telegraphVisual.gameObject);
            telegraphVisual = null;
        }
    }
}