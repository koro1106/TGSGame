using UnityEngine;

public class SquareRedInward : EnemyBase
{
    public float moveSpeed = 2f;
    public float rotateSpeed = 3f;

    public float spiralStrength = 2f;   // 渦の強さ
    public float inwardStrength = 1.5f; // 中心へ引く力
    public float noiseStrength = 0.3f;  // ちょい揺らぎ

    private Vector2 moveDirection;

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

        Vector2 pos = transform.position;

        // 中心方向（画面中心）
        Vector2 centerDir = -pos.normalized;

        // 接線方向（回転）
        Vector2 tangentDir = new Vector2(-centerDir.y, centerDir.x);

        // 渦巻き力
        Vector2 spiral =
            centerDir * inwardStrength +
            tangentDir * spiralStrength;

        //ノイズ
        spiral += Random.insideUnitCircle * noiseStrength;

        moveDirection += spiral * Time.deltaTime;
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