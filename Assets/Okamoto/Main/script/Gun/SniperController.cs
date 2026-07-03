using UnityEngine;

public class SniperController : MonoBehaviour
{
    [Header("ï\é¶êÿë÷")]
    public bool isActive = false;

    [Header("íeâï˙")]
    public bool unlockBullet = false;
    [SerializeField] PlayerStats stats;

    [Header("í èÌíe")]
    public GameObject defaultBulletPrefab;

    [Header("âï˙å„ÇÃíe")]
    public GameObject[] unlockedBulletPrefabs;

    [Header("ê´î\")]
    public float bulletSpeed = 20f;

    [Header("î≠éÀà íu")]
    public Transform muzzle;

    private Camera cam;
    [SerializeField] RectTransform crosshair;

    [SerializeField] GunController gunController;


    void Start()
    {
        cam = Camera.main;
    }
    public void ActivateSniper()
    {
        isActive = true;
        gameObject.SetActive(true);
    }

    public void Fire()
    {
        if (!isActive) return;

        GameObject bulletPrefabToShoot;
        if (stats.sniperBulletUnlocked) unlockBullet = true;

        // âï˙ëO
        if (!unlockBullet || unlockedBulletPrefabs.Length == 0)
        {
            bulletPrefabToShoot = defaultBulletPrefab;
        }
        else
        {
            int randomIndex =
                Random.Range(0, unlockedBulletPrefabs.Length);

            bulletPrefabToShoot =
                unlockedBulletPrefabs[randomIndex];
        }

        ShootBullet(bulletPrefabToShoot);
    }

    private void ShootBullet(GameObject prefab)
    {
        Vector3 screenPos =
            gunController.Crosshair.position;

        Vector3 worldPos =
            gunController.Cam.ScreenToWorldPoint(
                screenPos);

        worldPos.z = 0;

        Vector2 direction =
            (worldPos - muzzle.position).normalized;

        GameObject bullet =
            Instantiate(
                prefab,
                muzzle.position,
                Quaternion.identity);

        bullet.transform.right =
            direction;

        Rigidbody2D rb =
            bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                direction * bulletSpeed;
        }
    }
}