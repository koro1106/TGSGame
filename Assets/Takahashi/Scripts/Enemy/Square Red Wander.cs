using UnityEngine;

public class SquareRedWander : EnemyBase
{
    public float moveSpeed = 2f;
    public float rotateSpeed = 3f;
    public float changeInterval = 1f;

    private Vector2 moveDirection;
    private float timer;

    private float limitX = 11f;
    private float limitY = 7f;

    protected override void Start()
    {
        base.Start();
        SetSpawnOutside();

        moveDirection = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        base.Update();

        timer += Time.deltaTime;

        // ’èŠú“I‚Éƒ‰ƒ“ƒ_ƒ€•ûŒü•ÏX
        if (timer >= changeInterval)
        {
            moveDirection = Random.insideUnitCircle.normalized;
            timer = 0f;
        }

        Vector2 pos = transform.position;

        // š‚¿‚á‚ñ‚Æ‚µ‚½”½ŽËˆ—
        if (pos.x > limitX)
            moveDirection.x = -Mathf.Abs(moveDirection.x);

        if (pos.x < -limitX)
            moveDirection.x = Mathf.Abs(moveDirection.x);

        if (pos.y > limitY)
            moveDirection.y = -Mathf.Abs(moveDirection.y);

        if (pos.y < -limitY)
            moveDirection.y = Mathf.Abs(moveDirection.y);

        moveDirection = moveDirection.normalized;

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }

    void SetSpawnOutside()
    {
        Camera cam = Camera.main;

        float h = cam.orthographicSize;
        float w = h * cam.aspect;

        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0:
                transform.position = new Vector2(w + 1, Random.Range(-h, h));
                break;
            case 1:
                transform.position = new Vector2(-w - 1, Random.Range(-h, h));
                break;
            case 2:
                transform.position = new Vector2(Random.Range(-w, w), h + 1);
                break;
            case 3:
                transform.position = new Vector2(Random.Range(-w, w), -h - 1);
                break;
        }
    }
}