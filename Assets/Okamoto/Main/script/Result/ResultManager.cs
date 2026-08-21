using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ResultManager : MonoBehaviour
{
    public static ResultManager Instance;

    [Header("リザルトUI")]
    public GameObject resultPanel;

    [Header("取得アイテム表示")]
    public Transform itemContent;
    public GameObject resultItemPrefab;

    [Header("ボタン")]
    public Button continueButton;
    public Button skillTreeButton;

    [Header("弾")]
    public GunController gunController;

    [Header("クロスヘア")]
    public GameObject crosshairObject;

    [Header("停止するもの")]
    public PlayerMovement playerMovement;
    public Behaviour crosshairController;

    [Header("続行時スライド演出")]
    public RectTransform continueTransition;
    public float transitionDuration = 0.5f;

    [Header("アニメーションさせるPanel")]
    public RectTransform animationPanel;

    [Header("表示アニメーション")]
    public float resultStartScale = 0.1f;
    public float resultScaleDuration = 0.25f;

    private Vector3 animationPanelOriginalScale;

    private Vector3 resultOriginalScale;

    private Vector2 transitionOriginalPosition;



    // =========================================================
    // 回収したアイテム数
    // =========================================================

    private Dictionary<DropItemType, int> collectedItems =
        new Dictionary<DropItemType, int>();

    private bool resultShowing = false;

    private float previousTimeScale = 1f;




    // =========================================================
    // Awake
    // =========================================================

    void Awake()
    {
        Instance = this;

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        // アニメーションするPanelの元サイズを保存
        if (animationPanel != null)
        {
            animationPanelOriginalScale =
                animationPanel.localScale;
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(
                ContinueExploration
            );
        }

        if (skillTreeButton != null)
        {
            skillTreeButton.onClick.AddListener(
                GoToSkillTree
            );
        }

        if (continueTransition != null)
        {
            transitionOriginalPosition =
                continueTransition.anchoredPosition;
        }
    }


    // =========================================================
    // アイテム回収数を追加
    // =========================================================

    public void AddCollectedItem(
        DropItemType type,
        int amount)
    {
        if (collectedItems.ContainsKey(type))
        {
            collectedItems[type] += amount;
        }
        else
        {
            collectedItems.Add(type, amount);
        }
    }


    // =========================================================
    // リザルト表示
    // =========================================================

    public void ShowResult()
    {
        if (resultShowing)
            return;

        resultShowing = true;


        // =====================================================
        // すでに飛んでいる弾を削除
        // =====================================================

        Bullet[] bullets =
            FindObjectsOfType<Bullet>();

        foreach (Bullet bullet in bullets)
        {
            Destroy(bullet.gameObject);
        }


        // =====================================================
        // 現在いる敵を削除
        // =====================================================

        EnemyMove[] enemies =
            FindObjectsOfType<EnemyMove>();

        foreach (EnemyMove enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }


        // =====================================================
        // 現在のTimeScaleを保存
        // =====================================================

        previousTimeScale =
            Time.timeScale;


        // =====================================================
        // ゲーム停止
        // =====================================================

        Time.timeScale = 0f;


        // =====================================================
        // Player停止
        // =====================================================

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }


        // =====================================================
        // クロスヘアの移動停止
        // =====================================================

        if (crosshairController != null)
        {
            crosshairController.enabled = false;
        }


        // =====================================================
        // クロスヘア非表示
        // =====================================================

        if (crosshairObject != null)
        {
            crosshairObject.SetActive(false);
        }


        // =====================================================
        // 本物のマウスカーソルを表示
        // =====================================================

        Cursor.visible = true;

        Cursor.lockState =
            CursorLockMode.None;


        // =====================================================
        // カメラ揺れ停止
        // =====================================================

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.StopShake();
        }


        // =====================================================
        // リザルト表示
        // =====================================================

        StartCoroutine(
    ResultShowAnimation()
);


        // =====================================================
        // アイテム表示
        // =====================================================

        RefreshResultUI();
    }

    // =========================================================
    // 探索続行ボタン
    // =========================================================

    public void ContinueExploration()
    {
        StartCoroutine(
            ContinueExplorationCoroutine()
        );
    }


    // =========================================================
    // リザルトUI更新
    // =========================================================

    void RefreshResultUI()
    {
        if (itemContent == null ||
            resultItemPrefab == null)
        {
            return;
        }


        // =====================================================
        // 既存の表示を削除
        // =====================================================

        for (int i = itemContent.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                itemContent.GetChild(i).gameObject
            );
        }


        // =====================================================
        // 回収したアイテムだけ表示
        // =====================================================

        foreach (KeyValuePair<DropItemType, int> item
                 in collectedItems)
        {
            // 取得数が0以下なら表示しない
            if (item.Value <= 0)
                continue;


            // =================================================
            // ResultItemPrefabを生成
            // =================================================

            GameObject obj =
                Instantiate(
                    resultItemPrefab,
                    itemContent
                );


            // =================================================
            // ResultItemUI取得
            // =================================================

            ResultItemUI ui =
                obj.GetComponent<ResultItemUI>();


            if (ui != null)
            {
                ui.SetItem(
                    item.Key,
                    item.Value
                );
            }
        }
    }


    // =========================================================
    // 探索続行
    // =========================================================


    // =========================================================
    // スキルツリーへ
    // =========================================================

    public void GoToSkillTree()
    {
        // =====================================================
        // リザルト終了
        // =====================================================

        resultShowing = false;


        // =====================================================
        // 取得データリセット
        // =====================================================

        collectedItems.Clear();


        // =====================================================
        // UI削除
        // =====================================================

        ClearResultUI();


        // =====================================================
        // リザルト非表示
        // =====================================================

        StartCoroutine(
    ResultShowAnimation()
);


        // =====================================================
        // カメラを元に戻す
        // =====================================================

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ResumeShake();
        }


        // =====================================================
        // Player関連を復帰
        // =====================================================

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }


        if (crosshairController != null)
        {
            crosshairController.enabled = true;
        }


        if (crosshairObject != null)
        {
            crosshairObject.SetActive(true);
        }


        // =====================================================
        // マウスカーソル
        // =====================================================

        Cursor.visible = true;

        Cursor.lockState =
            CursorLockMode.None;


        // =====================================================
        // シーン移動
        // =====================================================

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            "MainStageSkillTreeScene"
        );
    }

    void ClearResultUI()
    {
        if (itemContent == null)
            return;


        for (int i = itemContent.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                itemContent.GetChild(i).gameObject
            );
        }
    }

    private IEnumerator ContinueExplorationCoroutine()
    {
        // =====================================================
        // 二重クリック防止
        // =====================================================

        resultShowing = false;


        // =====================================================
        // スライドがある場合
        // =====================================================

        if (continueTransition != null)
        {
            // =================================================
            // Inspectorで置いた開始位置
            // =================================================

            Vector2 startPos =
                transitionOriginalPosition;


            // =================================================
            // 移動先
            // =================================================

            Vector2 endPos =
                startPos + new Vector2(
                    20000f,
                    0f
                );


            // =================================================
            // 開始位置へ戻す
            // =================================================

            continueTransition.anchoredPosition =
                startPos;


            // =================================================
            // スライド表示
            // =================================================

            continueTransition.gameObject.SetActive(true);


            // 表示を反映
            yield return null;


            float timer = 0f;

            bool resultClosed = false;

            // ★ Playerリセット済みか
            bool playerRestarted = false;


            // =================================================
            // スライド処理
            // =================================================

            while (timer < transitionDuration)
            {
                timer += Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        timer / transitionDuration
                    );

                float smoothT =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        t
                    );


                // =============================================
                // スライド移動
                // =============================================

                continueTransition.anchoredPosition =
                    Vector2.Lerp(
                        startPos,
                        endPos,
                        smoothT
                    );


                // =============================================
                // リザルトを早めに消す
                // =============================================

                if (!resultClosed && t >= 0.1f)
                {
                    resultClosed = true;

                    if (resultPanel != null)
                    {
                        resultPanel.SetActive(false);
                    }

                    collectedItems.Clear();

                    ClearResultUI();
                }


                // =============================================
                // スライド終了少し前に
                // Playerをリスタート状態にする
                // =============================================

                if (!playerRestarted && t >= 0.7f)
                {
                    playerRestarted = true;

                    if (playerMovement != null)
                    {
                        playerMovement.enabled = false;

                        playerMovement.ResetPlayerPosition();

                        playerMovement.ResumeAfterResult();
                    }

                    if (gunController != null)
                    {
                        gunController.RefillAmmoAfterResult();
                    }
                }


                yield return null;
            }


            // =====================================================
            // 最終位置を確実にセット
            // =====================================================

            continueTransition.anchoredPosition =
                endPos;


            // =====================================================
            // スライド終了後、ゲーム再開
            // =====================================================

            // クロスヘア復帰
            if (crosshairController != null)
            {
                crosshairController.enabled = true;
            }


            if (crosshairObject != null)
            {
                crosshairObject.SetActive(true);
            }


            // =====================================================
            // ゲーム再開
            // =====================================================

            Time.timeScale = 1f;


            // Player操作可能
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }


            // カメラ揺れ再開
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.ResumeShake();
            }


            // マウスカーソル非表示
            Cursor.visible = false;

            Cursor.lockState =
                CursorLockMode.Confined;


            // =====================================================
            // 1フレーム待つ
            // =====================================================

            yield return null;


            // =====================================================
            // スライドを元に戻して非表示
            // =====================================================

            continueTransition.anchoredPosition =
                startPos;

            continueTransition.gameObject.SetActive(false);
        }
    }
    // =========================================================
    // 指定したPanelだけ表示アニメーション
    // 小さい → 大きい
    // =========================================================

    private IEnumerator ResultShowAnimation()
    {
        // リザルト全体がなければ終了
        if (resultPanel == null)
            yield break;


        // =====================================================
        // まずリザルト全体を表示
        // =====================================================

        resultPanel.SetActive(true);


        // =====================================================
        // アニメーションPanelが未設定なら
        // リザルトだけ表示して終了
        // =====================================================

        if (animationPanel == null)
            yield break;


        // =====================================================
        // 開始サイズ
        // =====================================================

        Vector3 startScale =
            animationPanelOriginalScale *
            resultStartScale;


        // 最初は小さくする
        animationPanel.localScale =
            startScale;


        float timer = 0f;


        // =====================================================
        // 小さい → 元のサイズ
        // =====================================================

        while (timer < resultScaleDuration)
        {
            timer += Time.unscaledDeltaTime;


            float t =
                Mathf.Clamp01(
                    timer / resultScaleDuration
                );


            // なめらかに広がる
            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );


            animationPanel.localScale =
                Vector3.Lerp(
                    startScale,
                    animationPanelOriginalScale,
                    t
                );


            yield return null;
        }


        // =====================================================
        // 最後は元のサイズに戻す
        // =====================================================

        animationPanel.localScale =
            animationPanelOriginalScale;
    }
}