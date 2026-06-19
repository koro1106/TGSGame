using UnityEngine;

public class ShotgunController : MonoBehaviour
{
    [Header("表示切替")]
    public bool isActive = false;

    [Header("弾解放")]
    public bool unlockBullet = false;

    [Header("散弾強化")]
    public bool unlockExtraBullet = false;

    [Header("通常弾")]
    public GameObject defaultBulletPrefab;

    [Header("解放後に使用する弾")]
    public GameObject[] unlockedBulletPrefabs;

    [Header("発射位置")]
    public Transform muzzle;

    [Header("性能")]
    public float bulletSpeed = 20f;
    public float spreadAngle = 15f;

    public void ActivateShotgun()
    {
        isActive = true;
        gameObject.SetActive(true);
    }

    public void Fire()
    {
        if (!isActive) return;

        GameObject bulletPrefabToShoot;

        // 解放前
        if (!unlockBullet ||
            unlockedBulletPrefabs.Length == 0)
        {
            bulletPrefabToShoot =
                defaultBulletPrefab;
        }
        else
        {
            // 解放後は配列からランダム
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
        Quaternion rot =
            muzzle.rotation *
            Quaternion.Euler(
                0f,
                0f,
                angleOffset);

        GameObject bullet =
            Instantiate(
                prefab,
                muzzle.position,
                rot);

        Rigidbody2D rb =
            bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                bullet.transform.right *
                bulletSpeed;
        }
    }
}