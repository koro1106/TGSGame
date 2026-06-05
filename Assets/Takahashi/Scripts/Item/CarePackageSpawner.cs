using UnityEngine;

public class CarePackageSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject carePackagePrefab; // スポーンするケアパッケージのPrefab

    [Header("Player")]
    [SerializeField] private Transform player; // プレイヤーの位置
    [SerializeField] private float safeRadius = 200f; // プレイヤーからこの距離以内には出さない

    private Camera cam; // メインカメラ参照

    [Header("スポーン間隔")]
    [SerializeField] private float spawnInterval = 10f; // スポーンする間隔（秒）

    [Header("スポーン確率")]
    [Range(0f, 100f)]
    [SerializeField] private float spawnChance = 30f; // スポーンする確率（%）

    [Header("画面外オフセット")]
    [SerializeField] private float spawnOffsetY = 5f; // カメラ上部からどれだけ上に出すか

    [Header("地面Yランダム")]
    [SerializeField] private float minGroundY = -400f; // 地面Yの最小値（後で渡す用）
    [SerializeField] private float maxGroundY = 400f;  // 地面Yの最大値（後で渡す用）

    private float timer; // スポーン用タイマー

    private void Start()
    {
        cam = Camera.main; // メインカメラを取得
    }

    private void Update()
    {
        timer += Time.deltaTime; // 時間を加算

        // 一定時間経過したらスポーン判定
        if (timer >= spawnInterval)
        {
            timer = 0f; // タイマーリセット
            TrySpawn(); // スポーン試行
        }
    }

    // スポーンするか確率判定
    private void TrySpawn()
    {
        // spawnChanceより大きい場合はスポーンしない
        if (Random.Range(0f, 100f) > spawnChance)
            return;

        Spawn();
    }

    // 実際のスポーン処理
    private void Spawn()
    {
        // カメラの表示範囲（横幅・高さ）を取得
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // 画面左端・右端のワールド座標
        float left = cam.transform.position.x - camWidth;
        float right = cam.transform.position.x + camWidth;

        // プレイヤーから安全距離を考慮したX座標を取得
        float x = GetSafeRandomX(left, right);

        // カメラの上から少し上にスポーン
        float y = cam.transform.position.y + camHeight + spawnOffsetY;

        Vector3 spawnPos = new Vector3(x, y, 0f);

        // プレハブ生成
        GameObject obj = Instantiate(carePackagePrefab, spawnPos, Quaternion.identity);

        // ランダムな地面Yを設定（落下先などに使う想定）
        float randomGroundY = Random.Range(minGroundY, maxGroundY);
        obj.GetComponent<CarePackage>().SetGroundY(randomGroundY);
    }

    // プレイヤーから一定距離離れたX座標を選ぶ
    private float GetSafeRandomX(float left, float right)
    {
        for (int i = 0; i < 20; i++)
        {
            float x = Random.Range(left, right);

            // プレイヤーからsafeRadius以上離れている場合OK
            if (Mathf.Abs(x - player.position.x) > safeRadius)
            {
                return x;
            }
        }

        // うまく見つからなかった場合は端を選ぶ
        float distLeft = Mathf.Abs(left - player.position.x);
        float distRight = Mathf.Abs(right - player.position.x);

        return (distLeft > distRight) ? left : right;
    }
}