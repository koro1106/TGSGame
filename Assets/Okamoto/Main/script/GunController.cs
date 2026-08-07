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

    // ★追加：クロスヘアが乗っているCanvasとそのRenderMode判定
    private Canvas crosshairCanvas;
    private bool isWorldSpaceCanvas;

    //ショットガン追加
    public ShotgunController shotgun;
    //スナイパー追加
    public SniperController sniper;
    //ハンドガン追加
    public HandGunController handgun;

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
        if (Input.GetMouseButton(0) && fireTimer >= fireRate)
        {
            //=========================
            // 撃てる弾があるか
            //=========================
            bool hasAmmo = currentAmmo > 0;

            // 回復演出中弾も撃てる扱い
            if (!hasAmmo)
            {
                for (int i = 0; i < ammoSlots.Length; i++)
                {
                    if (ammoSlots[i].isRecovering)
                    {
                        hasAmmo = true;
                        break;
                    }
                }
            }

            // 弾切れ
            if (!hasAmmo && !isChangingScene)
            {
                StartCoroutine(OutOfAmmoAndChangeScene());
                return;
            }

            // =========================
            // 使用する弾スロット決定
            // =========================
            // =========================================
            // 現在表示されている一番左の弾を撃つ
            // =========================================

            int uiIndex = 0;

            if (uiIndex < 0 ||
                uiIndex >= ammoSlots.Length)
            {
                return;
            }

            AmmoSlot slot =
                ammoSlots[uiIndex];

            // 実際に撃つ弾は、UIの0番ではなく
            // 実データのvisibleStartIndexを見る
            int dataIndex =
                visibleStartIndex;

            if (dataIndex < 0 ||
                dataIndex >= ammoTypes.Length)
            {
                return;
            }

            // 実際の弾タイプ
            AmmoType currentAmmoType =
                ammoTypes[dataIndex];

            slot.ammoType =
                currentAmmoType;

            // UI画像
            Image img =
                slot.image;

            // 範囲チェック
            if (uiIndex < 0 || uiIndex >= ammoSlots.Length)
                return;

            //// 現在使う弾
            //AmmoSlot slot = ammoSlots[uiIndex];
            //// UI画像
            //Image img = slot.image;

            // =========================
            // 発射する弾決定
            // =========================
            GameObject bulletToShoot = GetBulletPrefab(currentAmmoType);

            // =========================
            // 弾生成
            // =========================
            GameObject bulletInstance =
                Instantiate(bulletToShoot, muzzle.position, muzzle.rotation);

            PlayMuzzleFlash();
            crosshairTargetRotation += 90f;

            if (shotgun != null && shotgun.isActive)
            {
                shotgun.Fire();
            }
            if (sniper != null && sniper.isActive)
            {
                sniper.Fire();
            }

            // =========================
            // ダメージ設定
            // =========================
            Bullet bulletScript = bulletToShoot.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDamage(stats.bulletDamage);
            }

            // =========================
            // 発射方向
            // =========================
            Rigidbody2D rb = bulletInstance.GetComponent<Rigidbody2D>();

            // ★ World/Overlay両対応のワールド座標取得に変更
            Vector3 worldPos = GetCrosshairWorldPosition();

            Vector2 direction = (worldPos - muzzle.position).normalized;
            LastShootDirection = direction;

            bulletInstance.transform.right = direction;
            rb.linearVelocity = direction * bulletSpeed;

            // =========================
            // 弾消費
            // =========================
            currentAmmo--;

            // 回復演出中なら消す
            if (slot.recoverEffectObject != null)
            {
                Destroy(slot.recoverEffectObject);
                slot.recoverEffectObject = null;
                slot.isRecovering = false;
            }

            SEManager.Instance.PlayShootSE(); // SE再生

            // 弾UIドロップ演出
            if (ammoDropUIPrefab != null && img != null)
            {
                GameObject drop =
                    Instantiate(
                        ammoDropUIPrefab,
                        img.transform.position,
                        Quaternion.identity,
                        img.transform.parent);

                drop.transform.localScale = img.transform.localScale;

                Image dropImage = drop.GetComponent<Image>();

                if (dropImage != null)
                {
                    dropImage.sprite = img.sprite;
                }
            }

            // 弾UIを更新
            StartCoroutine(PlayAmmoSlideAnimation());

            // =========================
            // 演出
            // =========================
            CameraShake.Instance.Shake();
            PlayerHP.Instance.TakeDamage(1);

            // =========================
            // UI更新
            // =========================
            UpdateAmmoUI();

            // 発射間隔リセット
            fireTimer = 0;
        }
    }

    GameObject GetBulletPrefab(AmmoType type)
    {
        // 通常弾
        if (type == AmmoType.Normal)
        {
            return bulletPrefabs[0];
        }

        // 解放済み属性弾から探す
        foreach (GameObject prefab in stats.unlockedElementalBullets)
        {
            string bulletName = prefab.name;
            switch (type)
            {
                case AmmoType.Lightning:
                    if (bulletName.Contains("Lightning"))
                        return prefab;
                    break;
                case AmmoType.Gravity:
                    if (bulletName.Contains("Gravity"))
                        return prefab;
                    break;
                case AmmoType.Bind:
                    if (bulletName.Contains("Bind"))
                        return prefab;
                    break;
                case AmmoType.Poison:
                    if (bulletName.Contains("Poison"))
                        return prefab;
                    break;
                case AmmoType.Explosion:
                    if (bulletName.Contains("Explosion"))
                        return prefab;
                    break;
                case AmmoType.Penetrating:
                    if (bulletName.Contains("Penetrating"))
                        return prefab;
                    break;
            }
        }

        // 見つからなかったら通常弾
        return bulletPrefabs[0];
    }

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
        }

        // =========================================
        // 100発分の弾データを生成
        // =========================================

        for (int i = 0; i < ammoSlots.Length; i++)
        {
            AmmoType type = AmmoType.Normal;
            Sprite sprite = normalAmmoSprite;

            // -------------------------
            // 属性弾抽選
            // -------------------------

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

                if (bulletName.Contains("Lightning"))
                {
                    type = AmmoType.Lightning;
                    sprite = lightningAmmoSprite;
                }
                else if (bulletName.Contains("Gravity"))
                {
                    type = AmmoType.Gravity;
                    sprite = GravityAmmoSprite;
                }
                else if (bulletName.Contains("Bind"))
                {
                    type = AmmoType.Bind;
                    sprite = BindAmmoSprite;
                }
                else if (bulletName.Contains("Poison"))
                {
                    type = AmmoType.Poison;
                    sprite = PoisonAmmoSprite;
                }
                else if (bulletName.Contains("Explosion"))
                {
                    type = AmmoType.Explosion;
                    sprite = ExplosionAmmoSprite;
                }
                else if (bulletName.Contains("Penetrating"))
                {
                    type = AmmoType.Penetrating;
                    sprite = penetratingAmmoSprite;
                }
            }

            // 実際の弾データを保存
            ammoTypes[i] = type;
            ammoSprites[i] = sprite;
        }

        // =========================================
        // UI表示
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

        // -----------------------------------------
        // 最後の10発を表示
        // -----------------------------------------

        visibleStartIndex =
            Mathf.Max(
                0,
                currentAmmo - visibleCount
            );

        // -----------------------------------------
        // 100個のUIを一旦非表示
        // -----------------------------------------

        for (int i = 0; i < ammoSlots.Length; i++)
        {
            if (ammoSlots[i].image != null)
            {
                ammoSlots[i].image.enabled = false;
            }

            if (ammoSlots[i].emptyImage != null)
            {
                ammoSlots[i].emptyImage.gameObject.SetActive(false);
            }
        }

        // -----------------------------------------
        // 画面には最大10個だけ表示
        // -----------------------------------------

        for (int displayIndex = 0;
             displayIndex < visibleCount;
             displayIndex++)
        {
            int dataIndex =
                visibleStartIndex + displayIndex;

            if (dataIndex < 0 ||
                dataIndex >= ammoTypes.Length)
                continue;

            AmmoSlot uiSlot =
                ammoSlots[displayIndex];

            // Sprite
            uiSlot.image.sprite =
                ammoSprites[dataIndex];

            // 属性
            uiSlot.ammoType =
                ammoTypes[dataIndex];

            // 表示
            uiSlot.image.enabled = true;

            // 空枠
            if (uiSlot.emptyImage != null)
            {
                uiSlot.emptyImage.gameObject.SetActive(true);
            }

            // 位置を必ず元に戻す
            uiSlot.image.rectTransform.localPosition =
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

    IEnumerator OutOfAmmoAndChangeScene()
    {
        isChangingScene = true;

        outOfAmmoTimeline.Play();

        yield return new WaitForSeconds(
            (float)outOfAmmoTimeline.duration);

        SceneManager.LoadScene("MainStageSkillTreeScene");
    }

    IEnumerator PlayAmmoSlideAnimation()
    {
        // =====================================================
        // このアニメーションは「射撃を止めない」
        // =====================================================

        int beforeAmmo = currentAmmo + 1;
        int afterAmmo = currentAmmo;

        int beforeVisibleCount =
            Mathf.Min(visibleAmmoCount, beforeAmmo);

        int afterVisibleCount =
            Mathf.Min(visibleAmmoCount, afterAmmo);

        // =====================================================
        // 発射前の表示開始位置
        // =====================================================

        int beforeStartIndex =
            Mathf.Max(
                0,
                beforeAmmo - beforeVisibleCount
            );

        // =====================================================
        // 発射後の表示開始位置
        // =====================================================

        int afterStartIndex =
            Mathf.Max(
                0,
                afterAmmo - afterVisibleCount
            );

        // =====================================================
        // 弾が0なら即更新
        // =====================================================

        if (afterVisibleCount <= 0)
        {
            RefreshAmmoUIImmediate();
            yield break;
        }

        // =====================================================
        // 現在表示されている弾を取得
        // =====================================================

        Vector3[] startPositions =
            new Vector3[beforeVisibleCount];

        for (int i = 0;
             i < beforeVisibleCount &&
             i < ammoSlots.Length;
             i++)
        {
            startPositions[i] =
                ammoOriginalPositions[i];
        }

        // =====================================================
        // UIを即座に次の状態へ
        // =====================================================

        visibleStartIndex = afterStartIndex;

        RefreshAmmoUIImmediate();

        // =====================================================
        // 左スライド演出
        //
        // 「今表示されている弾」を左へ移動させる
        // =====================================================

        float timer = 0f;

        while (timer < ammoSlideDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / ammoSlideDuration
                );

            t = Mathf.SmoothStep(0f, 1f, t);

            // ---------------------------------------------
            // 2個目以降を左へ
            // ---------------------------------------------

            for (int i = 1;
                 i < beforeVisibleCount &&
                 i < ammoSlots.Length;
                 i++)
            {
                if (ammoSlots[i].image == null)
                    continue;

                ammoSlots[i]
                    .image
                    .rectTransform
                    .localPosition =
                    Vector3.Lerp(
                        startPositions[i],
                        ammoOriginalPositions[i - 1],
                        t
                    );
            }

            yield return null;
        }

        // =====================================================
        // UI位置を完全に元へ戻す
        // =====================================================

        for (int i = 0;
             i < visibleAmmoCount &&
             i < ammoSlots.Length;
             i++)
        {
            if (ammoSlots[i].image == null)
                continue;

            ammoSlots[i]
                .image
                .rectTransform
                .localPosition =
                ammoOriginalPositions[i];
        }

        // =====================================================
        // 10発以上残っている場合
        // ＋から新しい弾を出す
        // =====================================================

        if (afterAmmo >= visibleAmmoCount &&
            ammoPlusPoint != null)
        {
            int newDataIndex =
                afterStartIndex +
                afterVisibleCount -
                1;

            StartCoroutine(
                PlayAmmoEnterEffect(newDataIndex)
            );
        }
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
            timer += Time.deltaTime;

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

        // =====================================================
        // 最終位置・サイズ
        // =====================================================

        effectRect.position =
            endPosition;

        effectRect.localScale =
            Vector3.one;

        // =====================================================
        // 最終的なUI状態を更新
        // =====================================================

        RefreshAmmoUIImmediate();

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

}