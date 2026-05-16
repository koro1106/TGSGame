using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GravityBullet : MonoBehaviour
{
    [Header("弾設定")]
    public float lifeTime = 5f;
    [SerializeField] private int damage = 10;

    [Header("重力効果")]
    public float gravityRadius = 5f;      // 引き寄せ範囲
    public float pullForce = 15f;         // 引っ張る強さ
    public float gravityDuration = 2f;    // 吸い込み時間

    [Header("演出")]
    public GameObject gravityEffect;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy centerEnemy = other.GetComponent<Enemy>();

            if (centerEnemy != null)
            {
                // 着弾した敵にダメージ
                centerEnemy.TakeDamage(damage);

                // 重力発生
                StartCoroutine(GravityPull(centerEnemy.transform));
            }

            // 弾の見た目を消す
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;
        }
    }

    IEnumerator GravityPull(Transform centerTarget)
    {
        // ===== 着弾した敵を停止 =====

        // Enemy取得
        Enemy centerEnemy = centerTarget.GetComponent<Enemy>();

        // Rigidbody取得
        Rigidbody2D centerRb = centerTarget.GetComponent<Rigidbody2D>();

        if (centerRb != null)
        {
            centerRb.linearVelocity = Vector2.zero;

            // 位置固定
            centerRb.constraints = RigidbodyConstraints2D.FreezePosition;
        }

        // Enemy移動停止
        if (centerEnemy != null)
        {
            centerEnemy.enabled = false;
        }

        // エフェクト生成
        if (gravityEffect != null)
        {
            Instantiate(gravityEffect, centerTarget.position, Quaternion.identity);
        }

        float timer = 0f;

        while (timer < gravityDuration)
        {
            // 敵が死んだ時対策
            if (centerTarget == null)
            {
                Destroy(gameObject);
                yield break;
            }

            // 範囲内の敵取得
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                centerTarget.position,
                gravityRadius
            );

            foreach (Collider2D hit in hits)
            {
                if (!hit.CompareTag("Enemy"))
                    continue;

                // 着弾した敵自身は除外
                if (hit.transform == centerTarget)
                    continue;

                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    // 中心への方向
                    Vector2 dir =
                        (centerTarget.position - hit.transform.position).normalized;

                    // 引っ張る
                    rb.linearVelocity = dir * pullForce;
                }
                else
                {
                    // Rigidbody無い場合
                    hit.transform.position = Vector2.MoveTowards(
                        hit.transform.position,
                        centerTarget.position,
                        pullForce * Time.deltaTime
                    );
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // ===== 停止解除 =====
        if (centerRb != null)
        {
            // 回転だけ固定
            centerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Enemy再開
        if (centerEnemy != null)
        {
            centerEnemy.enabled = true;
        }

        Destroy(gameObject);
    }

    // ダメージ変更用
    public void SetDamage(int value)
    {
        damage = value;
    }

    // Sceneビューで範囲表示
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, gravityRadius);
    }
}