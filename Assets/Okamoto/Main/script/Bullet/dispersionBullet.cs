using UnityEngine;

public class dispersionBullet : MonoBehaviour
{
    public float lifeTime = 5f;

    [SerializeField] private int damage;
    [SerializeField] private float bulletSpeed = 15f;

    private bool hasSplit = false;
    private Vector2 direction;


    void Start()
    {
        Destroy(gameObject, lifeTime);

        if (!hasSplit)
        {
            Invoke(nameof(Split), 0.01f);
        }
    }
    void Update()
    {
        transform.Translate(
            direction * bulletSpeed * Time.deltaTime,
            Space.World
        );
    }


   void Split()
{
    Vector2 forward = direction.normalized;

    Vector2 rightUp =
        Quaternion.Euler(0, 0, 30) * forward;

    Vector2 leftUp =
        Quaternion.Euler(0, 0, -30) * forward;

    CreateBullet(forward);
    CreateBullet(rightUp);
    CreateBullet(leftUp);

    gameObject.SetActive(false);
}

    void CreateBullet(Vector2 dir)
    {
        GameObject bullet =
            Instantiate(
                gameObject,
                transform.position,
                Quaternion.FromToRotation(Vector2.right, dir));

        dispersionBullet script =
            bullet.GetComponent<dispersionBullet>();

        script.hasSplit = true;
        script.damage = damage;
        script.direction = dir.normalized;
    }

    public void SetDamage(int value)
    {
        damage = value;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                Debug.Log("ìGÇ… " + damage + " É_ÉÅÅ[ÉW");
            }

            Destroy(gameObject);
        }
    }
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }
}