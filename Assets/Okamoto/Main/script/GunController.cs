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

    [Range(0.1f, 10f)]
    public float sensitivity = 1f;

    [SerializeField]
    private PlayableDirector outOfAmmoTimeline;

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

        // 撃破時弾回復する
        if (stats.recoveryBullet)
            recoverAmmoOnKill = true;

        // 回復弾数増加
        recoverAmmoAmount += stats.recoveryBulletCount;

        //for (int i = 0; i < ammoSlots.Length; i++)
        //{
        //    bool active = i < maxAmmo;
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

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        gunImage.localPosition = new Vector3(0.5f, 0, 0);
        defaultLocalPos = gunImage.localPosition; // ★ 初期位置保存

        UpdateAmmoUI();
        sensitivityText.text = "感度 : " + sensitivity.ToString("F1");

        GenerateAmmo();
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
            int uiIndex = -1;

            // 回復演出中優先
            for (int i = ammoSlots.Length - 1; i >= 0; i--)
            {
                if (ammoSlots[i].isRecovering)
                {
                    uiIndex = i;
                    break;
                }
            }

            // 通常弾
            if (uiIndex == -1)
            {
                uiIndex = currentAmmo - 1;
            }

            // 範囲チェック
            if (uiIndex < 0 || uiIndex >= ammoSlots.Length)
                return;

            // 現在使う弾
            AmmoSlot slot = ammoSlots[uiIndex];
            // UI画像
            Image img = slot.image;

            // =========================
            // 発射する弾決定
            // =========================
            GameObject bulletToShoot = GetBulletPrefab(slot.ammoType);

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
            // 通常弾だけ減らす
            if (!slot.isRecovering)
            {
                currentAmmo--;
            }

            // 回復演出中なら消す
            if (slot.recoverEffectObject != null)
            {
                Destroy(slot.recoverEffectObject);
                slot.recoverEffectObject = null;
                slot.isRecovering = false;
            }

            SEManager.Instance.PlayShootSE(); // SE再生

            // UI消す
            if (img != null)
            {
                img.enabled = false;
            }

            // =========================
            // 弾UIドロップ演出
            // =========================
            if (ammoDropUIPrefab != null && img != null)
            {
                GameObject drop =
                    Instantiate(
                        ammoDropUIPrefab,
                        img.transform.position,
                        Quaternion.identity,
                        img.transform.parent);

                // サイズ合わせ
                drop.transform.localScale = img.transform.localScale;

                // Spriteコピー
                Image dropImage = drop.GetComponent<Image>();
                if (dropImage != null)
                {
                    dropImage.sprite = img.sprite;
                }
            }

            // 元UI消す
            img.enabled = false;

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
        for (int i = 0; i < ammoSlots.Length; i++)
        {
            AmmoSlot slot = ammoSlots[i];
            bool active = i < maxAmmo;

            // 空枠
            slot.emptyImage.gameObject.SetActive(active);
            // 弾
            slot.image.gameObject.SetActive(active);

            if (!active) continue;

            // 通常弾
            slot.ammoType = AmmoType.Normal;
            slot.image.sprite = normalAmmoSprite;

            // 属性弾抽選
            bool canElement =
                stats.unlockedElementalBullets != null &&
                stats.unlockedElementalBullets.Length > 0;

            if (canElement && Random.value < stats.elementalBulletChance)
            {
                // 解放済み属性弾からランダム
                GameObject randomPrefab =
                    stats.unlockedElementalBullets[Random.Range(0, stats.unlockedElementalBullets.Length)];
                string bulletName = randomPrefab.name;

                // =========================
                // 属性判定
                // =========================
                if (bulletName.Contains("Lightning"))
                {
                    slot.ammoType = AmmoType.Lightning;
                    slot.image.sprite = lightningAmmoSprite;
                }
                else if (bulletName.Contains("Gravity"))
                {
                    slot.ammoType = AmmoType.Gravity;
                    slot.image.sprite = GravityAmmoSprite;
                }
                else if (bulletName.Contains("Bind"))
                {
                    slot.ammoType = AmmoType.Bind;
                    slot.image.sprite = BindAmmoSprite;
                }
                else if (bulletName.Contains("Poison"))
                {
                    slot.ammoType = AmmoType.Poison;
                    slot.image.sprite = PoisonAmmoSprite;
                }
                else if (bulletName.Contains("Explosion"))
                {
                    slot.ammoType = AmmoType.Explosion;
                    slot.image.sprite = ExplosionAmmoSprite;
                }
                else if (bulletName.Contains("Penetrating"))
                {
                    slot.ammoType = AmmoType.Penetrating;
                    slot.image.sprite = penetratingAmmoSprite;
                }
            }

            // 弾表示
            slot.image.enabled = true;
        }
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
        // 最大弾数増加
        maxAmmo += amount;
        maxAmmo = Mathf.Clamp(maxAmmo, 0, ammoSlots.Length);

        // 満タン
        currentAmmo = maxAmmo;

        for (int i = 0; i < ammoSlots.Length; i++)
        {
            AmmoSlot slot = ammoSlots[i];
            bool active = i < maxAmmo;

            // 空枠表示
            slot.emptyImage.gameObject.SetActive(active);
            // 弾表示
            slot.image.gameObject.SetActive(active);

            if (!active) continue;

            // 通常弾
            slot.ammoType = AmmoType.Normal;
            slot.image.sprite = normalAmmoSprite;

            // 弾ON
            slot.image.enabled = true;
        }

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
}