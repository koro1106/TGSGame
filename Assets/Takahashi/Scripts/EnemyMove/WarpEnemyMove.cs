using UnityEngine;
using System.Collections;
public class WarpEnemyMove : MonoBehaviour, IHitSlowable
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

    // =========================================================
    // ★追加：重なり順（点滅対策）
    //   RabitEnemyMove / EnemyMove と同じ問題（Y座標に倍率を掛けると
    //   マイナスや異常値になって不安定）を防ぐため、移動エリア内での
    //   相対位置(0〜1)から一定範囲のsortingOrderを計算する方式を追加。
    // =========================================================
    [Header("── 重なり順（点滅対策） ──────────")]
    [Tooltip("ONにすると画面内のY位置に応じてSorting Orderを自動計算し、敵同士が重なった時の点滅（描画順の入れ替わり）を防ぎます")]
    public bool autoSortByY = true;

    [Tooltip("Sorting Orderの基準値。常にこの値以上になります（例：100）")]
    public int sortingOrderBase = 100;

    [Tooltip("Sorting Orderの変動幅。sortingOrderBase 〜 sortingOrderBase+sortingOrderRange の範囲に収まります（例：range=100なら100〜200）。ほぼ固定にしたい場合はこの値を小さく（例：10〜20）してください")]
    public int sortingOrderRange = 100;

    [Tooltip("Sorting Orderの計算に使う画面上下の範囲。0=画面上端、1=画面下端。EnemyMoveの移動エリア設定と合わせておくと敵同士の並び順の基準が揃います")]
    public float sortAreaTopRatio = 0.3f;
    public float sortAreaBottomRatio = 1.0f;

    // sortAreaTopRatio/BottomRatioから計算されるワールド座標（Startで一度だけ計算）
    private float sortAreaTop;
    private float sortAreaBottom;

    // =========================================================
    // 拘束
    // =========================================================
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

        // ★追加：Sorting Order計算用の画面上下範囲をワールド座標で確定しておく
        CalcSortAreaBounds();
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

    // =========================================================
    // ★追加：Sorting Order計算用のワールド座標範囲を計算
    //   EnemyMove.CalcAreaBounds() と同じ考え方で、
    //   カメラのorthographicSizeから画面の上端・下端をワールド座標で求める
    // =========================================================
    void CalcSortAreaBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float h = cam.orthographicSize;
        float camY = cam.transform.position.y;
        float fullH = h * 2f;

        // topRatio=0→画面上端、topRatio=1→画面下端 という比率の考え方をEnemyMoveに合わせる
        sortAreaTop = (camY + h) - fullH * sortAreaTopRatio;
        sortAreaBottom = (camY + h) - fullH * sortAreaBottomRatio;
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
    //  ：LateUpdate で Sorting Order を毎フレーム更新
    //   RabitEnemyMoveと同じ方式：ワールド座標のYをそのまま倍率で
    //   掛けるとマイナスや異常値になり不安定なため、
    //   sortAreaTop〜sortAreaBottom内でのY位置を0〜1に正規化してから
    //   sortingOrderBase〜sortingOrderBase+sortingOrderRange の範囲に収める。
    // =========================================================
    void LateUpdate()
    {
        if (autoSortByY && spriteRenderer != null)
        {
            // sortAreaTop（画面奥側）のとき0、sortAreaBottom（画面手前側）のとき1になるよう正規化
            // ※範囲外の値が来ても InverseLerp は自動で0〜1にクランプされる
            float normalizedY = Mathf.InverseLerp(sortAreaTop, sortAreaBottom, transform.position.y);

            // 手前（画面下＝normalizedYが大きい）ほどOrderが大きくなる＝手前に描画される
            int order = sortingOrderBase + Mathf.RoundToInt(normalizedY * sortingOrderRange);
            spriteRenderer.sortingOrder = order;
        }
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