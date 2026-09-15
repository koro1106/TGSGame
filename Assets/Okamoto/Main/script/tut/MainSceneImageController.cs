using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MainStageScene開始時に表示する説明Image
/// ・ゲーム停止
/// ・クロスヘアのみ操作可能
/// ・左右矢印でページ切り替え
/// ・Imageをフェードして次のページへ
/// </summary>
public class MainSceneImageUI : MonoBehaviour
{
    [Header("説明UI全体")]
    [SerializeField] private GameObject imagePanel;

    [Header("説明Image")]
    [SerializeField] private Image[] images;

    [Header("左右ボタン")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("フェード時間")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("ゲーム操作")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GunController gunController;

    [Header("クロスヘア")]
    [SerializeField] private Behaviour crosshairController;
    [SerializeField] private GameObject crosshairObject;

    [Header("クロスヘアUI操作")]
    [SerializeField] private CrosshairUIController crosshairUIController;

    private int currentPage = 0;

    private static bool isShowing = false;
    private bool isChangingPage = false;

    // ImageUI表示中かどうか
    public static bool IsShowing => isShowing;

    // シーン移動時に表示状態をリセット
    public static void ResetShowingState()
    {
        isShowing = false;
    }

    // =========================================================
    // Start
    // MainStageSceneに入っても、すぐには表示しない
    // =========================================================

    private void Start()
    {
        // =====================================================
        // 最初は全ての説明Imageを非表示
        // SceneStartTimeline1のTimeline終了後に表示する
        // =====================================================

        if (imagePanel != null)
        {
            imagePanel.SetActive(false);
        }

        if (images != null)
        {
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] == null)
                    continue;

                images[i].gameObject.SetActive(false);
                SetAlpha(images[i], 0f);
            }
        }
    }

    public void ShowAfterSlide()
    {
        ShowImageUI();
    }


    // =========================================================
    // 説明UI表示
    // =========================================================

    private void ShowImageUI()
    {
        if (imagePanel == null)
            return;

        if (images == null || images.Length == 0)
            return;

        isShowing = true;
        isChangingPage = false;

        currentPage = 0;

        // =====================================================
        // クロスヘアによるUI操作を有効化
        // =====================================================

        if (crosshairUIController != null)
        {
            crosshairUIController.SetMainSceneImageMode(true);
        }

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
        // GunController停止
        //
        // 射撃はできないようにする
        // =====================================================

        if (gunController != null)
        {
            gunController.enabled = true;
        }

        // =====================================================
        // クロスヘアは動かす
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
        // 全Imageを一旦完全に非表示
        // =====================================================

        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] == null)
                continue;

            images[i].gameObject.SetActive(false);
            SetAlpha(images[i], 0f);
        }

        // =====================================================
        // 1枚目だけ表示
        // =====================================================

        currentPage = 0;

        images[0].gameObject.SetActive(true);
        SetAlpha(images[0], 1f);

        // =====================================================
        // ボタン設定
        // =====================================================

        if (leftButton != null)
        {
            leftButton.onClick.RemoveAllListeners();
            leftButton.onClick.AddListener(PreviousPage);
        }

        if (rightButton != null)
        {
            rightButton.onClick.RemoveAllListeners();
            rightButton.onClick.AddListener(NextPage);
        }

        UpdateButtons();

        // =====================================================
        // UI表示
        // =====================================================

        imagePanel.SetActive(true);
    }


    // =========================================================
    // 次のページ
    // =========================================================

    public void NextPage()
    {
        if (!isShowing)
            return;

        if (isChangingPage)
            return;

        // 最後のページなら説明終了
        if (currentPage >= images.Length - 1)
        {
            CloseImageUI();
            return;
        }

        StartCoroutine(ChangePage(currentPage + 1));
    }


    // =========================================================
    // 前のページ
    // =========================================================

    public void PreviousPage()
    {
        if (!isShowing)
            return;

        if (isChangingPage)
            return;

        // 最初のページでは戻れない
        if (currentPage <= 0)
            return;

        StartCoroutine(ChangePage(currentPage - 1));
    }


    // =========================================================
    // ページ切り替え
    // =========================================================

    private IEnumerator ChangePage(int nextPage)
    {
        isChangingPage = true;

        Image currentImage = images[currentPage];
        Image nextImage = images[nextPage];

        // =====================================================
        // 現在のImageを薄くする
        // =====================================================

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / fadeDuration
                );

            float alpha =
                Mathf.Lerp(1f, 0f, t);

            SetAlpha(currentImage, alpha);

            yield return null;
        }

        SetAlpha(currentImage, 0f);

        currentImage.gameObject.SetActive(false);

        // =====================================================
        // 次のImageを表示
        // =====================================================

        nextImage.gameObject.SetActive(true);

        SetAlpha(nextImage, 0f);

        timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / fadeDuration
                );

            float alpha =
                Mathf.Lerp(0f, 1f, t);

            SetAlpha(nextImage, alpha);

            yield return null;
        }

        SetAlpha(nextImage, 1f);

        currentPage = nextPage;

        UpdateButtons();

        isChangingPage = false;
    }


    // =========================================================
    // ボタン表示更新
    // =========================================================

    private void UpdateButtons()
    {
        if (leftButton != null)
        {
            leftButton.gameObject.SetActive(
                currentPage > 0
            );
        }

        if (rightButton != null)
        {
            rightButton.gameObject.SetActive(true);
        }
    }


    // =========================================================
    // 説明終了
    // =========================================================

    private void CloseImageUI()
    {
        isShowing = false;

        if (imagePanel != null)
        {
            imagePanel.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (gunController != null)
        {
            gunController.enabled = true;

            // ★ 追加：スライド終了後の射撃禁止を解除
            gunController.SetTimelinePlaying(false);
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
        // クロスヘアによるUI操作を解除
        // =====================================================

        if (crosshairUIController != null)
        {
            crosshairUIController.SetMainSceneImageMode(false);
        }

        Time.timeScale = 1f;
    }


    // =========================================================
    // Imageの透明度変更
    // =========================================================

    private void SetAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;

        color.a = alpha;

        image.color = color;
    }
}