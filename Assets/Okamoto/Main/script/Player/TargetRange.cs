using UnityEngine;

public class TargetRange : MonoBehaviour
{
    [Header("ターゲット範囲")]
    public float range = 5f;

    [Header("一番近いEnemyに表示するImage")]
    public GameObject targetImage;

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

        // EnemyControllerが付いているオブジェクトだけ取得
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

            // 範囲外は無視
            if (distance > range)
                continue;

            // 一番近いEnemyを保存
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

        // 範囲内にEnemyがいない
        if (CurrentTarget == null)
        {
            targetImage.SetActive(false);
            return;
        }

        // Imageを表示
        targetImage.SetActive(true);

        // Enemyの位置に移動
        targetImage.transform.position =
            CurrentTarget.position;
    }
}