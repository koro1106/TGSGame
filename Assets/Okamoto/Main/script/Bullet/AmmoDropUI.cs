using UnityEngine;

public class AmmoDropUI : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] float lifeTime = 1.2f;

    [Header("Gravity")]
    // ¶•ûŒü‚Ö“­‚­d—Í
    [SerializeField] float gravity = 1200f;

    [Header("Initial Velocity")]
    // Å‰‚Ì‰¡•ûŒü‘¬“x
    [SerializeField] float minX = 0f;
    [SerializeField] float maxX = 0f;

    // Å‰‚Ìc•ûŒü‘¬“x
    [SerializeField] float minY = 350f;
    [SerializeField] float maxY = 600f;

    [Header("Rotation")]
    [SerializeField] float minRotateSpeed = -1000f;
    [SerializeField] float maxRotateSpeed = 1000f;

    [Header("Rotation Acceleration")]
    [SerializeField] float minRotateAccel = -2000f;
    [SerializeField] float maxRotateAccel = 2000f;

    [Header("Drag")]
    [SerializeField] float drag = 0.99f;

    Vector2 velocity;

    float rotateSpeed;
    float rotateAccel;

    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        // ‰‘¬
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);

        velocity = new Vector2(x, y);

        // ‰ñ“]
        rotateSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);

        // ‰ñ“]‰Á‘¬
        rotateAccel = Random.Range(minRotateAccel, maxRotateAccel);

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // d—Í‚ğ‰E•ûŒü‚Ö
        velocity.x += gravity * dt;

        // ˆÚ“®
        rect.anchoredPosition += velocity * dt;

        // ‰ñ“]
        rotateSpeed += rotateAccel * dt;
        rect.Rotate(0, 0, rotateSpeed * dt);
    }
}