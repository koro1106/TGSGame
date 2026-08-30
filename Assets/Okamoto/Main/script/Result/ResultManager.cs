using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    public static ResultManager Instance;

    [Header("リザルトUI")]
    public GameObject resultPanel;

    [Header("取得アイテム表示")]
    public Transform itemContent;
    public GameObject resultItemPrefab;

    [Header("現在の所持アイテム")]
    public PlayerData playerData;

    // Exp1
    public Image exp1Image;
    public TMP_Text exp1Text;

    // Exp2
    public Image exp2Image;
    public TMP_Text exp2Text;

    // Exp3
    public Image exp3Image;
    public TMP_Text exp3Text;

    // PreExp
    public Image preExpImage;
    public TMP_Text preExpText;

    [Header("ボタン")]
    public Button continueButton;
    public Button skillTreeButton;

    [Header("弾")]
    public GunController gunController;

    [Header("敵Spawner")]
    public EnemySpawner enemySpawner;

    [Header("クロスヘア")]
    public GameObject crosshairObject;

    [Header("停止するもの")]
    public PlayerMovement playerMovement;
    public Behaviour crosshairController;

    [Header("リザルト中に停止するPlayer")]
    public GameObject playerObject;

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

    public static bool IsResultActive = false;



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
        // Playerの操作・向きを停止
        // =====================================================

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (gunController != null)
        {
            gunController.enabled = false;
        }

        // =================================================
        // 敵の強さ・スポーン時間をリセット
        // =================================================

        if (enemySpawner != null)
        {
            enemySpawner.ResetEnemyGrowth();
        }

        // =====================================================
        // Playerの向き変更も停止
        // =====================================================

        if (gunController != null)
        {
            gunController.enabled = false;
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

        // =====================================================
        // 現在の所持アイテム数を表示
        // =====================================================

        RefreshOwnedItemUI();
    }

    // =========================================================
    // 探索続行ボタン
    // =========================================================

    public void ContinueExploration()
    {
        // ===============================
        // フィールドに残っている
        // ドロップアイテムを全削除
        // ===============================
        ClearAllDropItems();

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
        // 表示するアイテムだけ取得
        // =====================================================

        List<KeyValuePair<DropItemType, int>> displayItems =
            new List<KeyValuePair<DropItemType, int>>();

        foreach (KeyValuePair<DropItemType, int> item
                 in collectedItems)
        {
            // 取得数が0以下なら表示しない
            if (item.Value <= 0)
                continue;

            displayItems.Add(item);
        }


        // =====================================================
        // アイテムの数
        // =====================================================

        int itemCount = displayItems.Count;


        // =====================================================
        // アイテム同士の間隔
        // =====================================================

        float spacing = 400f;


        // =====================================================
        // アイテムを生成
        // =====================================================

        for (int i = 0; i < itemCount; i++)
        {
            KeyValuePair<DropItemType, int> item =
                displayItems[i];


            // =================================================
            // Prefab生成
            // =================================================

            GameObject obj =
                Instantiate(
                    resultItemPrefab,
                    itemContent
                );


            // =================================================
            // RectTransform取得
            // =================================================

            RectTransform rect =
                obj.GetComponent<RectTransform>();


            if (rect != null)
            {
                // =================================================
                // 中央を基準に左右へ均等配置
                //
                // 1個
                //       0
                //
                // 2個
                //    -85    +85
                //
                // 3個
                //   -170    0    +170
                //
                // 4個
                // -255   -85   +85   +255
                // =================================================

                float x =
                    (i - (itemCount - 1) / 2f)
                    * spacing;


                rect.anchoredPosition =
                    new Vector2(
                        x,
                        0f
                    );
            }


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

                    // 敵のHP増加をリセット
                    if (enemySpawner != null)
                    {
                        enemySpawner.ResetEnemyHPGrowth();
                    }


                    // Player全体を復帰
                    UnfreezePlayer();

                    // Player位置リセット
                    if (playerMovement != null)
                    {
                        playerMovement.enabled = false;

                        playerMovement.ResetPlayerPosition();

                        playerMovement.ResumeAfterResult();
                    }

                    // 弾を回復
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

            // GunController復帰
            if (gunController != null)
            {
                gunController.enabled = true;
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

    // =========================================================
    // Playerと子オブジェクトのBehaviourを停止
    // Player自体は消さない
    // =========================================================

    // =========================================================
    // Playerの位置と回転を固定する
    // Player自体は消さない
    // =========================================================

    private Vector3 playerFixedPosition;

    private Quaternion playerFixedRotation;

    private bool isPlayerFrozen = false;


    private void FreezePlayer()
    {
        if (playerObject == null)
            return;


        // 現在位置を保存
        playerFixedPosition =
            playerObject.transform.position;


        // 現在の回転を保存
        playerFixedRotation =
            playerObject.transform.rotation;


        isPlayerFrozen = true;
    }


    private void UnfreezePlayer()
    {
        isPlayerFrozen = false;
    }
    void LateUpdate()
    {
        if (!isPlayerFrozen)
            return;

        if (playerObject == null)
            return;


        // =====================================================
        // Playerの位置を固定
        // =====================================================

        playerObject.transform.position =
            playerFixedPosition;


        // =====================================================
        // Playerの向きを固定
        // =====================================================

        playerObject.transform.rotation =
            playerFixedRotation;
    }
    private void ClearAllDropItems()
    {
        DropBounce[] drops =
            FindObjectsOfType<DropBounce>();

        foreach (DropBounce drop in drops)
        {
            if (drop != null)
            {
                Destroy(drop.gameObject);
            }
        }
    }

    // =========================================================
    // 現在の所持アイテム数を表示
    // =========================================================

    void RefreshOwnedItemUI()
    {
        if (playerData == null)
            return;


        // =====================================================
        // Exp1
        // =====================================================

        if (exp1Text != null)
        {
            exp1Text.text =
                "×" + playerData.currentExp_1.ToString();
        }


        // =====================================================
        // Exp2
        // =====================================================

        if (exp2Text != null)
        {
            exp2Text.text =
                "×" + playerData.currentExp_2.ToString();
        }


        // =====================================================
        // Exp3
        // =====================================================

        if (exp3Text != null)
        {
            exp3Text.text =
                "×" + playerData.currentExp_3.ToString();
        }


        // =====================================================
        // PreExp
        // =====================================================

        if (preExpText != null)
        {
            preExpText.text =
                "×" + playerData.currentPreExp.ToString();
        }
    }
}