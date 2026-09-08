using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private RectTransform fadeImage;

    [SerializeField] private GameObject loadingEnemy;

    // 左端の位置
    [SerializeField] private Vector2 endPos = Vector2.zero;

    // 画面外(右)
    [SerializeField] private Vector2 startPos = new Vector2(1920, 0);

    [SerializeField] private float slideTime = 0.5f;

    [SerializeField] private float waitTime = 0.5f;

    // OptionMenu
    [SerializeField] private OptionMenu optionMenu;

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


        StartCoroutine(Transition("MainStageScene"));
    }

    // オプション
    public void OpenOption()
    {
        // メニューの矢印を消す
        MenuButton[] buttons = FindObjectsByType<MenuButton>(
            FindObjectsSortMode.None
        );

        foreach (MenuButton button in buttons)
        {
            button.HideArrow();
        }

        StartCoroutine(Transition("OptionScene"));
    }

    // オプション用カーテン
    private IEnumerator OptionTransition()
    {
        // カーテンを右側へ
        fadeImage.anchoredPosition = startPos;

        float time = 0f;

        // 右 → 中央
        while (time < slideTime)
        {
            time += Time.deltaTime;

            float t = time / slideTime;

            fadeImage.anchoredPosition =
                Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        fadeImage.anchoredPosition = endPos;

        // カーテンが画面を覆っている間にオプションを開く
        if (optionMenu != null)
        {
            optionMenu.OpenOption();
        }

        // 少し待つ
        yield return new WaitForSeconds(waitTime);

        // カーテンを左側へ移動
        Vector2 leftPos = new Vector2(-1920, 0);

        time = 0f;

        while (time < slideTime)
        {
            time += Time.deltaTime;

            float t = time / slideTime;

            fadeImage.anchoredPosition =
                Vector2.Lerp(endPos, leftPos, t);

            yield return null;
        }

        fadeImage.anchoredPosition = startPos;
    }

    private IEnumerator Transition(string sceneName)
    {
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

        yield return new WaitForSeconds(waitTime);

        SceneManager.LoadScene(sceneName);
    }
}