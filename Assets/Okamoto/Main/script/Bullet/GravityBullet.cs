using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GravityBullet : MonoBehaviour
{
    [Header("’eİ’è")]
    public float lifeTime = 5f;

    [SerializeField]
    private int damage = 10;

    [Header("d—ÍŒø‰Ê")]
    public float gravityRadius = 5f;
    public float pullForce = 15f;
    public float gravityDuration = 2f;

    [Header("‰‰o")]
    public GameObject gravityEffect;

    public PlayerStats stats;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // EnemyHPæ“¾
        EnemyHP centerEnemy =
            other.GetComponent<EnemyHP>();

        // EnemyHP‚ª–³‚¯‚ê‚Î–³‹
        if (centerEnemy == null)
            return;

        int totalDamage =
            damage +
            stats.effectBulletDamage;

        // ’…’e‚µ‚½“G‚Éƒ_ƒ[ƒW
        centerEnemy.TakeDamage(totalDamage);

        // d—Í”­¶
        StartCoroutine(
            GravityPull(centerEnemy.transform)
        );

        // ’e‚ÌŒ©‚½–ÚÁ‚·
        SpriteRenderer sr =
            GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.enabled = false;
        }

        Collider2D col =
            GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }
    }

    IEnumerator GravityPull(Transform centerTarget)
    {
        // ===== ’…’e‚µ‚½“G’â~ =====

        EnemyHP centerEnemy =
            centerTarget.GetComponent<EnemyHP>();

        Rigidbody2D centerRb =
            centerTarget.GetComponent<Rigidbody2D>();

        if (centerRb != null)
        {
            centerRb.linearVelocity =
                Vector2.zero;

            // ˆÊ’uŒÅ’è
            centerRb.constraints =
                RigidbodyConstraints2D.FreezePosition;
        }

        // EnemyMove’â~
        EnemyMove enemyMove =
            centerTarget.GetComponent<EnemyMove>();

        if (enemyMove != null)
        {
            enemyMove.enabled = false;
        }

        // EyeEnemy’â~
        EyeEnemy eyeEnemy =
            centerTarget.GetComponent<EyeEnemy>();

        if (eyeEnemy != null)
        {
            eyeEnemy.enabled = false;
        }

        // ƒGƒtƒFƒNƒg
        if (gravityEffect != null)
        {
            Instantiate(
                gravityEffect,
                centerTarget.position,
                Quaternion.identity
            );
        }

        float timer = 0f;

        while (timer < gravityDuration)
        {
            // “G€–S‘Îô
            if (centerTarget == null)
            {
                Destroy(gameObject);

                yield break;
            }

            // ”ÍˆÍ“à“Gæ“¾
            Collider2D[] hits =
                Physics2D.OverlapCircleAll(
                    centerTarget.position,
                    gravityRadius
                );

            foreach (Collider2D hit in hits)
            {
                EnemyHP enemy =
                    hit.GetComponent<EnemyHP>();

                if (enemy == null)
                    continue;

                // ’†S“GœŠO
                if (hit.transform == centerTarget)
                    continue;

                Rigidbody2D rb =
                    hit.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    // ’†S•ûŒü
                    Vector2 dir =
                        (
                            centerTarget.position -
                            hit.transform.position
                        ).normalized;

                    // ˆø‚Á’£‚é
                    rb.linearVelocity =
                        dir * pullForce;
                }
                else
                {
                    // Rigidbody–³‚µ
                    hit.transform.position =
                        Vector2.MoveTowards(
                            hit.transform.position,
                            centerTarget.position,
                            pullForce * Time.deltaTime
                        );
                }
            }

            timer += Time.deltaTime;

            yield return null;
        }

        // ===== ’â~‰ğœ =====

        if (centerRb != null)
        {
            // ‰ñ“]‚¾‚¯ŒÅ’è
            centerRb.constraints =
                RigidbodyConstraints2D.FreezeRotation;
        }

        // EnemyMoveÄŠJ
        if (enemyMove != null)
        {
            enemyMove.enabled = true;
        }

        // EyeEnemyÄŠJ
        if (eyeEnemy != null)
        {
            eyeEnemy.enabled = true;
        }

        Destroy(gameObject);
    }

    // ƒ_ƒ[ƒW•ÏX
    public void SetDamage(int value)
    {
        damage = value;
    }

    // ”ÍˆÍ•\¦
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(
            transform.position,
            gravityRadius
        );
    }
}