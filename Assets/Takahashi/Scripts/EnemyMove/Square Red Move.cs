using UnityEngine;

public class SquareRedMove : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float rotateSpeed = 3f;
    public float wanderStrength = 0.5f;

    private float limitX = 11f;
    private float limitY = 7f;
    private float returnForce = 1.5f;

    private Vector2 moveDirection;

    void Start()
    {
        SetSpawnAndDirection();
    }

    void Update()
    {
        // ‚ä‚ç‚¬
        moveDirection += Random.insideUnitCircle * wanderStrength * Time.deltaTime;

        Vector2 pos = transform.position;

        // ”ÍˆÍ“à‚É–ß‚·
        if (pos.x > limitX) moveDirection += Vector2.left * returnForce;
        if (pos.x < -limitX) moveDirection += Vector2.right * returnForce;
        if (pos.y > limitY) moveDirection += Vector2.down * returnForce;
        if (pos.y < -limitY) moveDirection += Vector2.up * returnForce;

        moveDirection = moveDirection.normalized;

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }

    void SetSpawnAndDirection()
    {
        Camera cam = Camera.main;

        float height = cam.orthographicSize;
        float width = height * cam.aspect;

        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0:
                transform.position = new Vector2(width + 1, Random.Range(-height, height));
                moveDirection = Vector2.left;
                break;

            case 1:
                transform.position = new Vector2(-width - 1, Random.Range(-height, height));
                moveDirection = Vector2.right;
                break;

            case 2:
                transform.position = new Vector2(Random.Range(-width, width), height + 1);
                moveDirection = Vector2.down;
                break;

            case 3:
                transform.position = new Vector2(Random.Range(-width, width), -height - 1);
                moveDirection = Vector2.up;
                break;
        }

        moveDirection += Random.insideUnitCircle * 0.3f;
        moveDirection = moveDirection.normalized;
    }
}
