using UnityEngine;
/// <summary>
/// ポイント獲得時の演出
/// </summary>
public class PointGetEffect : MonoBehaviour
{
    [Header("タイムリミット")]
    [SerializeField] float lifeTime = 1.2f; // このオブジェクトが存在する時間（秒）

    [Header("重力")]
    [SerializeField] float gravity = 1200f; // 下方向への加速度（見た目上の重力）

    [Header("初速")]
    [SerializeField] float minX = -400f;    // X方向初速の最小値
    [SerializeField] float maxX = 400f;     // X方向初速の最大値
    [SerializeField] float minY = 350f;     // Y方向初速の最小値
    [SerializeField] float maxY = 600f;     // Y方向初速の最大値

    [Header("回転速度")]
    [SerializeField] float minRotateSpeed = -1000f; // 回転速度の最小値
    [SerializeField] float maxRotateSpeed = 1000f;  // 回転速度の最大値

    [Header("回転加速度")]
    [SerializeField] float minRotateAccel = -2000f; // 回転加速度の最小値
    [SerializeField] float maxRotateAccel = 2000f;  // 回転加速度の最大値

    [Header("空気抵抗")]
    [SerializeField] float drag = 0.99f; // 空気抵抗。0?1で小さいほど減速が強い

    // 現在の速度ベクトル
    Vector2 velocity;

    // 現在の回転速度
    float rotateSpeed;

    // 回転の加速度
    float rotateAccel;

    // このUIオブジェクトの RectTransform
    RectTransform rect;

    public void PlayEffect()
    {
        rect = GetComponent<RectTransform>();

        // 初速ランダム
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        velocity = new Vector2(x, y);

        // 回転
        rotateSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);

        // 回転加速
        rotateAccel = Random.Range(minRotateAccel, maxRotateAccel);

        // 自動削除
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (rect == null) return;

        float dt = Time.deltaTime;

        // 重力
        velocity.y -= gravity * dt;

        // 空気抵抗
        velocity.x *= drag;

        // 移動
        rect.anchoredPosition += velocity * dt;

        // 回転
        rotateSpeed += rotateAccel * dt;
        rect.Rotate(0, 0, rotateSpeed * dt);
    }
}
