using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class HandGunController : MonoBehaviour
{
    [Header("References")]
    public Transform gunPivot;
    public Transform muzzle;

    [Header("Crosshair")]
    public RectTransform crosshair;
    public TMP_Text sensitivityText;

    [Range(0.1f, 10f)]
    public float sensitivity = 1f;

    [Header("Gun Visual")]
    public Transform gunImage;

    [Header("•\Ž¦Ø‘Ö")]
    public bool isActive = false;

    [Header("’eÝ’è")]
    public GameObject defaultBulletPrefab;
    public GameObject[] unlockedBulletPrefabs;
    public bool unlockBullet = false;
    [SerializeField] PlayerStats stats;

    [Header("Shoot Settings")]
    public float shootInterval = 2f;
    public float bulletSpeed = 15f;

    [Header("Target Settings")]
    public float searchRange = 20f;

    [Header("Aim Settings")]
    public float rotateSpeed = 5f;

    private Vector3 crosshairPos;
    private Vector3 defaultLocalPos;

    private float timer;
    private Transform lastTarget;

    public void ActivateHandGun()
    {
        isActive = true;
        gameObject.SetActive(true);
    }

    void Start()
    {
        defaultLocalPos = gunImage.localPosition;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        crosshairPos = new Vector3(
            Screen.width / 2f,
            Screen.height / 2f,
            0f
        );

        crosshair.position = crosshairPos;

        if (sensitivityText != null)
        {
            sensitivityText.text =
                "Š´“x : " + sensitivity.ToString("F1");
        }
    }

    void Update()
    {
        if (!isActive) return;

        MoveCrosshair();

        GameObject target = FindNearestEnemy();

        if (target != null)
        {
            lastTarget = target.transform;
        }

        if (lastTarget == null) return;

        Aim(lastTarget);

        timer += Time.deltaTime;

        if (timer >= shootInterval)
        {
            Shoot();
            timer = 0f;
        }
    }

    void Shoot()
    {
        GameObject prefabToShoot;
        if (stats.handgunBulletUnlocked) unlockBullet = true;

        if (!unlockBullet || unlockedBulletPrefabs.Length == 0)
        {
            prefabToShoot = defaultBulletPrefab;
        }
        else
        {
            int index =
                Random.Range(0, unlockedBulletPrefabs.Length);

            prefabToShoot = unlockedBulletPrefabs[index];
        }

        GameObject bullet = Instantiate(
            prefabToShoot,
            muzzle.position,
            Quaternion.identity
        );

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                gunPivot.right * bulletSpeed;
        }
    }

    // ˆÈ‰º‚»‚Ì‚Ü‚Ü
    void MoveCrosshair()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        crosshairPos += new Vector3(mouseX, mouseY, 0f)
            * sensitivity * 25f;

        crosshairPos.x = Mathf.Clamp(crosshairPos.x, 0, Screen.width);
        crosshairPos.y = Mathf.Clamp(crosshairPos.y, 0, Screen.height);

        crosshair.position = crosshairPos;
    }

    void Aim(Transform target)
    {
        Vector3 dir = target.position - gunPivot.position;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        bool isLeft = dir.x < 0;

        Quaternion targetRotation =
            Quaternion.Euler(0, 0, angle);

        gunPivot.rotation =
            Quaternion.Lerp(
                gunPivot.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );

        gunImage.localScale =
            isLeft ? new Vector3(1, -1, 1)
                   : new Vector3(1, 1, 1);

        gunImage.localPosition = defaultLocalPos;
    }

    GameObject FindNearestEnemy()
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        GameObject nearest = null;

        float minDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance =
                Vector2.Distance(
                    transform.position,
                    enemy.transform.position
                );

            if (distance < minDistance &&
                distance <= searchRange)
            {
                minDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    public void SetSensitivity(float value)
    {
        sensitivity = value;

        if (sensitivityText != null)
        {
            sensitivityText.text =
                "Š´“x : " + sensitivity.ToString("F1");
        }
    }
}