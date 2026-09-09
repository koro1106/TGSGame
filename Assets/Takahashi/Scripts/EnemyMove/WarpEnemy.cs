using UnityEngine;
using System.Collections;
public class WarpEnemy : MonoBehaviour, IHitSlowable
{
    [Header("移動")]
    public float moveSpeed = 3f;
    [Tooltip("playerが未設定の場合に使うフォールバック方向")]
    public Vector2 moveDirection = Vector2.left;

    [Header("プレイヤー")]
    [Tooltip("未設定ならStartで自動取得を試みます（EnemySpawnerから渡されるのでも可）")]
    public Transform player;

    // 実際に移動に使っている方向（プレイヤーがいればそちら、いなければmoveDirection）
    private Vector2 currentDirection;

    [Header("見た目（画像アニメーション）")]
    [Tooltip("再生したい画像を順番通りにここへドラッグ＆ドロップしてください")]
    public Sprite[] frames;
    [Tooltip("1秒間に何コマ切り替えるか")]
    public float frameRate = 24f;
    [Tooltip("未設定なら自分のGameObjectから自動取得します")]
    public SpriteRenderer spriteRenderer;
    [Tooltip("元画像が右向きに見える場合はチェックを入れてください（左向きが基準ならOFFのまま）")]
    public bool spriteFacesRightByDefault = false;

    private int currentFrame = 0;
    private float frameTimer = 0f;

    [Header("影（任意）")]
    public GameObject shadowObject; // 影オブジェクトがあれば設定。無ければ空のままでOK

    [Header("被弾時の鈍化")]
    public float hitSlowMultiplier = 0.3f; // 鈍化中の速度倍率（1fで鈍化なし、0fで完全停止）
    public float hitSlowDuration = 0.5f;   // 鈍化が続く時間（秒）

    private float slowTimer = 0f;          // 鈍化の残り時間
    private float speedMultiplier = 1f;    // 現在の速度倍率（鈍化中は1未満になる）

    private bool isAnimating = true; // falseにするとコマ送りが止まる（死亡時などに使用）

    //========================
    // 拘束
    //========================

    private Coroutine bindCoroutine;
    private bool isBind = false;

    private EnemyHP enemyHP;

    void Start()
    {
        // EnemyHPを取得し、拘束チェックと死亡イベントの両方に使う
        enemyHP = GetComponent<EnemyHP>();

        if (enemyHP != null)
        {
            // 死亡時に自分で影を消す（EnemyHP側は一切変更不要）
            enemyHP.OnDeath += HideShadow;
        }

        // EnemySpawnerから渡されていなければ自力で取得を試みる
        TryGetPlayer();

        // 見た目まわりの自動取得
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // 最初のコマ・向きを反映しておく
        if (frames != null && frames.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = frames[0];
        }

        currentDirection = moveDirection.normalized;
        FlipSprite();
    }

    void OnDestroy()
    {
        if (enemyHP != null)
        {
            enemyHP.OnDeath -= HideShadow;
        }
    }

    // =========================================================
    // プレイヤー自動取得（PlayerMovement優先、無ければPlayer）
    // EnemyMoveと同じ考え方。EnemySpawnerからすでに渡されていれば何もしない
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

    void Update()
    {
        //========================
        // 拘束中停止
        //========================

        if (enemyHP != null &&
   enemyHP.IsBind())
        {
            return;
        }

        // プレイヤー未取得ならここでも再試行（生成タイミング対策）
        if (player == null)
        {
            TryGetPlayer();
        }

        // 被弾鈍化の更新
        UpdateHitSlow();

        // プレイヤーがいればプレイヤー方向へ、いなければmoveDirectionへ移動
        if (player != null)
        {
            currentDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }
        else
        {
            currentDirection = moveDirection.normalized;
        }

        transform.position += (Vector3)currentDirection * moveSpeed * speedMultiplier * Time.deltaTime;

        FlipSprite();

        // 画像のコマ送り
        UpdateFrameAnimation();
    }

    // =========================================================
    // 画像のコマ送り（Animatorを使わず自前で切り替える）
    // =========================================================
    void UpdateFrameAnimation()
    {
        if (!isAnimating) return;
        if (frames == null || frames.Length == 0) return;
        if (spriteRenderer == null) return;
        if (frameRate <= 0f) return;

        frameTimer += Time.deltaTime;
        float frameInterval = 1f / frameRate;

        if (frameTimer >= frameInterval)
        {
            frameTimer -= frameInterval;
            currentFrame = (currentFrame + 1) % frames.Length;
            spriteRenderer.sprite = frames[currentFrame];
        }
    }

    // =========================================================
    // 左右反転（実際に移動している方向＝currentDirectionに合わせる）
    // =========================================================
    void FlipSprite()
    {
        if (spriteRenderer == null) return;
        if (currentDirection == Vector2.zero) return;

        bool movingRight = currentDirection.x > 0f;

        // 元画像が左向き基準ならmovingRight時にflip、右向き基準なら逆にする
        spriteRenderer.flipX = spriteFacesRightByDefault ? !movingRight : movingRight;
    }

    // 死亡時に呼ばれる（EnemyHPのOnDeathイベントから）
    public void HideShadow()
    {
        if (shadowObject != null)
        {
            shadowObject.SetActive(false);
        }

        // コマ送りも止めておく（死亡演出中に呼吸ループが続かないように）
        isAnimating = false;
    }

    // 被弾時に呼ぶ（EnemyHP側からの呼び出し用）
    public void ApplyHitSlow()
    {
        slowTimer = hitSlowDuration;
        speedMultiplier = hitSlowMultiplier;
    }

    // 鈍化タイマーの経過処理
    void UpdateHitSlow()
    {
        if (slowTimer <= 0f)
        {
            speedMultiplier = 1f;
            return;
        }

        slowTimer -= Time.deltaTime;

        if (slowTimer <= 0f)
        {
            speedMultiplier = 1f; // 鈍化終了、通常速度に戻す
        }
        else
        {
            speedMultiplier = hitSlowMultiplier; // 鈍化継続中
        }
    }
}