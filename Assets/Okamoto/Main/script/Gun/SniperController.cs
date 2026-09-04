using UnityEngine;

public class SniperController : MonoBehaviour
{
    [Header("表示切替")]
    public bool isActive = false;

    [Header("弾解放")]
    public bool unlockBullet = false;
    [SerializeField] PlayerStats stats;

    [Header("通常弾")]
    public GameObject defaultBulletPrefab;

    [Header("解放後の弾")]
    public GameObject[] unlockedBulletPrefabs;

    [Header("性能")]
    public float bulletSpeed = 20f;

    [Header("発射位置")]
    public Transform muzzle;

    [Header("自動ターゲット")]
    public TargetRange targetRange;

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

        // 解放前
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
        Vector3 targetPosition;

        // =========================================
        // ロックオン中の敵がいる場合
        // =========================================
        if (targetRange != null &&
            targetRange.CurrentTarget != null)
        {
            targetPosition =
                targetRange.CurrentTarget.position;
        }
        else
        {
            // =========================================
            // ロックオンしていない場合
            // クロスヘア方向へ撃つ
            // =========================================
            Vector3 screenPos =
                gunController.Crosshair.position;

            targetPosition =
                gunController.Cam.ScreenToWorldPoint(screenPos);

            targetPosition.z = 0;
        }

        // =========================================
        // 発射方向
        // =========================================

        Vector2 direction =
            (targetPosition - muzzle.position).normalized;

        // =========================================
        // 弾生成
        // =========================================

        GameObject bullet =
            Instantiate(
                prefab,
                muzzle.position,
                Quaternion.identity);

        bullet.transform.right = direction;

        Rigidbody2D rb =
            bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                direction * bulletSpeed;
        }
    }

}