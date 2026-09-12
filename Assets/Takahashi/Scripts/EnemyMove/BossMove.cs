using UnityEngine;

/// <summary>
/// Boss（連番スプライト版・常時プレイヤー追尾＋検知円で狙いを定めて突進）
///
/// 【動きの流れ】
///   ① 画面外から画面内へ侵入（Enter）
///   ② 着地（Land）
///   ③ Follow：常にプレイヤーの方向へ向かって歩き続ける（待機・徘徊はしない）
///      → 検知円（detectRadius）内にプレイヤーが入ったら狙いを定める演出へ
///   ④ Telegraph：赤いビームでプレイヤー方向を狙う（aimDuration秒）
///   ⑤ 突進（Charge：狙いを定めた方向へ直進）→ 着地 → ③へ戻る
///
/// 【見た目】
///   全状態共通で Frames（連番）をループ再生。
///   状態ごとに再生速度（fps）だけ変えることで緩急をつけています。
///   移動中は進行方向へわずかに傾ける演出（bodyTilt）と
///   左右反転（flipX）で向きを表現します。
///
/// 【速度】
///   通常（侵入・追尾） → walkSpeed
///   突進              → chargeSpeed
///
/// 【2026/09 修正メモ】
///   ・ボス撃破の通知（EnemySpawner.BossDefeated()の呼び出し）が
///     EnemyHP.OnDeathイベントの発火だけに依存していたため、
///     何らかの理由でイベントが発火しない場合に通常敵が二度と
///     湧かなくなる不具合があった。
///     → OnDestroy()でも念のため同じ通知処理を呼ぶフェイルセーフを追加。
///        hasNotifiedDefeatフラグで二重通知は防止済み。
///   ・Telegraph（赤いビームで狙いを定めている）最中にボスが死亡すると、
///     Update()がenemyHP.IsDying()で即returnしてしまうため
///     UpdateTelegraph()内のビーム非表示処理が実行されず、
///     赤いビームが画面に残り続けてしまう不具合があった。
///     → HandleBossDeath()内で即座にtelegraphVisualを非表示にする処理を追加。
/// </summary>
[RequireComponent(typeof(EnemyHP))]
[RequireComponent(typeof(SpriteRenderer))]
public class BossMove : MonoBehaviour, IHitSlowable
{
    // =========================================================
    // 参照
    // =========================================================
    [Header("参照")]
    public Transform player; // プレイヤーのTransform（未設定ならタグ"Player"から自動取得）
    public EnemySpawner spawner; // ボス撃破をスポナーに通知するための参照（EnemySpawner.SpawnBoss()側で自動セット）

    // =========================================================
    // 状態管理（Rabitと同じ）
    // =========================================================
    enum State { Enter, Land, Follow, Telegraph, Charge }
    State state = State.Enter;
    Vector2 direction;

    // =========================================================
    // アニメーション（連番スプライト）
    // =========================================================
    [Header("── アニメーション用スプライト（連番を順番に） ──")]
    public Sprite[] frames;

    [Header("── 状態ごとの再生速度（fps） ──")]
    public float idleFps = 12f;   // 着地・待機・溜め中
    public float walkFps = 18f;   // 侵入・徘徊中
    public float chargeFps = 30f; // 突進中

    [Header("── 移動中の傾き演出（Transform回転で簡易再現） ──")]
    public float bodyTiltAngle = 8f;
    public float bodyTiltSpeed = 10f;
    private float currentBodyTilt = 0f;

    private SpriteRenderer sr;
    private int frameIndex;
    private float frameTimer;

    // =========================================================
    // 移動パラメータ（Rabitと同じ）
    // =========================================================
    [Header("── 移動（通常スピードはここ） ──────────")]
    [Tooltip("侵入時・徘徊時に使う通常スピード")]
    public float walkSpeed = 4f;

    [Header("着地にかかる時間")]
    public float landDuration = 0.2f;

    // =========================================================
    // 突進攻撃（狙い演出＋突進）
    // =========================================================
    [Header("狙いを定める演出（画像の赤いビーム）")]
    public GameObject telegraphPrefab;          // 狙いを示す赤いビームのPrefab（ここにPrefabアセットをドラッグ）
    public float telegraphLength = 8f;          // ビームが伸びきったときの長さ
    public float aimDuration = 1f;              // 狙いを定めている時間（この間にプレイヤーは避ける準備ができる）

    private Transform telegraphVisual;          // ↑のPrefabを実際にInstantiateしたシーン上の実体
    private float telegraphTimer = 0f;
    private Vector2 telegraphDirection;

    [Header("── 突進攻撃（検知円に入ったら狙いを定めてから突進） ──")]
    [Tooltip("この距離までプレイヤーが近づいたら突進する（＝検知円の半径）")]
    public float detectRadius = 4f;
    [Tooltip("突進スピード")]
    public float chargeSpeed = 18f;
    [Tooltip("突進を続ける時間（秒）")]
    public float chargeDuration = 0.6f;
    [Tooltip("突進後、再び突進判定を行えるようになるまでのクールダウン（秒）")]
    public float chargeCooldown = 1.2f;
    [Tooltip("突進が当たった時のダメージ")]
    public int chargeDamage = 20;
    [Tooltip("突進中の当たり判定用レイヤー")]
    public LayerMask playerLayer;
    [Tooltip("突進中の当たり判定の半径")]
    public float chargeHitRadius = 0.6f;

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
    // 移動エリア制限（Rabitと同じ）
    // =========================================================
    [Header("── 移動エリア ─────────────────")]
    public float moveAreaTopRatio = 0f;
    public float moveAreaBottomRatio = 1.0f;

    private float areaLeft, areaRight, areaTop, areaBottom;

    // =========================================================
    // 被弾鈍化
    // =========================================================
    [Header("── 被弾時の鈍化 ────────────────")]
    public float hitSlowMultiplier = 0.3f;
    public float hitSlowDuration = 0.5f;
    private float slowTimer = 0f;
    private float speedMultiplier = 1f;

    [Header("── デバッグ ──────────────────")]
    [Tooltip("Playモード中にこのキーを押すと、検知円やクールダウンを無視して即座に突進攻撃を1回実行")]
    public KeyCode debugTriggerKey = KeyCode.T;

    private float landTimer = 0f;

    private EnemyHP enemyHP;

    // =========================================================
    // ★追加：ボス撃破通知の二重呼び出し防止フラグ
    //   EnemyHP.OnDeath経由の通知とOnDestroy()のフェイルセーフ通知が
    //   両方とも発火した場合に、BossDefeated()が2回呼ばれないようにする
    // =========================================================
    private bool hasNotifiedDefeat = false;

    // =========================================================
    // 影
    // =========================================================
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

        // ボスのHPが0になって死亡演出が始まった瞬間、スポナーへ通知する
        enemyHP.OnDeath += HandleBossDeath;

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

        // 侵入方向は「スポーン位置 → 移動エリア中心」へ自動計算
        // （画面のどちら側にスポーンしても正しく画面内へ向かって侵入してくる）
        Vector2 areaCenter = new Vector2((areaLeft + areaRight) * 0.5f, (areaTop + areaBottom) * 0.5f);
        Vector2 toCenter = areaCenter - (Vector2)transform.position;
        direction = (toCenter.sqrMagnitude > 0.0001f) ? toCenter.normalized : Vector2.left;

        state = State.Enter;
    }

    void HandleBossDeath()
    {
        // ★追加：死亡した瞬間に赤いチャージ（狙いビーム）が残らないよう即座に非表示にする
        //   Telegraph状態の途中で死亡すると、Update()がenemyHP.IsDying()で
        //   即returnしてしまいUpdateTelegraph()側の非表示処理が実行されないため、
        //   ここで確実に消す。
        if (telegraphVisual != null)
        {
            telegraphVisual.gameObject.SetActive(false);
        }

        // ★変更：二重通知を防ぐガードを追加
        if (hasNotifiedDefeat) return;
        hasNotifiedDefeat = true;

        if (spawner != null)
        {
            spawner.BossDefeated();
        }
    }

    // =========================================================
    // プレイヤー取得
    // =========================================================
    void TryGetPlayer()
    {
        if (player != null) return;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            Debug.Log("Bossが見つけたPlayer: " + p.name + " / 座標: " + p.transform.position);
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

        // 安全策：万が一比率の設定次第でareaTop/areaBottomが逆転しても
        // 押し戻しが無限ループしないように入れ替えておく
        if (areaTop < areaBottom)
        {
            float tmp = areaTop;
            areaTop = areaBottom;
            areaBottom = tmp;
        }
    }

    // =========================================================
    // Update
    // =========================================================
    void Update()
    {
        if (enemyHP != null && enemyHP.IsBind()) return;
        if (enemyHP != null && enemyHP.IsDying()) return; // 死亡演出中は本体を一切動かさない

        UpdateHitSlow();

        if (debugTriggerKey != KeyCode.None && Input.GetKeyDown(debugTriggerKey))
        {
            DebugTriggerChargeAttack();
        }

        if (chargeCooldownTimer > 0f) chargeCooldownTimer -= Time.deltaTime;

        // 検知円のサイズをリアルタイムで反映
        if (showDetectCircle && detectCircleRenderer != null)
        {
            DrawDetectCircle();
        }

        // ── 検知判定（Follow中のみ） ──
        if (state == State.Follow && chargeCooldownTimer <= 0f)
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
            case State.Follow: UpdateFollow(); break;
            case State.Telegraph: UpdateTelegraph(); break;
            case State.Charge: UpdateCharge(); break;
        }
    }

    [ContextMenu("デバッグ：突進攻撃を今すぐ実行")]
    public void DebugTriggerChargeAttack()
    {
        if (!Application.isPlaying) return;
        TryGetPlayer();
        if (player == null) return;

        if (state == State.Follow)
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
        transform.Translate((Vector3)direction * walkSpeed * speedMultiplier * Time.deltaTime);

        AnimateFrames(walkFps);
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

        if (landTimer >= landDuration) EnterFollow();
    }

    // =========================================================
    // 【Follow】常時プレイヤー追尾
    // =========================================================
    void UpdateFollow()
    {
        TryGetPlayer();

        if (player != null)
        {
            direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }

        transform.Translate((Vector3)direction * walkSpeed * speedMultiplier * Time.deltaTime);

        ClampToArea();
        AnimateFrames(walkFps);
        ApplyTilt();
        FlipSprite();
        UpdateShadow();
    }

    // =========================================================
    // 【Telegraph】突進前に狙いを定める（赤いビーム演出）
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
        direction = telegraphDirection;
        FlipSprite();

        if (telegraphVisual != null)
        {
            float ratio = Mathf.Clamp01(telegraphTimer / aimDuration);
            float currentLength = telegraphLength * ratio;

            float angle = Mathf.Atan2(telegraphDirection.y, telegraphDirection.x) * Mathf.Rad2Deg;
            telegraphVisual.rotation = Quaternion.Euler(0f, 0f, angle);

            Vector3 baseScale = telegraphVisual.localScale;
            telegraphVisual.localScale = new Vector3(currentLength, baseScale.y, baseScale.z);
            telegraphVisual.position = transform.position + (Vector3)(telegraphDirection * currentLength * 0.5f);
        }

        if (telegraphTimer >= aimDuration)
        {
            if (telegraphVisual != null) telegraphVisual.gameObject.SetActive(false);

            if (player != null)
            {
                telegraphDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
                StartCharge();
            }
            else
            {
                EnterFollow();
            }
        }
    }

    // =========================================================
    // 【Charge】突進（狙いを定めた方向へ直進）
    // =========================================================
    void StartCharge()
    {
        state = State.Charge;
        chargeTimer = 0f;

        chargeDirection = telegraphDirection;
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

        Collider2D hitPlayer = Physics2D.OverlapCircle(transform.position, chargeHitRadius, playerLayer);
        if (hitPlayer != null)
        {
            hitPlayer.SendMessage("TakeDamage", chargeDamage, SendMessageOptions.DontRequireReceiver);
        }

        if (chargeTimer >= chargeDuration)
        {
            chargeCooldownTimer = chargeCooldown;
            EnterLand();
        }
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

    // 移動方向へわずかに傾ける簡易演出
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

    void EnterFollow()
    {
        state = State.Follow;
    }

    // =========================================================
    // 左右反転
    // =========================================================
    void FlipSprite()
    {
        if (direction == Vector2.zero) return;
        sr.flipX = direction.x > 0f;
    }

    // =========================================================
    // エリア内判定・押し戻し（Rabitと同じ：方向も反射させる）
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
            reflected = true;
        }
        else if (pos.x >= areaRight)
        {
            pos.x = areaRight - 0.01f;
            reflected = true;
        }

        if (pos.y <= areaBottom)
        {
            pos.y = areaBottom + 0.01f;
            reflected = true;
        }
        else if (pos.y >= areaTop)
        {
            pos.y = areaTop - 0.01f;
            reflected = true;
        }

        if (reflected)
        {
            transform.position = pos;
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

        // ★追加：フェイルセーフ。
        // 何らかの理由でEnemyHP.OnDeathが発火せずHandleBossDeath()が
        // 呼ばれないまま、ボスのGameObjectが破棄されてしまった場合の保険。
        // これがあれば「ボスは消えたのに通常敵が二度と湧かない」という
        // 事態を確実に防げる（hasNotifiedDefeatで二重通知も防止済み）。
        HandleBossDeath();
    }

    // =========================================================
    // 編集モードでもSceneビューで検知円のサイズを確認できるギズモ
    // =========================================================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = detectCircleColor;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chargeHitRadius);
    }
}