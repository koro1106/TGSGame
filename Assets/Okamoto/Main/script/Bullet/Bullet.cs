using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;
    private int damage = 20; // ダメージ量
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
}