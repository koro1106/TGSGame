using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private RectTransform fadeImage;

    [SerializeField] private GameObject loadingEnemy;

    // 右端の位置
    [SerializeField] private Vector2 endPos = Vector2.zero;

    // 画面外(左)
    [SerializeField] private Vector2 startPos = new Vector2(1920, 0);

    [SerializeField] private float slideTime = 0.5f;

    [SerializeField] private float waitTime = 0.5f;


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

        SceneManager.LoadScene(sceneName);
    }
}