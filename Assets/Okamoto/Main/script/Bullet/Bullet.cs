using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;
    [SerializeField]private int damage; // ダメージ量☆
    private Vector2 direction;


    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
   
    // Playerから呼ばれる
    public void SetDirection(Vector2 dir)
    {
        direction = dir;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Enemyスクリプト取得
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage); // ダメージを与える
            }

            Destroy(gameObject); // 弾は消える
        }
    }

    //ダメージ設定
    public void SetDamage(int value)
    {
        damage = value;
        Debug.Log("ダメージ数" + damage);
    }
}