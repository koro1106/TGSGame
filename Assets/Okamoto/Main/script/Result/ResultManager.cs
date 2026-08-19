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

        // ★ Inspectorで置いたImageの位置を最初に保存
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

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }


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

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }


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
        // 最初にPlayerを停止したままリセット
        // =====================================================

        if (playerMovement != null)
        {
            playerMovement.enabled = false;

            playerMovement.ResetPlayerPosition();

            playerMovement.ResumeAfterResult();
        }


        // =====================================================
        // 弾を満タン
        // =====================================================

        if (gunController != null)
        {
            gunController.RefillAmmoAfterResult();
        }


        // =====================================================
        // スライド開始
        // =====================================================

        if (continueTransition != null)
        {
            // スライドImageを表示
            continueTransition.gameObject.SetActive(true);


            // Inspectorで置いた元の位置
            Vector2 startPos =
                transitionOriginalPosition;


            // Xプラス方向へ移動
            Vector2 endPos =
                startPos + new Vector2(
                    25000f,
                    0f
                );


            // 必ず元の位置から開始
            continueTransition.anchoredPosition =
                startPos;


            float timer = 0f;

            bool gameResumed = false;


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
                // スライド
                // =============================================

                continueTransition.anchoredPosition =
                    Vector2.Lerp(
                        startPos,
                        endPos,
                        smoothT
                    );


                // =============================================
                // スライド終わり頃にゲーム復帰
                // =============================================

                if (!gameResumed && t >= 0.1f)
                {
                    gameResumed = true;


                    // クロスヘア復帰
                    if (crosshairController != null)
                    {
                        crosshairController.enabled = true;
                    }


                    if (crosshairObject != null)
                    {
                        crosshairObject.SetActive(true);
                    }


                    // ゲーム再開
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
                }


                yield return null;
            }


            // =====================================================
            // スライド終了位置
            // =====================================================

            continueTransition.anchoredPosition =
                endPos;


            // =====================================================
            // 少しだけ最後まで表示
            // =====================================================

            yield return new WaitForSecondsRealtime(
                0.15f
            );


            // =====================================================
            // ここでリザルトを消す
            // =====================================================

            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }


            // =====================================================
            // アイテムデータリセット
            // =====================================================

            collectedItems.Clear();

            ClearResultUI();


            // =====================================================
            // Transitionを元の位置へ戻す
            // =====================================================

            continueTransition.anchoredPosition =
                transitionOriginalPosition;


            // =====================================================
            // Transitionを非表示
            // =====================================================

            continueTransition.gameObject.SetActive(false);
        }


        // =====================================================
        // continueTransitionがない場合
        // =====================================================

        else
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }


            collectedItems.Clear();

            ClearResultUI();


            if (crosshairController != null)
            {
                crosshairController.enabled = true;
            }


            if (crosshairObject != null)
            {
                crosshairObject.SetActive(true);
            }


            Time.timeScale = 1f;


            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }


            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.ResumeShake();
            }


            Cursor.visible = false;

            Cursor.lockState =
                CursorLockMode.Confined;
        }
    }
}