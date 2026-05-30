using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private Vector3 crosshairPos;

    [Range(0.1f, 10f)]
    public float sensitivity = 1f;

    [Header("弾UI画像")]
    public Sprite normalAmmoSprite;
    public Sprite lightningAmmoSprite;

    [Header("マズルフラッシュ")]
    public GameObject muzzleFlash;
    public float muzzleFlashTime = 0.05f;
    private Coroutine flashRoutine;

    public AmmoSlot[] ammoSlots;
    public TMP_Text ammoText;

    public RectTransform crosshair; // UIのクロスヘア

    private SpriteRenderer sr;
    private Camera cam;

    public GameObject[] ammoDropPrefabs; // bulletPrefabs と同じ順番で設定弾プレハブ

    public PlayerStats stats; // プレイヤーステータス☆

    private float crosshairTargetRotation = 0f;

    void Start()
    {
        crosshairPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        crosshair.position = crosshairPos;

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
    }

    void Awake()
    {
        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();

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
    }

    void Aim()
    {
        Vector3 screenPos = crosshair.position;
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        Vector3 dir = worldPos - gunPivot.position;

        float angle =
            Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        bool isLeft = dir.x < 0;

        // 角度はいじらない
        gunPivot.rotation =
            Quaternion.Euler(0, 0, angle);

        // 見た目だけ反転
        if (isLeft)
        {
            gunImage.localScale =
                new Vector3(1, -1, 1);
        }
        else
        {
            gunImage.localScale =
                new Vector3(1, 1, 1);
        }

        // 位置は常に固定
        gunImage.localPosition =
            defaultLocalPos;
    }

    void Shoot()
    {
        fireTimer += Time.deltaTime;

        if (Input.GetMouseButton(0) && fireTimer >= fireRate)
        {
            // 弾切れ
            if (currentAmmo <= 0) return;

            // =========================
            // 右端の弾UI取得
            // =========================
            int uiIndex = currentAmmo - 1;

            // 念のため範囲チェック
            if (uiIndex < 0 || uiIndex >= ammoSlots.Length)
                return;

            // 現在使う弾スロット
            AmmoSlot slot = ammoSlots[uiIndex];

            // UI画像
            Image img = slot.image;

            // =========================
            // 発射する弾を決定
            // =========================

            // デフォルトは通常弾
            GameObject bulletToShoot = bulletPrefabs[0];

            // 弾タイプで変更
            switch (slot.ammoType)
            {
                // 通常弾
                case AmmoType.Normal:
                    bulletToShoot = bulletPrefabs[0];
                    break;

                // 雷属性弾
                case AmmoType.Lightning:
                    bulletToShoot = bulletPrefabs[1];
                    break;
            }

            // =========================
            // 弾スクリプト取得
            // =========================
            Bullet bulletScript =
                bulletToShoot.GetComponent<Bullet>();

            ChainBullet chainScript =
                bulletToShoot.GetComponent<ChainBullet>();

            // =========================
            // 弾生成
            // =========================
            GameObject bulletInstance =
                Instantiate(
                    bulletToShoot,
                    muzzle.position,
                    muzzle.rotation);
            PlayMuzzleFlash();
            crosshairTargetRotation += 90f;


            // =========================
            // ダメージ設定
            // =========================
            if (bulletScript != null)
            {
                bulletScript.SetDamage(stats.bulletDamage);
            }

            // =========================
            // 発射方向
            // =========================
            Rigidbody2D rb =
                bulletInstance.GetComponent<Rigidbody2D>();

            rb.linearVelocity =
                muzzle.right * bulletSpeed;

            // =========================
            // 弾消費
            // =========================
            currentAmmo--;

            // UIを消す
            if (img != null)
            {
                img.enabled = false;
            }

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
        for(int i = 0; i < ammoSlots.Length; i++)
        {
            AmmoSlot slot = ammoSlots[i];

            // デフォルト通常弾
            slot.ammoType = AmmoType.Normal;
            slot.image.sprite = normalAmmoSprite;

            // 属性弾抽選
            bool canElement =
                stats.unlockedElementalBullets != null &&
                stats.unlockedElementalBullets.Length > 0;

            if (canElement &&
           Random.value < stats.elementalBulletChance)
            {
                slot.ammoType = AmmoType.Lightning;
                slot.image.sprite = lightningAmmoSprite;
            }

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

        crosshair.position = crosshairPos;

        // 追加
        Quaternion targetRot =
            Quaternion.Euler(0, 0, crosshairTargetRotation);

        crosshair.rotation =
            Quaternion.Lerp(
                crosshair.rotation,
                targetRot,
                Time.deltaTime * 30f);
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
        muzzleFlash.transform.localRotation =
            Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        float size = Random.Range(0.8f, 1.2f);
        muzzleFlash.transform.localScale =
            Vector3.one * size;

        muzzleFlash.SetActive(true);

        yield return new WaitForSeconds(muzzleFlashTime);

        muzzleFlash.SetActive(false);
    }

}