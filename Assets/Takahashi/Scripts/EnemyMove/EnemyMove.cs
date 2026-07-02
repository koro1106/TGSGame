using UnityEngine;

/// <summary>
/// ゴミ箱敵：アーチ移動・箱傾き・着地埋まり・待機モーション対応版
///
/// 【動きの流れ】
///   ① 画面外から画面内へアーチ状ジャンプで侵入（侵入方向へ飛ぶ）
///   ② 着地 → ウサギ・蓋が少し埋まってから戻る（着地エフェクト）
///   ③ 待機モーション（ウサギ・蓋だけバウンス、箱は固定）
///   ④ ランダム方向へアーチ状ジャンプ → ②へ戻る
///
/// 【箱の傾き】
///   ジャンプ前半：進行方向へ傾く（前のめり感）
///   ジャンプ後半：徐々に元に戻る
///   着地・待機中：完全に垂直へリセット
///
/// 【スプライト切り替え】
///   待機中 → ゴミ箱_前 / 蓋_前 / 耳_前 を表示
///   ジャンプ中 → ゴミ箱_後ろ / 蓋_後ろ / 耳_後ろ を表示
///   ウサギ・手は常に表示
///
/// 【手の挙動】
///   左右の手はウサギと完全連動（ウサギの基準位置からのズレ量をそのまま手に適用）
///
/// 【耳の待機モーション】
///   Yスケールは一切変更しない。回転（パタパタ）のみで揺れを表現する
///
/// 【オブジェクト構成】
///   TrashEnemy (親)
///     ├ Body_Back  (ゴミ箱_後ろ)
///     ├ Lid_Back   (蓋_後ろ)
///     ├ Ear_Back   (耳_後ろ)
///     ├ Rabbit     (ウサギ)
///     ├ HandRight  (右手)
///     ├ HandLeft   (左手)
///     ├ Body_Front (ゴミ箱_前)
///     ├ Ear_Front  (耳_前)
///     └ Lid_Front  (蓋_前)
/// </summary>
public class EnemyMove : MonoBehaviour, IHitSlowable
{
    // =========================================================
    // 状態管理
    // =========================================================
    enum State { Enter, Land, Wait, Jump }
    State state = State.Enter;
    Vector2 direction;

    // =========================================================
    // 移動パラメータ
    // =========================================================
    [Header("── 移動 ──────────────────")]
    public float jumpMoveSpeed = 2.5f;

    [Header("着地後の待機時間")]
    public float waitTimeMin = 0.3f;
    public float waitTimeMax = 0.8f;
    private float waitTimer;
    private float waitDuration;

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
    public float jumpHeight = 1.2f;
    public float jumpDuration = 0.5f;

    [Header("── 蓋 ──────────────────────")]
    public float lidOpenAngle = -40f;
    public float lidHeight = 0f;
    public float lidOpenSpeed = 8f;
    public float lidMoveSpeed = 10f;
    private float lidAngle;

    [Header("── ウサギ ───────────────────")]
    public float rabbitRiseHeight = 0.6f;
    public float rabbitRiseSpeed = 5f;

    [Header("── 耳の揺れ ──────────────────")]
    public float earSwingAngle = 25f;
    public float earSwingSpeed = 8f;
    public float earPhaseOffset = 1.0f;
    [Tooltip("待機中の耳パタパタ角度（ジャンプ中より控えめにするのが目安）")]
    public float earIdleSwingAngle = 8f;
    [Tooltip("待機中の耳パタパタ速度")]
    public float earIdleSwingSpeed = 3f;

    // =========================================================
    // 着地エフェクトパラメータ
    // =========================================================
    [Header("── 着地エフェクト ───────────────")]
    [Tooltip("着地時にウサギ・蓋が沈む深さ")]
    public float landSinkDepth = 0.15f;
    [Tooltip("沈み込みの速さ")]
    public float landSinkSpeed = 20f;
    [Tooltip("戻りの速さ")]
    public float landRiseSpeed = 8f;
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
    public float idleBobHeight = 0.06f;
    [Tooltip("バウンス速さ")]
    public float idleBobSpeed = 3f;

    private float idleTimer = 0f;

    // =========================================================
    // 箱傾きパラメータ
    // =========================================================
    [Header("── ジャンプ中の箱傾き ─────────────")]
    [Tooltip("最大傾き角度（度）。進行方向に傾く")]
    public float bodyTiltAngle = 18f;
    [Tooltip("傾きの補間速さ")]
    public float bodyTiltSpeed = 10f;

    private float currentBodyTilt = 0f;

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

        switch (state)
        {
            case State.Enter: UpdateEnter(); break;
            case State.Land: UpdateLand(); break;
            case State.Wait: UpdateWait(); break;
            case State.Jump: UpdateJump(); break;
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

        AnimateBodyLid(t, halfDur);
        AnimateEar(earSwingTimer);
        AnimateRabbit(halfDur);

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
        waitTimer += Time.deltaTime;
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
        for (int i = 0; i < ears.Length; i++)
        {
            if (ears[i] == null) continue;
            float swing = Mathf.Sin(idleTimer * earIdleSwingSpeed + i * earPhaseOffset) * earIdleSwingAngle;
            ears[i].localRotation = earBaseRot[i] * Quaternion.Euler(0f, 0f, swing);
        }

        if (waitTimer >= waitDuration)
        {
            SetRandomDirection();
            StartJump();
            FixShadow();
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

        AnimateBodyLid(t, halfDur);
        AnimateEar(earSwingTimer);
        AnimateRabbit(halfDur);

        FlipSprite();

        if (jumpTimer >= jumpDuration) EndJump();
        UpdateShadow();
    }

    // =========================================================
    // Body + Lid アニメーション
    // =========================================================
    void AnimateBodyLid(float t, float halfDur)
    {
        float bodyY = Mathf.Sin(t * Mathf.PI) * jumpHeight;

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
        lidAngle = Mathf.LerpAngle(lidAngle, targetAngle, Time.deltaTime * lidOpenSpeed);

        AnimateSingleLid(lidBack, lidBackBaseLocalPos, bodyY);
        AnimateSingleLid(lidFront, lidFrontBaseLocalPos, bodyY);
    }

    void AnimateSingleLid(Transform lid, Vector3 basePos, float bodyY)
    {
        if (lid == null) return;
        Vector3 targetPos = basePos + Vector3.up * (bodyY + lidHeight);
        lid.localPosition = Vector3.Lerp(lid.localPosition, targetPos, lidMoveSpeed * Time.deltaTime);
        lid.localEulerAngles = new Vector3(0f, 0f, lidAngle);
    }

    // =========================================================
    // 耳のパタパタ（ジャンプ中）
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
    // ウサギ・手の飛び出し（完全連動）
    // =========================================================
    void AnimateRabbit(float halfDur)
    {
        if (rabbit == null) return;

        Vector3 showPos = rabbitHideLocalPos + Vector3.up * rabbitRiseHeight;
        Vector3 rabbitTarget = (jumpTimer < halfDur) ? showPos : rabbitHideLocalPos;
        rabbit.localPosition = Vector3.MoveTowards(
            rabbit.localPosition, rabbitTarget, rabbitRiseSpeed * Time.deltaTime);

        // ウサギが基準位置からどれだけズレたかを計算し、
        // そのズレ量をそのまま手にも適用する（＝完全連動）
        Vector3 rabbitOffset = rabbit.localPosition - rabbitHideLocalPos;

        if (handRight != null) handRight.localPosition = handRightBaseLocalPos + rabbitOffset;
        if (handLeft != null) handLeft.localPosition = handLeftBaseLocalPos + rabbitOffset;
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

        for (int i = 0; i < ears.Length; i++)
        {
            if (ears[i] == null) continue;
            ears[i].localRotation = earBaseRot[i];
        }
    }

    // =========================================================
    // スプライト表示切り替え
    // =========================================================
    void SetSpritesForJump()
    {
        SetActive(bodyBackSR, true);
        SetActive(lidBackSR, true);
        SetActive(bodyFrontSR, false);
        SetActive(lidFrontSR, false);
        for (int i = 0; i < earSRs.Length; i++) SetActive(earSRs[i], true);
    }

    void SetSpritesForWait()
    {
        SetActive(bodyBackSR, false);
        SetActive(lidBackSR, false);
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
            pos.x = facingLeft ? -Mathf.Abs(earBaseLocalX[i]) : Mathf.Abs(earBaseLocalX[i]);
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
    }
}