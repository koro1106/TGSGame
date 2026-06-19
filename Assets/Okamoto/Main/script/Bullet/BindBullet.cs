using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// çΩëÆê´íe
/// </summary>
public class BindBullet : MonoBehaviour
{
    private Vector3 shootStartPos;
    [SerializeField] PlayerStats stats;
    // ëΩèdÉqÉbÉgñhé~
    private bool hasHit = false;

    //==============================
    // íeê›íË
    //==============================

    [Header("íeê›íË")]
    public float lifeTime = 3f;
    public float bindTime = 3f;

    //==============================
    // çSë©ê›íË
    //==============================

    [Header("çSë©ê›íË")]
    public float searchRadius = 5f;
    public int bindCount = 3;

    //==============================
    // çΩê›íË
    //==============================

    [Header("çΩê›íË")]
    public GameObject chainPrefab;

    public float chainScale = 1f;

    [Range(0.1f, 3f)]
    public float spacingMultiplier = 0.20f;

    public float enemyOffset = 0.5f;

    public float minChainSpacing = 0.01f;

    public float chainExtendLength = 20f;

    public float firstChainLength = 30f;

    //==============================
    // äJén
    //==============================

    void Start()
    {
        shootStartPos = transform.position;

        // ìGÇ…ìñÇΩÇÁÇ»Ç©Ç¡ÇΩÇÁè¡Ç¶ÇÈ
        StartCoroutine(AutoDestroy());
        bindCount += stats.chainBulletUP;   // ê´î\UPï™â¡éZ
    }

    IEnumerator AutoDestroy()
    {
        yield return new WaitForSeconds(lifeTime);

        if (!hasHit)
        {
            Destroy(gameObject);
        }
    }

    //==============================
    // EnemyÇ…ìñÇΩÇ¡ÇΩ
    //==============================

    void OnTriggerEnter2D(Collider2D other)
    {
        // ëΩèdÉqÉbÉgñhé~
        if (hasHit)
            return;

        // Enemyà»äOñ≥éã
        if (!other.CompareTag("Enemy"))
            return;

        hasHit = true;

        // EnemyHPéÊìæ
        EnemyHP firstEnemy =
    other.GetComponent<EnemyHP>();

        if (firstEnemy == null)
            return;

        // Colliderí‚é~
        Collider2D col =
            GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }

        // å©ÇΩñ⁄è¡Ç∑
        SpriteRenderer sr =
            GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.enabled = false;
        }

        // çSë©äJén
        StartCoroutine(
            BindEnemies(firstEnemy)
        );
    }

    //==============================
    // çSë©èàóù
    //==============================

    IEnumerator BindEnemies(
    EnemyHP firstEnemy
    )
    {
        GameObject chainRoot =
            new GameObject("ChainRoot");

        Vector3 hitPoint =
            firstEnemy.transform.position;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                hitPoint,
                searchRadius
            );

        //==============================
        // ëŒè€Enemy
        //==============================

        List<EnemyHP> targets =
            new List<EnemyHP>();

        targets.Add(firstEnemy);

        //==============================
        // ãﬂÇ¢Enemyí«â¡
        //==============================


        while (targets.Count < bindCount)
        {
            EnemyHP nearestEnemy = null;

            float nearestDistance =
                Mathf.Infinity;

            foreach (Collider2D hit in hits)
            {
                if (!hit.CompareTag("Enemy"))
                    continue;

                EnemyHP enemy =
                    hit.GetComponent<EnemyHP>();

                if (enemy == null)
                    continue;

                if (targets.Contains(enemy))
                    continue;

                float distance =
                    Vector2.Distance(
                        hitPoint,
                        enemy.transform.position
                    );

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            if (nearestEnemy == null)
                break;

            targets.Add(nearestEnemy);
        }

        //==============================
        // ê⁄ë±Enemy
        //==============================

        List<EnemyHP> connectedEnemies =
            new List<EnemyHP>();

        //==============================
        // çΩï€ë∂
        //==============================

        List<List<GameObject>> allChains =
            new List<List<GameObject>>();

        //==============================
        // çSë©äJén
        //==============================

        foreach (EnemyHP enemy in targets)
        {
            enemy.StartBind(bindTime);

            if (enemy == firstEnemy)
                continue;

            connectedEnemies.Add(enemy);

            List<GameObject> chains =
                new List<GameObject>();

            allChains.Add(chains);
        }

        //==============================
        // SpriteèÓïÒ
        //==============================

        SpriteRenderer spriteRenderer =
            chainPrefab.GetComponent<SpriteRenderer>();

        Sprite sprite =
            spriteRenderer.sprite;

        float spriteWidth =
            sprite.rect.width /
            sprite.pixelsPerUnit;

        float spacing =
            spriteWidth *
            chainScale *
            spacingMultiplier;

        spacing =
            Mathf.Max(
                spacing,
                minChainSpacing
            );

        //==============================
        // çSë©íÜ
        //==============================

        float timer = 0f;

        while (timer < bindTime)
        {
            timer += Time.deltaTime;

            // Enemyè¡ñ≈ëŒçÙ
            if (firstEnemy == null)
                break;

            for (
                int i = 0;
                i < connectedEnemies.Count;
                i++
            )
            {
                EnemyHP targetEnemy =
                    connectedEnemies[i];

                List<GameObject> chains =
                    allChains[i];

                // ìGè¡Ç¶ÇΩ
                if (targetEnemy == null)
                {
                    foreach (GameObject chain in chains)
                    {
                        if (chain != null)
                        {
                            Destroy(chain);
                        }
                    }

                    chains.Clear();

                    continue;
                }

                //==============================
                // äJénà íu
                //==============================

                Vector3 start =
                    firstEnemy.transform.position;

                //==============================
                // èIóπà íu
                //==============================

                Vector3 realEnd =
                    targetEnemy.transform.position;

                Vector3 dir =
                    (realEnd - start)
                    .normalized;

                start -=
                    dir *
                    chainExtendLength;

                realEnd +=
                    dir *
                    chainExtendLength;

                //==============================
                // ãóó£
                //==============================

                float distance =
                    Vector3.Distance(
                        start,
                        realEnd
                    );

                //==============================
                // ïKóvçΩêî
                //==============================

                int chainCount =
                    Mathf.Max(
                        2,
                        Mathf.CeilToInt(
                            distance / spacing
                        ) + 1
                    );

                //==============================
                // ë´ÇËÇ»Ç¢çΩê∂ê¨
                //==============================

                while (
                    chains.Count <
                    chainCount
                )
                {
                    GameObject chain =
                        Instantiate(chainPrefab);

                    chain.transform.SetParent(
                        chainRoot.transform,
                        true
                    );

                    chains.Add(chain);
                }

                //==============================
                // ëΩÇ¢çΩçÌèú
                //==============================

                while (
                    chains.Count >
                    chainCount
                )
                {
                    Destroy(chains[0]);

                    chains.RemoveAt(0);
                }

                //==============================
                // çΩîzíu
                //==============================

                for (
                    int j = 0;
                    j < chainCount;
                    j++
                )
                {
                    Vector3 pos;

                    if (j == chainCount - 1)
                    {
                        pos = realEnd;
                    }
                    else
                    {
                        pos =
                            start +
                            dir *
                            (j * spacing);
                    }

                    chains[j]
                        .transform.position =
                        pos;

                    float angle =
                        Mathf.Atan2(
                            dir.y,
                            dir.x
                        ) * Mathf.Rad2Deg;

                    chains[j]
                        .transform.rotation =
                        Quaternion.Euler(
                            0,
                            0,
                            angle
                        );

                    chains[j]
                        .transform.localScale =
                        Vector3.one *
                        chainScale;
                }
            }

            yield return null;
        }

        //==============================
        // çΩçÌèú
        //==============================

        for (int i = 0; i < allChains.Count; i++)
        {
            for (int j = 0; j < allChains[i].Count; j++)
            {
                if (allChains[i][j] != null)
                {
                    Destroy(allChains[i][j]);
                }
            }

            allChains[i].Clear();
        }

        allChains.Clear();

        // RootçÌèú
        if (chainRoot != null)
        {
            Destroy(chainRoot);
        }

        // íeçÌèú
        Destroy(gameObject);
    }

    //==============================
    // îÕàÕï\é¶
    //==============================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            searchRadius
        );
    }
}