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

    public Image[] ammoUI;
    public TMP_Text ammoText;

    public RectTransform crosshair; // UIのクロスヘア

    private SpriteRenderer sr;
    private Camera cam;

    public GameObject[] ammoDropPrefabs; // bulletPrefabs と同じ順番で設定弾プレハブ

    public PlayerStats stats; // プレイヤーステータス☆

    void Start()
    {
        crosshairPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        crosshair.position = crosshairPos;

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

        for (int i = 0; i < ammoUI.Length; i++)
        {
            ammoUI[i].enabled = true;
        }

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

        // ▼追加
        // 数字キーで弾を切り替える
        ChangeBullet();
    }

    // ▼追加
    // 弾切り替え処理
    void ChangeBullet()
    {
        // 1キーで1番目の弾
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentBulletIndex = 0;
        }

        // 2キーで2番目の弾
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // 配列に2個以上ある時だけ
            if (bulletPrefabs.Length > 1)
            {
                currentBulletIndex = 1;
            }
        }

        // 3キーで3番目の弾
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // 配列に3個以上ある時だけ
            if (bulletPrefabs.Length > 2)
            {
                currentBulletIndex = 2;
            }
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
        //fireTimer += Time.deltaTime;

        //if (Input.GetMouseButton(0) && fireTimer >= fireRate)
        //{
        //    if (currentAmmo <= 0) return;

        //    // ▼変更
        //    // 現在選択中の弾を取得
        //    GameObject normalBullet =
        //        bulletPrefabs[currentBulletIndex];

        //    // 属性弾が解放されてる場合、30%の確率で置き換える☆
        //    if (stats.unlockedElementalBullets != null &&
        //        stats.unlockedElementalBullets.Length > 0 &&
        //        Random.value < stats.elementalBulletChance)
        //    {
        //        int index = Random.Range(0, stats.unlockedElementalBullets.Length);

        //        normalBullet = stats.unlockedElementalBullets[index];

        //        // 解放された属性弾の中からランダムで一つ選ぶ
        //        Debug.Log("属性弾開放");
        //    }

        //    // Instantiate するのはここで☆
        //    GameObject bulletInstance =
        //        Instantiate(normalBullet,
        //        muzzle.position,
        //        muzzle.rotation);

        //    // ダメージ設定☆
        //    Bullet bulletScript = bulletInstance.GetComponent<Bullet>();
        //    ChainBullet chainScript = bulletInstance.GetComponent<ChainBullet>();

        //    if (bulletScript != null)
        //    {
        //        bulletScript.SetDamage(stats.bulletDamage);
        //        Debug.Log("現在ダメージ : " + stats.bulletDamage);
        //    }


        //    Rigidbody2D rb = bulletInstance.GetComponent<Rigidbody2D>();

        //    // ★ muzzleの向きに飛ばす
        //    Vector2 dir = muzzle.right;
        //    rb.linearVelocity = dir * bulletSpeed;

        //    // ▼ここは共通弾数
        //    // どの弾でもMAXAMMO分だけ撃てる
        //    currentAmmo--;

        //    CameraShake.Instance.Shake();
        //    PlayerHP.Instance.TakeDamage(1);

        //    // UI用プレハブ決定
        //    GameObject dropPrefab = ammoDropPrefab; // デフォルトは通常弾
        //    if (bulletScript != null && bulletScript.ammoDropPrefab != null)
        //    {
        //        dropPrefab = bulletScript.ammoDropPrefab;
        //    }
        //    else if (chainScript != null && chainScript.ammoDropPrefab != null)
        //    {
        //        dropPrefab = chainScript.ammoDropPrefab; // 雷属性弾
        //    }

        //    // UI処理
        //    Image img = ammoUI[currentAmmo]; // ここでUIの右端を取得

        //    if (img != null)
        //    {
        //        // 弾の種類に応じて Image を差し替え
        //        Sprite uiSprite = null;
        //        if (bulletScript != null && bulletScript.ammoUISprite != null)
        //            uiSprite = bulletScript.ammoUISprite;
        //        else if (chainScript != null && chainScript.ammoUISprite != null)
        //            uiSprite = chainScript.ammoUISprite;

        //        if (uiSprite != null)
        //            img.sprite = uiSprite;

        //        // 弾を落とす演出
        //        GameObject drop = Instantiate(dropPrefab, img.transform.position, Quaternion.identity, img.transform.parent);
        //        drop.transform.localScale = img.transform.localScale;

        //        img.enabled = false; // 元UIは非表示
        //    }

        //    UpdateAmmoUI();
        //    fireTimer = 0;
        //}
        fireTimer += Time.deltaTime;

        if (Input.GetMouseButton(0) && fireTimer >= fireRate)
        {
            if (currentAmmo <= 0) return;

            // UIの右端インデックスを取得
            int uiIndex = currentAmmo - 1;
            if (uiIndex < 0 || uiIndex >= ammoUI.Length) uiIndex = 0;

            Image img = ammoUI[uiIndex];

            // 発射する弾をUIに合わせて決定
            GameObject bulletToShoot = bulletPrefabs[currentBulletIndex]; // デフォルト
            Bullet bulletScript = bulletToShoot.GetComponent<Bullet>();
            ChainBullet chainScript = bulletToShoot.GetComponent<ChainBullet>();
            GameObject dropPrefab = null;

            // 属性弾が解放されている場合、ランダムで置き換え
            if (stats.unlockedElementalBullets != null &&
                stats.unlockedElementalBullets.Length > 0 &&
                Random.value < stats.elementalBulletChance)
            {
                int index = Random.Range(0, stats.unlockedElementalBullets.Length);
                bulletToShoot = stats.unlockedElementalBullets[index];

                bulletScript = bulletToShoot.GetComponent<Bullet>();
                chainScript = bulletToShoot.GetComponent<ChainBullet>();

                // 属性弾の落下Prefabを優先
                if (bulletScript != null && bulletScript.ammoDropPrefab != null)
                    dropPrefab = bulletScript.ammoDropPrefab;
                else if (chainScript != null && chainScript.ammoDropPrefab != null)
                    dropPrefab = chainScript.ammoDropPrefab;

                Debug.Log("属性弾開放");
            }
            else
            {
                // 通常弾の場合は配列 ammoDropPrefabs から取得
                if (ammoDropPrefabs != null &&
                    currentBulletIndex >= 0 &&
                    currentBulletIndex < ammoDropPrefabs.Length)
                {
                    dropPrefab = ammoDropPrefabs[currentBulletIndex];
                }
            }

            // UIのスプライトを弾に合わせる
            if (img != null)
            {
                Sprite uiSprite = null;
                if (bulletScript != null) uiSprite = bulletScript.ammoUISprite;
                else if (chainScript != null) uiSprite = chainScript.ammoUISprite;

                if (uiSprite != null)
                    img.sprite = uiSprite;
            }

            // 弾発射
            GameObject bulletInstance = Instantiate(bulletToShoot, muzzle.position, muzzle.rotation);

            if (bulletScript != null)
                bulletScript.SetDamage(stats.bulletDamage);

            Rigidbody2D rb = bulletInstance.GetComponent<Rigidbody2D>();
            rb.linearVelocity = muzzle.right * bulletSpeed;

            currentAmmo--;

            CameraShake.Instance.Shake();
            PlayerHP.Instance.TakeDamage(1);

            // UIの落下演出
            if (img != null && dropPrefab != null)
            {
                GameObject drop = Instantiate(dropPrefab, img.transform.position, Quaternion.identity, img.transform.parent);
                drop.transform.localScale = img.transform.localScale;
                img.enabled = false;
            }

            UpdateAmmoUI();
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

        for (int i = 0; i < ammoUI.Length; i++)
        {
            ammoUI[i].enabled = true;
        }

        StartCoroutine(Reload());
    }

    System.Collections.IEnumerator Reload()
    {
        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
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
}