using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    public GameObject[] panels;
    public GameObject startPanel; // ← 最初に出すUI
    public float animTime = 0.2f;

    private GameObject currentPanel;
    private Coroutine currentAnim;
    private bool isOpen = false;

    private bool playOpenAnim = false;

    public static bool IsPaused;

    void Start()
    {
        foreach (var panel in panels)
        {
            panel.SetActive(false);
            panel.transform.localScale = Vector3.zero;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isOpen)
                CloseAll();
            else
                OpenMenu();
        }
    }

    // ESCで開く
    public void OpenMenu()
    {
        Time.timeScale = 0f;
        isOpen = true;
        IsPaused = true;

        playOpenAnim = true;
        ShowPanel(startPanel);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

    }

    public void CloseAll()
    {
        if (currentAnim != null) StopCoroutine(currentAnim);

        foreach (var panel in panels)
        {
            panel.SetActive(false);
            panel.transform.localScale = Vector3.zero;
        }

        currentPanel = null;

        Time.timeScale = 1f;
        isOpen = false;
        IsPaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // ボタン用（今まで通り）
    public void ShowPanel(GameObject panel)
    {
        if (currentAnim != null) StopCoroutine(currentAnim);

        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }

        currentPanel = panel;
        currentPanel.SetActive(true);

        // ESCで開いた直後のstartPanelだけアニメ
        if (panel == startPanel && playOpenAnim)
        {
            currentPanel.transform.localScale = Vector3.zero;
            currentAnim = StartCoroutine(ScaleAnim(currentPanel, Vector3.zero, Vector3.one));

            playOpenAnim = false; //一回だけにする
        }
        else
        {
            currentPanel.transform.localScale = Vector3.one;
        }
    }

    IEnumerator ScaleAnim(GameObject panel, Vector3 start, Vector3 end)
    {
        float time = 0f;

        while (time < animTime)
        {
            time += Time.unscaledDeltaTime;
            float t = time / animTime;
            t = 1f - Mathf.Pow(1f - t, 3f);

            panel.transform.localScale = Vector3.Lerp(start, end, t);

            yield return null;
        }

        panel.transform.localScale = end;
        currentAnim = null;
    }

    public void BackToMenu()
    {
        ShowPanel(startPanel);
    }

}