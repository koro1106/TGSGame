using UnityEngine;

public class SniperController : MonoBehaviour
{
    [Header("ï\é¶êÿë÷")]
    public bool isActive = false;

    [Header("íe")]
    public GameObject bulletPrefab;
    public Transform muzzle;

    [Header("ê´î\")]
    public float bulletSpeed = 20f;

    public void ActivateSniper()
    {
        isActive = true;

        gameObject.SetActive(true);
    }

    public void Fire()
    {
        if (!isActive) return;

        GameObject bullet =
            Instantiate(
                bulletPrefab,
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