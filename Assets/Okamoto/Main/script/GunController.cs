using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GunController : MonoBehaviour
{
    public Transform gunPivot;
    public Transform muzzle;
    public GameObject bulletPrefab;

    public float fireRate = 0.1f;
    public float bulletSpeed = 15f;

    public int maxAmmo = 15;
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

    public GameObject ammoDropPrefab;

    void Start()
    {
        crosshairPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        crosshair.position = crosshairPos;
    }

    void Awake()
    {
        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();
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
    }

    void Aim()
    {
        Vector3 screenPos = crosshair.position;
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        Vector3 dir = worldPos - gunPivot.position;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        bool isLeft = dir.x < 0;

        // ★角度はいじらない
        gunPivot.rotation = Quaternion.Euler(0, 0, angle);

        // ★見た目だけ反転
        if (isLeft)
        {
            gunImage.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            gunImage.localScale = new Vector3(1, 1, 1);
        }

        // 位置は常に固定
        gunImage.localPosition = defaultLocalPos;
    }

    void Shoot()
    {
        fireTimer += Time.deltaTime;

        if (Input.GetMouseButton(0) && fireTimer >= fireRate)
        {
            if (currentAmmo <= 0) return;

            GameObject bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            // ★ muzzleの向きに飛ばす
            Vector2 dir = muzzle.right;

            rb.linearVelocity = dir * bulletSpeed;

            currentAmmo--;

            CameraShake.Instance.Shake();

            PlayerHP.Instance.TakeDamage(1);

            Image img = ammoUI[currentAmmo];

            if (img != null)
            {
                // ★ 落ちるUIを生成
                GameObject drop = Instantiate(ammoDropPrefab, img.transform.position, Quaternion.identity, img.transform.parent);

                // ★ 見た目サイズ合わせる（重要）
                drop.transform.localScale = img.transform.localScale;

                // ★ 元UIは消すんじゃなくて非表示
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

}