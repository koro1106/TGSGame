using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TargetRange : MonoBehaviour
{
    [Header("ターゲット範囲")]
    public float range = 4000f;


    // =========================================================
    // 通常ロック
    // =========================================================

    [Header("========== 通常ロック ==========")]

    [Header("ロック演出の親")]
    public GameObject targetImage;

    [Header("中央の四角いロックImage")]
    public Image lockSquareImage;

    [Header("中央の四角いSprite")]
    public Sprite lockSquareSprite;

    [Header("右上コーナーImage")]
    public Image lockTopRightImage;

    [Header("左下コーナーImage")]
    public Image lockBottomLeftImage;

    [Header("コーナーに使用するSprite")]
    public Sprite lockCornerSprite;


    // =========================================================
    // クロスヘア
    // =========================================================

    [Header("========== クロスヘア ==========")]

    [Header("クロスヘア")]
    public RectTransform crosshair;

    public PlayerStats playerStats;


    // =========================================================
    // ロック設定
    // =========================================================

    [Header("========== ロック設定 ==========")]

    [Header("ロック変更時間")]
    public float changeTargetTime = 0.5f;

    [Header("範囲内クロスヘア判定")]
    public float crosshairRange = 130f;


    // =========================================================
    // コーナー演出
    // =========================================================

    [Header("========== コーナー演出 ==========")]

    [Header("コーナー開始距離")]
    public float cornerStartDistance = 150f;

    [Header("コーナー移動時間")]
    public float cornerMoveDuration = 0.12f;

    [Header("中央画像の角からの内側距離")]
    public float cornerOffset = 1f;

    [Header("ロック後の揺れ幅")]
    public float cornerShakeAmount = 3f;

    [Header("ロック後の揺れ速度")]
    public float cornerShakeSpeed = 20f;

    [Header("コーナーを中央へ強制的に近づける距離")]
    public float cornerCloseDistance = 50f;





    // =========================================================
    // 範囲外ロック
    // =========================================================

    [Header("========== 範囲外ロック ==========")]

    [Header("範囲外ロックの親")]
    public GameObject outOfRangeTargetImage;

    [Header("範囲外 中央ロックImage")]
    public Image outRangeLockSquareImage;

    [Header("範囲外 中央ロックSprite")]
    public Sprite outRangeLockSquareSprite;

    [Header("範囲外 右上コーナーImage")]
    public Image outRangeLockTopRightImage;

    [Header("範囲外 左下コーナーImage")]
    public Image outRangeLockBottomLeftImage;

    [Header("範囲外 右上コーナーSprite")]
    public Sprite outRangeLockTopRightSprite;

    [Header("範囲外 左下コーナーSprite")]
    public Sprite outRangeLockBottomLeftSprite;

    [Header("範囲外ロック時間")]
    public float outRangeChangeTargetTime = 0.5f;

    [Header("範囲外クロスヘア判定")]
    public float outOfRangeCrosshairRange = 30f;

    // =====================================================
    // 範囲外ロックの揺れ
    // =====================================================

    [Header("========== 範囲外ロック揺れ ==========")]

    [Header("範囲外ロック後の揺れ幅")]
    public float outRangeShakeAmount = 5f;

    [Header("範囲外ロック後の揺れ速度")]
    public float outRangeShakeSpeed = 35f;


    // =========================================================
    // ターゲット
    // =========================================================

    private EnemyHP aimingEnemy;

    private float aimingTimer = 0f;

    public Transform CurrentTarget { get; private set; }

    private EnemyHP currentEnemyHP;

    private bool wasTargetInRange = false;

    private bool isOutOfRangeTarget = false;


    // =========================================================
    // ロック演出
    // =========================================================

    private Coroutine lockAnimationCoroutine;

    // 通常ロック用
    private Vector2 topRightBasePosition;
    private Vector2 bottomLeftBasePosition;

    // 範囲外ロック用
    private Vector2 outRangeTopRightBasePosition;
    private Vector2 outRangeBottomLeftBasePosition;

    private bool isLockAnimationFinished = false;


    // =========================================================
    // Start
    // =========================================================

    void Start()
    {
        // =====================================================
        // 通常ロック Sprite設定
        // =====================================================

        if (lockSquareImage != null &&
            lockSquareSprite != null)
        {
            lockSquareImage.sprite =
                lockSquareSprite;
        }


        if (lockTopRightImage != null &&
            lockCornerSprite != null)
        {
            lockTopRightImage.sprite =
                lockCornerSprite;
        }


        if (lockBottomLeftImage != null &&
            lockCornerSprite != null)
        {
            lockBottomLeftImage.sprite =
                lockCornerSprite;

            // 左下用に反転
            lockBottomLeftImage.rectTransform.localScale =
                new Vector3(
                    -1f,
                    -1f,
                    1f
                );

            // 左に90度回転
            lockBottomLeftImage.rectTransform.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    90f
                );
        }


        if (lockTopRightImage != null)
        {
            lockTopRightImage.rectTransform.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    90f
                );
        }


        // =====================================================
        // 範囲外ロック Sprite設定
        // =====================================================

        if (outRangeLockSquareImage != null &&
            outRangeLockSquareSprite != null)
        {
            outRangeLockSquareImage.sprite =
                outRangeLockSquareSprite;
        }


        // =====================================================
        // 範囲外 右上
        // =====================================================

        if (outRangeLockTopRightImage != null &&
            outRangeLockTopRightSprite != null)
        {
            outRangeLockTopRightImage.sprite =
                outRangeLockTopRightSprite;

            // 右上：＋90°
            outRangeLockTopRightImage.rectTransform.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    0f
                );
        }


        // =====================================================
        // 範囲外 左下
        // =====================================================

        if (outRangeLockBottomLeftImage != null &&
            outRangeLockBottomLeftSprite != null)
        {
            outRangeLockBottomLeftImage.sprite =
                outRangeLockBottomLeftSprite;

            // 左下：－90°
            outRangeLockBottomLeftImage.rectTransform.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    0f
                );
        }


        // -----------------------------------------------------
        // 範囲外 左下
        // -----------------------------------------------------

        if (outRangeLockBottomLeftImage != null &&
            outRangeLockBottomLeftSprite != null)
        {
            outRangeLockBottomLeftImage.sprite =
                outRangeLockBottomLeftSprite;

            // 通常ロックと同じ反転
            outRangeLockBottomLeftImage.rectTransform.localScale =
                new Vector3(
                    -1f,
                    -1f,
                    1f
                );

            // 通常ロックと同じ回転
            outRangeLockBottomLeftImage.rectTransform.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    180f
                );
        }


        // =====================================================
        // 最初は全部非表示
        // =====================================================

        HideNormalLockUI();

        HideOutRangeLockUI();
    }


    // =========================================================
    // Update
    // =========================================================

    void Update()
    {
        // =====================================================
        // まだターゲットがいない
        // =====================================================

        if (CurrentTarget == null)
        {
            FindNearestEnemy();
        }


        // =====================================================
        // クロスヘアで敵を狙っているか
        // =====================================================

        CheckCrosshairTarget();


        // =====================================================
        // ロック中の敵が死亡中
        // =====================================================

        if (CurrentTarget != null)
        {
            EnemyHP hp =
                CurrentTarget.GetComponent<EnemyHP>();

            if (hp != null &&
                hp.IsDying())
            {
                ClearTarget();
            }
        }


        // =====================================================
        // ロック中の敵が消えた
        // =====================================================

        if (currentEnemyHP == null &&
            CurrentTarget != null)
        {
            ClearTarget();
        }


        // =====================================================
        // ロックUI更新
        // =====================================================

        UpdateTargetImage();
    }


    // =========================================================
    // クロスヘアで敵を狙っているか
    // =========================================================

    void CheckCrosshairTarget()
    {
        if (crosshair == null)
            return;


        EnemyHP[] enemies =
            FindObjectsOfType<EnemyHP>();


        EnemyHP closestToCrosshair = null;

        float closestDistance =
            Mathf.Infinity;


        // =====================================================
        // プレイヤーからのターゲット範囲
        // =====================================================

        float targetRange =
            range;

        if (playerStats != null)
        {
            targetRange +=
                playerStats.targetingRangeUP;
        }


        // =====================================================
        // 敵をチェック
        // =====================================================

        foreach (EnemyHP enemy in enemies)
        {
            if (enemy == null)
                continue;


            // 死亡中は対象外
            if (enemy.IsDying())
                continue;


            // 現在ロック中の敵は無視
            if (enemy.transform == CurrentTarget)
                continue;


            // =================================================
            // プレイヤーから敵までの距離
            // =================================================

            float enemyDistanceFromPlayer =
                Vector2.Distance(
                    transform.position,
                    enemy.transform.position
                );


            bool isInRange =
                enemyDistanceFromPlayer <=
                targetRange;


            // =================================================
            // クロスヘアから敵までの距離
            // =================================================

            float distanceFromCrosshair =
                Vector2.Distance(
                    crosshair.position,
                    enemy.transform.position
                );


            // =================================================
            // 範囲内
            // =================================================

            if (isInRange)
            {
                // 通常は130以内
                if (distanceFromCrosshair >
                    crosshairRange)
                {
                    continue;
                }
            }


            // =================================================
            // 範囲外
            // =================================================

            else
            {
                // 範囲外はかなり精度を下げる
                if (distanceFromCrosshair >
                    outOfRangeCrosshairRange)
                {
                    continue;
                }
            }


            // =================================================
            // 一番クロスヘアに近い敵
            // =================================================

            if (distanceFromCrosshair <
                closestDistance)
            {
                closestDistance =
                    distanceFromCrosshair;

                closestToCrosshair =
                    enemy;
            }
        }


        // =====================================================
        // 敵にカーソルが合っていない
        // =====================================================

        if (closestToCrosshair == null)
        {
            aimingEnemy = null;

            aimingTimer = 0f;

            return;
        }


        // =====================================================
        // 同じ敵を狙っている
        // =====================================================

        if (aimingEnemy ==
            closestToCrosshair)
        {
            aimingTimer +=
                Time.deltaTime;
        }
        else
        {
            aimingEnemy =
                closestToCrosshair;

            aimingTimer = 0f;
        }


        // =====================================================
        // ロック対象の範囲判定
        // =====================================================

        float aimingEnemyDistance =
            Vector2.Distance(
                transform.position,
                aimingEnemy.transform.position
            );


        bool aimingEnemyIsOutOfRange =
            aimingEnemyDistance >
            targetRange;


        // =====================================================
        // ロックに必要な時間
        // =====================================================

        float requiredLockTime =
            aimingEnemyIsOutOfRange
                ? outRangeChangeTargetTime
                : changeTargetTime;


        // =====================================================
        // ロック完了
        // =====================================================

        if (aimingTimer >=
            requiredLockTime)
        {
            ChangeTarget(
                aimingEnemy
            );

            aimingEnemy = null;

            aimingTimer = 0f;
        }
    }


    // =========================================================
    // ターゲット変更
    // =========================================================

    void ChangeTarget(EnemyHP newTarget)
    {
        if (newTarget == null)
            return;


        // =====================================================
        // 古いイベント解除
        // =====================================================

        if (currentEnemyHP != null)
        {
            currentEnemyHP.OnDeath -=
                OnCurrentTargetDeath;
        }


        currentEnemyHP =
            newTarget;

        CurrentTarget =
            newTarget.transform;


        // =====================================================
        // 範囲判定
        // =====================================================

        float targetRange =
            range;

        if (playerStats != null)
        {
            targetRange +=
                playerStats.targetingRangeUP;
        }


        float distance =
            Vector2.Distance(
                transform.position,
                newTarget.transform.position
            );


        isOutOfRangeTarget =
            distance > targetRange;


        // =====================================================
        // 範囲状態を記録
        // =====================================================

        wasTargetInRange =
            !isOutOfRangeTarget;


        // =====================================================
        // 死亡イベント登録
        // =====================================================

        currentEnemyHP.OnDeath +=
            OnCurrentTargetDeath;


        // =====================================================
        // ロック演出開始
        // =====================================================

        PlayLockAnimation();
    }


    // =========================================================
    // ロック演出開始
    // =========================================================

    void PlayLockAnimation()
    {
        if (lockAnimationCoroutine != null)
        {
            StopCoroutine(
                lockAnimationCoroutine
            );
        }


        if (isOutOfRangeTarget)
        {
            lockAnimationCoroutine =
                StartCoroutine(
                    OutRangeLockAnimationRoutine()
                );
        }
        else
        {
            lockAnimationCoroutine =
                StartCoroutine(
                    LockAnimationRoutine()
                );
        }
    }


    // =========================================================
    // 一番近い敵を取得
    // =========================================================

    void FindNearestEnemy()
    {
        float nearestDistance =
            Mathf.Infinity;


        EnemyHP[] enemies =
            FindObjectsOfType<EnemyHP>();


        float targetRange =
            range;

        if (playerStats != null)
        {
            targetRange +=
                playerStats.targetingRangeUP;
        }


        foreach (EnemyHP enemy in enemies)
        {
            if (enemy == null)
                continue;


            if (enemy.IsDying())
                continue;


            float distance =
                Vector2.Distance(
                    transform.position,
                    enemy.transform.position
                );


            if (distance >
                targetRange)
            {
                continue;
            }


            if (distance <
                nearestDistance)
            {
                nearestDistance =
                    distance;


                CurrentTarget =
                    enemy.transform;


                currentEnemyHP =
                    enemy;


                isOutOfRangeTarget =
                    false;


                wasTargetInRange =
                    true;


                currentEnemyHP.OnDeath +=
                    OnCurrentTargetDeath;


                PlayLockAnimation();
            }
        }
    }


    // =========================================================
    // 敵が死亡
    // =========================================================

    void OnCurrentTargetDeath()
    {
        ClearTarget();
    }


    // =========================================================
    // ターゲット解除
    // =========================================================

    void ClearTarget()
    {
        if (currentEnemyHP != null)
        {
            currentEnemyHP.OnDeath -=
                OnCurrentTargetDeath;
        }


        CurrentTarget = null;

        currentEnemyHP = null;

        aimingEnemy = null;

        aimingTimer = 0f;

        isLockAnimationFinished = false;

        isOutOfRangeTarget = false;

        wasTargetInRange = false;


        if (lockAnimationCoroutine != null)
        {
            StopCoroutine(
                lockAnimationCoroutine
            );

            lockAnimationCoroutine =
                null;
        }


        HideNormalLockUI();

        HideOutRangeLockUI();
    }


    // =========================================================
    // 通常ロックUIを非表示
    // =========================================================

    void HideNormalLockUI()
    {
        if (targetImage != null)
        {
            targetImage.SetActive(false);
        }


        if (lockSquareImage != null)
        {
            lockSquareImage.gameObject.SetActive(false);
        }


        if (lockTopRightImage != null)
        {
            lockTopRightImage.gameObject.SetActive(false);
        }


        if (lockBottomLeftImage != null)
        {
            lockBottomLeftImage.gameObject.SetActive(false);
        }
    }


    // =========================================================
    // 範囲外ロックUIを非表示
    // =========================================================

    void HideOutRangeLockUI()
    {
        if (outOfRangeTargetImage != null)
        {
            outOfRangeTargetImage.SetActive(false);
        }


        if (outRangeLockSquareImage != null)
        {
            outRangeLockSquareImage.gameObject.SetActive(false);
        }


        if (outRangeLockTopRightImage != null)
        {
            outRangeLockTopRightImage.gameObject.SetActive(false);
        }


        if (outRangeLockBottomLeftImage != null)
        {
            outRangeLockBottomLeftImage.gameObject.SetActive(false);
        }
    }


    // =========================================================
    // ロックUI更新
    // =========================================================

    void UpdateTargetImage()
    {
        if (CurrentTarget == null)
        {
            HideNormalLockUI();

            HideOutRangeLockUI();

            return;
        }


        // =====================================================
        // ターゲット範囲
        // =====================================================

        float targetRange =
            range;

        if (playerStats != null)
        {
            targetRange +=
                playerStats.targetingRangeUP;
        }


        // =====================================================
        // 敵との距離
        // =====================================================

        float enemyDistance =
            Vector2.Distance(
                transform.position,
                CurrentTarget.position
            );


        bool isInRange =
            enemyDistance <= targetRange;


        // =====================================================
        // 範囲外 → 範囲内
        // =====================================================

        if (isInRange &&
            !wasTargetInRange)
        {
            isOutOfRangeTarget = false;

            wasTargetInRange = true;

            HideOutRangeLockUI();

            PlayLockAnimation();
        }


        // =====================================================
        // 範囲内 → 範囲外
        // =====================================================

        if (!isInRange &&
            wasTargetInRange)
        {
            isOutOfRangeTarget = true;

            wasTargetInRange = false;

            HideNormalLockUI();

            PlayLockAnimation();
        }


        // =====================================================
        // 範囲内
        // =====================================================

        if (isInRange)
        {
            UpdateNormalLockUI();

            return;
        }


        // =====================================================
        // 範囲外
        // =====================================================

        UpdateOutRangeLockUI();
    }


    // =========================================================
    // 通常ロックUI更新
    // =========================================================

    void UpdateNormalLockUI()
    {
        HideOutRangeLockUI();


        if (targetImage == null)
            return;


        targetImage.SetActive(true);


        targetImage.transform.position =
            CurrentTarget.position;


        // 演出中
        if (!isLockAnimationFinished)
            return;


        if (lockTopRightImage == null ||
            lockBottomLeftImage == null)
        {
            return;
        }


        // =====================================================
        // 揺れ
        // =====================================================

        float time =
            Time.time *
            cornerShakeSpeed;


        float rightX =
            Mathf.Sin(time) *
            cornerShakeAmount;


        float rightY =
            Mathf.Cos(
                time * 1.3f
            ) *
            cornerShakeAmount;


        float leftX =
            Mathf.Sin(
                time + Mathf.PI
            ) *
            cornerShakeAmount;


        float leftY =
            Mathf.Cos(
                time * 1.3f +
                Mathf.PI
            ) *
            cornerShakeAmount;


        lockTopRightImage
            .rectTransform
            .anchoredPosition =
            topRightBasePosition +
            new Vector2(
                rightX,
                rightY
            );


        lockBottomLeftImage
            .rectTransform
            .anchoredPosition =
            bottomLeftBasePosition +
            new Vector2(
                leftX,
                leftY
            );
    }


    // =========================================================
    // 範囲外ロックUI更新
    // =========================================================

    void UpdateOutRangeLockUI()
    {
        HideNormalLockUI();


        if (outOfRangeTargetImage != null)
        {
            outOfRangeTargetImage.SetActive(true);

            outOfRangeTargetImage.transform.position =
                CurrentTarget.position;
        }


        // 演出中
        if (!isLockAnimationFinished)
            return;


        if (outRangeLockTopRightImage == null ||
            outRangeLockBottomLeftImage == null)
        {
            return;
        }


        // =====================================================
        // 範囲外コーナーの揺れ
        // =====================================================

        float time =
            Time.time *
            outRangeShakeSpeed;


        float rightX =
            Mathf.Sin(time) *
            outRangeShakeAmount;

        float rightY =
            Mathf.Sin(time * 1.7f) *
            outRangeShakeAmount;


        float leftX =
            Mathf.Sin(
                time + Mathf.PI
            ) *
            outRangeShakeAmount;

        float leftY =
            Mathf.Sin(
                time * 1.7f +
                Mathf.PI
            ) *
            outRangeShakeAmount;


        outRangeLockTopRightImage
            .rectTransform
            .anchoredPosition =
            outRangeTopRightBasePosition +
            new Vector2(
                rightX,
                rightY
            );


        outRangeLockBottomLeftImage
            .rectTransform
            .anchoredPosition =
            outRangeBottomLeftBasePosition +
            new Vector2(
                leftX,
                leftY
            );
    }


        // =========================================================
        // 通常ロック演出
        // =========================================================

        IEnumerator LockAnimationRoutine()
    {
        isLockAnimationFinished = false;


        // =====================================================
        // 通常UI表示
        // =====================================================

        HideOutRangeLockUI();


        if (targetImage != null)
        {
            targetImage.SetActive(true);
        }


        if (lockSquareImage != null)
        {
            lockSquareImage.gameObject.SetActive(true);
        }


        if (lockTopRightImage != null)
        {
            lockTopRightImage.gameObject.SetActive(true);
        }


        if (lockBottomLeftImage != null)
        {
            lockBottomLeftImage.gameObject.SetActive(true);
        }


        if (lockSquareImage == null ||
            lockTopRightImage == null ||
            lockBottomLeftImage == null)
        {
            isLockAnimationFinished = true;

            yield break;
        }


        // =====================================================
        // RectTransform
        // =====================================================

        RectTransform squareRect =
            lockSquareImage.rectTransform;

        RectTransform rightRect =
            lockTopRightImage.rectTransform;

        RectTransform leftRect =
            lockBottomLeftImage.rectTransform;


        RectTransform parentRect =
            rightRect.parent as RectTransform;


        if (parentRect == null)
        {
            yield break;
        }


        // =====================================================
        // 中央四角
        // =====================================================

        Vector3[] squareCorners =
            new Vector3[4];


        squareRect.GetWorldCorners(
            squareCorners
        );


        Vector3 rightWorldPosition =
            squareCorners[2];


        Vector3 leftWorldPosition =
            squareCorners[0];


        // =====================================================
        // World → Local
        // =====================================================

        topRightBasePosition =
            parentRect.InverseTransformPoint(
                rightWorldPosition
            );


        bottomLeftBasePosition =
            parentRect.InverseTransformPoint(
                leftWorldPosition
            );


        // =====================================================
        // 中央方向へ近づける
        // =====================================================

        topRightBasePosition +=
            new Vector2(
                -cornerCloseDistance,
                -cornerCloseDistance
            );


        bottomLeftBasePosition +=
            new Vector2(
                cornerCloseDistance,
                cornerCloseDistance
            );


        // =====================================================
        // 少し内側
        // =====================================================

        topRightBasePosition +=
            new Vector2(
                cornerOffset,
                cornerOffset
            );


        bottomLeftBasePosition -=
            new Vector2(
                cornerOffset,
                cornerOffset
            );


        // =====================================================
        // 外側の開始位置
        // =====================================================

        Vector2 rightStart =
            topRightBasePosition +
            new Vector2(
                cornerStartDistance,
                cornerStartDistance
            );


        Vector2 leftStart =
            bottomLeftBasePosition -
            new Vector2(
                cornerStartDistance,
                cornerStartDistance
            );


        rightRect.anchoredPosition =
            rightStart;


        leftRect.anchoredPosition =
            leftStart;


        // =====================================================
        // コーナー移動
        // =====================================================

        float timer = 0f;


        while (timer <
               cornerMoveDuration)
        {
            timer +=
                Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    timer /
                    cornerMoveDuration
                );


            t =
                1f -
                Mathf.Pow(
                    1f - t,
                    4f
                );


            rightRect.anchoredPosition =
                Vector2.Lerp(
                    rightStart,
                    topRightBasePosition,
                    t
                );


            leftRect.anchoredPosition =
                Vector2.Lerp(
                    leftStart,
                    bottomLeftBasePosition,
                    t
                );


            yield return null;
        }


        // =====================================================
        // 最終位置
        // =====================================================

        rightRect.anchoredPosition =
            topRightBasePosition;


        leftRect.anchoredPosition =
            bottomLeftBasePosition;


        isLockAnimationFinished = true;
    }


    // =========================================================
    // 範囲外ロック演出
    // =========================================================

    IEnumerator OutRangeLockAnimationRoutine()
    {
        isLockAnimationFinished = false;


        // =====================================================
        // 通常UI OFF
        // =====================================================

        HideNormalLockUI();


        // =====================================================
        // 範囲外UI ON
        // =====================================================

        if (outOfRangeTargetImage != null)
        {
            outOfRangeTargetImage.SetActive(true);
        }


        if (outRangeLockSquareImage != null)
        {
            outRangeLockSquareImage.gameObject.SetActive(true);
        }


        if (outRangeLockTopRightImage != null)
        {
            outRangeLockTopRightImage.gameObject.SetActive(true);
        }


        if (outRangeLockBottomLeftImage != null)
        {
            outRangeLockBottomLeftImage.gameObject.SetActive(true);
        }


        if (outRangeLockSquareImage == null ||
            outRangeLockTopRightImage == null ||
            outRangeLockBottomLeftImage == null)
        {
            isLockAnimationFinished = true;

            yield break;
        }


        // =====================================================
        // RectTransform
        // =====================================================

        RectTransform squareRect =
            outRangeLockSquareImage.rectTransform;

        RectTransform rightRect =
            outRangeLockTopRightImage.rectTransform;

        RectTransform leftRect =
            outRangeLockBottomLeftImage.rectTransform;


        RectTransform parentRect =
            rightRect.parent as RectTransform;


        if (parentRect == null)
        {
            yield break;
        }


        // =====================================================
        // 中央四角
        // =====================================================

        Vector3[] squareCorners =
            new Vector3[4];


        squareRect.GetWorldCorners(
            squareCorners
        );


        Vector3 rightWorldPosition =
            squareCorners[2];


        Vector3 leftWorldPosition =
            squareCorners[0];


        // =====================================================
        // World → Local
        // =====================================================

        outRangeTopRightBasePosition =
            parentRect.InverseTransformPoint(
                rightWorldPosition
            );


        outRangeBottomLeftBasePosition =
            parentRect.InverseTransformPoint(
                leftWorldPosition
            );


        // =====================================================
        // 中央方向へ近づける
        // =====================================================

        outRangeTopRightBasePosition +=
            new Vector2(
                -cornerCloseDistance,
                -cornerCloseDistance
            );


        outRangeBottomLeftBasePosition +=
            new Vector2(
                cornerCloseDistance,
                cornerCloseDistance
            );


        // =====================================================
        // 少し内側
        // =====================================================

        outRangeTopRightBasePosition +=
            new Vector2(
                cornerOffset,
                cornerOffset
            );


        outRangeBottomLeftBasePosition -=
            new Vector2(
                cornerOffset,
                cornerOffset
            );


        // =====================================================
        // 外側の開始位置
        // =====================================================

        Vector2 rightStart =
            outRangeTopRightBasePosition +
            new Vector2(
                cornerStartDistance,
                cornerStartDistance
            );


        Vector2 leftStart =
            outRangeBottomLeftBasePosition -
            new Vector2(
                cornerStartDistance,
                cornerStartDistance
            );


        rightRect.anchoredPosition =
            rightStart;


        leftRect.anchoredPosition =
            leftStart;


        // =====================================================
        // コーナー移動
        // =====================================================

        float timer = 0f;


        while (timer <
               cornerMoveDuration)
        {
            timer +=
                Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    timer /
                    cornerMoveDuration
                );


            t =
                1f -
                Mathf.Pow(
                    1f - t,
                    4f
                );


            rightRect.anchoredPosition =
                Vector2.Lerp(
                    rightStart,
                    outRangeTopRightBasePosition,
                    t
                );


            leftRect.anchoredPosition =
                Vector2.Lerp(
                    leftStart,
                    outRangeBottomLeftBasePosition,
                    t
                );


            yield return null;
        }


        // =====================================================
        // 最終位置
        // =====================================================

        rightRect.anchoredPosition =
            outRangeTopRightBasePosition;


        leftRect.anchoredPosition =
            outRangeBottomLeftBasePosition;


        isLockAnimationFinished = true;
    }


    // =========================================================
    // Destroy
    // =========================================================

    void OnDestroy()
    {
        if (currentEnemyHP != null)
        {
            currentEnemyHP.OnDeath -=
                OnCurrentTargetDeath;
        }
    }
}