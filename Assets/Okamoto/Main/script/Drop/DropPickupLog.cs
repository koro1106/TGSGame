using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropPickupLog : MonoBehaviour
{
    public static DropPickupLog Instance;

    // =====================================================
    // 素材ごとのPanel
    // =====================================================

    [Header("素材ごとのPanel")]
    public GameObject exp1Panel;
    public GameObject exp2Panel;
    public GameObject exp3Panel;
    public GameObject exp4Panel;
    public GameObject preExpPanel;


    // =====================================================
    // Exp1 Panel
    // =====================================================

    [Header("Exp1 Panel UI")]
    public Image exp1BackgroundImage;
    public Image exp1ItemImage;
    public TMP_Text exp1AmountText;


    // =====================================================
    // Exp2 Panel
    // =====================================================

    [Header("Exp2 Panel UI")]
    public Image exp2BackgroundImage;
    public Image exp2ItemImage;
    public TMP_Text exp2AmountText;


    // =====================================================
    // Exp3 Panel
    // =====================================================

    [Header("Exp3 Panel UI")]
    public Image exp3BackgroundImage;
    public Image exp3ItemImage;
    public TMP_Text exp3AmountText;


    // =====================================================
    // Exp4 Panel
    // =====================================================

    [Header("Exp4 Panel UI")]
    public Image exp4BackgroundImage;
    public Image exp4ItemImage;
    public TMP_Text exp4AmountText;


    // =====================================================
    // PreExp Panel
    // =====================================================

    [Header("PreExp Panel UI")]
    public Image preExpBackgroundImage;
    public Image preExpItemImage;
    public TMP_Text preExpAmountText;


    // =====================================================
    // アイテム画像
    // =====================================================

    [Header("アイテム画像")]
    public Sprite exp1Sprite;
    public Sprite exp2Sprite;
    public Sprite exp3Sprite;
    public Sprite exp4Sprite;
    public Sprite preExpSprite;


    // =====================================================
    // 表示時間
    // =====================================================

    [Header("表示時間")]
    public float displayTime = 2f;


    // =====================================================
    // 白フラッシュ
    // =====================================================

    [Header("白フラッシュ")]
    public float flashDuration = 0.12f;


    // =====================================================
    // 文字ポップ演出
    // =====================================================

    [Header("文字ポップ演出")]
    public float popScale = 1.4f;
    public float popDuration = 0.15f;


    // =====================================================
    // フェード
    // =====================================================

    [Header("フェード")]
    public float fadeDuration = 0.3f;


    // =====================================================
    // 各PanelのCanvasGroup
    // =====================================================

    private CanvasGroup exp1CanvasGroup;
    private CanvasGroup exp2CanvasGroup;
    private CanvasGroup exp3CanvasGroup;
    private CanvasGroup exp4CanvasGroup;
    private CanvasGroup preExpCanvasGroup;


    // =====================================================
    // 各素材の個数
    // =====================================================

    private int exp1Amount;
    private int exp2Amount;
    private int exp3Amount;
    private int exp4Amount;
    private int preExpAmount;


    // =====================================================
    // 各素材の非表示Coroutine
    // =====================================================

    private Coroutine exp1HideCoroutine;
    private Coroutine exp2HideCoroutine;
    private Coroutine exp3HideCoroutine;
    private Coroutine exp4HideCoroutine;
    private Coroutine preExpHideCoroutine;


    // =====================================================
    // 元の色
    // =====================================================

    private Color exp1DefaultBackgroundColor;
    private Color exp1DefaultItemColor;
    private Color exp1DefaultTextColor;

    private Color exp2DefaultBackgroundColor;
    private Color exp2DefaultItemColor;
    private Color exp2DefaultTextColor;

    private Color exp3DefaultBackgroundColor;
    private Color exp3DefaultItemColor;
    private Color exp3DefaultTextColor;

    private Color exp4DefaultBackgroundColor;
    private Color exp4DefaultItemColor;
    private Color exp4DefaultTextColor;

    private Color preExpDefaultBackgroundColor;
    private Color preExpDefaultItemColor;
    private Color preExpDefaultTextColor;


    // =====================================================
    // 元の文字サイズ
    // =====================================================

    private Vector3 exp1DefaultTextScale;
    private Vector3 exp2DefaultTextScale;
    private Vector3 exp3DefaultTextScale;
    private Vector3 exp4DefaultTextScale;
    private Vector3 preExpDefaultTextScale;


    // =====================================================
    // Awake
    // =====================================================

    void Awake()
    {
        Instance = this;


        // =================================================
        // CanvasGroup取得
        // =================================================

        exp1CanvasGroup = GetCanvasGroup(exp1Panel);
        exp2CanvasGroup = GetCanvasGroup(exp2Panel);
        exp3CanvasGroup = GetCanvasGroup(exp3Panel);
        exp4CanvasGroup = GetCanvasGroup(exp4Panel);
        preExpCanvasGroup = GetCanvasGroup(preExpPanel);


        // =================================================
        // 元の色を保存
        // =================================================

        if (exp1BackgroundImage != null)
            exp1DefaultBackgroundColor = exp1BackgroundImage.color;

        if (exp1ItemImage != null)
            exp1DefaultItemColor = exp1ItemImage.color;

        if (exp1AmountText != null)
        {
            exp1DefaultTextColor = exp1AmountText.color;
            exp1DefaultTextScale = exp1AmountText.transform.localScale;
        }


        if (exp2BackgroundImage != null)
            exp2DefaultBackgroundColor = exp2BackgroundImage.color;

        if (exp2ItemImage != null)
            exp2DefaultItemColor = exp2ItemImage.color;

        if (exp2AmountText != null)
        {
            exp2DefaultTextColor = exp2AmountText.color;
            exp2DefaultTextScale = exp2AmountText.transform.localScale;
        }


        if (exp3BackgroundImage != null)
            exp3DefaultBackgroundColor = exp3BackgroundImage.color;

        if (exp3ItemImage != null)
            exp3DefaultItemColor = exp3ItemImage.color;

        if (exp3AmountText != null)
        {
            exp3DefaultTextColor = exp3AmountText.color;
            exp3DefaultTextScale = exp3AmountText.transform.localScale;
        }


        if (exp4BackgroundImage != null)
            exp4DefaultBackgroundColor = exp4BackgroundImage.color;

        if (exp4ItemImage != null)
            exp4DefaultItemColor = exp4ItemImage.color;

        if (exp4AmountText != null)
        {
            exp4DefaultTextColor = exp4AmountText.color;
            exp4DefaultTextScale = exp4AmountText.transform.localScale;
        }


        if (preExpBackgroundImage != null)
            preExpDefaultBackgroundColor = preExpBackgroundImage.color;

        if (preExpItemImage != null)
            preExpDefaultItemColor = preExpItemImage.color;

        if (preExpAmountText != null)
        {
            preExpDefaultTextColor = preExpAmountText.color;
            preExpDefaultTextScale = preExpAmountText.transform.localScale;
        }


        // =================================================
        // 最初は透明
        // =================================================
        // Panel自体は非アクティブにしない
        // =================================================

        if (exp1CanvasGroup != null)
            exp1CanvasGroup.alpha = 0f;

        if (exp2CanvasGroup != null)
            exp2CanvasGroup.alpha = 0f;

        if (exp3CanvasGroup != null)
            exp3CanvasGroup.alpha = 0f;

        if (exp4CanvasGroup != null)
            exp4CanvasGroup.alpha = 0f;

        if (preExpCanvasGroup != null)
            preExpCanvasGroup.alpha = 0f;
    }


    // =====================================================
    // CanvasGroup取得
    // =====================================================

    CanvasGroup GetCanvasGroup(GameObject panel)
    {
        if (panel == null)
            return null;

        CanvasGroup group =
            panel.GetComponent<CanvasGroup>();

        if (group == null)
        {
            group =
                panel.AddComponent<CanvasGroup>();
        }

        return group;
    }


    // =====================================================
    // アイテム回収ログ表示
    // =====================================================

    public void ShowPickup(
        DropItemType type,
        int amount
    )
    {
        switch (type)
        {
            case DropItemType.Exp1:

                ShowExp1(amount);

                break;


            case DropItemType.Exp2:

                ShowExp2(amount);

                break;


            case DropItemType.Exp3:

                ShowExp3(amount);

                break;


            case DropItemType.Exp4:

                ShowExp4(amount);

                break;


            case DropItemType.PreExp:

                ShowPreExp(amount);

                break;
        }
    }


    // =====================================================
    // Exp1
    // =====================================================

    void ShowExp1(int amount)
    {
        if (exp1Panel == null)
            return;

        exp1Amount += amount;

        if (exp1ItemImage != null)
            exp1ItemImage.sprite = exp1Sprite;

        if (exp1AmountText != null)
            exp1AmountText.text = "×" + exp1Amount;

        if (exp1CanvasGroup != null)
            exp1CanvasGroup.alpha = 1f;


        // 表示時の色を元の色に戻す
        ResetPanelColor(
            exp1BackgroundImage,
            exp1ItemImage,
            exp1AmountText,
            exp1DefaultBackgroundColor,
            exp1DefaultItemColor,
            exp1DefaultTextColor,
            exp1DefaultTextScale
        );


        if (exp1HideCoroutine != null)
            StopCoroutine(exp1HideCoroutine);


        StartCoroutine(
            PickupEffectRoutine(
                exp1BackgroundImage,
                exp1ItemImage,
                exp1AmountText,
                exp1DefaultBackgroundColor,
                exp1DefaultItemColor,
                exp1DefaultTextColor,
                exp1DefaultTextScale
            )
        );


        exp1HideCoroutine =
            StartCoroutine(
                HideRoutine(
                    exp1CanvasGroup,
                    () =>
                    {
                        exp1Amount = 0;
                    }
                )
            );
    }


    // =====================================================
    // Exp2
    // =====================================================

    void ShowExp2(int amount)
    {
        if (exp2Panel == null)
            return;

        exp2Amount += amount;

        if (exp2ItemImage != null)
            exp2ItemImage.sprite = exp2Sprite;

        if (exp2AmountText != null)
            exp2AmountText.text = "×" + exp2Amount;

        if (exp2CanvasGroup != null)
            exp2CanvasGroup.alpha = 1f;


        ResetPanelColor(
            exp2BackgroundImage,
            exp2ItemImage,
            exp2AmountText,
            exp2DefaultBackgroundColor,
            exp2DefaultItemColor,
            exp2DefaultTextColor,
            exp2DefaultTextScale
        );


        if (exp2HideCoroutine != null)
            StopCoroutine(exp2HideCoroutine);


        StartCoroutine(
            PickupEffectRoutine(
                exp2BackgroundImage,
                exp2ItemImage,
                exp2AmountText,
                exp2DefaultBackgroundColor,
                exp2DefaultItemColor,
                exp2DefaultTextColor,
                exp2DefaultTextScale
            )
        );


        exp2HideCoroutine =
            StartCoroutine(
                HideRoutine(
                    exp2CanvasGroup,
                    () =>
                    {
                        exp2Amount = 0;
                    }
                )
            );
    }


    // =====================================================
    // Exp3
    // =====================================================

    void ShowExp3(int amount)
    {
        if (exp3Panel == null)
            return;

        exp3Amount += amount;

        if (exp3ItemImage != null)
            exp3ItemImage.sprite = exp3Sprite;

        if (exp3AmountText != null)
            exp3AmountText.text = "×" + exp3Amount;

        if (exp3CanvasGroup != null)
            exp3CanvasGroup.alpha = 1f;


        ResetPanelColor(
            exp3BackgroundImage,
            exp3ItemImage,
            exp3AmountText,
            exp3DefaultBackgroundColor,
            exp3DefaultItemColor,
            exp3DefaultTextColor,
            exp3DefaultTextScale
        );


        if (exp3HideCoroutine != null)
            StopCoroutine(exp3HideCoroutine);


        StartCoroutine(
            PickupEffectRoutine(
                exp3BackgroundImage,
                exp3ItemImage,
                exp3AmountText,
                exp3DefaultBackgroundColor,
                exp3DefaultItemColor,
                exp3DefaultTextColor,
                exp3DefaultTextScale
            )
        );


        exp3HideCoroutine =
            StartCoroutine(
                HideRoutine(
                    exp3CanvasGroup,
                    () =>
                    {
                        exp3Amount = 0;
                    }
                )
            );
    }


    // =====================================================
    // Exp4
    // =====================================================

    void ShowExp4(int amount)
    {
        if (exp4Panel == null)
            return;

        exp4Amount += amount;

        if (exp4ItemImage != null)
            exp4ItemImage.sprite = exp4Sprite;

        if (exp4AmountText != null)
            exp4AmountText.text = "×" + exp4Amount;

        if (exp4CanvasGroup != null)
            exp4CanvasGroup.alpha = 1f;


        ResetPanelColor(
            exp4BackgroundImage,
            exp4ItemImage,
            exp4AmountText,
            exp4DefaultBackgroundColor,
            exp4DefaultItemColor,
            exp4DefaultTextColor,
            exp4DefaultTextScale
        );


        if (exp4HideCoroutine != null)
            StopCoroutine(exp4HideCoroutine);


        StartCoroutine(
            PickupEffectRoutine(
                exp4BackgroundImage,
                exp4ItemImage,
                exp4AmountText,
                exp4DefaultBackgroundColor,
                exp4DefaultItemColor,
                exp4DefaultTextColor,
                exp4DefaultTextScale
            )
        );


        exp4HideCoroutine =
            StartCoroutine(
                HideRoutine(
                    exp4CanvasGroup,
                    () =>
                    {
                        exp4Amount = 0;
                    }
                )
            );
    }


    // =====================================================
    // PreExp
    // =====================================================

    void ShowPreExp(int amount)
    {
        if (preExpPanel == null)
            return;

        preExpAmount += amount;

        if (preExpItemImage != null)
            preExpItemImage.sprite = preExpSprite;

        if (preExpAmountText != null)
            preExpAmountText.text = "×" + preExpAmount;

        if (preExpCanvasGroup != null)
            preExpCanvasGroup.alpha = 1f;


        ResetPanelColor(
            preExpBackgroundImage,
            preExpItemImage,
            preExpAmountText,
            preExpDefaultBackgroundColor,
            preExpDefaultItemColor,
            preExpDefaultTextColor,
            preExpDefaultTextScale
        );


        if (preExpHideCoroutine != null)
            StopCoroutine(preExpHideCoroutine);


        StartCoroutine(
            PickupEffectRoutine(
                preExpBackgroundImage,
                preExpItemImage,
                preExpAmountText,
                preExpDefaultBackgroundColor,
                preExpDefaultItemColor,
                preExpDefaultTextColor,
                preExpDefaultTextScale
            )
        );


        preExpHideCoroutine =
            StartCoroutine(
                HideRoutine(
                    preExpCanvasGroup,
                    () =>
                    {
                        preExpAmount = 0;
                    }
                )
            );
    }


    // =====================================================
    // 色とサイズを元に戻す
    // =====================================================

    void ResetPanelColor(
        Image background,
        Image item,
        TMP_Text text,
        Color defaultBackground,
        Color defaultItem,
        Color defaultText,
        Vector3 defaultScale
    )
    {
        if (background != null)
            background.color = defaultBackground;

        if (item != null)
            item.color = defaultItem;

        if (text != null)
        {
            text.color = defaultText;
            text.transform.localScale = defaultScale;
        }
    }


    // =====================================================
    // 回収時演出
    //
    // ・元の色 → 一瞬グレー
    // ・グレー → 元の色
    // ・文字 → 一瞬大きくなる
    // =====================================================

    IEnumerator PickupEffectRoutine(
        Image background,
        Image item,
        TMP_Text text,
        Color defaultBackground,
        Color defaultItem,
        Color defaultText,
        Vector3 defaultScale
    )
    {
        Color flashColor =
            new Color(
                0.75f,
                0.75f,
                0.75f,
                1f
            );


        // =================================================
        // 最初にグレーにする
        // =================================================

        if (background != null)
            background.color = flashColor;

        if (item != null)
            item.color = flashColor;

        if (text != null)
        {
            text.color = flashColor;

            text.transform.localScale =
                defaultScale * popScale;
        }


        float timer = 0f;

        float duration =
            Mathf.Max(
                flashDuration,
                popDuration
            );


        // =================================================
        // 元の色へ戻す
        // =================================================

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );


            if (background != null)
            {
                background.color =
                    Color.Lerp(
                        flashColor,
                        defaultBackground,
                        t
                    );
            }


            if (item != null)
            {
                item.color =
                    Color.Lerp(
                        flashColor,
                        defaultItem,
                        t
                    );
            }


            if (text != null)
            {
                text.color =
                    Color.Lerp(
                        flashColor,
                        defaultText,
                        t
                    );

                text.transform.localScale =
                    Vector3.Lerp(
                        defaultScale * popScale,
                        defaultScale,
                        t
                    );
            }


            yield return null;
        }


        // =================================================
        // 完全に元へ戻す
        // =================================================

        if (background != null)
            background.color = defaultBackground;

        if (item != null)
            item.color = defaultItem;

        if (text != null)
        {
            text.color = defaultText;
            text.transform.localScale = defaultScale;
        }
    }


    // =====================================================
    // 指定時間後に消す
    // =====================================================

    IEnumerator HideRoutine(
        CanvasGroup group,
        System.Action onHidden
    )
    {
        // =============================================
        // 指定時間待つ
        // =============================================

        yield return new WaitForSecondsRealtime(
            displayTime
        );


        // =============================================
        // 徐々に透明にする
        // =============================================

        float timer = 0f;

        float startAlpha =
            group != null
                ? group.alpha
                : 1f;


        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / fadeDuration
                );


            if (group != null)
            {
                group.alpha =
                    Mathf.Lerp(
                        startAlpha,
                        0f,
                        t
                    );
            }


            yield return null;
        }


        // =============================================
        // 完全に透明
        // =============================================

        if (group != null)
        {
            group.alpha = 0f;
        }


        // =============================================
        // 個数リセット
        // =============================================

        if (onHidden != null)
        {
            onHidden();
        }
    }
}