using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireInterval = 0.2f;

    private float timer;

    void Start()
    {
        transform.position = Vector3.zero; // 中央固定
    }

    void Update()
    {
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
        bullet.GetComponent<Bullet>().SetDirection(direction);
    }
}