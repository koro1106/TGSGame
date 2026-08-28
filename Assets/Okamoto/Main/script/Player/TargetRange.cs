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
    public float cornerMoveDuration = 0.25f;


    [Header("ロック中の小刻み移動量")]
    public float cornerShakeAmount = 5f;

    [Header("ロック中の小刻み移動速度")]
    public float cornerShakeSpeed = 8f;

    // クロスヘアを合わせている敵
    private EnemyHP aimingEnemy;

    // 合わせ続けた時間
    private float aimingTimer = 0f;

    // 現在ロックしている敵
    public Transform CurrentTarget { get; private set; }

    // ロック中のEnemyHP
    private EnemyHP currentEnemyHP;

    private Coroutine lockAnimationCoroutine;
    private Vector2 topRightBasePosition;
    private Vector2 bottomLeftBasePosition;

    private bool isLockAnimationFinished = false;


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


        if (lockTopRightImage != null)
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
        }


        // =========================
        // 最初は全体を非表示
        // =========================

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

        if (currentEnemyHP != null)
        {
            currentEnemyHP.OnDeath -=
                OnCurrentTargetDeath;
        }

        currentEnemyHP =
            newTarget;

        CurrentTarget =
            newTarget.transform;

        currentEnemyHP.OnDeath +=
            OnCurrentTargetDeath;

        // ★ロック演出開始
        PlayLockAnimation();
    }

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
        if (currentEnemyHP != null)
        {
            currentEnemyHP.OnDeath -=
                OnCurrentTargetDeath;
        }

        CurrentTarget = null;
        currentEnemyHP = null;

        isLockAnimationFinished = false;


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
    }


    void UpdateTargetImage()
    {
        if (targetImage == null)
            return;


        // =========================
        // ロックしていない
        // =========================

        if (CurrentTarget == null)
        {
            targetImage.SetActive(false);
            return;
        }


        // =========================
        // 敵の位置へ追従
        // =========================

        targetImage.SetActive(true);

        targetImage.transform.position =
            CurrentTarget.position;


        // =========================
        // ロック演出終了後
        // コーナーを小刻みに動かす
        // =========================

        if (!isLockAnimationFinished)
            return;


        if (
            lockTopRightImage == null ||
            lockBottomLeftImage == null
        )
        {
            return;
        }


        float time =
            Time.time *
            cornerShakeSpeed;


        // 右上
        float topRightX =
            Mathf.Sin(time) *
            cornerShakeAmount;

        float topRightY =
            Mathf.Cos(time * 1.3f) *
            cornerShakeAmount;


        // 左下
        float bottomLeftX =
            Mathf.Sin(
                time + Mathf.PI
            ) *
            cornerShakeAmount;

        float bottomLeftY =
            Mathf.Cos(
                time * 1.3f + Mathf.PI
            ) *
            cornerShakeAmount;


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


    void OnDestroy()
    {
        // 念のためイベント解除
        if (currentEnemyHP != null)
        {
            currentEnemyHP.OnDeath -=
                OnCurrentTargetDeath;
        }
    }
    IEnumerator LockAnimationRoutine()
    {
        isLockAnimationFinished = false;

        // =========================
        // ロックUI表示
        // =========================

        if (targetImage != null)
        {
            targetImage.SetActive(true);
        }


        // =========================
        // 中央の四角はずっと表示
        // =========================

        if (lockSquareImage != null)
        {
            lockSquareImage.gameObject.SetActive(true);
        }


        // =========================
        // コーナーを表示
        // =========================

        if (lockTopRightImage != null)
        {
            lockTopRightImage.gameObject.SetActive(true);
        }

        if (lockBottomLeftImage != null)
        {
            lockBottomLeftImage.gameObject.SetActive(true);
        }


        RectTransform topRightRect =
            lockTopRightImage.rectTransform;

        RectTransform bottomLeftRect =
            lockBottomLeftImage.rectTransform;


        // =========================
        // Inspectorで設定した位置を最終位置にする
        // =========================

        topRightBasePosition =
            topRightRect.anchoredPosition;

        bottomLeftBasePosition =
            bottomLeftRect.anchoredPosition;


        // =========================
        // 外側から開始
        // =========================

        Vector2 topRightStart =
            topRightBasePosition +
            new Vector2(
                cornerStartDistance,
                cornerStartDistance
            );

        Vector2 bottomLeftStart =
            bottomLeftBasePosition -
            new Vector2(
                cornerStartDistance,
                cornerStartDistance
            );


        topRightRect.anchoredPosition =
            topRightStart;

        bottomLeftRect.anchoredPosition =
            bottomLeftStart;


        // =========================
        // 外側から中央へ移動
        // =========================

        float timer = 0f;

        while (timer < cornerMoveDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    cornerMoveDuration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );


            topRightRect.anchoredPosition =
                Vector2.Lerp(
                    topRightStart,
                    topRightBasePosition,
                    t
                );

            bottomLeftRect.anchoredPosition =
                Vector2.Lerp(
                    bottomLeftStart,
                    bottomLeftBasePosition,
                    t
                );

            yield return null;
        }


        // 最終位置に固定

        topRightRect.anchoredPosition =
            topRightBasePosition;

        bottomLeftRect.anchoredPosition =
            bottomLeftBasePosition;


        isLockAnimationFinished = true;
    }
}