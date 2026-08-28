using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    // 追加：他のスクリプト（EnemyMoveなど）からはこのInstanceで参照する
    public static PlayerMovement Instance { get; private set; }

    [Header("移動速度")]
    public float moveSpeed = 5f;

    [Header("追従するクロスヘア(UI)")]
    public RectTransform crosshair;

    [Header("クロスヘアがあるCanvas")]
    public Canvas canvas;

    [Header("移動するシーン名")]
    public string gameOverSceneName;

    [Header("リザルト後に戻る位置")]
    public Transform startPoint;


    [Header("1回だけダメージを耐える")]
    // public bool enableSurvivalDamage = false;

    [Header("ダメージ点滅設定")]
    public float damageBlinkDuration = 1f;
    public float damageBlinkInterval = 0.1f;

    private bool hasSurvivedDamage = false;
    private bool isDamageBlinking = false;

    [Header("ダメージ時に点滅させるObject")]
    public GameObject damageBlinkObject;



    public GunController gunController;


    // =========================================================
    // ブリンク設定
    // =========================================================
    [Header("ブリンクを有効にする")]
    public bool enableBlink = true;

    [Header("ブリンク設定")]
    public float blinkMoveSpeed = 30f;

    [Header("ブリンク時間")]
    public float blinkDuration = 0.3f;

    [Header("ブリンククールダウン")]
    public float blinkCooldown = 8f;

    [Header("移動方向に向ける親Object")]
    public Transform playerImage;

    [Header("実際に表示するImage")]
    public Image playerDisplayImage;

    [Header("停止時の立ち画像")]
    public Sprite idleSprite;

    [Header("歩きアニメーション画像")]
    public Sprite[] walkFrames;

    [Header("走りアニメーション画像")]
    public Sprite[] runFrames;

    [Header("走り判定速度")]
    public float runSpeedThreshold = 300f;

    [Header("歩きアニメーション速度")]
    public float walkAnimationInterval = 0.1f;

    [Header("走りアニメーション速度")]
    public float runAnimationInterval = 0.06f;

    [Header("アニメーション速度")]
    public float animationInterval = 0.1f;

    [Header("左に移動した時に反転")]
    public bool flipWhenMovingLeft = true;

    private float animationTimer = 0f;
    private int currentFrame = 0;

    // 元のScaleを保存
    private Vector3 originalImageScale;

    // 前回の位置
    private Vector2 lastPosition;

    private bool isBlinking = false;
    private float blinkTimer = 0f;
    private float blinkCooldownTimer = 0f;

    private Rigidbody2D rb;
    private Camera cam;

    // リザルト復帰直後の移動を1回だけ止める
    private bool skipNextMove = false;

    private bool isDead = false;

    // ブリンク開始時の方向
    private Vector2 blinkDirection;

    public PlayerStats playerStats;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        if (playerImage != null)
        {
            originalImageScale = playerImage.localScale;
        }

        lastPosition = transform.position;

        if (
     playerDisplayImage != null &&
     idleSprite != null
 )
        {
            playerDisplayImage.sprite = idleSprite;
        }
    }


    void Update()
    {
        // ブリンク未開放なら即座に停止
        if (!playerStats.dash)
        {
            isBlinking = false;
            return;
        }

        // クールダウン
        if (blinkCooldownTimer > 0f)
        {
            blinkCooldownTimer -= Time.deltaTime;
        }

        // 右クリックでブリンク開始
        if (
            Input.GetMouseButtonDown(1) &&
            blinkCooldownTimer <= 0f &&
            !isDead &&
            !isBlinking
        )
        {
            StartBlink();
        }
    }





    // =========================================================
    // Playerを開始位置へ戻す
    // =========================================================

    public void ResetPlayerPosition()
    {
        Debug.Log("★★★★★ ResetPlayerPosition 呼ばれた ★★★★★");

        if (startPoint == null)
        {
            Debug.LogError(
                "★★★★★ startPoint が NULL です ★★★★★"
            );
            return;
        }

        Vector2 startPos = startPoint.position;

        Debug.Log(
            "★★★★★ 戻す位置 : " + startPos + " ★★★★★"
        );

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
            rb.position = startPos;
        }

        transform.position = new Vector3(
            startPos.x,
            startPos.y,
            transform.position.z
        );

        skipNextMove = true;
    }


    void FixedUpdate()
    {
        if (skipNextMove)
        {
            skipNextMove = false;
            return;
        }

        if (isDead)
            return;

        // ブリンク中
        if (isBlinking)
        {
            BlinkMove();
            return;
        }

        MoveToCrosshair();
    }

    void BlinkMove()
    {
        blinkTimer -= Time.fixedDeltaTime;

        rb.MovePosition(
            rb.position +
            blinkDirection *
            blinkMoveSpeed *
            Time.fixedDeltaTime
        );

        // ブリンク方向に画像を向ける
        UpdatePlayerImageDirection(
            blinkDirection
        );

        // ブリンク中もアニメーション
        UpdatePlayerAnimation(
     true,
     blinkMoveSpeed
 );
    }


    // =========================================================
    // クロスヘアへ移動
    // =========================================================

    void MoveToCrosshair()
    {
        if (gunController == null)
            return;

        Vector3 targetPos =
            gunController.GetCrosshairWorldPosition();

        targetPos.z =
            transform.position.z;

        // プレイヤーからクロスヘアへの方向
        Vector2 difference =
            (Vector2)targetPos - rb.position;

        // 移動しているか
        bool isMoving =
            difference.sqrMagnitude > 0.001f;

        // 移動方向
        Vector2 moveDirection =
            difference.normalized;

        // 移動速度
        float currentMoveSpeed =
     moveSpeed + playerStats.moveSpeed;

        float moveDistance =
            currentMoveSpeed *
            Time.fixedDeltaTime;

        // 移動
        rb.MovePosition(
            Vector2.MoveTowards(
                rb.position,
                targetPos,
                moveDistance
            )
        );

        // 左右の向きを変更
        if (isMoving)
        {
            UpdatePlayerImageDirection(
                moveDirection
            );
        }

        // 動いている間だけアニメーション
        UpdatePlayerAnimation(
    isMoving,
    currentMoveSpeed
);
    }


    // =========================================================
    // Enemy接触
    // =========================================================

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHP enemy =
            other.GetComponent<EnemyHP>();

        if (enemy == null)
            return;

        Die();
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        EnemyHP enemy =
            collision.gameObject.GetComponent<EnemyHP>();

        if (enemy == null)
            return;

        Die();
    }


    // =========================================================
    // 死亡
    // =========================================================

    void Die()
    {
        if (isDead)
            return;

        // 点滅中は無敵
        if (isDamageBlinking)
            return;

        // 1回だけ耐える
        if (playerStats.oneShotDurability &&
            !hasSurvivedDamage)
        {
            hasSurvivedDamage = true;

            StartCoroutine(
                DamageBlinkRoutine()
            );

            return;
        }

        // OFFの場合、または2回目以降
        isDead = true;

        if (ResultManager.Instance != null)
        {
            ResultManager.Instance.ShowResult();
        }
    }

    IEnumerator DamageBlinkRoutine()
    {
        if (damageBlinkObject == null)
        {
            Debug.LogWarning(
                "点滅させるObjectが設定されていません。"
            );

            isDamageBlinking = false;
            yield break;
        }

        isDamageBlinking = true;

        float timer = 0f;

        while (timer < damageBlinkDuration)
        {
            // 非表示
            damageBlinkObject.SetActive(false);

            yield return new WaitForSeconds(
                damageBlinkInterval
            );

            // 表示
            damageBlinkObject.SetActive(true);

            yield return new WaitForSeconds(
                damageBlinkInterval
            );

            timer += damageBlinkInterval * 2f;
        }

        // 最後は必ず表示状態に戻す
        damageBlinkObject.SetActive(true);

        isDamageBlinking = false;
    }


    // =========================================================
    // リザルトから復帰
    // =========================================================

    public void ResumeAfterResult()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
        }

        isDead = false;

        skipNextMove = true;
    }


    // =========================================================
    // ブリンクの残りクールダウン取得
    // =========================================================

    public float GetBlinkCooldownRemaining()
    {
        return Mathf.Max(0f, blinkTimer);
    }

    void StartBlink()
    {
        if (gunController == null)
            return;

        // ブリンク開始時のクロスヘア位置
        Vector3 crosshairPos =
            gunController.GetCrosshairWorldPosition();

        // 右クリックした瞬間の方向を保存
        blinkDirection =
            ((Vector2)crosshairPos - rb.position)
            .normalized;

        // クロスヘアとプレイヤーが完全に同じ位置の場合はブリンクしない
        if (blinkDirection == Vector2.zero)
            return;

        isBlinking = true;

        // ブリンク時間
        blinkTimer = blinkDuration;


        // クールダウン開始
        blinkCooldownTimer = blinkCooldown - playerStats.dashCT;
    }

    void UpdatePlayerImageDirection(
    Vector2 moveDirection
)
    {
        if (playerImage == null)
            return;

        // 横方向の移動がほとんどない場合は
        // 左右の向きを変えない
        if (Mathf.Abs(moveDirection.x) < 0.01f)
            return;

        Vector3 scale = originalImageScale;

        // 右方向
        if (moveDirection.x > 0f)
        {
            scale.x =
                Mathf.Abs(originalImageScale.x);
        }
        // 左方向
        else if (moveDirection.x < 0f)
        {
            scale.x =
                -Mathf.Abs(originalImageScale.x);
        }

        playerImage.localScale = scale;
    }

    void UpdatePlayerAnimation(
    bool isMoving,
    float currentMoveSpeed
)
    {
        if (playerDisplayImage == null)
            return;

        // =====================
        // 停止中
        // =====================
        if (!isMoving)
        {
            animationTimer = 0f;
            currentFrame = 0;

            if (idleSprite != null)
            {
                playerDisplayImage.sprite =
                    idleSprite;
            }

            return;
        }

        // =====================
        // 走りか歩きか判定
        // =====================
        bool isRunning =
            currentMoveSpeed >= runSpeedThreshold;

        Sprite[] currentFrames;
        float currentInterval;

        // =====================
        // 走り
        // =====================
        if (isRunning)
        {
            currentFrames = runFrames;
            currentInterval = runAnimationInterval;
        }
        // =====================
        // 歩き
        // =====================
        else
        {
            currentFrames = walkFrames;
            currentInterval = walkAnimationInterval;
        }

        // 画像が設定されていない場合
        if (
            currentFrames == null ||
            currentFrames.Length == 0
        )
        {
            return;
        }

        // =====================
        // アニメーション
        // =====================
        animationTimer += Time.fixedDeltaTime;

        if (animationTimer >= currentInterval)
        {
            animationTimer = 0f;

            currentFrame++;

            if (currentFrame >= currentFrames.Length)
            {
                currentFrame = 0;
            }

            playerDisplayImage.sprite =
                currentFrames[currentFrame];
        }
    }
}