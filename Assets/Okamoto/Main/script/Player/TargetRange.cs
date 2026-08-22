using UnityEngine;

public class TargetRange : MonoBehaviour
{
    [Header("ターゲット範囲")]
    public float range = 4000f;

    [Header("ロックしたEnemyに表示するImage")]
    public GameObject targetImage;

    public PlayerStats playerStats;

    [Header("クロスヘア")]
    public RectTransform crosshair;

    [Header("ロック変更時間")]
    public float changeTargetTime = 2f;

    // クロスヘアを合わせている敵
    private EnemyHP aimingEnemy;

    // 合わせ続けた時間
    private float aimingTimer = 0f;

    // 現在ロックしている敵
    public Transform CurrentTarget { get; private set; }

    // ロック中のEnemyHP
    private EnemyHP currentEnemyHP;


    void Start()
    {
        if (targetImage != null)
        {
            targetImage.SetActive(false);
        }
    }


    void Update()
    {
        // =====================================
        // まだ敵をロックしていない場合だけ探す
        // =====================================

        if (CurrentTarget == null)
        {
            FindNearestEnemy();
        }

        // =====================================
        // クロスヘアを合わせている敵を確認
        // =====================================

        CheckCrosshairTarget();

        // =====================================
        // ロックしていた敵が消えた場合
        // =====================================

        if (currentEnemyHP == null &&
            CurrentTarget != null)
        {
            ClearTarget();
        }

        UpdateTargetImage();
    }

    void CheckCrosshairTarget()
    {
        if (crosshair == null)
            return;

        EnemyHP[] enemies =
            FindObjectsOfType<EnemyHP>();

        EnemyHP closestToCrosshair = null;

        float closestDistance =
            Mathf.Infinity;

        // =====================================
        // クロスヘアに一番近い敵を探す
        // =====================================

        foreach (EnemyHP enemy in enemies)
        {
            if (enemy == null)
                continue;

            // 現在ロック中の敵は無視
            if (enemy.transform == CurrentTarget)
                continue;

            float distance =
                Vector2.Distance(
                    crosshair.position,
                    enemy.transform.position
                );

            // クロスヘアからの判定範囲
            if (distance > 50f)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestToCrosshair = enemy;
            }
        }

        // =====================================
        // 敵にクロスヘアが合っていない
        // =====================================

        if (closestToCrosshair == null)
        {
            aimingEnemy = null;
            aimingTimer = 0f;

            return;
        }

        // =====================================
        // 同じ敵を狙い続けている
        // =====================================

        if (aimingEnemy == closestToCrosshair)
        {
            aimingTimer += Time.deltaTime;
        }
        else
        {
            // 別の敵にクロスヘアを移動した
            aimingEnemy =
                closestToCrosshair;

            aimingTimer = 0f;
        }

        // =====================================
        // 2秒間狙い続けたらロック変更
        // =====================================

        if (aimingTimer >= changeTargetTime)
        {
            ChangeTarget(
                aimingEnemy
            );

            aimingEnemy = null;
            aimingTimer = 0f;
        }
    }

    void ChangeTarget(EnemyHP newTarget)
    {
        if (newTarget == null)
            return;

        // =====================================
        // 今までの敵のイベントを解除
        // =====================================

        if (currentEnemyHP != null)
        {
            currentEnemyHP.OnDeath -=
                OnCurrentTargetDeath;
        }

        // =====================================
        // 新しい敵をロック
        // =====================================

        currentEnemyHP =
            newTarget;

        CurrentTarget =
            newTarget.transform;

        // =====================================
        // 新しい敵の死亡イベント登録
        // =====================================

        currentEnemyHP.OnDeath +=
            OnCurrentTargetDeath;

        Debug.Log(
            "ロックオン変更: " +
            newTarget.name
        );
    }


    void FindNearestEnemy()
    {
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


            float targetRange = range;

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

                // ★一番近い敵をロック
                CurrentTarget =
                    enemy.transform;

                currentEnemyHP =
                    enemy;

                // ★敵が死んだ時のイベントを登録
                currentEnemyHP.OnDeath +=
                    OnCurrentTargetDeath;
            }
        }
    }


    // =====================================
    // ロック中の敵が死んだ時
    // =====================================

    void OnCurrentTargetDeath()
    {
        ClearTarget();
    }


    void ClearTarget()
    {
        // イベント解除
        if (currentEnemyHP != null)
        {
            currentEnemyHP.OnDeath -=
                OnCurrentTargetDeath;
        }

        CurrentTarget = null;
        currentEnemyHP = null;

        if (targetImage != null)
        {
            targetImage.SetActive(false);
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


    void OnDestroy()
    {
        // 念のためイベント解除
        if (currentEnemyHP != null)
        {
            currentEnemyHP.OnDeath -=
                OnCurrentTargetDeath;
        }
    }

}