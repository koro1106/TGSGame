using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private RectTransform fadeImage;

    [SerializeField] private GameObject loadingEnemy;
    [SerializeField] private SEManager seManager;

    // 右端の位置
    [SerializeField] private Vector2 endPos = Vector2.zero;

    // 画面外(左)
    [SerializeField] private Vector2 startPos = new Vector2(1920, 0);

    [SerializeField] private float slideTime = 0.5f;

    [SerializeField] private float waitTime = 0.5f;


    public static bool ShowMainSceneImage = false;

    public void StartGame()
    {
        // メニューの矢印を消す
        MenuButton[] buttons = FindObjectsByType<MenuButton>(
            FindObjectsSortMode.None
        );

        foreach (MenuButton button in buttons)
        {
            button.HideArrow();
        }

        seManager.PlayClickSE();

        // タイトルからMainStageSceneへ移動
        ShowMainSceneImage = true;

        StartCoroutine(Transition("MainStageScene"));
    }

    private IEnumerator Transition(string sceneName)
    {
        // カーテンを最前面にする
        //fadeImage.SetAsLastSibling();

        float time = 0f;

        fadeImage.anchoredPosition = startPos;

        while (time < slideTime)
        {
            time += Time.deltaTime;

            float t = time / slideTime;

            fadeImage.anchoredPosition =
                Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        fadeImage.anchoredPosition = endPos;

        loadingEnemy.SetActive(true);

        yield return new WaitForSeconds(waitTime);

        yield return new WaitForSeconds(waitTime);

        // =====================================================
        // シーン移動前にゲーム状態をリセット
        // =====================================================

        Time.timeScale = 1f;

        PauseMenu.IsPaused = false;
        ResultManager.IsResultActive = false;

        // Physics2Dを通常状態に戻す
        Physics2D.simulationMode =
            SimulationMode2D.FixedUpdate;

        // EventSystemを通常状態に戻す
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();

        if (eventSystem != null)
        {
            eventSystem.enabled = true;
            eventSystem.SetSelectedGameObject(null);
        }

        // Playerの操作を解除
        PlayerMovement playerMovement =
            FindFirstObjectByType<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Gunの操作を解除
        GunController gunController =
            FindFirstObjectByType<GunController>();

        if (gunController != null)
        {
            gunController.enabled = true;

            // Timelineによる射撃禁止も解除
            gunController.SetTimelinePlaying(false);
        }

        // MainSceneImageUIの表示状態をリセット
        MainSceneImageUI.ResetShowingState();

        // カーソル状態をリセット
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        // =====================================================
        // シーン移動
        // =====================================================

        SceneManager.LoadScene(sceneName);
    }
}