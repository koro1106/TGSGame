using System.Collections.Generic;
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

    [Header("ボタン")]
    public Button continueButton;
    public Button skillTreeButton;

    [Header("弾")]
    public GunController gunController;

    [Header("クロスヘア")]
    public GameObject crosshairObject;

    [Header("停止するもの")]
    public Behaviour playerMovement;
    public Behaviour crosshairController;


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

    public void ContinueExploration()
    {
        if (!resultShowing)
            return;


        resultShowing = false;


        // =====================================================
        // リザルトを閉じる
        // =====================================================

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }


        // =====================================================
        // Player再開
        // =====================================================

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }


        // =====================================================
        // クロスヘア再開
        // =====================================================

        if (crosshairController != null)
        {
            crosshairController.enabled = true;
        }


        if (crosshairObject != null)
        {
            crosshairObject.SetActive(true);
        }


        // =====================================================
        // マウスカーソルを非表示
        // =====================================================

        Cursor.visible = false;

        Cursor.lockState =
            CursorLockMode.Confined;


        // =====================================================
        // カメラ揺れ再開
        // =====================================================

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ResumeShake();
        }


        // =====================================================
        // 弾を満タン
        // =====================================================

        if (gunController != null)
        {
            gunController.RefillAmmoAfterResult();
        }


        // =====================================================
        // アイテム取得データをリセット
        // =====================================================

        collectedItems.Clear();


        // =====================================================
        // 次回用にUIもクリア
        // =====================================================

        ClearResultUI();


        // =====================================================
        // ゲーム再開
        // =====================================================

        Time.timeScale =
            previousTimeScale;

        if (Time.timeScale <= 0f)
        {
            Time.timeScale = 1f;
        }
    }


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
}