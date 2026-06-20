using UnityEngine;

public class SniperController : MonoBehaviour
{
    [Header("•\¦Ø‘Ö")]
    public bool isActive = false;

    [Header("’e‰ğ•ú")]
    public bool unlockBullet = false;
    [SerializeField] PlayerStats stats;

    [Header("’Êí’e")]
    public GameObject defaultBulletPrefab;

    [Header("‰ğ•úŒã‚Ì’e")]
    public GameObject[] unlockedBulletPrefabs;

    [Header("«”\")]
    public float bulletSpeed = 20f;

    [Header("”­ËˆÊ’u")]
    public Transform muzzle;

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

        // ‰ğ•ú‘O
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
        GameObject bullet =
            Instantiate(
                prefab,
                muzzle.position,
                muzzle.rotation);

        Rigidbody2D rb =
            bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                bullet.transform.right * bulletSpeed;
        }
    }
}