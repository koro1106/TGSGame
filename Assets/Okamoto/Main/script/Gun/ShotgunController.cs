using UnityEngine;

public class ShotgunController : MonoBehaviour
{
    [Header("ï\é¶êÿë÷")]
    public bool isActive = false;

    [Header("íeâï˙")]
    public bool unlockBullet = false;
    [SerializeField] PlayerStats stats;

    [Header("éUíeã≠âª")]
    public bool unlockExtraBullet = false;

    [Header("í èÌíe")]
    public GameObject defaultBulletPrefab;

    [Header("âï˙å„Ç…égópÇ∑ÇÈíe")]
    public GameObject[] unlockedBulletPrefabs;

    [Header("î≠éÀà íu")]
    public Transform muzzle;

    [Header("ê´î\")]
    public float bulletSpeed = 20f;
    public float spreadAngle = 15f;

    [SerializeField] GunController gunController;

    private Camera cam;
    [SerializeField] RectTransform crosshair;


    void Start()
    {
        cam = Camera.main;
    }
    public void ActivateShotgun()
    {
        isActive = true;
        gameObject.SetActive(true);
    }

    public void Fire()
    {
        if (!isActive) return;

        GameObject bulletPrefabToShoot;
        if (stats.shotgunBulletUnlocked) unlockBullet = true;

        // âï˙ëO
        if (!unlockBullet ||
            unlockedBulletPrefabs.Length == 0)
        {
            bulletPrefabToShoot =
                defaultBulletPrefab;
        }
        else
        {
            // âï˙å„ÇÕîzóÒÇ©ÇÁÉâÉìÉ_ÉÄ
            int randomIndex =
                Random.Range(
                    0,
                    unlockedBulletPrefabs.Length);

            bulletPrefabToShoot =
                unlockedBulletPrefabs[randomIndex];
        }

        ShootSpread(bulletPrefabToShoot);
    }

    private void ShootSpread(GameObject prefab)
    {
        ShootBullet(prefab, -spreadAngle);
        ShootBullet(prefab, 0f);
        ShootBullet(prefab, spreadAngle);

        if (unlockExtraBullet)
        {
            ShootBullet(prefab, -spreadAngle * 2f);
            ShootBullet(prefab, spreadAngle * 2f);
        }
    }

    private void ShootBullet(
     GameObject prefab,
     float angleOffset)
    {
        // =========================
        // ÉNÉçÉXÉwÉAà íuéÊìæ
        // =========================

        Vector3 screenPos =
            gunController.Crosshair.position;

        Vector3 worldPos =
            gunController.Cam.ScreenToWorldPoint(
                screenPos);

        worldPos.z = 0;

        // =========================
        // ÉNÉçÉXÉwÉAï˚å¸
        // =========================

        Vector2 baseDirection =
            (worldPos - muzzle.position).normalized;

        // éUíeäpìxí«â¡
        Vector2 shootDirection =
            Quaternion.Euler(0f, 0f, angleOffset)
            * baseDirection;

        // =========================
        // íeê∂ê¨
        // =========================

        GameObject bullet =
            Instantiate(
                prefab,
                muzzle.position,
                Quaternion.identity);

        bullet.transform.right =
            shootDirection;

        Rigidbody2D rb =
            bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                shootDirection * bulletSpeed;
        }
    }
}