using Unity.Mathematics;
using UnityEngine;

public class Bullet1 : MonoBehaviour
{
    [Header("移動速度")]
    [SerializeField] public float speed = 8f;

    [Header("通常ダメージ")]
    [SerializeField] public int damage = 10; // ダメージ量

    [Header("クリティカル設定")]

    //クリティカル発生確率
    [Range(0f, 100f)]
    [SerializeField] public float criticalChance = 50f;

    //クリティカルダメージ倍率
    [SerializeField] public float criticalMultiplier = 2f;

    //球の進行方向
    private Vector2 direction;

    // Playerから呼ばれる
    public void SetDirection(Vector2 dir)
    {
        direction = dir;
    }

    void Update()
    {
        // 指定方向に移動
        transform.Translate(
           direction * speed * Time.deltaTime,
           Space.World
       );

        // 画面外で削除
        if (Mathf.Abs(transform.position.x) > 1100f ||
           Mathf.Abs(transform.position.y) > 600f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Enemyスクリプト取得
            EnemyHP enemy = other.GetComponent<EnemyHP>();

            if (enemy == null)
                return;

            //クリティカル判定
            float randomValue = UnityEngine.Random.Range(0f, 100f); //確率範囲

            //クリティカルしたか
            bool isCritical = randomValue < criticalChance;

            //  最終ダメージ
            int finalDamage = damage;

            //クリティカル時の処理
            if(isCritical)
            {
                //ダメージ倍率を適用
                finalDamage = Mathf.RoundToInt(
                    damage * criticalMultiplier);

                Debug.Log("クリティカル");


            }

            //敵にダメージ
            enemy.TakeDamage(finalDamage, isCritical);

            Destroy(gameObject); // 弾は消える
        }
    }
}
