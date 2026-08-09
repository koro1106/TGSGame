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

    public void StartGame()
    {

        StartCoroutine(Transition());
    }

    IEnumerator Transition()
    {
        fadeImage.anchoredPosition = startPos;

        float t = 0;

        while (t < slideTime)
        {
            t += Time.deltaTime;

            fadeImage.anchoredPosition =
                Vector2.Lerp(startPos, endPos, t / slideTime);

            yield return null;
        }

        loadingEnemy.SetActive(true);

        // 少し待つ
        yield return new WaitForSeconds(waitTime);

        SceneManager.LoadScene("MainStageScene");
    }
}