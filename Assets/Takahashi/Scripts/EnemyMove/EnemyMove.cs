using UnityEngine;

/// <summary>
/// ゴミ箱敵：ジャンプしながら移動するスクリプト（前後スプライト対応版）
///
/// 【動きの流れ】
///   ① 画面外から画面内へジャンプアニメ付きで侵入
///   ② 着地したら少し待つ（後ろスプライト表示）
///   ③ ランダムな方向へジャンプしながら移動（前スプライト表示）
///   ④ ②〜③を繰り返す
///
/// 【スプライト切り替え】
///   待機中 → ゴミ箱_後ろ / 蓋_後ろ / 耳_後ろ を表示
///   ジャンプ中 → ゴミ箱_前 / 蓋_前 / 耳_前 を表示
///   ウサギは常に表示
///
/// 【耳アニメーション】
///   ジャンプ中にぷらぷら回転（Sin波で揺れる）
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
    enum State { Enter, Wait, Jump }
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
    private Quaternion[] earBaseRot;
    private SpriteRenderer[] earSRs;

    // SpriteRenderer
    private SpriteRenderer bodyBackSR;
    private SpriteRenderer lidBackSR;
    private SpriteRenderer bodyFrontSR;
    private SpriteRenderer lidFrontSR;
    private SpriteRenderer rabbitSR;

    private EnemyHP enemyHP;

    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        enemyHP = GetComponent<EnemyHP>();
        target = Vector2.zero;

        // SpriteRenderer 取得
        if (bodyBack != null) bodyBackSR = bodyBack.GetComponent<SpriteRenderer>();
        if (lidBack != null) lidBackSR = lidBack.GetComponent<SpriteRenderer>();
        if (bodyFront != null) bodyFrontSR = bodyFront.GetComponent<SpriteRenderer>();
        if (lidFront != null) lidFrontSR = lidFront.GetComponent<SpriteRenderer>();
        if (rabbit != null) rabbitSR = rabbit.GetComponent<SpriteRenderer>();

        // 耳の初期値を記憶
        earBaseLocalX = new float[ears.Length];
        earBaseRot = new Quaternion[ears.Length];
        earSRs = new SpriteRenderer[ears.Length];
        for (int i = 0; i < ears.Length; i++)
        {
            if (ears[i] == null) continue;
            earBaseLocalX[i] = ears[i].localPosition.x;
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

        transform.Translate(direction * jumpMoveSpeed * speedMultiplier * Time.deltaTime);

        AnimateBodyLid(t, halfDur);
        AnimateEar(earSwingTimer);
        AnimateRabbit(halfDur);

        if (jumpTimer >= jumpDuration) jumpTimer = 0f;

        FlipSprite();

        if (IsInsideScreen()) EnterWait();
    }

    // =========================================================
    // 【Wait】待機
    // =========================================================
    void UpdateWait()
    {
        waitTimer += Time.deltaTime;
        if (waitTimer >= waitDuration)
        {
            SetRandomDirection();
            StartJump();
        }
    }

    // =========================================================
    // 【Jump】ジャンプ移動
    // =========================================================
    void UpdateJump()
    {
        jumpTimer += Time.deltaTime;
        earSwingTimer += Time.deltaTime;

        float t = Mathf.Clamp01(jumpTimer / jumpDuration);
        float halfDur = jumpDuration * 0.5f;

        transform.Translate(direction * jumpMoveSpeed * speedMultiplier * Time.deltaTime);
        StayInScreen();

        AnimateBodyLid(t, halfDur);
        AnimateEar(earSwingTimer);
        AnimateRabbit(halfDur);

        FlipSprite();

        if (jumpTimer >= jumpDuration) EndJump();
    }

    // =========================================================
    // Body + Lid アニメーション
    // =========================================================
    void AnimateBodyLid(float t, float halfDur)
    {
        float bodyY = Mathf.Sin(t * Mathf.PI) * jumpHeight;

        if (bodyBack != null) bodyBack.localPosition = bodyBaseLocalPos + Vector3.up * bodyY;
        if (bodyFront != null) bodyFront.localPosition = bodyBaseLocalPos + Vector3.up * bodyY;

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
    // 耳のぷらぷら揺れ
    // =========================================================
    void AnimateEar(float timer)
    {
        for (int i = 0; i < ears.Length; i++)
        {
            if (ears[i] == null) continue;
            // 耳ごとに位相をずらして揺れる
            float swing = Mathf.Sin(timer * earSwingSpeed + i * earPhaseOffset) * earSwingAngle;
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
    // 待機状態へ移行
    // =========================================================
    void EnterWait()
    {
        state = State.Wait;
        waitTimer = 0f;
        waitDuration = Random.Range(waitTimeMin, waitTimeMax);
        earSwingTimer = 0f;
        ResetParts();
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

    void EndJump() => EnterWait();

    // =========================================================
    // パーツリセット
    // =========================================================
    void ResetParts()
    {
        if (bodyBack != null) bodyBack.localPosition = bodyBaseLocalPos;
        if (bodyFront != null) bodyFront.localPosition = bodyBaseLocalPos;
        if (lidBack != null) lidBack.localPosition = lidBackBaseLocalPos;
        if (lidFront != null) lidFront.localPosition = lidFrontBaseLocalPos;
        if (rabbit != null) rabbit.localPosition = rabbitHideLocalPos;

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
        SetActive(bodyBackSR, true);   // ジャンプ中に表示
        SetActive(lidBackSR, true);
        SetActive(bodyFrontSR, false);
        SetActive(lidFrontSR, false);
        // 耳は常に表示
        for (int i = 0; i < earSRs.Length; i++)
            SetActive(earSRs[i], true);
    }

    void SetSpritesForWait()
    {
        SetActive(bodyBackSR, false);
        SetActive(lidBackSR, false);
        SetActive(bodyFrontSR, true);   // 待機中に表示
        SetActive(lidFrontSR, true);
        // 耳は常に表示
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

    void SetRandomDirection() => direction = Random.insideUnitCircle.normalized;
}