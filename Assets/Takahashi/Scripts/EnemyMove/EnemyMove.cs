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
///   ウサギは常に表示
///
/// 【オブジェクト構成】
///   TrashEnemy (親)
///     ├ Body_Back  (ゴミ箱_後ろ)
///     ├ Lid_Back   (蓋_後ろ)
///     ├ Ear_Back   (耳_後ろ)
///     ├ Rabbit     (ウサギ)
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
    // Start
    // =========================================================
    void Start()
    {
        enemyHP = GetComponent<EnemyHP>();
        target = Vector2.zero;

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

        // 耳の初期値を記憶
        earBaseLocalX = new float[ears.Length];
        earBaseLocalY = new float[ears.Length]; 
        earBaseRot = new Quaternion[ears.Length];
        earSRs = new SpriteRenderer[ears.Length];
        for (int i = 0; i < ears.Length; i++)
        {
            if (ears[i] == null) continue;
            earBaseLocalX[i] = ears[i].localPosition.x;
            earBaseLocalY[i] = ears[i].localPosition.y; // ★必ず追加
            earBaseRot[i] = ears[i].localRotation;
            earSRs[i] = ears[i].GetComponent<SpriteRenderer>();
        }

        // 初期位置を記憶
        if (bodyBack != null) bodyBaseLocalPos = bodyBack.localPosition;
        if (lidBack != null) lidBackBaseLocalPos = lidBack.localPosition;
        if (lidFront != null) lidFrontBaseLocalPos = lidFront.localPosition;
        if (rabbit != null) rabbitHideLocalPos = rabbit.localPosition;

        // 侵入方向
        direction = ((Vector2)target - (Vector2)transform.position).normalized;

        StartJump();
        state = State.Enter;
        SetSpritesForJump();

        if (rabbitSR != null) rabbitSR.enabled = true;
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

        // アーチ移動（水平方向のみ移動し、Y方向はアニメで表現）
        transform.Translate((Vector3)direction * jumpMoveSpeed * speedMultiplier * Time.deltaTime);

        AnimateBodyLid(t, halfDur);
        AnimateEar(earSwingTimer);
        AnimateRabbit(halfDur);

        if (jumpTimer >= jumpDuration) jumpTimer = 0f;

        FlipSprite();

        if (IsInsideScreen()) EnterLand();

        UpdateShadow();
    }

    // =========================================================
    // 【Land】着地エフェクト
    // =========================================================
    void UpdateLand()
    {
        landTimer += Time.deltaTime;

        float progress = Mathf.Clamp01(landTimer / landDuration);

        // 前半: 沈み込み、後半: 戻り
        float sinkTarget;
        if (progress < 0.4f)
        {
            // 沈み込む
            sinkTarget = -landSinkDepth;
            rabbitLandOffset = Mathf.MoveTowards(rabbitLandOffset, sinkTarget, landSinkSpeed * Time.deltaTime);
            lidLandOffset = Mathf.MoveTowards(lidLandOffset, sinkTarget, landSinkSpeed * Time.deltaTime);
        }
        else
        {
            // 戻る
            rabbitLandOffset = Mathf.MoveTowards(rabbitLandOffset, 0f, landRiseSpeed * Time.deltaTime);
            lidLandOffset = Mathf.MoveTowards(lidLandOffset, 0f, landRiseSpeed * Time.deltaTime);
        }

        // ウサギを沈める
        if (rabbit != null)
            rabbit.localPosition = rabbitHideLocalPos + Vector3.up * rabbitLandOffset;

        // 蓋を沈める
        if (lidFront != null)
            lidFront.localPosition = lidFrontBaseLocalPos + Vector3.up * lidLandOffset;
        if (lidBack != null)
            lidBack.localPosition = lidBackBaseLocalPos + Vector3.up * lidLandOffset;

        if (landTimer >= landDuration)
        {
            // 着地エフェクト終了 → 待機へ
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

        // ★ 箱（Body）は固定。ウサギと蓋だけバウンス
        float bob = Mathf.Sin(idleTimer * idleBobSpeed) * idleBobHeight;

        // 蓋だけ上下
        if (lidFront != null) lidFront.localPosition = lidFrontBaseLocalPos + Vector3.up * bob;
        if (lidBack != null) lidBack.localPosition = lidBackBaseLocalPos + Vector3.up * bob;

        // ウサギも少し上下（蓋と同期）
        if (rabbit != null) rabbit.localPosition = rabbitHideLocalPos + Vector3.up * (bob * 0.5f);

        // 箱は BasePos に固定（念のためリセット）
        if (bodyFront != null) bodyFront.localPosition = bodyBaseLocalPos;
        if (bodyBack != null) bodyBack.localPosition = bodyBaseLocalPos;

        // UpdateWait 内の耳処理を差し替え
        for (int i = 0; i < ears.Length; i++)
        {
            if (ears[i] == null) continue;

            // 待機中はゆっくり小さくパタパタ
            float pata = (Mathf.Sin(idleTimer * (earSwingSpeed * 0.3f) + i * earPhaseOffset) + 1f) * 0.5f;
            float scaleY = Mathf.Lerp(0.7f, 1.0f, pata); // 待機中は控えめ
            Vector3 baseScale = ears[i].localScale;
            ears[i].localScale = new Vector3(baseScale.x, scaleY, baseScale.z);

            ears[i].localRotation = earBaseRot[i]; // 回転はリセット
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

        // ★ アーチ移動：directionへ移動し、Y方向の浮き沈みはアニメーション側で上乗せ
        transform.Translate((Vector3)direction * jumpMoveSpeed * speedMultiplier * Time.deltaTime);
        StayInScreen();

        AnimateBodyLid(t, halfDur);
        AnimateEar(earSwingTimer);
        AnimateRabbit(halfDur);

        FlipSprite();

        if (jumpTimer >= jumpDuration) EndJump();
        UpdateShadow();
    }

    // =========================================================
    // Body + Lid アニメーション（アーチ：Sin波でY移動 ＋ 箱傾き）
    // =========================================================
    void AnimateBodyLid(float t, float halfDur)
    {
        // アーチ：0→頂点→0 のSin曲線
        float bodyY = Mathf.Sin(t * Mathf.PI) * jumpHeight;

        // ★ 箱の傾き
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

        // ★ ウサギにも同じ傾きを適用
        if (rabbit != null)
            rabbit.localEulerAngles = bodyRot;

        // 蓋の開閉
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

            // 反転方向（右向きなら＋、左向きなら－）
            float dir = (direction.x >= 0f) ? -1f : 1f;

            float swing = Mathf.Sin(timer * earSwingSpeed + i * earPhaseOffset)
                          * earSwingAngle
                          * dir;

            ears[i].localRotation = Quaternion.Euler(0f, 0f, swing);
        }
    }

    // =========================================================
    // ウサギの飛び出し
    // =========================================================
    void AnimateRabbit(float halfDur)
    {
        if (rabbit == null) return;
        Vector3 showPos = rabbitHideLocalPos + Vector3.up * rabbitRiseHeight;
        Vector3 rabbitTarget = (jumpTimer < halfDur) ? showPos : rabbitHideLocalPos;
        rabbit.localPosition = Vector3.MoveTowards(
            rabbit.localPosition, rabbitTarget, rabbitRiseSpeed * Time.deltaTime);
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

        // まず見た目をリセットして前スプライトへ切り替え
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

    // ジャンプ終了 → 着地エフェクトへ
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
            rabbit.localEulerAngles = Vector3.zero;  // ★ 傾きリセット追加
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
        for (int i = 0; i < earSRs.Length; i++)
            SetActive(earSRs[i], true);
    }

    void SetSpritesForWait()
    {
        SetActive(bodyBackSR, false);
        SetActive(lidBackSR, false);
        SetActive(bodyFrontSR, true);
        SetActive(lidFrontSR, true);
        for (int i = 0; i < earSRs.Length; i++)
            SetActive(earSRs[i], true);
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

        for (int i = 0; i < ears.Length; i++)
        {
            Flip(earSRs[i], facingLeft);

            if (ears[i] == null) continue;
            Vector3 pos = ears[i].localPosition;
            pos.x = facingLeft
                ? -Mathf.Abs(earBaseLocalX[i])
                : Mathf.Abs(earBaseLocalX[i]);
            ears[i].localPosition = pos;
        }
    }

    void Flip(SpriteRenderer sr, bool flip)
    {
        if (sr != null) sr.flipX = flip;
    }

    // =========================================================
    // 画面内判定 / 反射
    // =========================================================
    bool IsInsideScreen()
    {
        Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
        float m = 0.05f;
        return vp.x > m && vp.x < 1 - m && vp.y > m && vp.y < 1 - m;
    }

    void StayInScreen()
    {
        Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
        float m = 0.05f;
        if (vp.x < m || vp.x > 1 - m) direction.x *= -1;
        if (vp.y < m || vp.y > 1 - m) direction.y *= -1;
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
        direction = Random.insideUnitCircle.normalized;
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

    // =========================================================
    // 影固定（Wait / Land 中）
    // =========================================================
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

    // =========================================================
    // 死亡時に影を消す
    // =========================================================
    public void HideShadow()
    {
        if (shadow != null)
        {
            Destroy(shadow.gameObject);
            shadow = null;
        }
    }

    // =========================================================
    // 削除時に影も削除
    // =========================================================
    void OnDestroy()
    {
        if (shadow != null)
        {
            Destroy(shadow.gameObject);
            shadow = null;
        }
    }
}