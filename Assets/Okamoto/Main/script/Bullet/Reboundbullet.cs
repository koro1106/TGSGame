using UnityEngine;

public class Reboundbullet : MonoBehaviour
{
    [Header("İ’è")]
    public float speed = 15f;
    public float lifeTime = 5f;
    public int damage = 1;

    private Vector2 direction;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // ’e‚ÌŒü‚¢‚Ä‚é•ûŒü‚É”ò‚Ô
        direction = transform.right.normalized;

        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;
    }

    // ŠO•”‚©‚ç•ûŒü•ÏX‚µ‚½‚¢—p
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // •Ç‚Å”½Ë
        if (collision.gameObject.CompareTag("Wall"))
        {
            Vector2 normal = collision.contacts[0].normal;

            direction = Vector2.Reflect(direction, normal).normalized;
        }

        // “G
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}