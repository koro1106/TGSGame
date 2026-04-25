using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    public GameObject settingsPanel;
    public float animTime = 0.2f;

    private bool isOpen = false;

    void Start()
    {
        settingsPanel.transform.localScale = Vector3.zero;
        settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isOpen)
                Close();
            else
                Open();
        }
    }

    public void Open()
    {
        settingsPanel.SetActive(true);
        StartCoroutine(ScaleAnim(Vector3.zero, Vector3.one));
        Time.timeScale = 0f; // ゲーム停止
        isOpen = true;
    }

    public void Close()
    {
        StartCoroutine(CloseAnim());
        Time.timeScale = 1f;
        isOpen = false;
    }

    IEnumerator CloseAnim()
    {
        yield return ScaleAnim(settingsPanel.transform.localScale, Vector3.zero);
        settingsPanel.SetActive(false);
    }

    IEnumerator ScaleAnim(Vector3 start, Vector3 end)
    {
        float time = 0f;

        while (time < animTime)
        {
            time += Time.unscaledDeltaTime;
            float t = time / animTime;

            // イージング（ちょっと気持ちよくする）
            t = 1f - Mathf.Pow(1f - t, 3f);

            settingsPanel.transform.localScale = Vector3.Lerp(start, end, t);

            yield return null;
        }

        settingsPanel.transform.localScale = end;
    }
}