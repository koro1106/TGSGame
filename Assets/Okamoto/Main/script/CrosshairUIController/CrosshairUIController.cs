using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CrosshairUIController : MonoBehaviour
{
    [Header("クロスヘア")]
    public RectTransform crosshair;

    [Header("リザルト・ポーズ共通UIのCanvas")]
    public GraphicRaycaster graphicRaycaster;

    // Sliderを左クリックで掴んで操作中か
    private bool isDraggingSlider = false;

    // 現在操作しているSlider
    private Slider draggingSlider;

    private EventSystem eventSystem;

    // 現在クロスヘアが乗っているボタン
    private Button currentButton;

    // 現在クロスヘアが乗っているSlider
    private Slider currentSlider;

    // ポインター情報
    private PointerEventData pointerData;

    // UI Raycast結果
    private readonly List<RaycastResult> raycastResults =
        new List<RaycastResult>();

    // ポーズ画面操作中か
    private bool isPauseMode = false;

    // 次にクリックできる時間
    private float nextClickTime = 0f;

    // クリック後にButton判定を強制的にやり直す
    private bool forceRefreshButton = false;


    private void Awake()
    {
        eventSystem = EventSystem.current;

        CreatePointerData();
    }


    private void OnEnable()
    {
        eventSystem = EventSystem.current;

        CreatePointerData();

        // 有効化された時はButton判定を最初からやり直す
        currentButton = null;
        currentSlider = null;
        forceRefreshButton = false;
        nextClickTime = 0f;
    }


    // =====================================================
    // PointerEventData作成
    // =====================================================

    private void CreatePointerData()
    {
        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
        }

        if (eventSystem != null)
        {
            pointerData =
                new PointerEventData(eventSystem);
        }
    }


    // =====================================================
    // ポーズ画面モード切り替え
    // =====================================================

    public void SetPauseMode(bool pause)
    {
        isPauseMode = pause;

        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
        }

        if (eventSystem != null)
        {
            pointerData =
                new PointerEventData(eventSystem);
        }

        // 前回のButton状態を完全解除
        ClearCurrentButton();

        // 前回のSlider状態を解除
        currentSlider = null;

        // クリック状態を完全リセット
        forceRefreshButton = false;
        nextClickTime = 0f;
    }


    private void Update()
    {
        // =====================================================
        // EventSystem取得
        // =====================================================

        if (eventSystem == null)
        {
            eventSystem =
                EventSystem.current;

            if (eventSystem != null)
            {
                pointerData =
                    new PointerEventData(eventSystem);
            }
        }

        if (eventSystem == null)
        {
            return;
        }

        if (pointerData == null)
        {
            pointerData =
                new PointerEventData(eventSystem);
        }


        // =====================================================
        // リザルト中
        // =====================================================

        bool resultMode =
            ResultManager.IsResultActive;


        // =====================================================
        // ポーズ中
        // =====================================================

        bool pauseMode =
            isPauseMode;


        // =====================================================
        // リザルトでもポーズでもない
        // =====================================================

        if (!resultMode && !pauseMode)
        {
            return;
        }


        // =====================================================
        // ここではEventSystemを変更しない
        //
        // EventSystemのON/OFFはPauseMenu側で管理する
        // =====================================================


        // =====================================================
        // クロスヘア確認
        // =====================================================

        if (crosshair == null)
        {
            return;
        }


        // =====================================================
        // クロスヘアのスクリーン座標
        // =====================================================

        Vector2 screenPosition =
            GetCrosshairScreenPosition();


        // =====================================================
        // PointerEventData
        // =====================================================

        pointerData.Reset();

        pointerData.position =
            screenPosition;

        pointerData.button =
            PointerEventData.InputButton.Left;


        // =====================================================
        // クロスヘア位置にあるButtonを取得
        // =====================================================

        Button hitButton = null;


        // =====================================================
        // クロスヘア位置にあるSliderを取得
        // =====================================================

        Slider hitSlider = null;


        // =====================================================
        // ポーズ中
        //
        // PauseMenu配下のUIだけを見る
        // =====================================================

        if (pauseMode)
        {
            hitButton =
                GetPauseButtonAtCrosshair(
                    screenPosition
                );

            hitSlider =
                GetPauseSliderAtCrosshair(
                    screenPosition
                );
        }


        // =====================================================
        // リザルト中
        //
        // Canvas内のUIを見る
        // =====================================================

        else if (resultMode)
        {
            hitButton =
                GetResultButtonAtCrosshair(
                    screenPosition
                );

            hitSlider =
                GetResultSliderAtCrosshair(
                    screenPosition
                );
        }


        // =====================================================
        // Slider操作
        //
        // Sliderに乗っただけでは動かさない
        //
        // 左クリックを押している間だけ操作する
        // =====================================================

        // =================================================
        // Sliderをクリックした瞬間
        // =================================================

        if (Input.GetMouseButtonDown(0))
        {
            // Sliderの上にいる場合
            if (hitSlider != null)
            {
                isDraggingSlider = true;
                draggingSlider = hitSlider;

                // クリックした瞬間の位置をSliderへ反映
                SetSliderValueFromCrosshair(
                    draggingSlider,
                    screenPosition
                );
            }
        }


        // =================================================
        // Sliderを操作中
        // =================================================

        if (isDraggingSlider)
        {
            // 左クリックを離した
            if (!Input.GetMouseButton(0))
            {
                isDraggingSlider = false;
                draggingSlider = null;
            }
            else
            {
                // 押している間だけSliderを動かす
                if (draggingSlider != null)
                {
                    SetSliderValueFromCrosshair(
                        draggingSlider,
                        screenPosition
                    );
                }
            }
        }


        // =================================================
        // 現在クロスヘアが乗っているSlider
        // =================================================

        if (hitSlider != null)
        {
            currentSlider = hitSlider;
        }
        else
        {
            currentSlider = null;
        }


        // =====================================================
        // Sliderの上にいる場合
        //
        // Buttonとの重複判定を避ける
        // =====================================================

        if (hitSlider != null)
        {
            hitButton = null;
        }


        // =====================================================
        // クリック後はButton判定を強制的にやり直す
        // =====================================================

        if (forceRefreshButton)
        {
            // =================================================
            // 前のButtonのHover状態を解除
            // =================================================

            if (currentButton != null)
            {
                ExecuteEvents.Execute(
                    currentButton.gameObject,
                    pointerData,
                    ExecuteEvents.pointerExitHandler
                );
            }

            // =================================================
            // 現在のButtonを一旦解除
            // =================================================

            currentButton = null;

            // =================================================
            // 強制再判定フラグを解除
            // =================================================

            forceRefreshButton = false;
        }


        // =====================================================
        // Buttonが変わった
        // =====================================================

        if (currentButton != hitButton)
        {
            // =================================================
            // 前のButtonから離れる
            // =================================================

            if (currentButton != null)
            {
                ExecuteEvents.Execute(
                    currentButton.gameObject,
                    pointerData,
                    ExecuteEvents.pointerExitHandler
                );
            }


            // =================================================
            // 新しいButton
            // =================================================

            currentButton =
                hitButton;


            // =================================================
            // 新しいButtonに入る
            // =================================================

            if (currentButton != null)
            {
                ExecuteEvents.Execute(
                    currentButton.gameObject,
                    pointerData,
                    ExecuteEvents.pointerEnterHandler
                );
            }
        }


        // =====================================================
        // 左クリック
        //
        // 本物のマウスカーソル位置は使用しない
        //
        // クロスヘア位置にあるButtonを実行
        // =====================================================

        if (Input.GetMouseButtonDown(0))
        {
            // =================================================
            // Sliderをクリックした場合
            //
            // Button処理はしない
            // =================================================

            if (hitSlider != null)
            {
                return;
            }


            // =================================================
            // クリック可能な状態か確認
            // =================================================

            if (Time.unscaledTime >= nextClickTime)
            {
                Button clickButton = null;


                // =================================================
                // クリックした瞬間に
                // クロスヘア位置からもう一度Buttonを取得
                // =================================================

                if (pauseMode)
                {
                    clickButton =
                        GetPauseButtonAtCrosshair(
                            screenPosition
                        );
                }

                // =================================================
                // リザルト中
                // =================================================

                else if (resultMode)
                {
                    clickButton =
                        GetResultButtonAtCrosshair(
                            screenPosition
                        );
                }


                // =================================================
                // Button実行
                // =================================================

                if (clickButton != null)
                {
                    // =========================
                    // クリック前に現在のボタン状態を完全解除
                    // =========================

                    if (currentButton != null)
                    {
                        ExecuteEvents.Execute(
                            currentButton.gameObject,
                            pointerData,
                            ExecuteEvents.pointerExitHandler
                        );
                    }

                    currentButton = null;


                    // =========================
                    // ボタンをクリック
                    // =========================

                    clickButton.onClick.Invoke();


                    // =========================
                    // 次のフレームで再判定させる
                    // =========================

                    forceRefreshButton = true;

                    nextClickTime =
                        Time.unscaledTime + 0.1f;
                }
            }
        }
    }


    // =====================================================
    // ポーズ画面のButtonを取得
    // =====================================================

    private Button GetPauseButtonAtCrosshair(
        Vector2 screenPosition)
    {
        // =================================================
        // PauseMenuを探す
        // =================================================

        PauseMenu pauseMenu =
            FindFirstObjectByType<PauseMenu>();

        if (pauseMenu == null)
        {
            return null;
        }


        // =================================================
        // PauseMenuに登録されている全パネルを取得
        // =================================================

        GameObject[] panels =
            pauseMenu.GetPanels();

        if (panels == null)
        {
            return null;
        }


        Button hitButton = null;

        float nearestDistance =
            float.MaxValue;


        // =================================================
        // 全パネルを確認
        // =================================================

        foreach (GameObject panel in panels)
        {
            if (panel == null)
            {
                continue;
            }


            // =================================================
            // 現在表示されているパネルだけ対象
            // =================================================

            if (!panel.activeInHierarchy)
            {
                continue;
            }


            // =================================================
            // パネル内のButtonを取得
            // =================================================

            Button[] buttons =
                panel.GetComponentsInChildren<Button>(
                    true
                );


            // =================================================
            // Buttonを1つずつ確認
            // =================================================

            foreach (Button button in buttons)
            {
                if (button == null)
                {
                    continue;
                }


                if (!button.gameObject.activeInHierarchy)
                {
                    continue;
                }


                // =================================================
                // Buttonが操作可能か
                // =================================================

                if (!button.interactable)
                {
                    continue;
                }


                RectTransform buttonRect =
                    button.GetComponent<RectTransform>();

                if (buttonRect == null)
                {
                    continue;
                }


                // =================================================
                // Buttonが所属しているCanvas
                // =================================================

                Canvas canvas =
                    button.GetComponentInParent<Canvas>();

                Camera buttonCamera =
                    GetCanvasCamera(canvas);


                // =================================================
                // クロスヘアがButton内にあるか
                // =================================================

                bool inside =
                    RectTransformUtility.RectangleContainsScreenPoint(
                        buttonRect,
                        screenPosition,
                        buttonCamera
                    );

                if (!inside)
                {
                    continue;
                }


                // =================================================
                // Button中央との距離
                // =================================================

                Vector2 buttonCenter =
                    RectTransformUtility.WorldToScreenPoint(
                        buttonCamera,
                        buttonRect.position
                    );


                float distance =
                    Vector2.Distance(
                        screenPosition,
                        buttonCenter
                    );


                if (distance < nearestDistance)
                {
                    nearestDistance =
                        distance;

                    hitButton =
                        button;
                }
            }
        }


        return hitButton;
    }


    // =====================================================
    // ポーズ画面のSliderを取得
    // =====================================================

    private Slider GetPauseSliderAtCrosshair(
        Vector2 screenPosition)
    {
        // =================================================
        // PauseMenuを探す
        // =================================================

        PauseMenu pauseMenu =
            FindFirstObjectByType<PauseMenu>();

        if (pauseMenu == null)
        {
            return null;
        }


        // =================================================
        // PauseMenuに登録されている全パネルを取得
        // =================================================

        GameObject[] panels =
            pauseMenu.GetPanels();

        if (panels == null)
        {
            return null;
        }


        Slider hitSlider = null;

        float nearestDistance =
            float.MaxValue;


        // =================================================
        // 全パネルを確認
        // =================================================

        foreach (GameObject panel in panels)
        {
            if (panel == null)
            {
                continue;
            }


            // =================================================
            // 現在表示されているパネルだけ対象
            // =================================================

            if (!panel.activeInHierarchy)
            {
                continue;
            }


            // =================================================
            // パネル内のSliderを取得
            // =================================================

            Slider[] sliders =
                panel.GetComponentsInChildren<Slider>(
                    true
                );


            // =================================================
            // Sliderを1つずつ確認
            // =================================================

            foreach (Slider slider in sliders)
            {
                if (slider == null)
                {
                    continue;
                }


                if (!slider.gameObject.activeInHierarchy)
                {
                    continue;
                }


                // =================================================
                // Sliderが操作可能か
                // =================================================

                if (!slider.interactable)
                {
                    continue;
                }


                RectTransform sliderRect =
                    slider.GetComponent<RectTransform>();

                if (sliderRect == null)
                {
                    continue;
                }


                // =================================================
                // Sliderが所属しているCanvas
                // =================================================

                Canvas canvas =
                    slider.GetComponentInParent<Canvas>();

                Camera sliderCamera =
                    GetCanvasCamera(canvas);


                // =================================================
                // クロスヘアがSlider内にあるか
                // =================================================

                bool inside =
                    RectTransformUtility.RectangleContainsScreenPoint(
                        sliderRect,
                        screenPosition,
                        sliderCamera
                    );

                if (!inside)
                {
                    continue;
                }


                // =================================================
                // Slider中央との距離
                // =================================================

                Vector2 sliderCenter =
                    RectTransformUtility.WorldToScreenPoint(
                        sliderCamera,
                        sliderRect.position
                    );


                float distance =
                    Vector2.Distance(
                        screenPosition,
                        sliderCenter
                    );


                if (distance < nearestDistance)
                {
                    nearestDistance =
                        distance;

                    hitSlider =
                        slider;
                }
            }
        }


        return hitSlider;
    }


    // =====================================================
    // リザルト画面のButtonを取得
    // =====================================================

    private Button GetResultButtonAtCrosshair(
        Vector2 screenPosition)
    {
        if (graphicRaycaster == null)
        {
            return null;
        }


        Canvas canvas =
            graphicRaycaster.GetComponent<Canvas>();


        if (canvas == null)
        {
            canvas =
                graphicRaycaster.GetComponentInParent<Canvas>();
        }


        if (canvas == null)
        {
            return null;
        }


        Button[] buttons =
            canvas.GetComponentsInChildren<Button>(
                true
            );


        Button nearestButton = null;

        float nearestDistance =
            float.MaxValue;


        foreach (Button button in buttons)
        {
            if (button == null)
            {
                continue;
            }


            if (!button.gameObject.activeInHierarchy)
            {
                continue;
            }


            if (!button.interactable)
            {
                continue;
            }


            RectTransform buttonRect =
                button.GetComponent<RectTransform>();


            if (buttonRect == null)
            {
                continue;
            }


            Camera buttonCamera =
                GetCanvasCamera(canvas);


            // =================================================
            // クロスヘアがButtonの範囲内か
            // =================================================

            bool inside =
                RectTransformUtility.RectangleContainsScreenPoint(
                    buttonRect,
                    screenPosition,
                    buttonCamera
                );


            if (!inside)
            {
                continue;
            }


            // =================================================
            // Button中央との距離
            // =================================================

            Vector2 buttonCenter =
                RectTransformUtility.WorldToScreenPoint(
                    buttonCamera,
                    buttonRect.position
                );


            float distance =
                Vector2.Distance(
                    screenPosition,
                    buttonCenter
                );


            if (distance < nearestDistance)
            {
                nearestDistance =
                    distance;

                nearestButton =
                    button;
            }
        }


        return nearestButton;
    }


    // =====================================================
    // リザルト画面のSliderを取得
    // =====================================================

    private Slider GetResultSliderAtCrosshair(
        Vector2 screenPosition)
    {
        if (graphicRaycaster == null)
        {
            return null;
        }


        Canvas canvas =
            graphicRaycaster.GetComponent<Canvas>();


        if (canvas == null)
        {
            canvas =
                graphicRaycaster.GetComponentInParent<Canvas>();
        }


        if (canvas == null)
        {
            return null;
        }


        Slider[] sliders =
            canvas.GetComponentsInChildren<Slider>(
                true
            );


        Slider nearestSlider = null;

        float nearestDistance =
            float.MaxValue;


        foreach (Slider slider in sliders)
        {
            if (slider == null)
            {
                continue;
            }


            if (!slider.gameObject.activeInHierarchy)
            {
                continue;
            }


            if (!slider.interactable)
            {
                continue;
            }


            RectTransform sliderRect =
                slider.GetComponent<RectTransform>();


            if (sliderRect == null)
            {
                continue;
            }


            Camera sliderCamera =
                GetCanvasCamera(canvas);


            // =================================================
            // クロスヘアがSlider内にあるか
            // =================================================

            bool inside =
                RectTransformUtility.RectangleContainsScreenPoint(
                    sliderRect,
                    screenPosition,
                    sliderCamera
                );


            if (!inside)
            {
                continue;
            }


            // =================================================
            // Slider中央との距離
            // =================================================

            Vector2 sliderCenter =
                RectTransformUtility.WorldToScreenPoint(
                    sliderCamera,
                    sliderRect.position
                );


            float distance =
                Vector2.Distance(
                    screenPosition,
                    sliderCenter
                );


            if (distance < nearestDistance)
            {
                nearestDistance =
                    distance;

                nearestSlider =
                    slider;
            }
        }


        return nearestSlider;
    }


    // =====================================================
    // クロスヘア位置からSliderの値を変更
    // =====================================================

    private void SetSliderValueFromCrosshair(
        Slider slider,
        Vector2 screenPosition)
    {
        if (slider == null)
        {
            return;
        }


        RectTransform sliderRect =
            slider.GetComponent<RectTransform>();


        if (sliderRect == null)
        {
            return;
        }


        Canvas canvas =
            slider.GetComponentInParent<Canvas>();


        Camera sliderCamera =
            GetCanvasCamera(canvas);


        // =================================================
        // クロスヘアをSliderのローカル座標に変換
        // =================================================

        Vector2 localPoint;


        bool converted =
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                sliderRect,
                screenPosition,
                sliderCamera,
                out localPoint
            );


        if (!converted)
        {
            return;
        }


        // =================================================
        // Sliderのサイズ
        // =================================================

        Rect rect =
            sliderRect.rect;


        float normalizedValue = 0f;


        // =================================================
        // 横Slider
        // =================================================

        if (slider.direction ==
            Slider.Direction.LeftToRight ||
            slider.direction ==
            Slider.Direction.RightToLeft)
        {
            float minX =
                rect.xMin;

            float maxX =
                rect.xMax;


            // =================================================
            // Handleがある場合
            //
            // Handleの中心がSlider端から
            // はみ出さないようにする
            // =================================================

            if (slider.handleRect != null)
            {
                float handleWidth =
                    slider.handleRect.rect.width *
                    Mathf.Abs(
                        slider.handleRect.localScale.x
                    );

                minX +=
                    handleWidth * 0.5f;

                maxX -=
                    handleWidth * 0.5f;
            }


            if (maxX <= minX)
            {
                return;
            }


            normalizedValue =
                Mathf.InverseLerp(
                    minX,
                    maxX,
                    localPoint.x
                );


            // =================================================
            // 右から左
            // =================================================

            if (slider.direction ==
                Slider.Direction.RightToLeft)
            {
                normalizedValue =
                    1f - normalizedValue;
            }
        }


        // =================================================
        // 縦Slider
        // =================================================

        else
        {
            float minY =
                rect.yMin;

            float maxY =
                rect.yMax;


            // =================================================
            // Handleがある場合
            // =================================================

            if (slider.handleRect != null)
            {
                float handleHeight =
                    slider.handleRect.rect.height *
                    Mathf.Abs(
                        slider.handleRect.localScale.y
                    );

                minY +=
                    handleHeight * 0.5f;

                maxY -=
                    handleHeight * 0.5f;
            }


            if (maxY <= minY)
            {
                return;
            }


            normalizedValue =
                Mathf.InverseLerp(
                    minY,
                    maxY,
                    localPoint.y
                );


            // =================================================
            // 上から下
            // =================================================

            if (slider.direction ==
                Slider.Direction.TopToBottom)
            {
                normalizedValue =
                    1f - normalizedValue;
            }
        }


        // =================================================
        // 0～1に制限
        // =================================================

        normalizedValue =
            Mathf.Clamp01(
                normalizedValue
            );


        // =================================================
        // Sliderのmin～maxへ変換
        // =================================================

        float value =
            Mathf.Lerp(
                slider.minValue,
                slider.maxValue,
                normalizedValue
            );


        // =================================================
        // Sliderへ反映
        //
        // ここではGunControllerの感度を直接変更しない
        // PauseMenuへ一時保存する
        // =================================================

        slider.value = value;


        // =================================================
        // 感度Sliderの場合
        //
        // PauseMenuに登録されているSensitivity Sliderだけ
        // 一時保存する
        // =================================================

        PauseMenu pauseMenu =
            FindFirstObjectByType<PauseMenu>();

        if (pauseMenu != null &&
            slider == pauseMenu.sensitivitySlider)
        {
            pauseMenu.SetPendingSensitivity(value);
        }
    }


    // =====================================================
    // Canvas用カメラ取得
    // =====================================================

    private Camera GetCanvasCamera(Canvas canvas)
    {
        if (canvas == null)
        {
            return Camera.main;
        }


        // Screen Space - Overlay
        if (canvas.renderMode ==
            RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }


        // Screen Space - Camera
        if (canvas.worldCamera != null)
        {
            return canvas.worldCamera;
        }


        return Camera.main;
    }


    // =====================================================
    // クロスヘアのスクリーン座標を取得
    // =====================================================

    private Vector2 GetCrosshairScreenPosition()
    {
        Canvas canvas =
            crosshair.GetComponentInParent<Canvas>();


        if (canvas == null)
        {
            return RectTransformUtility.WorldToScreenPoint(
                Camera.main,
                crosshair.position
            );
        }


        // =================================================
        // Screen Space - Overlay
        // =================================================

        if (canvas.renderMode ==
            RenderMode.ScreenSpaceOverlay)
        {
            return RectTransformUtility.WorldToScreenPoint(
                null,
                crosshair.position
            );
        }


        // =================================================
        // Screen Space - Camera / World Space
        // =================================================

        Camera targetCamera =
            canvas.worldCamera;


        if (targetCamera == null)
        {
            targetCamera =
                Camera.main;
        }


        return RectTransformUtility.WorldToScreenPoint(
            targetCamera,
            crosshair.position
        );
    }


    // =====================================================
    // 無効化時
    // =====================================================

    private void OnDisable()
    {
        ClearCurrentButton();

        currentSlider = null;

        // EventSystemの状態は変更しない
        //
        // EventSystemのON/OFFはPauseMenu側だけが管理する
    }


    // =====================================================
    // 現在のButtonを解除
    // =====================================================

    private void ClearCurrentButton()
    {
        if (currentButton != null &&
            pointerData != null)
        {
            ExecuteEvents.Execute(
                currentButton.gameObject,
                pointerData,
                ExecuteEvents.pointerExitHandler
            );
        }


        currentButton = null;

        // 強制再判定フラグも解除
        forceRefreshButton = false;
    }
}