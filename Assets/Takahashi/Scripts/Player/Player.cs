using UnityEngine;
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireInterval = 0.2f;
    [SerializeField] private float moveSpeed = 500f; // 移動速度

    private float timer;

    void Awake()
    {
        Instance = this;
    }
    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        transform.position = Vector3.zero; // 中央固定（初期位置のみ）
    }

    void Update()
    {
        Move();

        timer += Time.deltaTime;
        // 左クリック
        if (Input.GetMouseButton(0))
        {
            if (timer >= fireInterval)
            {
                Shoot();
                timer = 0f;
            }
        }
    }

    // WASD / 矢印キーで移動
    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D または ←/→
        float v = Input.GetAxisRaw("Vertical");   // W/S または ↑/↓

        Vector2 dir = new Vector2(h, v).normalized; // 斜め移動が速くならないように正規化

        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
    }

    void Shoot()
    {
        // マウスの位置をワールド座標に変換
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        // プレイヤー → マウス方向のベクトル
        Vector2 direction = (mousePos - transform.position).normalized;
        // 弾生成
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        // 弾に方向を渡す
        bullet.GetComponent<Bullet1>().SetDirection(direction);
    }
}