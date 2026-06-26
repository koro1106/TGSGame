using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [Header("ダメージ")]
    public int damage = 30;

    [Header("生存時間")]
    public float lifeTime = 1f;

    [Header("参照")]
    [SerializeField]
    private CircleCollider2D col;

    void Start()
    {
        // 爆発発生時に範囲内Enemyへ即ダメージ
        DamageEnemies();

        // 一定時間後削除
        Destroy(gameObject, lifeTime);
    }

    //========================
    // 範囲ダメージ
    //========================

    private void DamageEnemies()
    {
        if (col == null)
            return;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                col.radius * transform.localScale.x
            );

        foreach (Collider2D hit in hits)
        {
            EnemyHP enemy =
                hit.GetComponent<EnemyHP>();

            if (enemy == null)
            {
                enemy =
                    hit.GetComponentInParent<EnemyHP>();
            }

            if (enemy == null)
                continue;

            enemy.TakeDamage(damage);

            Debug.Log(
                enemy.name +
                " に爆発ダメージ " +
                damage
            );
        }
    }
}