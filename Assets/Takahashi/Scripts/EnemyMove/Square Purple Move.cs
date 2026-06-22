using UnityEngine;

/// <summary>
/// ゴミ箱敵：ジャンプしながら移動するスクリプト
///
/// 【動きの流れ】
///   ① 画面外から画面内へジャンプアニメ付きで侵入
///   ② 着地したら少し待つ
///   ③ ランダムな方向へジャンプしながら移動
///   ④ ②〜③を繰り返す
///
/// 【ジャンプ中のアニメーション】
///   ・Body（ゴミ箱本体）が放物線で上下する
///   ・Lid（蓋）がジャンプ前半に開き、後半に閉じる
///   ・Rabbit（ウサギ）が蓋の開閉に合わせて飛び出す（常に表示）
///
/// 【向きの制御】
///   ・SpriteRenderer.flipX で左右反転する
///   ・transform.rotation は使わない（Lid の角度アニメと干渉するため）
///
/// 【オブジェクト構成】
///   TrashEnemy (親)        ← このスクリプト + EnemyHP をアタッチ
///     ├ Body (ゴミ箱本体)  ← SpriteRenderer に本体画像
///     ├ Lid  (蓋)          ← SpriteRenderer に蓋画像
///     └ Rabbit (ウサギ)    ← SpriteRenderer にウサギ画像（常に表示）
/// </summary>
public class EnemyMove : MonoBehaviour, IHitSlowable
{
    // =========================================================
    // 状態管理
    // =========================================================

    /// <summary>敵の行動状態</summary>
    enum State
    {
        Enter, // 画面外から画面内へ侵入中（ジャンプアニメ付き）
        Wait,  // 着地して次のジャンプを待機中
        Jump   // ジャンプしながら移動中
    }
    State state = State.Enter;

    /// <summary>現在のジャンプ移動方向（正規化済みベクトル）</summary>
    Vector2 direction;

    // =========================================================
    // 移動パラメータ
    // =========================================================

    [Header("── 移動 ──────────────────")]
    /// <summary>侵入時の移動スピード</summary>
    public float enterSpeed = 4f;

    /// <summary>ジャンプ中の移動スピード</summary>
    public float jumpMoveSpeed = 2.5f;

    [Header("着地後の待機時間")]
    /// <summary>待機時間の最小値（秒）</summary>
    public float waitTimeMin = 0.3f;

    /// <summary>待機時間の最大値（秒）</summary>
    public float waitTimeMax = 0.8f;

    /// <summary>待機タイマー（経過時間を蓄積）</summary>
    private float waitTimer;

    /// <summary>今回の待機時間（ランダムで決定）</summary>
    private float waitDuration;

    // =========================================================
    // 被弾鈍化パラメータ
    // =========================================================

    [Header("被弾時の鈍化")]
    /// <summary>鈍化中の速度倍率（0 = 完全停止、1 = 鈍化なし）</summary>
    public float hitSlowMultiplier = 0.3f;

    /// <summary>鈍化が続く時間（秒）</summary>
    public float hitSlowDuration = 0.5f;

    /// <summary>鈍化の残り時間（0 以下で通常速度に戻る）</summary>
    private float slowTimer = 0f;

    /// <summary>現在の速度倍率（鈍化中は hitSlowMultiplier、通常時は 1）</summary>
    private float speedMultiplier = 1f;

    /// <summary>侵入先のターゲット座標（画面中央 = Vector2.zero）</summary>
    private Vector2 target;

    // =========================================================
    // 子オブジェクト参照
    // =========================================================

    [Header("── 子オブジェクト参照 ────────")]
    /// <summary>ゴミ箱本体（放物線で上下する）</summary>
    public Transform body;

    /// <summary>蓋（ジャンプ中に開閉する）</summary>
    public Transform lid;

    /// <summary>ウサギ（ジャンプ中に飛び出す・常に表示）</summary>
    public Transform rabbit;

    // =========================================================
    // ジャンプアニメーションパラメータ
    // =========================================================

    [Header("── ジャンプアニメーション ──────")]
    /// <summary>ジャンプの高さ（大きいほど高く跳ねる）</summary>
    public float jumpHeight = 1.2f;

    /// <summary>1回のジャンプにかかる時間（秒）</summary>
    public float jumpDuration = 0.5f;

    [Header("── 蓋 ──────────────────────")]
    /// <summary>蓋が開くときのZ角度</summary>
    public float lidOpenAngle = -40f;

    /// <summary>蓋の上下移動量</summary>
    public float lidHeight = 0f;

    /// <summary>蓋の開閉スピード</summary>
    public float lidOpenSpeed = 8f;

    private float lidAngle; // 現在の蓋角度

    [Header("蓋の移動速度")]
    public float lidMoveSpeed = 10f;

    [Header("── ウサギ ───────────────────")]
    /// <summary>ウサギが蓋から飛び出す高さ</summary>
    public float rabbitRiseHeight = 0.6f;

    /// <summary>ウサギの飛び出しスピード</summary>
    public float rabbitRiseSpeed = 5f;

    // =========================================================
    // アニメーション内部変数
    // =========================================================

    /// <summary>ジャンプ開始からの経過時間</summary>
    private float jumpTimer = 0f;

    // 各パーツの初期ローカル位置・回転（着地時に元へ戻すために記憶）
    private Vector3 bodyBaseLocalPos;
    private Vector3 lidBaseLocalPos;
    private Quaternion lidBaseRot;
    private Vector3 rabbitHideLocalPos; // ウサギが引っ込んでいるときのローカル位置
    private Vector3 rabbitShowLocalPos; // ウサギが飛び出したときのローカル位置

    // 各パーツの SpriteRenderer（flipX による左右反転に使う）
    private SpriteRenderer bodySR;
    private SpriteRenderer lidSR;
    private SpriteRenderer rabbitSR;

    // =========================================================
    // 外部参照
    // =========================================================

    /// <summary>EnemyHP コンポーネント（拘束中かどうかの判定に使う）</summary>
    private EnemyHP enemyHP;

    // =========================================================
    // Start：初期化
    // =========================================================
    void Start()
    {
        enemyHP = GetComponent<EnemyHP>();

        // 侵入先は画面中央
        target = Vector2.zero;

        // 各パーツの SpriteRenderer を取得（flipX で左右反転するため）
        if (body != null) bodySR = body.GetComponent<SpriteRenderer>();
        if (lid != null) lidSR = lid.GetComponent<SpriteRenderer>();
        if (rabbit != null) rabbitSR = rabbit.GetComponent<SpriteRenderer>();

        // アニメーション用に各パーツの初期位置・回転を記憶
        if (body != null)
            bodyBaseLocalPos = body.localPosition;

        if (lid != null)
        {
            lidBaseLocalPos = lid.localPosition;
            lidBaseRot = lid.localRotation;

            lidAngle = 0f;
        }

        if (rabbit != null)
        {
            rabbitHideLocalPos = rabbit.localPosition;
            // 飛び出し先は初期位置から上方向に rabbitRiseHeight 分
            rabbitShowLocalPos = rabbitHideLocalPos + Vector3.up * rabbitRiseHeight;
        }

        // 侵入方向を画面中央へ向けてセット
        direction = ((Vector2)target - (Vector2)transform.position).normalized;

        //    Enter 状態でもジャンプアニメを使うため、先に StartJump() を呼んでから
        //    state を Enter に上書きする
        StartJump();
        state = State.Enter;
    }

    // =========================================================
    // Update：毎フレーム処理
    // =========================================================
    void Update()
    {
        // 拘束中はすべての動きを止める
        if (enemyHP != null && enemyHP.IsBind()) return;

        // 鈍化タイマーを毎フレーム更新
        UpdateHitSlow();

        switch (state)
        {
            case State.Enter: UpdateEnter(); break;
            case State.Wait: UpdateWait(); break;
            case State.Jump: UpdateJump(); break;
        }
    }

    // =========================================================
    // 【Enter】画面中央へジャンプアニメ付きで侵入
    // =========================================================
    void UpdateEnter()
    {
        // ジャンプアニメーション用タイマーを進める
        jumpTimer += Time.deltaTime;

        // t : ジャンプの進行度（0 = 開始、1 = 終了）
        float t = Mathf.Clamp01(jumpTimer / jumpDuration);
        float halfDur = jumpDuration * 0.5f; // ジャンプの折り返し時間

        // 画面中央へ向かって移動
        transform.Translate(direction * enterSpeed * speedMultiplier * Time.deltaTime);

        // --- Body：Sin カーブで放物線ジャンプ ---
        if (body != null)
            body.localPosition = bodyBaseLocalPos
                + Vector3.up * (Mathf.Sin(t * Mathf.PI) * jumpHeight);

        // --- Lid：Body と同じ高さ + 前半に開き後半に閉じる ---
        if (lid != null)
        {
            Vector3 targetPos = lidBaseLocalPos +
                     Vector3.up * (Mathf.Sin(t * Mathf.PI) * jumpHeight + lidHeight);

            lid.localPosition = Vector3.Lerp(
                lid.localPosition,
                targetPos,
                lidMoveSpeed * Time.deltaTime);

            // 左向きなら -40°、右向きなら +40°
            float openAngle = (direction.x < 0f) ? lidOpenAngle : -lidOpenAngle;

            // ジャンプ前半は開く、後半は閉じる
            float targetAngle = (jumpTimer < halfDur) ? openAngle : 0f;

            float curZ = lid.localEulerAngles.z;
            float cur = (curZ > 180f) ? curZ - 360f : curZ;
            // 滑らか補間
            lidAngle = Mathf.LerpAngle(lidAngle, targetAngle, Time.deltaTime * lidOpenSpeed);

            // 反映
            lid.localEulerAngles = new Vector3(0f, 0f, lidAngle);
        }

        // --- Rabbit：前半に飛び出す → 後半に引っ込む ---
        if (rabbit != null)
        {
            Vector3 rabbitTarget = (jumpTimer < halfDur) ? rabbitShowLocalPos : rabbitHideLocalPos;
            rabbit.localPosition = Vector3.MoveTowards(
                rabbit.localPosition, rabbitTarget, rabbitRiseSpeed * Time.deltaTime);
        }

        // 1ジャンプ周期が終わったらタイマーをループ（画面内に入るまで繰り返す）
        if (jumpTimer >= jumpDuration)
            jumpTimer = 0f;

        // 進行方向に合わせて左右反転
        FlipSprite();

        // 画面内に入ったら待機状態へ移行
        if (IsInsideScreen())
            EnterWait();
    }

    // =========================================================
    // 【Wait】着地後の待機
    //   waitDuration 秒経過したらランダム方向へジャンプ開始
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
    // 【Jump】ジャンプしながら direction 方向へ移動
    // =========================================================
    void UpdateJump()
    {
        jumpTimer += Time.deltaTime;

        // t : ジャンプの進行度（0 = 開始、1 = 終了）
        float t = Mathf.Clamp01(jumpTimer / jumpDuration);
        float halfDur = jumpDuration * 0.5f; // ジャンプの折り返し時間

        // 移動（画面端で方向を反射）
        transform.Translate(direction * jumpMoveSpeed * speedMultiplier * Time.deltaTime);
        StayInScreen();

        // --- Body：Sin カーブで放物線ジャンプ ---
        if (body != null)
            body.localPosition = bodyBaseLocalPos
                + Vector3.up * (Mathf.Sin(t * Mathf.PI) * jumpHeight);

        // --- Lid：Body と同じ高さ + 前半に開き後半に閉じる ---
        if (lid != null)
        {
            Vector3 targetPos = lidBaseLocalPos +
                      Vector3.up * (Mathf.Sin(t * Mathf.PI) * jumpHeight + lidHeight);

            lid.localPosition = Vector3.Lerp(
                lid.localPosition,
                targetPos,
                lidMoveSpeed * Time.deltaTime);
            // 左向きなら -40°、右向きなら +40°
            float openAngle = (direction.x < 0f) ? lidOpenAngle : -lidOpenAngle;

            // ジャンプ前半は開く、後半は閉じる
            float targetAngle = (jumpTimer < halfDur) ? openAngle : 0f;

            float curZ = lid.localEulerAngles.z;
            float cur = (curZ > 180f) ? curZ - 360f : curZ;
            // 滑らか補間
            lidAngle = Mathf.LerpAngle(lidAngle, targetAngle, Time.deltaTime * lidOpenSpeed);

            // 反映
            lid.localEulerAngles = new Vector3(0f, 0f, lidAngle);
        }

        // --- Rabbit：前半に飛び出す → 後半に引っ込む ---
        if (rabbit != null)
        {
            Vector3 rabbitTarget = (jumpTimer < halfDur) ? rabbitShowLocalPos : rabbitHideLocalPos;
            rabbit.localPosition = Vector3.MoveTowards(
                rabbit.localPosition, rabbitTarget, rabbitRiseSpeed * Time.deltaTime);
        }

        // 進行方向に合わせて左右反転
        FlipSprite();

        // ジャンプ終了 → 待機へ
        if (jumpTimer >= jumpDuration)
            EndJump();
    }

    // =========================================================
    // 待機状態に入る（着地直後に呼ぶ）
    // =========================================================
    void EnterWait()
    {
        state = State.Wait;
        waitTimer = 0f;
        // 待機時間をランダムに決定
        waitDuration = Random.Range(waitTimeMin, waitTimeMax);
        // 各パーツを初期位置・回転にリセット
        ResetParts();

        if (lid != null)
            lid.localRotation = Quaternion.identity;
    }

    // =========================================================
    // ジャンプ開始
    // =========================================================
    void StartJump()
    {
        state = State.Jump;
        jumpTimer = 0f;

        // ジャンプ開始時だけリセット
        if (lid != null)
            lid.localRotation = Quaternion.identity;
    }

    // =========================================================
    // ジャンプ終了 → 待機へ
    // =========================================================
    void EndJump() => EnterWait();

    // =========================================================
    // 各パーツを初期位置・回転にリセット（着地時に呼ぶ）
    // =========================================================
    void ResetParts()
    {
        if (body != null) body.localPosition = bodyBaseLocalPos;

        if (lid != null)
        {
            lid.localPosition = lidBaseLocalPos;
            // 回転は触らない
        }

        if (rabbit != null)
            rabbit.localPosition = rabbitHideLocalPos;
    }

    // =========================================================
    // ランダムな方向ベクトルを設定する
    // =========================================================
    void SetRandomDirection() => direction = Random.insideUnitCircle.normalized;

    // =========================================================
    // 進行方向に合わせて左右反転する
    //
    //    transform.rotation（Z軸回転）は使わない。
    //    理由：Lid の localEulerAngles.z によるアニメーションと干渉して
    //          向きが意図しない方向になるため。
    //    代わりに SpriteRenderer.flipX で左右を反転させる。
    // =========================================================
    void FlipSprite()
    {
        if (direction == Vector2.zero) return;

        // 左へ移動なら反転
        bool facingLeft = direction.x > 0f;

        if (bodySR != null) bodySR.flipX = facingLeft;
        if (lidSR != null) lidSR.flipX = facingLeft;
        if (rabbitSR != null) rabbitSR.flipX = facingLeft;
    }

    // =========================================================
    // 画面内判定（ビューポート座標で判定）
    // =========================================================
    bool IsInsideScreen()
    {
        // ワールド座標をビューポート座標（0〜1）に変換して判定
        Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
        float m = 0.05f; // 端から 5% のマージン
        return vp.x > m && vp.x < 1 - m && vp.y > m && vp.y < 1 - m;
    }

    // =========================================================
    // 画面端で反射（画面外に出ないようにする）
    // =========================================================
    void StayInScreen()
    {
        Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
        float m = 0.05f; // 端から 5% のマージン

        // 左右端に当たったら X 方向を反転
        if (vp.x < m || vp.x > 1 - m) direction.x *= -1;
        // 上下端に当たったら Y 方向を反転
        if (vp.y < m || vp.y > 1 - m) direction.y *= -1;
    }

    // =========================================================
    // IHitSlowable：被弾時の鈍化
    // =========================================================

    /// <summary>
    /// EnemyHP から呼ばれる。被弾時に鈍化を開始する。
    /// </summary>
    public void ApplyHitSlow()
    {
        slowTimer = hitSlowDuration;
        speedMultiplier = hitSlowMultiplier;
    }

    /// <summary>
    /// 鈍化タイマーを毎フレーム更新する。
    /// タイマーが 0 以下になったら速度倍率を 1 に戻す。
    /// </summary>
    void UpdateHitSlow()
    {
        if (slowTimer <= 0f) { speedMultiplier = 1f; return; }

        slowTimer -= Time.deltaTime;
        speedMultiplier = (slowTimer <= 0f) ? 1f : hitSlowMultiplier;
    }
}