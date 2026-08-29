using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TargetRange : MonoBehaviour
{
    [Header("ターゲット範囲")]
    public float range = 4000f;

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

    [Header("クロスヘア")]
    public RectTransform crosshair;

    public PlayerStats playerStats;

    [Header("ロック変更時間")]
    public float changeTargetTime = 2f;

    [Header("コーナー開始距離")]
    public float cornerStartDistance = 150f;

    [Header("コーナー移動時間")]
    public float cornerMoveDuration = 0.12f;

    [Header("中央画像の角からの隙間")]
    public float cornerOffset = 5f;

    [Header("ロック後の揺れ幅")]
    public float cornerShakeAmount = 3f;

    [Header("ロック後の揺れ速度")]
    public float cornerShakeSpeed = 20f;


    // =====================================
    // ターゲット
    // =====================================

    private EnemyHP aimingEnemy;

    private float aimingTimer = 0f;

    public Transform CurrentTarget { get; private set; }

    private EnemyHP currentEnemyHP;


    // =====================================
    // ロック演出
    // =====================================

    private Coroutine lockAnimationCoroutine;

    private Vector2 topRightBasePosition;
    private Vector2 bottomLeftBasePosition;

    private bool isLockAnimationFinished = false;


    // =====================================
    // Start
    // =====================================

    void Start()
    {
        // =========================
        // Sprite設定
        // =========================

        if (
            lockSquareImage != null &&
            lockSquareSprite != null
        )
        {
            lockSquareImage.sprite =
                lockSquareSprite;
        }


        if (
            lockTopRightImage != null &&
            lockCornerSprite != null
        )
        {
            lockTopRightImage.sprite =
                lockCornerSprite;
        }


        if (lockBottomLeftImage != null)
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

            if (lockTopRightImage != null)
            {
                lockTopRightImage.sprite =
                    lockCornerSprite;

                // 左に90度回転
                lockTopRightImage.rectTransform.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        90f
                    );
            }
        }


        // =========================
        // 最初は非表示
        // =========================

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


    // =====================================
    // Update
    // =====================================

    void Update()
    {
        // =====================================
        // まだターゲットがいない場合
        // =====================================

        if (CurrentTarget == null)
        {
            FindNearestEnemy();
        }


        // =====================================
        // クロスヘアで敵を狙っているか確認
        // =====================================

        CheckCrosshairTarget();


        // =====================================
        // ロック中の敵が消えた
        // =====================================

        if (
            currentEnemyHP == null &&
            CurrentTarget != null
        )
        {
            ClearTarget();
        }


        // =====================================
        // ロックUI更新
        // =====================================

        UpdateTargetImage();
    }


    // =====================================
    // クロスヘアで敵を狙っているか
    // =====================================

    void CheckCrosshairTarget()
    {
        if (crosshair == null)
            return;


        EnemyHP[] enemies =
            FindObjectsOfType<EnemyHP>();


        EnemyHP closestToCrosshair = null;

        float closestDistance =
            Mathf.Infinity;


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


            // クロスヘア判定範囲
            if (distance > 50f)
                continue;


            if (distance < closestDistance)
            {
                closestDistance = distance;

                closestToCrosshair = enemy;
            }
        }


        // =====================================
        // 敵に合っていない
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
            aimingEnemy =
                closestToCrosshair;

            aimingTimer = 0f;
        }


        // =====================================
        // 2秒狙った
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


    // =====================================
    // ターゲット変更
    // =====================================

    void ChangeTarget(EnemyHP newTarget)
    {
        if (newTarget == null)
            return;


        // 古いイベント解除
        if (currentEnemyHP != null)
        {
            currentEnemyHP.OnDeath -=
                OnCurrentTargetDeath;
        }


        currentEnemyHP =
            newTarget;

        CurrentTarget =
            newTarget.transform;


        // 死亡イベント登録
        currentEnemyHP.OnDeath +=
            OnCurrentTargetDeath;


        // =====================================
        // ★ここでロック演出開始
        // =====================================

        PlayLockAnimation();
    }


    // =====================================
    // ロック演出開始
    // =====================================

    void PlayLockAnimation()
    {
        if (lockAnimationCoroutine != null)
        {
            StopCoroutine(
                lockAnimationCoroutine
            );
        }


        lockAnimationCoroutine =
            StartCoroutine(
                LockAnimationRoutine()
            );
    }


    // =====================================
    // 一番近い敵を取得
    // =====================================

    void FindNearestEnemy()
    {
        float nearestDistance =
            Mathf.Infinity;


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
                nearestDistance =
                    distance;


                CurrentTarget =
                    enemy.transform;


                currentEnemyHP =
                    enemy;


                currentEnemyHP.OnDeath +=
                    OnCurrentTargetDeath;
            }
        }
    }


    // =====================================
    // 敵が死亡
    // =====================================

    void OnCurrentTargetDeath()
    {
        ClearTarget();
    }


    // =====================================
    // ターゲット解除
    // =====================================

    void ClearTarget()
    {
        if (currentEnemyHP != null)
        {
            currentEnemyHP.OnDeath -=
                OnCurrentTargetDeath;
        }


        CurrentTarget = null;

        currentEnemyHP = null;


        isLockAnimationFinished =
            false;


        if (lockAnimationCoroutine != null)
        {
            StopCoroutine(
                lockAnimationCoroutine
            );

            lockAnimationCoroutine = null;
        }


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


    // =====================================
    // ロックUI更新
    // =====================================

    void UpdateTargetImage()
    {
        if (targetImage == null)
            return;


        // =====================================
        // ロックしていない
        // =====================================

        if (CurrentTarget == null)
        {
            targetImage.SetActive(false);

            return;
        }


        // =====================================
        // 敵の位置へ追従
        // =====================================

        targetImage.SetActive(true);

        targetImage.transform.position =
            CurrentTarget.position;


        // =====================================
        // 演出がまだ終わっていない
        // =====================================

        if (!isLockAnimationFinished)
            return;


        if (
            lockTopRightImage == null ||
            lockBottomLeftImage == null
        )
        {
            return;
        }


        // =====================================
        // ロック後の小刻みな揺れ
        // =====================================

        float time =
            Time.time *
            cornerShakeSpeed;


        float topRightX =
            Mathf.Sin(time) *
            cornerShakeAmount;

        float topRightY =
            Mathf.Cos(time * 1.3f) *
            cornerShakeAmount;


        float bottomLeftX =
            Mathf.Sin(
                time + Mathf.PI
            ) *
            cornerShakeAmount;

        float bottomLeftY =
            Mathf.Cos(
                time * 1.3f +
                Mathf.PI
            ) *
            cornerShakeAmount;


        // =====================================
        // 揺れを反映
        // =====================================

        lockTopRightImage.rectTransform.anchoredPosition =
            topRightBasePosition +
            new Vector2(
                topRightX,
                topRightY
            );


        lockBottomLeftImage.rectTransform.anchoredPosition =
            bottomLeftBasePosition +
            new Vector2(
                bottomLeftX,
                bottomLeftY
            );
    }


    // =====================================
    // ロック演出
    // =====================================

    IEnumerator LockAnimationRoutine()
    {
        isLockAnimationFinished =
            false;


        // =====================================
        // UI表示
        // =====================================

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


        // =====================================
        // RectTransform取得
        // =====================================

        RectTransform squareRect =
            lockSquareImage.rectTransform;

        RectTransform rightRect =
            lockTopRightImage.rectTransform;

        RectTransform leftRect =
            lockBottomLeftImage.rectTransform;


        // =====================================
        // 中央四角のWorld座標を取得
        // =====================================

        Vector3[] squareCorners =
            new Vector3[4];


        squareRect.GetWorldCorners(
            squareCorners
        );


        // =====================================
        // 中央画像の右上
        // =====================================

        Vector3 rightWorldPosition =
            squareCorners[2];


        // =====================================
        // 中央画像の左下
        // =====================================

        Vector3 leftWorldPosition =
            squareCorners[0];


        // =====================================
        // コーナー画像の親
        // =====================================

        RectTransform parentRect =
            rightRect.parent as RectTransform;


        if (parentRect == null)
        {
            yield break;
        }


        // =====================================
        // World → 親のLocal座標
        // =====================================

        topRightBasePosition =
            parentRect.InverseTransformPoint(
                rightWorldPosition
            );


        bottomLeftBasePosition =
            parentRect.InverseTransformPoint(
                leftWorldPosition
            );


        // =====================================
        // 少し外側へ
        // =====================================

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


        // =====================================
        // 外側の開始位置
        // =====================================

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


        // =====================================
        // コーナーを高速移動
        // =====================================

        float timer = 0f;

        while (timer < cornerMoveDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / cornerMoveDuration
                );

            // 最初は一気に加速して
            // 最後だけ少し減速
            t = 1f - Mathf.Pow(1f - t, 4f);


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


        // =====================================
        // 最終位置
        // =====================================

        rightRect.anchoredPosition =
            topRightBasePosition;

        leftRect.anchoredPosition =
            bottomLeftBasePosition;


        // =====================================
        // 移動完了
        // 揺れ開始
        // =====================================

        isLockAnimationFinished =
            true;
    }


    // =====================================
    // Destroy
    // =====================================

    void OnDestroy()
    {
        if (currentEnemyHP != null)
        {
            currentEnemyHP.OnDeath -=
                OnCurrentTargetDeath;
        }
    }
}