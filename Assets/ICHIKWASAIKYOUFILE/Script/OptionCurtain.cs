using UnityEngine;
using System.Collections;

public class OptionCurtain : MonoBehaviour
{
    [SerializeField] private RectTransform fadeImage;

    // 画面中央
    [SerializeField] private Vector2 endPos = Vector2.zero;

    // 画面外（右）＝初期位置
    [SerializeField] private Vector2 startPos = new Vector2(-1920, 0);

    // 画面外（左）
    [SerializeField] private Vector2 leftPos = new Vector2(1920, 0);

    [SerializeField] private float slideTime = 0.5f;

    [SerializeField] private float waitTime = 0.5f;

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

        StartCoroutine(OptionTransition());
    }

    private IEnumerator OptionTransition()
    {
        // カーテンを最前面にする
        fadeImage.SetAsLastSibling();

        // 最初は右端
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

        // 中央で止める
        //fadeImage.anchoredPosition = endPos;

        // 少し待つ
       // yield return new WaitForSeconds(waitTime);

        time = 0f;

        // 中央 → 左
        while (time < slideTime)
        {
            time += Time.deltaTime;

            float t = time / slideTime;

            fadeImage.anchoredPosition =
                Vector2.Lerp(endPos, leftPos, t);

            yield return null;
        }

        // 左端まで移動
        fadeImage.anchoredPosition = leftPos;

        // 初期位置（右端）へ戻す
        fadeImage.anchoredPosition = startPos;
    }
}
