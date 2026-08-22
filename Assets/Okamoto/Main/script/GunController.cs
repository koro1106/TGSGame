using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Playables;

public class GunController : MonoBehaviour
{
    public Transform gunPivot;
    public Transform muzzle;

    // ▼追加
    // 複数の弾PrefabをInspectorに入れる
    public GameObject[] bulletPrefabs;

    // ▼追加
    // 現在使っている弾番号
    private int currentBulletIndex = 0;

    public float fireRate = 0.1f;
    public float bulletSpeed = 15f;

    [Header("射撃設定")]
    public bool autoFire = true;

    public int maxAmmo = 15; // 弾数
    public float reloadTime = 1.5f;

    private int currentAmmo;
    private float fireTimer;
    private bool isReloading;
    public float radius = 0.5f;
    public float rotateSpeed = 15f; // ← 調整用
    public float flipOffset = 0.5f; // 調整ポイント
    public Transform gunImage;
    Vector3 defaultLocalPos;

    public TMP_Text sensitivityText;
    private Vector3 crosshairPos; // ★ スクリーン座標で保持（World/Overlay共通の基準値）

    private int ammoUIAnimationQueue = 0;

    private bool playFirstAmmoLoadEffect = false;


    [Range(0.1f, 10f)]
    public float sensitivity = 1f;

    [SerializeField]
    private PlayableDirector outOfAmmoTimeline;

    // 弾UIの本来の位置を保存
    private Vector3[] ammoSlotOriginalPositions;

    [Header("弾UI表示設定")]
    public int visibleAmmoCount = 10;

    // ＋から弾が出てくる位置
    public RectTransform ammoPlusPoint;

    // 左へスライドする時間
    public float ammoSlideDuration = 0.15f;
    // ＋から飛んでくる時間
    public float ammoEnterDuration = 0.15f;

    [Header("左端弾の装填演出")]
    public float firstAmmoDropHeight = 60f;
    public float firstAmmoDropDuration = 0.12f;
    public float firstAmmoStartScale = 0.7f;
    [Header("NEXT弾の跳ね演出")]
    public float firstAmmoBounceDown = 10f;   // 目標より少し下まで落ちる距離
    public float firstAmmoBounceUp = 6f;      // そこから上に跳ねる距離
    public float firstAmmoBounceDuration = 0.08f; // 跳ねる時間

    [Header("弾UI画像")]
    public Sprite normalAmmoSprite;
    public Sprite lightningAmmoSprite;
    public Sprite BindAmmoSprite;
    public Sprite ExplosionAmmoSprite;
    public Sprite GravityAmmoSprite;
    public Sprite PoisonAmmoSprite;
    public Sprite penetratingAmmoSprite;
    //public Sprite ReboundAmmoSprite;

    [Header("マズルフラッシュ")]
    public GameObject muzzleFlash;
    public float muzzleFlashTime = 0.05f;
    private Coroutine flashRoutine;

    [Header("弾UIドロップ演出")]
    public GameObject ammoDropUIPrefab;
    public Transform uiEffectParent;

    //[Header("敵撃破時の弾回復")]
    //public bool recoverAmmoOnKill = false;
    //public int recoverAmmoAmount = 1;

    [Header("弾切れUI")]
    public GameObject outOfAmmoUIImage;

    [Header("敵撃破時の弾回復")]
    public bool recoverAmmoOnKill = false;


    [Range(0f, 100f)]
    public float recoverAmmoChance = 50f; // 回復確率 %

    public int recoverAmmoAmount = 1;
    public GameObject ammoRecoverEffectPrefab;

    public AmmoSlot[] ammoSlots;
    // =====================================================
    // 実際の弾データ
    // AmmoSlotとは分離して管理する
    // =====================================================

    private AmmoType[] ammoTypes;
    private Sprite[] ammoSprites;
    private GameObject[] ammoPrefabs;

    // 弾UIの本来の位置
    private Vector3[] ammoOriginalPositions;

    // 現在UIに表示している最初の弾
    private int visibleStartIndex = 0;

    // UIアニメーション中
    private bool isAmmoUIAnimating = false;

    public TMP_Text ammoText;
    public RectTransform crosshair; // UIのクロスヘア（Screen Space / World Space どちらのCanvasでもOK）
    private SpriteRenderer sr;
    private Camera cam;
    public GameObject[] ammoDropPrefabs; // bulletPrefabs と同じ順番で設定弾プレハブ
    public PlayerStats stats; // プレイヤーステータス☆
    private float crosshairTargetRotation = 0f;
    private bool isChangingScene = false;

    private Coroutine ammoSlideCoroutine;

    private int ammoSlideQueue = 0; // 弾UIスライド待ち数

    private bool isAmmoSlidePlaying = false;// スライドアニメーションが現在動いているか

    // ★追加：クロスヘアが乗っているCanvasとそのRenderMode判定
    private Canvas crosshairCanvas;
    private bool isWorldSpaceCanvas;

    //ショットガン追加
    public ShotgunController shotgun;
    //スナイパー追加
    public SniperController sniper;
    //ハンドガン追加
    public HandGunController handgun;

    [Header("自動ターゲット")]
    public TargetRange targetRange;

    public RectTransform Crosshair => crosshair;
    public Camera Cam => cam;
    public Vector2 LastShootDirection { get; private set; }



    void Start()
    {
        crosshairPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        UpdateCrosshairPosition(); // ★ World/Overlay両対応の反映処理に変更

        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(false);
        }

        // stats.unlockedElementalBulletsがnullじゃないならコピー☆
        if (stats.unlockedElementalBullets != null && stats.unlockedElementalBullets.Length > 0)
        {
            // 現在のbulletPrefabsに解放弾を追加
            var tempList = new List<GameObject>(bulletPrefabs);
            tempList.AddRange(stats.unlockedElementalBullets);
            bulletPrefabs = tempList.ToArray();
        }

        // 弾UIの本来の位置を保存
        ammoSlotOriginalPositions =
            new Vector3[ammoSlots.Length];

        // =====================================================
        // UI本来の位置を保存
        // =====================================================

        ammoOriginalPositions =
            new Vector3[ammoSlots.Length];

        for (int i = 0; i < ammoSlots.Length; i++)
        {
            if (ammoSlots[i].image != null)
            {
                ammoOriginalPositions[i] =
                    ammoSlots[i].image.rectTransform.localPosition;
            }
        }

        // 撃破時弾回復する
        if (stats.recoveryBullet)
            recoverAmmoOnKill = true;

        // 回復弾数増加
        recoverAmmoAmount += stats.recoveryBulletCount;

        // 弾データ生成
        GenerateAmmo();

        //for (int i = 0; i < ammoSlots.Length; i++)
        //{
        //bool active = i < maxAmmo;

        //bool visible =
        //    i < visibleAmmoCount;

        //    ammoSlots[i].image.transform.parent.gameObject.SetActive(active);
        //}
    }

    void Awake()
    {
        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();

        // ★追加：クロスヘアのCanvasを取得しWorldSpaceかどうか判定
        if (crosshair != null)
        {
            crosshairCanvas = crosshair.GetComponentInParent<Canvas>();
            isWorldSpaceCanvas =
                crosshairCanvas != null &&
                crosshairCanvas.renderMode == RenderMode.WorldSpace;
        }

        // 最大弾数反映☆
        maxAmmo = stats.maxAmmo;
        currentAmmo = maxAmmo;

        // =====================================================
        // 実際の弾データ配列
        // =====================================================

        ammoTypes =
    new AmmoType[ammoSlots.Length];

        ammoSprites =
            new Sprite[ammoSlots.Length];

        ammoPrefabs =
            new GameObject[ammoSlots.Length];

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        gunImage.localPosition = new Vector3(0.5f, 0, 0);
        defaultLocalPos = gunImage.localPosition; // ★ 初期位置保存

        UpdateAmmoUI();
        sensitivityText.text = "感度 : " + sensitivity.ToString("F1");
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;

        Aim();

        if (!isReloading)
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartReload();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            IncreaseMaxAmmo(1);
        }

        //ショットガンPキーで出現
        if (Input.GetKeyDown(KeyCode.P))
        {
            shotgun.gameObject.SetActive(true);
            shotgun.ActivateShotgun();
        }

        //スナイパーOキーで出現
        if (Input.GetKeyDown(KeyCode.O))
        {
            sniper.gameObject.SetActive(true);
            sniper.ActivateSniper();
        }
    }

    void Aim()
    {
        // ★ World/Overlay両対応のワールド座標取得に変更
        Vector3 worldPos = GetCrosshairWorldPosition();

        Vector3 dir = worldPos - gunPivot.position;
        bool isLeft = dir.x < 0;

        // 左右反転だけ
        if (isLeft)
        {
            gunImage.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            gunImage.localScale = new Vector3(1, 1, 1);
        }

        // 位置は固定
        gunImage.localPosition = defaultLocalPos;
    }

    void Shoot()
    {
        fireTimer += Time.deltaTime;

        bool shouldShoot = false;

        // =========================
        // 連射ON
        // =========================
        if(stats.rapidFire)
        {
            autoFire = true;
        }

        if (autoFire)
        {
            shouldShoot =
                Input.GetMouseButton(0) &&
                fireTimer >= fireRate;
        }
        // =========================
        // 連射OFF
        // =========================
        else
        {
            shouldShoot =
                Input.GetMouseButtonDown(0) &&
                fireTimer >= fireRate;
        }

        if (shouldShoot)
        {
            // =========================
            // 弾切れチェック
            // =========================
            if (currentAmmo <= 0)
            {
                if (!isChangingScene)
                {
                    isChangingScene = true;

                    if (ResultManager.Instance != null)
                    {
                        ResultManager.Instance.ShowResult();
                    }
                }

                return;
            }

            // =========================
            // 弾データチェック
            // =========================
            if (ammoPrefabs == null ||
                ammoPrefabs.Length == 0 ||
                ammoPrefabs[0] == null)
            {
                Debug.LogError("撃つ弾データがありません。");
                return;
            }

            // =========================
            // 先頭の弾を取得
            // =========================

            GameObject bulletToShoot = ammoPrefabs[0];

            AmmoType currentType = ammoTypes[0];
            Sprite currentSprite = ammoSprites[0];

            // =========================
            // NEXT UI
            // =========================

            Image nextImage = null;

            if (ammoSlots != null &&
                ammoSlots.Length > 0 &&
                ammoSlots[0].image != null)
            {
                nextImage = ammoSlots[0].image;
            }

            // =========================
            // 弾生成
            // =========================

            GameObject bulletInstance =
                Instantiate(
                    bulletToShoot,
                    muzzle.position,
                    muzzle.rotation
                );

            PlayMuzzleFlash();

            //crosshairTargetRotation += 90f;

            if (shotgun != null && shotgun.isActive)
            {
                shotgun.Fire();
            }

            if (sniper != null && sniper.isActive)
            {
                sniper.Fire();
            }

            // =========================
            // ダメージ
            // =========================

            Bullet bulletScript =
                bulletInstance.GetComponent<Bullet>();

            if (bulletScript != null)
            {
                bulletScript.SetDamage(stats.bulletDamage);
            }
            // =========================
            // 発射方向
            // =========================

            Rigidbody2D rb =
                bulletInstance.GetComponent<Rigidbody2D>();

            Vector3 targetPosition;

            // ターゲットしているEnemyがいる場合
            if (targetRange != null &&
                targetRange.CurrentTarget != null)
            {
                targetPosition =
                    targetRange.CurrentTarget.position;
            }
            else
            {
                // Enemyがいなければクロスヘア方向
                targetPosition =
                    GetCrosshairWorldPosition();
            }

            Vector2 direction =
                (targetPosition - muzzle.position).normalized;

            LastShootDirection = direction;

            bulletInstance.transform.right =
                direction;

            if (rb != null)
            {
                rb.linearVelocity =
                    direction * bulletSpeed;
            }

            // =========================
            // 発射UIドロップ演出
            // =========================

            if (ammoDropUIPrefab != null &&
                nextImage != null)
            {
                GameObject drop =
                    Instantiate(
                        ammoDropUIPrefab,
                        nextImage.transform.position,
                        Quaternion.identity,
                        nextImage.transform.parent
                    );

                drop.transform.localScale =
                    nextImage.transform.localScale;

                Image dropImage =
                    drop.GetComponent<Image>();

                if (dropImage != null)
                {
                    dropImage.sprite =
                        nextImage.sprite;
                }
            }

            // =========================
            // 弾データを1発消費
            // =========================
            ConsumeCurrentAmmo();

            // =========================
            // SE
            // =========================

            SEManager.Instance.PlayShootSE();

            // =========================
            // 演出
            // =========================

            CameraShake.Instance.Shake();
            PlayerHP.Instance.TakeDamage(1);

            // =========================
            // UIアニメーションをキューに追加
            // =========================

            ammoSlideQueue++;

            if (!isAmmoSlidePlaying)
            {
                ammoSlideCoroutine =
                    StartCoroutine(ProcessAmmoSlideQueue());
            }

            // =========================
            // 数字UI
            // =========================

            UpdateAmmoUI();

            // =========================
            // 発射間隔
            // =========================

            fireTimer = 0f;
        }
    }

    void ConsumeCurrentAmmo()
    {
        if (currentAmmo <= 0)
            return;

        // =========================================
        // 現在存在している弾だけを前に詰める
        // =========================================

        int oldAmmoCount = currentAmmo;

        for (int i = 0; i < oldAmmoCount - 1; i++)
        {
            ammoPrefabs[i] = ammoPrefabs[i + 1];
            ammoTypes[i] = ammoTypes[i + 1];
            ammoSprites[i] = ammoSprites[i + 1];
        }

        // =========================================
        // 最後に存在していた弾を消す
        // =========================================

        int lastIndex = oldAmmoCount - 1;

        if (lastIndex >= 0 &&
            lastIndex < ammoPrefabs.Length)
        {
            ammoPrefabs[lastIndex] = null;
            ammoTypes[lastIndex] = AmmoType.Normal;
            ammoSprites[lastIndex] = null;
        }

        // =========================================
        // 弾数を減らす
        // =========================================

        currentAmmo--;

        // =========================================
        // UIの開始位置
        // =========================================

        visibleStartIndex++;
    }

    //GameObject GetBulletPrefab(AmmoType type)
    //{
    //    // 通常弾
    //    if (type == AmmoType.Normal)
    //    {
    //        return bulletPrefabs[0];
    //    }

    //    // 解放済み属性弾から探す
    //    foreach (GameObject prefab in stats.unlockedElementalBullets)
    //    {
    //        string bulletName = prefab.name;
    //        switch (type)
    //        {
    //            case AmmoType.Lightning:
    //                if (bulletName.Contains("Lightning"))
    //                    return prefab;
    //                break;
    //            case AmmoType.Gravity:
    //                if (bulletName.Contains("Gravity"))
    //                    return prefab;
    //                break;
    //            case AmmoType.Bind:
    //                if (bulletName.Contains("Bind"))
    //                    return prefab;
    //                break;
    //            case AmmoType.Poison:
    //                if (bulletName.Contains("Poison"))
    //                    return prefab;
    //                break;
    //            case AmmoType.Explosion:
    //                if (bulletName.Contains("Explosion"))
    //                    return prefab;
    //                break;
    //            case AmmoType.Penetrating:
    //                if (bulletName.Contains("Penetrating"))
    //                    return prefab;
    //                break;
    //        }
    //    }

    //    // 見つからなかったら通常弾
    //    return bulletPrefabs[0];
    //}

    void StartReload()
    {
        if (currentAmmo == maxAmmo) return;
        if (isReloading) return;

        isReloading = true;

        // ★UIだけ即満タン表示
        ammoText.text = maxAmmo + " / " + maxAmmo;

        StartCoroutine(Reload());
    }

    System.Collections.IEnumerator Reload()
    {
        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        GenerateAmmo(); // 弾の内容決める
        isReloading = false;
    }

    // 弾内容決める
    void GenerateAmmo()
    {
        if (ammoTypes == null ||
            ammoTypes.Length != ammoSlots.Length)
        {
            ammoTypes =
                new AmmoType[ammoSlots.Length];

            ammoSprites =
                new Sprite[ammoSlots.Length];

            ammoPrefabs =
                new GameObject[ammoSlots.Length];
        }

        // =========================================
        // 弾データ生成
        // =========================================

        for (int i = 0; i < ammoSlots.Length; i++)
        {
            AmmoType type = AmmoType.Normal;
            Sprite sprite = normalAmmoSprite;

            // 最初は通常弾
            GameObject prefab = bulletPrefabs[0];

            // =========================================
            // 属性弾抽選
            // =========================================

            bool canElement =
                stats.unlockedElementalBullets != null &&
                stats.unlockedElementalBullets.Length > 0;

            if (canElement &&
                Random.value < stats.elementalBulletChance)
            {
                GameObject randomPrefab =
                    stats.unlockedElementalBullets[
                        Random.Range(
                            0,
                            stats.unlockedElementalBullets.Length
                        )
                    ];

                string bulletName =
                    randomPrefab.name;

                // =====================================
                // 属性弾
                // =====================================

                if (bulletName.Contains("Lightning"))
                {
                    type = AmmoType.Lightning;
                    sprite = lightningAmmoSprite;
                    prefab = randomPrefab;
                }
                else if (bulletName.Contains("Gravity"))
                {
                    type = AmmoType.Gravity;
                    sprite = GravityAmmoSprite;
                    prefab = randomPrefab;
                }
                else if (bulletName.Contains("Bind"))
                {
                    type = AmmoType.Bind;
                    sprite = BindAmmoSprite;
                    prefab = randomPrefab;
                }
                else if (bulletName.Contains("Poison"))
                {
                    type = AmmoType.Poison;
                    sprite = PoisonAmmoSprite;
                    prefab = randomPrefab;
                }
                else if (bulletName.Contains("Explosion"))
                {
                    type = AmmoType.Explosion;
                    sprite = ExplosionAmmoSprite;
                    prefab = randomPrefab;
                }
                else if (bulletName.Contains("Penetrating"))
                {
                    type = AmmoType.Penetrating;
                    sprite = penetratingAmmoSprite;
                    prefab = randomPrefab;
                }
            }

            // =========================================
            // 1発分のデータを保存
            // =========================================

            ammoTypes[i] = type;
            ammoSprites[i] = sprite;
            ammoPrefabs[i] = prefab;
        }

        // =========================================
        // UI更新
        // =========================================

        RefreshAmmoUIImmediate();
    }

    void UpdateAmmoUI()
    {
        ammoText.text = currentAmmo + " / " + maxAmmo;
    }

    void LateUpdate()
    {
        if (PauseMenu.IsPaused) return;

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        crosshairPos += new Vector3(mouseX, mouseY, 0f) * sensitivity * 25f;

        crosshairPos.x = Mathf.Clamp(crosshairPos.x, 0, Screen.width);
        crosshairPos.y = Mathf.Clamp(crosshairPos.y, 0, Screen.height);

        UpdateCrosshairPosition(); // ★ World/Overlay両対応の反映処理に変更

        // 追加
        Quaternion targetRot = Quaternion.Euler(0, 0, crosshairTargetRotation);
        crosshair.rotation = Quaternion.Lerp(crosshair.rotation, targetRot, Time.deltaTime * 30f);
    }

    /// <summary>
    /// スクリーン座標(crosshairPos)を、実際のクロスヘアのTransformへ反映する。
    /// Canvasが World Space の場合はカメラからのレイとCanvas平面の交点をワールド座標として設定し、
    /// Screen Space Overlay / Camera の場合は従来通りスクリーン座標をそのまま設定する。
    /// </summary>
    void UpdateCrosshairPosition()
    {
        if (crosshair == null) return;

        if (isWorldSpaceCanvas && crosshairCanvas != null)
        {
            Ray ray = cam.ScreenPointToRay(crosshairPos);
            Plane canvasPlane =
                new Plane(crosshairCanvas.transform.forward, crosshairCanvas.transform.position);

            if (canvasPlane.Raycast(ray, out float distance))
            {
                crosshair.position = ray.GetPoint(distance);
            }
        }
        else
        {
            crosshair.position = crosshairPos;
        }
    }

    void RefreshAmmoUIImmediate()
    {
        if (ammoSlots == null)
            return;

        int visibleCount =
            Mathf.Min(
                visibleAmmoCount,
                currentAmmo
            );

        // =========================================
        // 全UIを非表示
        // =========================================

        for (int i = 0; i < ammoSlots.Length; i++)
        {
            if (ammoSlots[i].image != null)
            {
                ammoSlots[i].image.enabled = false;

                ammoSlots[i]
                    .image
                    .rectTransform
                    .localPosition =
                    ammoOriginalPositions[i];
            }

            if (ammoSlots[i].emptyImage != null)
            {
                ammoSlots[i]
                    .emptyImage
                    .gameObject
                    .SetActive(false);
            }
        }

        // =========================================
        // 0番目から表示
        // =========================================

        for (int displayIndex = 0;
             displayIndex < visibleCount;
             displayIndex++)
        {
            if (displayIndex >= ammoPrefabs.Length)
                break;

            AmmoSlot slot =
                ammoSlots[displayIndex];

            if (slot.image == null)
                continue;

            // =====================================
            // 弾データ → UI
            // =====================================

            slot.ammoType =
                ammoTypes[displayIndex];

            slot.image.sprite =
                ammoSprites[displayIndex];

            slot.image.enabled =
                true;

            if (slot.emptyImage != null)
            {
                slot.emptyImage
                    .gameObject
                    .SetActive(true);
            }

            slot.image
                .rectTransform
                .localPosition =
                ammoOriginalPositions[displayIndex];
        }
    }

    /// <summary>
    /// クロスヘアの現在位置を「ワールド座標（Z=0）」として取得する。
    /// World Space Canvasならクロスヘアのpositionをそのまま使い、
    /// Screen Space Overlay / Camera ならScreenToWorldPointで変換する。
    /// </summary>
    public Vector3 GetCrosshairWorldPosition()
    {
        if (isWorldSpaceCanvas)
        {
            Vector3 worldPos = crosshair.position;
            worldPos.z = 0;
            return worldPos;
        }
        else
        {
            Vector3 screenPos = crosshair.position;
            Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;
            return worldPos;
        }
    }

    public void SetSensitivity(float value)
    {
        sensitivity = value;
        sensitivityText.text = "感度 : " + sensitivity.ToString("F1");
    }

    /// <summary>
    /// 属性弾を追加する（実行時用）
    /// </summary>
    public void AddElementalBullet(GameObject bulletPrefab)
    {
        if (bulletPrefab == null) return;

        // 配列をListに変換
        List<GameObject> bullets = new List<GameObject>(bulletPrefabs);

        // すでに追加済みかチェック
        if (!bullets.Contains(bulletPrefab))
        {
            bullets.Add(bulletPrefab);
            bulletPrefabs = bullets.ToArray(); // 配列に戻す
            Debug.Log("GunController に属性弾追加: " + bulletPrefab.name);
        }
    }

    void PlayMuzzleFlash()
    {
        if (muzzleFlash == null) return;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(MuzzleFlashRoutine());
    }

    System.Collections.IEnumerator MuzzleFlashRoutine()
    {
        // 毎回少しランダムにする
        muzzleFlash.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        float size = Random.Range(0.8f, 1.2f);
        muzzleFlash.transform.localScale = Vector3.one * size;

        muzzleFlash.SetActive(true);

        yield return new WaitForSeconds(muzzleFlashTime);

        muzzleFlash.SetActive(false);
    }

    public void AddAmmo(int amount)
    {
        int oldAmmo = currentAmmo;
        int targetAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);

        for (int i = oldAmmo; i < targetAmmo; i++)
        {
            if (i >= 0 && i < ammoSlots.Length)
            {
                //========================
                // スロット取得
                //========================
                AmmoSlot slot = ammoSlots[i];

                // すでに回復中なら飛ばす
                if (slot.isRecovering)
                    continue;

                // 回復中フラグ
                slot.isRecovering = true;

                //========================
                // Sprite決定
                //========================
                Sprite targetSprite = normalAmmoSprite;
                switch (slot.ammoType)
                {
                    case AmmoType.Normal:
                        targetSprite = normalAmmoSprite;
                        break;
                    case AmmoType.Lightning:
                        targetSprite = lightningAmmoSprite;
                        break;
                    case AmmoType.Gravity:
                        targetSprite = GravityAmmoSprite;
                        break;
                    case AmmoType.Bind:
                        targetSprite = BindAmmoSprite;
                        break;
                    case AmmoType.Poison:
                        targetSprite = PoisonAmmoSprite;
                        break;
                    case AmmoType.Explosion:
                        targetSprite = ExplosionAmmoSprite;
                        break;
                    case AmmoType.Penetrating:
                        targetSprite = penetratingAmmoSprite;
                        break;
                }

                // 元UI非表示
                slot.image.enabled = false;

                //========================
                // 回復演出生成
                //========================
                if (ammoRecoverEffectPrefab != null)
                {
                    GameObject obj =
                        Instantiate(ammoRecoverEffectPrefab, slot.image.canvas.transform);

                    // 演出保持
                    slot.recoverEffectObject = obj;

                    AmmoRecoverEffect effect = obj.GetComponent<AmmoRecoverEffect>();
                    effect.Init(
                        targetSprite,
                        slot.image.transform.position,
                        slot.image.rectTransform,
                        () =>
                        {
                            // 途中で撃たれてたら終了
                            if (slot.recoverEffectObject == null)
                                return;

                            slot.isRecovering = false;
                            currentAmmo++;

                            slot.image.sprite = targetSprite;
                            slot.image.enabled = true;

                            slot.recoverEffectObject = null;

                            UpdateAmmoUI();
                        });
                }
                else
                {
                    slot.isRecovering = false;
                    currentAmmo++;

                    slot.image.sprite = targetSprite;
                    slot.image.enabled = true;

                    UpdateAmmoUI();
                }
            }
        }

        UpdateAmmoUI();
    }

    void IncreaseMaxAmmo(int amount)
    {
        // =========================================
        // 最大弾数を増やす
        // =========================================

        maxAmmo += amount;

        maxAmmo =
            Mathf.Clamp(
                maxAmmo,
                0,
                ammoSlots.Length
            );

        // =========================================
        // 満タン
        // =========================================

        currentAmmo =
            maxAmmo;

        // =========================================
        // 弾データを作り直す
        // =========================================

        GenerateAmmo();

        UpdateAmmoUI();
    }

    //IEnumerator OutOfAmmoAndChangeScene()
    //{
    //    isChangingScene = true;

    //    if (outOfAmmoUIImage != null)
    //    {
    //        outOfAmmoUIImage.SetActive(true);
    //    }

    //    yield return new WaitForSeconds(0.5f);

    //    SceneManager.LoadScene("MainStageSkillTreeScene");
    //}

    //IEnumerator OutOfAmmoAndChangeScene()
    //{
    //    isChangingScene = true;

    //    outOfAmmoTimeline.Play();

    //    yield return new WaitForSeconds(
    //        (float)outOfAmmoTimeline.duration);

    //    SceneManager.LoadScene("MainStageSkillTreeScene");
    //}

    IEnumerator ProcessAmmoSlideQueue()
    {
        isAmmoSlidePlaying = true;

        while (ammoSlideQueue > 0)
        {
            ammoSlideQueue--;

            yield return StartCoroutine(
                PlaySingleAmmoSlideAnimation()
            );
        }

        isAmmoSlidePlaying = false;
        ammoSlideCoroutine = null;

        // 最後に必ず現在の弾数と一致させる
        RefreshAmmoUIImmediate();
    }
    IEnumerator PlaySingleAmmoSlideAnimation()
    {
        bool wasInterrupted = false;

        if (ammoSlots == null ||
            ammoSlots.Length == 0)
        {
            yield break;
        }

        int displayCount =
            Mathf.Min(
                visibleAmmoCount,
                ammoSlots.Length
            );

        // =========================================
        // 現在の残弾数
        // =========================================

        int remainingAmmo = currentAmmo;

        // =========================================
        // 弾が0発
        // =========================================

        if (remainingAmmo <= 0)
        {
            for (int i = 0; i < displayCount; i++)
            {
                if (ammoSlots[i].image != null)
                {
                    ammoSlots[i]
                        .image
                        .enabled = false;

                    ammoSlots[i]
                        .image
                        .rectTransform
                        .localPosition =
                        ammoOriginalPositions[i];
                }

                if (ammoSlots[i].emptyImage != null)
                {
                    ammoSlots[i]
                        .emptyImage
                        .gameObject
                        .SetActive(false);
                }
            }

            yield break;
        }

        // =========================================
        // 現在のUI位置を保存
        // =========================================

        Vector3[] startPositions =
            new Vector3[displayCount];

        for (int i = 0; i < displayCount; i++)
        {
            if (ammoSlots[i].image != null)
            {
                startPositions[i] =
                    ammoSlots[i]
                        .image
                        .rectTransform
                        .localPosition;
            }
            else
            {
                startPositions[i] =
                    ammoOriginalPositions[i];
            }
        }

        // =========================================
        // 一番左の弾を消す
        // =========================================

        if (ammoSlots[0].image != null)
        {
            ammoSlots[0]
                .image
                .enabled = false;
        }

        // =========================================
        // NEXT弾の情報
        //
        // ConsumeCurrentAmmo() 後なので
        // ammoSprites[0] が「次に撃つ弾」
        // =========================================

        Sprite nextSprite = null;
        AmmoType nextType = AmmoType.Normal;

        if (remainingAmmo > 0 &&
            ammoSprites != null &&
            ammoTypes != null &&
            ammoSprites.Length > 0)
        {
            nextSprite = ammoSprites[0];
            nextType = ammoTypes[0];
        }

        // =========================================
        // NEXT弾の上から落下する演出
        // =========================================

        Coroutine nextDropCoroutine = null;

        if (nextSprite != null &&
            ammoSlots[0].image != null)
        {
            // 本物の2番目以降のUIが
            // 左端に一瞬表示されないようにする
            if (displayCount >= 2 &&
                ammoSlots[1].image != null)
            {
                ammoSlots[1]
                    .image
                    .enabled = false;
            }

            nextDropCoroutine =
    StartCoroutine(
        AnimateNextAmmoDropFromAbove(
            nextSprite,
            ammoSlots[0].image.rectTransform
        )
    );
        }

        // =========================================
        // 3番目以降を左へスライド
        // =========================================

        float timer = 0f;


        // =========================================
        // ＋から11個目以降の弾を入れる演出
        // =========================================
        //
        // 発射後に11発以上残っている場合、
        // 10番目の表示位置に新しい弾が入ってくる
        //
        Coroutine ammoEnterCoroutine = null;

        if (remainingAmmo > visibleAmmoCount &&
            visibleAmmoCount <= ammoSlots.Length &&
            ammoSprites != null &&
            visibleAmmoCount < ammoSprites.Length &&
            ammoSprites[visibleAmmoCount - 1] != null)
        {
            // 10番目のスロットを一旦非表示
            if (ammoSlots[visibleAmmoCount - 1].image != null)
            {
                ammoSlots[visibleAmmoCount - 1]
                    .image
                    .enabled = false;
            }

            // ＋から10番目の位置へ入ってくる
            ammoEnterCoroutine =
                StartCoroutine(
                    PlayAmmoEnterEffect(visibleAmmoCount - 1)
                );
        }

        while (timer < ammoSlideDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / ammoSlideDuration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            // =====================================
            // 3番目 → 2番目
            // 4番目 → 3番目
            // ...
            // =====================================

            for (int i = 2;
     i < displayCount;
     i++)
            {
                if (ammoSlots[i].image == null)
                    continue;

                // 存在しない弾は表示しない
                if (i >= remainingAmmo)
                {
                    ammoSlots[i].image.enabled = false;
                    continue;
                }

                Vector3 start =
                    startPositions[i];

                Vector3 target =
                    ammoOriginalPositions[i - 1];

                // SmoothStepよりさらに滑らかな補間
                float smoothT =
                    t * t * (3f - 2f * t);

                ammoSlots[i]
                    .image
                    .rectTransform
                    .localPosition =
                    Vector3.LerpUnclamped(
                        start,
                        target,
                        smoothT
                    );
            }

            yield return null;
        }

        // =========================================
        // スライド終了
        // =========================================

        for (int i = 0; i < displayCount; i++)
        {
            if (ammoSlots[i].image == null)
                continue;

            ammoSlots[i]
                .image
                .rectTransform
                .localPosition =
                ammoOriginalPositions[i];
        }

        // =========================================
        // NEXT弾の落下演出が終わるまで待つ
        // =========================================

        //if (nextDropCoroutine != null)
        //{
        //    yield return nextDropCoroutine;
        //}

        // =========================================
        // ＋から入ってくる弾の演出が終わるまで待つ
        // =========================================

        if (ammoEnterCoroutine != null)
        {
            yield return ammoEnterCoroutine;
        }

        // =========================================
        // 現在の弾数に合わせてUIを確定
        // =========================================

        int newVisibleCount =
            Mathf.Min(
                visibleAmmoCount,
                remainingAmmo
            );

        for (int i = 0; i < displayCount; i++)
        {
            if (ammoSlots[i].image == null)
                continue;

            // =====================================
            // 存在する弾
            // =====================================

            if (i < newVisibleCount &&
                i < ammoSprites.Length &&
                ammoSprites[i] != null)
            {
                ammoSlots[i]
                    .image
                    .sprite =
                    ammoSprites[i];

                ammoSlots[i]
                    .ammoType =
                    ammoTypes[i];

                ammoSlots[i]
                    .image
                    .enabled = true;

                if (ammoSlots[i].emptyImage != null)
                {
                    ammoSlots[i]
                        .emptyImage
                        .gameObject
                        .SetActive(true);
                }
            }
            // =====================================
            // 存在しない弾
            // =====================================

            else
            {
                ammoSlots[i]
                    .image
                    .enabled = false;

                if (ammoSlots[i].emptyImage != null)
                {
                    ammoSlots[i]
                        .emptyImage
                        .gameObject
                        .SetActive(false);
                }
            }
        }
    }

    IEnumerator AnimateNextAmmoDropFromAbove(
    Sprite sprite,
    RectTransform target)
    {
        if (sprite == null ||
            target == null)
            yield break;

        GameObject effectObject =
            new GameObject("NextAmmoDropEffect");

        effectObject.transform.SetParent(
            target.parent,
            false
        );

        Image image =
            effectObject.AddComponent<Image>();

        image.sprite = sprite;
        image.preserveAspect = true;

        RectTransform rect =
            effectObject.GetComponent<RectTransform>();

        rect.sizeDelta = target.sizeDelta;
        rect.localScale = target.localScale;

        Vector3 targetPosition =
            target.localPosition;

        Vector3 startPosition =
            targetPosition +
            Vector3.up * firstAmmoDropHeight;

        rect.localPosition =
            startPosition;

        // ① 落下
        float timer = 0f;

        while (timer < firstAmmoDropDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / firstAmmoDropDuration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            rect.localPosition =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            yield return null;
        }

        // ② 下に跳ねる
        Vector3 bounceDownPosition =
            targetPosition +
            Vector3.down * firstAmmoBounceDown;

        timer = 0f;

        while (timer < firstAmmoBounceDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / firstAmmoBounceDuration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            rect.localPosition =
                Vector3.Lerp(
                    targetPosition,
                    bounceDownPosition,
                    t
                );

            yield return null;
        }

        // ③ 元に戻る
        timer = 0f;

        while (timer < firstAmmoBounceDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / firstAmmoBounceDuration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            rect.localPosition =
                Vector3.Lerp(
                    bounceDownPosition,
                    targetPosition,
                    t
                );

            yield return null;
        }

        rect.localPosition =
            targetPosition;

        target.GetComponent<Image>().enabled = true;

        Destroy(effectObject);
    }


    IEnumerator PlayNextAmmoDropAnimation(int dataIndex)
    {
        if (dataIndex < 0 ||
            dataIndex >= ammoSprites.Length)
        {
            yield break;
        }

        if (ammoSlots == null ||
            ammoSlots.Length == 0)
        {
            yield break;
        }

        Image targetImage =
            ammoSlots[0].image;

        if (targetImage == null)
            yield break;

        if (targetImage.sprite == null)
            yield break;

        // =========================================
        // 元の位置
        // =========================================

        Vector3 targetPosition =
            targetImage.rectTransform.position;

        // =========================================
        // 少し上からスタート
        // =========================================

        Vector3 startPosition =
            targetPosition + Vector3.up * 80f;

        // =========================================
        // 一旦UIを非表示
        // =========================================

        targetImage.enabled = false;

        // =========================================
        // 演出用Image
        // =========================================

        GameObject effectObject =
            new GameObject("NextAmmoDropEffect");

        effectObject.transform.SetParent(
            targetImage.transform.parent,
            false
        );

        Image effectImage =
            effectObject.AddComponent<Image>();

        effectImage.sprite =
            ammoSprites[dataIndex];

        effectImage.preserveAspect =
            true;

        RectTransform effectRect =
            effectObject.GetComponent<RectTransform>();

        effectRect.position =
            startPosition;

        // =========================================
        // 最初は少し小さく
        // =========================================

        effectRect.localScale =
            Vector3.one * 0.75f;

        float timer = 0f;

        float duration = 0.12f;

        // =========================================
        // 上から降りてくる
        // =========================================

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            effectRect.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            effectRect.localScale =
                Vector3.Lerp(
                    Vector3.one * 0.75f,
                    Vector3.one,
                    t
                );

            yield return null;
        }

        // =========================================
        // 到着
        // =========================================

        effectRect.position =
            targetPosition;

        effectRect.localScale =
            Vector3.one;

        // =========================================
        // 本物のUIを表示
        // =========================================

        targetImage.enabled = true;

        targetImage.sprite =
            ammoSprites[dataIndex];

        ammoSlots[0].ammoType =
            ammoTypes[dataIndex];

        // =========================================
        // 演出オブジェクト削除
        // =========================================

        Destroy(effectObject);
    }

    IEnumerator PlayAmmoEnterEffect(int dataIndex)
    {
        if (ammoPlusPoint == null)
            yield break;

        if (ammoSprites == null)
            yield break;

        if (dataIndex < 0 ||
            dataIndex >= ammoSprites.Length)
            yield break;

        int displayIndex =
            visibleAmmoCount - 1;

        if (displayIndex < 0 ||
            displayIndex >= ammoSlots.Length)
            yield break;

        Image targetImage =
            ammoSlots[displayIndex].image;

        if (targetImage == null)
            yield break;

        // =====================================================
        // 演出用Image作成
        // =====================================================

        GameObject effectObject =
            new GameObject("AmmoEnterEffect");

        effectObject.transform.SetParent(
            targetImage.transform.parent,
            false
        );

        Image effectImage =
            effectObject.AddComponent<Image>();

        effectImage.sprite =
            ammoSprites[dataIndex];

        effectImage.preserveAspect = true;

        RectTransform effectRect =
            effectObject.GetComponent<RectTransform>();

        // =====================================================
        // ＋の位置
        // =====================================================

        Vector3 startPosition =
            ammoPlusPoint.position;

        Vector3 endPosition =
            targetImage.rectTransform.position;

        effectRect.position =
            startPosition;

        // =====================================================
        // サイズ
        // 最初は小さく
        // 最後は通常サイズ
        // =====================================================

        Vector3 normalScale =
            targetImage.transform.lossyScale;

        Vector3 startScale =
            normalScale * 0.25f;

        effectRect.localScale =
            Vector3.one * 0.25f;

        // =====================================================
        // アニメーション
        // =====================================================

        float timer = 0f;

        while (timer < ammoEnterDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t =
                Mathf.Clamp01(
                    timer / ammoEnterDuration
                );

            // なめらかに
            float moveT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            // -------------------------------------------------
            // ＋ → スロット
            // -------------------------------------------------

            effectRect.position =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    moveT
                );

            // -------------------------------------------------
            // 小 → 通常サイズ
            // -------------------------------------------------

            float scaleT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            effectRect.localScale =
                Vector3.Lerp(
                    Vector3.one * 0.25f,
                    Vector3.one,
                    scaleT
                );

            yield return null;
        }

        // =========================================
        // 最終位置・サイズ
        // =========================================

        effectRect.position =
            endPosition;

        effectRect.localScale =
            Vector3.one;

        // =========================================
        // 演出オブジェクト削除
        // =========================================

        Destroy(effectObject);

        RefreshAmmoUIImmediate();
    }

    IEnumerator PlayFirstAmmoLoadEffect()
    {
        if (ammoSlots == null ||
            ammoSlots.Length == 0)
            yield break;

        if (ammoSlots[0].image == null)
            yield break;

        Image targetImage = ammoSlots[0].image;

        if (!targetImage.enabled)
            yield break;

        Sprite sprite = targetImage.sprite;

        if (sprite == null)
            yield break;

        // 元の位置
        Vector3 targetPosition =
            targetImage.rectTransform.position;

        // 少し上から開始
        Vector3 startPosition =
            targetPosition +
            Vector3.up * firstAmmoDropHeight;

        // 演出用Image
        GameObject effectObject =
            new GameObject("FirstAmmoLoadEffect");

        effectObject.transform.SetParent(
            targetImage.transform.parent,
            false
        );

        Image effectImage =
            effectObject.AddComponent<Image>();

        effectImage.sprite = sprite;
        effectImage.preserveAspect = true;

        RectTransform effectRect =
            effectObject.GetComponent<RectTransform>();

        effectRect.position =
            startPosition;

        // 最初は少し小さく
        effectRect.localScale =
            Vector3.one * firstAmmoStartScale;

        // ------------------------------------------------
        // 落下
        // ------------------------------------------------

        float timer = 0f;

        while (timer < firstAmmoDropDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / firstAmmoDropDuration
                );

            // なめらかに落下
            float moveT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            effectRect.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    moveT
                );

            // サイズも通常サイズへ
            effectRect.localScale =
                Vector3.Lerp(
                    Vector3.one * firstAmmoStartScale,
                    Vector3.one,
                    moveT
                );

            yield return null;
        }

        // 最終位置
        effectRect.position =
            targetPosition;

        effectRect.localScale =
            Vector3.one;

        // 元のUIを表示
        targetImage.enabled = true;

        Destroy(effectObject);
    }

    //IEnumerator ProcessAmmoUIAnimationQueue()
    //{
    //    isAmmoUIAnimating = true;

    //    while (ammoUIAnimationQueue > 0)
    //    {
    //        ammoUIAnimationQueue--;

    //        yield return StartCoroutine(
    //            PlayAmmoSlideAnimation()
    //        );
    //    }

    //    isAmmoUIAnimating = false;
    //}

    IEnumerator AnimateAmmoSlide(
    RectTransform rect,
    Vector3 startPosition,
    Vector3 endPosition)
    {
        float timer = 0f;

        while (timer < ammoSlideDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / ammoSlideDuration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            rect.localPosition =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    t
                );

            yield return null;
        }

        rect.localPosition =
            endPosition;

        Destroy(rect.gameObject);
    }

    IEnumerator AnimateNextAmmoDrop(
    Sprite sprite,
    RectTransform target)
    {
        if (sprite == null ||
            target == null)
            yield break;

        // =========================================
        // 一時Image
        // =========================================

        GameObject effectObject =
            new GameObject("NextAmmoDropEffect");

        effectObject.transform.SetParent(
            target.parent,
            false
        );

        Image image =
            effectObject.AddComponent<Image>();

        image.sprite =
            sprite;

        image.preserveAspect =
            true;

        RectTransform rect =
            effectObject.GetComponent<RectTransform>();

        // =========================================
        // 少し上から
        // =========================================

        Vector3 targetPosition =
            target.localPosition;

        Vector3 startPosition =
            targetPosition +
            new Vector3(0f, 60f, 0f);

        // =========================================
        // 最初は少し小さく
        // =========================================

        rect.localPosition =
            startPosition;

        rect.localScale =
            Vector3.one * 0.75f;

        float timer = 0f;

        float duration =
            ammoEnterDuration;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            // 上から降りる
            rect.localPosition =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            // 小さい → 通常サイズ
            rect.localScale =
                Vector3.Lerp(
                    Vector3.one * 0.75f,
                    Vector3.one,
                    t
                );

            yield return null;
        }

        rect.localPosition =
            targetPosition;

        rect.localScale =
            Vector3.one;

        Destroy(effectObject);
    }

    public void RefillAmmoAfterResult()
    {
        // =========================================
        // リザルト終了後の状態リセット
        // =========================================

        isChangingScene = false;
        isReloading = false;

        // =========================================
        // 弾を全回復
        // =========================================

        currentAmmo = maxAmmo;

        // =========================================
        // 新しい弾の内容を生成
        // =========================================

        GenerateAmmo();

        // =========================================
        // 数字UI更新
        // =========================================

        UpdateAmmoUI();

        // =========================================
        // 発射タイマーリセット
        // =========================================

        fireTimer = 0f;

        // =========================================
        // 弾UIを現在の弾データに合わせる
        // ※アニメーションは強制停止しない
        // =========================================

        RefreshAmmoUIImmediate();
    }

}