using UnityEngine;

public class ShotgunController : MonoBehaviour
{
    [Header("ï\é¶êÿë÷")]
    public bool isActive = false;

    [Header("íe")]
    public GameObject bulletPrefab;
    public Transform muzzle;

    [Header("ê´î\")]
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

        ShootBullet(-spreadAngle);
        ShootBullet(0);
        ShootBullet(spreadAngle);
    }

    void ShootBullet(float angleOffset)
    {
        Quaternion rot =
            muzzle.rotation *
            Quaternion.Euler(0, 0, angleOffset);

        GameObject bullet =
            Instantiate(
                bulletPrefab,
                muzzle.position,
                rot);

        Rigidbody2D rb =
            bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                bullet.transform.right * bulletSpeed;
        }
    }
}