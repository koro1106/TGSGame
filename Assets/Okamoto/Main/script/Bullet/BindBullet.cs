using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BindBullet : MonoBehaviour
{
    //==============================
    // ’eİ’è
    //==============================

    [Header("’eİ’è")]
    public float lifeTime = 3f;
    public float bindTime = 3f;

    //==============================
    // S‘©İ’è
    //==============================

    [Header("S‘©İ’è")]
    public float searchRadius = 5f;
    public int bindCount = 3;

    //==============================
    // ½İ’è
    //==============================

    [Header("½İ’è")]
    public GameObject chainPrefab;

    // ½ƒTƒCƒY
    public float chainScale = 1f;

    // ½“¯m‚ÌŠÔŠu’²®
    [Range(0.1f, 3f)]
    public float spacingMultiplier = 0.20f;

    // “G‚Ö‚Ì‚ß‚è‚İ–h~
    public float enemyOffset = 0.5f;

    // Å’áŠÔŠu
    public float minChainSpacing = 0.01f;

    //==============================
    // Enemy‚É“–‚½‚Á‚½
    //==============================

    void OnTriggerEnter2D(Collider2D other)
    {
        // EnemyˆÈŠO–³‹
        if (!other.CompareTag("Enemy"))
            return;

        // Enemyæ“¾
        Enemy firstEnemy =
            other.GetComponent<Enemy>();

        // –³‚¯‚ê‚ÎI—¹
        if (firstEnemy == null)
            return;

        // S‘©ŠJn
        StartCoroutine(
            BindEnemies(firstEnemy)
        );

        // ’e”ñ•\¦
        GetComponent<SpriteRenderer>().enabled =
            false;

        GetComponent<Collider2D>().enabled =
            false;
    }

    //==============================
    // S‘©ˆ—
    //==============================

    IEnumerator BindEnemies(Enemy firstEnemy)
    {
        // ’…’eˆÊ’u
        Vector3 hitPoint =
            firstEnemy.transform.position;

        // ”ÍˆÍ“àæ“¾
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                hitPoint,
                searchRadius
            );

        // ‘ÎÛEnemy
        List<Enemy> targets =
            new List<Enemy>();

        // Å‰‚Ì“G’Ç‰Á
        targets.Add(firstEnemy);

        //==============================
        // ‹ß‚¢Enemy’Ç‰Á
        //==============================

        while (targets.Count < bindCount)
        {
            Enemy nearestEnemy = null;

            float nearestDistance =
                Mathf.Infinity;

            foreach (Collider2D hit in hits)
            {
                // EnemyˆÈŠO–³‹
                if (!hit.CompareTag("Enemy"))
                    continue;

                Enemy enemy =
                    hit.GetComponent<Enemy>();

                // Enemy–³‚¢
                if (enemy == null)
                    continue;

                // ’Ç‰ÁÏ‚İ
                if (targets.Contains(enemy))
                    continue;

                // ‹——£
                float distance =
                    Vector2.Distance(
                        hitPoint,
                        enemy.transform.position
                    );

                // Å‚à‹ß‚¢EnemyXV
                if (distance < nearestDistance)
                {
                    nearestDistance =
                        distance;

                    nearestEnemy =
                        enemy;
                }
            }

            // Œ©‚Â‚©‚ç‚È‚¢
            if (nearestEnemy == null)
                break;

            // ’Ç‰Á
            targets.Add(nearestEnemy);
        }

        //==============================
        // Ú‘±Enemy
        //==============================

        List<Enemy> connectedEnemies =
            new List<Enemy>();

        //==============================
        // ½•Û‘¶
        //==============================

        List<List<GameObject>> allChains =
            new List<List<GameObject>>();

        //==============================
        // S‘©ŠJn
        //==============================

        foreach (Enemy enemy in targets)
        {
            // S‘©
            enemy.StartBind(bindTime);

            // Å‰‚Ì“G‚Í”ò‚Î‚·
            if (enemy == firstEnemy)
                continue;

            connectedEnemies.Add(enemy);

            // ½ƒŠƒXƒgì¬
            List<GameObject> chains =
                new List<GameObject>();

            allChains.Add(chains);
        }

        //==============================
        // Spriteî•ñ
        //==============================

        SpriteRenderer spriteRenderer =
            chainPrefab.GetComponent<SpriteRenderer>();

        Sprite sprite =
            spriteRenderer.sprite;

        // Sprite‰¡•
        float spriteWidth =
            sprite.rect.width /
            sprite.pixelsPerUnit;

        // Scale‚İ
        float spacing =
            spriteWidth *
            chainScale *
            spacingMultiplier;

        // Å’áŠÔŠu
        spacing =
            Mathf.Max(
                spacing,
                minChainSpacing
            );


        //==============================
        // S‘©’†
        //==============================

        float timer = 0f;

        while (timer < bindTime)
        {
            timer += Time.deltaTime;

            //==============================
            // ŠeEnemy
            //==============================

            for (int i = 0;
                i < connectedEnemies.Count;
                i++)
            {
                Enemy targetEnemy =
                    connectedEnemies[i];

                // EnemyÁ‚¦‚½
                if (targetEnemy == null)
                    continue;

                //==============================
                // ŠJnˆÊ’u
                //==============================

                Vector3 start =
                    firstEnemy.transform.position;

                //==============================
                // I—¹ˆÊ’u
                //==============================

                Vector3 end =
                    targetEnemy.transform.position;

                //==============================
                // •ûŒü
                //==============================

                Vector3 dir =
                    (end - start).normalized;

                //==============================
                // “G“à•”‚É‚ß‚è‚Ü‚È‚¢
                //==============================

                start += dir * enemyOffset;
                end -= dir * enemyOffset;

                //==============================
                // ‹——£
                //==============================

                float distance =
                    Vector3.Distance(
                        start,
                        end
                    );

                //==============================
                // •K—v½”
                //==============================

                // +1‚µ‚ÄÅŒã‚ÌŒ„ŠÔ–h~
                int chainCount =
                    Mathf.Max(
                        2,
                        Mathf.CeilToInt(
                            distance / spacing
                        ) + 1
                    );

                //==============================
                // ½ƒŠƒXƒg
                //==============================

                List<GameObject> chains =
                    allChains[i];

                //==============================
                // ‘«‚è‚È‚¢½¶¬
                //==============================

                while (chains.Count < chainCount)
                {
                    GameObject chain =
                        Instantiate(
                            chainPrefab
                        );

                    chains.Add(chain);
                }

                //==============================
                // ‘½‚¢½íœ
                //==============================

                while (chains.Count > chainCount)
                {
                    Destroy(chains[0]);

                    chains.RemoveAt(0);
                }

                //==============================
                // ½”z’u
                //==============================

                for (int j = 0;
                         j < chainCount;
                         j++)
                {
                    Vector3 pos;

                    // ÅŒã‚¾‚¯I“_‚É‡‚í‚¹‚é
                    if (j == chainCount - 1)
                    {
                        pos = end;
                    }
                    else
                    {
                        pos =
                            start +
                            dir *
                            (j * spacing);
                    }

                    // ˆÊ’u
                    chains[j]
                        .transform.position =
                        pos;

                    // ‰ñ“]
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

                    // ƒTƒCƒY
                    chains[j]
                        .transform.localScale =
                        Vector3.one *
                        chainScale;
                }
            }

            yield return null;
        }

        //==============================
        // ½íœ
        //==============================

        foreach (var chains in allChains)
        {
            foreach (var chain in chains)
            {
                if (chain != null)
                {
                    Destroy(chain);
                }
            }
        }

        //==============================
        // ’eíœ
        //==============================

        Destroy(gameObject);
    }

    //==============================
    // ”ÍˆÍ•\¦
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