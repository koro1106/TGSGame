
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class PauseMenu : MonoBehaviour
{
    public GameObject[] panels;
    public GameObject startPanel; // ← 最初に出すUI
    public float animTime = 0.2f;

    [Header("クロスヘアUI操作")]
    public CrosshairUIController crosshairUIController;

    [Header("感度設定")]
    public GunController gunController;

    // 感度を変更するSlider
    public Slider sensitivitySlider;

    // 設定画面で変更した感度を一時保存
    private float pendingSensitivity;

    // 感度を変更したか
    private bool hasPendingSensitivity = false;

    private GameObject currentPanel;
    private Coroutine currentAnim;
    private bool isOpen = false;

    private bool playOpenAnim = false;

    public static bool IsPaused;

    private EventSystem eventSystem;

    // EventSystemの元の状態
    private bool originalEventSystemEnabled = true;


    void Start()
    {
        foreach (var panel in panels)
        {
            panel.SetActive(false);
            panel.transform.localScale = Vector3.zero;
        }


        // =================================================
        // GunControllerから現在の感度を取得
        // =================================================

        if (gunController != null)
        {
            pendingSensitivity =
                gunController.sensitivity;

            hasPendingSensitivity = false;
        }


        // =================================================
        // Sensitivity Sliderの初期値
        // =================================================

        if (sensitivitySlider != null &&
            gunController != null)
        {
            sensitivitySlider.SetValueWithoutNotify(
                gunController.sensitivity
            );
        }


        // =================================================
        // CrosshairUIControllerは常に有効にしておく
        // =================================================

        if (crosshairUIController != null)
        {
            crosshairUIController.enabled = true;
            crosshairUIController.SetPauseMode(false);
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // リザルト表示中はポーズ画面を開かない
            if (ResultManager.IsResultActive)
                return;


            if (isOpen)
                CloseAll();
            else
                OpenMenu();
        }
    }


    // =====================================================
    // ESCで開く
    // =====================================================

    public void OpenMenu()
    {
        // リザルト中はポーズを開かない
        if (ResultManager.IsResultActive)
            return;


        // =========================
        // ゲーム停止
        // =========================

        Time.timeScale = 0f;

        // 2D物理も完全停止
        Physics2D.simulationMode =
            SimulationMode2D.Script;

        isOpen = true;
        IsPaused = true;

        playOpenAnim = true;

        ShowPanel(startPanel);


        // =========================
        // EventSystem取得
        // =========================

        eventSystem = EventSystem.current;


        if (eventSystem != null)
        {
            // 元の状態を保存
            originalEventSystemEnabled =
                eventSystem.enabled;

            // =========================
            // 本物のマウスによるUI操作を停止
            // =========================

            eventSystem.enabled = false;
        }


        // =========================
        // クロスヘアUI操作開始
        //
        // IMPORTANT:
        // CrosshairUIController自体は
        // enabled / disabledを切り替えない
        // =========================

        if (crosshairUIController != null)
        {
            crosshairUIController.enabled = true;
            crosshairUIController.SetPauseMode(true);
        }


        // =========================
        // 本物のマウスカーソルを表示しない
        // =========================

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;


        // =================================================
        // 現在の確定済み感度をSliderへ反映
        //
        // ここではonValueChangedを発火させない
        // =================================================

        if (sensitivitySlider != null &&
            gunController != null)
        {
            sensitivitySlider.SetValueWithoutNotify(
                gunController.sensitivity
            );
        }


        // =================================================
        // 新しくポーズを開いた時は
        // まだ確定していない変更を破棄
        // =================================================

        if (gunController != null)
        {
            pendingSensitivity =
                gunController.sensitivity;
        }

        hasPendingSensitivity = false;
    }


    // =====================================================
    // ポーズ終了
    // =====================================================

    public void CloseAll()
    {
        if (currentAnim != null)
            StopCoroutine(currentAnim);


        foreach (var panel in panels)
        {
            panel.SetActive(false);
            panel.transform.localScale = Vector3.zero;
        }

        currentPanel = null;


        // =========================
        // 未確定の感度変更を破棄
        //
        // 「戻る」を押さずにESCで閉じた場合、
        // 感度変更は確定しない
        // =========================

        if (gunController != null)
        {
            pendingSensitivity =
                gunController.sensitivity;
        }

        hasPendingSensitivity = false;


        // =========================
        // クロスヘアUI操作終了
        //
        // Controller自体は無効化しない
        // =========================

        if (crosshairUIController != null)
        {
            crosshairUIController.SetPauseMode(false);
        }


        // =========================
        // EventSystemを元に戻す
        // =========================

        if (eventSystem != null)
        {
            eventSystem.enabled =
                originalEventSystemEnabled;
        }


        // =========================
        // ゲーム再開
        // =========================

        Time.timeScale = 1f;

        // 2D物理を通常に戻す
        Physics2D.simulationMode =
            SimulationMode2D.FixedUpdate;

        isOpen = false;
        IsPaused = false;


        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    // =====================================================
    // ボタン用（今まで通り）
    // =====================================================

    public void ShowPanel(GameObject panel)
    {
        if (currentAnim != null)
            StopCoroutine(currentAnim);


        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }


        currentPanel = panel;
        currentPanel.SetActive(true);


        // =================================================
        // ESCで開いた直後のstartPanelだけアニメ
        // =================================================

        if (panel == startPanel && playOpenAnim)
        {
            currentPanel.transform.localScale =
                Vector3.zero;

            currentAnim =
                StartCoroutine(
                    ScaleAnim(
                        currentPanel,
                        Vector3.zero,
                        Vector3.one
                    )
                );

            playOpenAnim = false; //一回だけにする
        }
        else
        {
            currentPanel.transform.localScale =
                Vector3.one;
        }


        // =================================================
        // 設定画面を開いた時
        //
        // 現在確定している感度をSliderへ反映
        // =================================================

        if (sensitivitySlider != null &&
            gunController != null)
        {
            sensitivitySlider.SetValueWithoutNotify(
                gunController.sensitivity
            );
        }
    }


    IEnumerator ScaleAnim(
        GameObject panel,
        Vector3 start,
        Vector3 end)
    {
        float time = 0f;

        while (time < animTime)
        {
            time += Time.unscaledDeltaTime;

            float t =
                time / animTime;

            t =
                1f -
                Mathf.Pow(
                    1f - t,
                    3f
                );

            panel.transform.localScale =
                Vector3.Lerp(
                    start,
                    end,
                    t
                );

            yield return null;
        }

        panel.transform.localScale = end;
        currentAnim = null;
    }


    // =====================================================
    // 設定などから戻る
    // =====================================================

    public void BackToMenu()
    {
        // =================================================
        // 設定画面で変更した感度を確定
        // =================================================

        if (hasPendingSensitivity &&
            gunController != null)
        {
            gunController.SetSensitivity(
                pendingSensitivity
            );

            hasPendingSensitivity = false;
        }


        // =================================================
        // 確定した感度をSliderにも反映
        // =================================================

        if (sensitivitySlider != null &&
            gunController != null)
        {
            sensitivitySlider.SetValueWithoutNotify(
                gunController.sensitivity
            );
        }


        ShowPanel(startPanel);
    }


    // =====================================================
    // 感度Sliderの仮保存
    //
    // Sliderを動かしている間は実際の感度を変更しない
    // 「戻る」を押した時に初めて適用する
    // =====================================================
    public void SetPendingSensitivity(float value)
    {
        pendingSensitivity = value;

        // 感度変更が仮保存されていることを記録
        hasPendingSensitivity = true;
    }


    // =====================================================
    // 現在の仮保存中の感度を取得
    // =====================================================

    public float GetPendingSensitivity()
    {
        if (hasPendingSensitivity)
        {
            return pendingSensitivity;
        }

        if (gunController != null)
        {
            return gunController.sensitivity;
        }

        return 0f;
    }


    // =====================================================
    // CrosshairUIControllerから使用
    // =====================================================

    public GameObject[] GetPanels()
    {
        return panels;
    }


    public void ShowPanel0()
    {
        ShowPanel(panels[0]);
    }


    public void ShowPanel1()
    {
        ShowPanel(panels[1]);
    }


    public void ShowPanel2()
    {
        ShowPanel(panels[2]);
    }
}