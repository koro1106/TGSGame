using UnityEngine;

public class AmmoDropUI : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] float lifeTime = 1.2f;

    [Header("Gravity")]
    [SerializeField] float gravity = 1200f;

    [Header("Initial Velocity")]
    [SerializeField] float minX = -400f;
    [SerializeField] float maxX = 400f;
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

        // èâë¨
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        velocity = new Vector2(x, y);

        // âÒì]
        rotateSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);

        // âÒì]â¡ë¨
        rotateAccel = Random.Range(minRotateAccel, maxRotateAccel);

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // èdóÕ
        velocity.y -= gravity * dt;

        // ãÛãCíÔçR
        velocity.x *= drag;

        // à⁄ìÆ
        rect.anchoredPosition += velocity * dt;

        // âÒì]
        rotateSpeed += rotateAccel * dt;
        rect.Rotate(0, 0, rotateSpeed * dt);
    }
}