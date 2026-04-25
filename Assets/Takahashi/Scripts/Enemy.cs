//using UnityEngine;

//public class Enemy : MonoBehaviour
//{
//    public float moveSpeed = 2f;     // 移動速度
//    public float rotateSpeed = 3f; // 回転速度
//    private Vector2 moveDirection;

//    void Start()
//    {
//        // ランダム方向
//        moveDirection = Random.insideUnitCircle.normalized;
//    }

//    void Update()
//    {
//        // 少しずつ方向をランダムに変える
//        moveDirection += Random.insideUnitCircle * 0.06f;
//        moveDirection = moveDirection.normalized;

//        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
//        // 回転
//        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
//    }
//}
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;       //移動速度
    public float rotateSpeed = 3f;     //回転速度
    public float wanderStrength = 0.5f;//ゆらゆらの大きさ
    private float limitX = 11f;        // 移動できるX範囲（左右）
    private float limitY = 7f;         // 移動できるY範囲（上下）
    private float returnForce = 1.5f;  // 範囲外に出そうなときに内側へ戻す力
    private Vector2 moveDirection;     // 現在の移動方向

    private int maxHP = 40; // 最大HP
    private int currentHP; // 現在HP
    private float scaleSmooth = 12f;// サイズ変化の速さ（大きいほど早く追従）
    private Vector3 baseScale;   // 初期サイズ
    private Vector3 targetScale; // 目標サイズ

    void Start()
    {
        currentHP = maxHP; // 初期化

        baseScale = transform.localScale; // 初期サイズ保存

        targetScale = baseScale;

        SetSpawnAndDirection();         // スポーン位置と初期方向を設定

        UpdateTargetScale(); // 初期サイズ反映
    }

    void Update()
    {
        //ランダムなゆらぎを加える
        moveDirection += Random.insideUnitCircle * wanderStrength * Time.deltaTime;

        // 現在位置を取得
        Vector2 pos = transform.position;

        //範囲外に出そうなら内側に戻す
        if (pos.x > limitX) moveDirection += Vector2.left * returnForce;
        if (pos.x < -limitX) moveDirection += Vector2.right * returnForce;
        if (pos.y > limitY) moveDirection += Vector2.down * returnForce;
        if (pos.y < -limitY) moveDirection += Vector2.up * returnForce;

        // 方向ベクトルを正規化（速度が一定になる）
        moveDirection = moveDirection.normalized;

        // 移動 
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        // 回転（見た目用） 
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

        //徐々にサイズを変える 
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * scaleSmooth
        );
    }

    // ダメージを受ける関数
    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            UpdateTargetScale(); // HP減ったらサイズ更新
        }
    }

    void UpdateTargetScale()
    {
        // HP割合 = サイズ倍率
        float ratio = (float)currentHP / maxHP;

        targetScale = baseScale * ratio;
    }
    void Die()
    {
        Destroy(gameObject); // 敵を消す
    }

    void SetSpawnAndDirection()
    {
        Camera cam = Camera.main;

        float height = cam.orthographicSize;
        float width = height * cam.aspect;

        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: // 右
                transform.position = new Vector2(width + 1, Random.Range(-height, height));
                moveDirection = Vector2.left;
                break;

            case 1: // 左
                transform.position = new Vector2(-width - 1, Random.Range(-height, height));
                moveDirection = Vector2.right;
                break;

            case 2: // 上
                transform.position = new Vector2(Random.Range(-width, width), height + 1);
                moveDirection = Vector2.down;
                break;

            case 3: // 下
                transform.position = new Vector2(Random.Range(-width, width), -height - 1);
                moveDirection = Vector2.up;
                break;
        }

        // 最初の方向を少しだけランダムにして自然にする
        moveDirection += Random.insideUnitCircle * 0.3f;
        moveDirection = moveDirection.normalized;
    }
}