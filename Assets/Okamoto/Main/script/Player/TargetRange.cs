using UnityEngine;

public class TargetRange : MonoBehaviour
{
    [Header("ターゲット範囲")]
    public float range = 4000f;

    [Header("一番近いEnemyに表示するImage")]
    public GameObject targetImage;

    public PlayerStats playerStats;

    public Transform CurrentTarget { get; private set; }

    void Start()
    {
        if (targetImage != null)
        {
            targetImage.SetActive(false);
        }
    }

    void Update()
    {
        FindNearestEnemy();
        UpdateTargetImage();
    }

    void FindNearestEnemy()
    {
        CurrentTarget = null;

        float nearestDistance = Mathf.Infinity;

        EnemyHP[] enemies =
            FindObjectsOfType<EnemyHP>();

        foreach (EnemyHP enemy in enemies)
        {
            if (enemy == null)
                continue;

            float distance =
                Vector2.Distance(
                    transform.position,
                    enemy.transform.position
                );

            float targetRange =
                range;

            if (playerStats != null)
            {
                targetRange +=
                    playerStats.targetingRangeUP;
            }

            if (distance > targetRange)
                continue;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;

                CurrentTarget =
                    enemy.transform;
            }
        }
    }

    void UpdateTargetImage()
    {
        if (targetImage == null)
            return;

        if (CurrentTarget == null)
        {
            targetImage.SetActive(false);
            return;
        }

        targetImage.SetActive(true);

        targetImage.transform.position =
            CurrentTarget.position;
    }
}