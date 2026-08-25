using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropPickupLog : MonoBehaviour
{
    public static DropPickupLog Instance;

    [Header("ログ全体")]
    public GameObject logObject;

    [Header("UI")]
    public Image backgroundImage;
    public Image itemImage;
    public TMP_Text amountText;

    [Header("アイテム画像")]
    public Sprite exp1Sprite;
    public Sprite exp2Sprite;
    public Sprite exp3Sprite;
    public Sprite exp4Sprite;
    public Sprite preExpSprite;

    [Header("表示時間")]
    public float displayTime = 2f;

    [Header("白フラッシュ")]
    public float flashDuration = 0.12f;

    [Header("文字ポップ演出")]
    public float popScale = 1.4f;
    public float popDuration = 0.15f;

    [Header("フェード")]
    public float fadeDuration = 0.3f;

    private CanvasGroup canvasGroup;

    private Coroutine hideCoroutine;
    private Coroutine effectCoroutine;

    private DropItemType currentType;
    private int currentAmount;

    private Color defaultBackgroundColor;
    private Color defaultImageColor;
    private Color defaultTextColor;

    private Vector3 defaultTextScale;

    void Awake()
    {
        Instance = this;

        if (logObject == null)
        {
            logObject = gameObject;
        }

        canvasGroup =
            logObject.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                logObject.AddComponent<CanvasGroup>();
        }

        if (backgroundImage != null)
        {
            defaultBackgroundColor =
                backgroundImage.color;
        }

        if (itemImage != null)
        {
            defaultImageColor =
                itemImage.color;
        }

        if (amountText != null)
        {
            defaultTextColor =
                amountText.color;

            defaultTextScale =
                amountText.transform.localScale;
        }

        logObject.SetActive(false);
    }


    // =====================================================
    // アイテム回収ログ表示
    // =====================================================

    public void ShowPickup(
        DropItemType type,
        int amount
    )
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        // =============================================
        // 同じアイテムを連続回収
        // =============================================

        if (
            logObject.activeSelf &&
            currentType == type
        )
        {
            currentAmount += amount;
        }
        else
        {
            // =============================================
            // 新しいアイテム
            // =============================================

            currentType = type;
            currentAmount = amount;

            SetItemSprite(type);

            logObject.SetActive(true);
        }

        // 個数更新
        UpdateAmountText();


        // =============================================
        // 演出を最初からやり直す
        // =============================================

        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);

            // 途中で止まった場合に元へ戻す
            ResetEffect();
        }

        effectCoroutine =
            StartCoroutine(PickupEffectRoutine());


        // =============================================
        // 2秒タイマーをリセット
        // =============================================

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine =
            StartCoroutine(HideRoutine());
    }


    // =====================================================
    // 個数更新
    // =====================================================

    void UpdateAmountText()
    {
        if (amountText != null)
        {
            amountText.text =
                "×" + currentAmount;
        }
    }


    // =====================================================
    // アイテム画像設定
    // =====================================================

    void SetItemSprite(
        DropItemType type
    )
    {
        if (itemImage == null)
            return;

        switch (type)
        {
            case DropItemType.Exp1:

                itemImage.sprite =
                    exp1Sprite;

                break;


            case DropItemType.Exp2:

                itemImage.sprite =
                    exp2Sprite;

                break;


            case DropItemType.Exp3:

                itemImage.sprite =
                    exp3Sprite;

                break;


            case DropItemType.Exp4:

                itemImage.sprite =
                    exp4Sprite;

                break;


            case DropItemType.PreExp:

                itemImage.sprite =
                    preExpSprite;

                break;
        }
    }


    // =====================================================
    // 回収時演出
    //
    // ・黒背景 → 一瞬白
    // ・アイテム → 一瞬白
    // ・文字 → 一瞬大きくなる
    // =====================================================

    IEnumerator PickupEffectRoutine()
    {
        Color flashColor = new Color(
            0.75f,
            0.75f,
            0.75f,
            1f
        );

        // 最初に灰色っぽくする

        if (backgroundImage != null)
        {
            backgroundImage.color =
                flashColor;
        }

        if (itemImage != null)
        {
            itemImage.color =
                flashColor;
        }

        if (amountText != null)
        {
            amountText.color =
                flashColor;

            amountText.transform.localScale =
                defaultTextScale * popScale;
        }


        float timer = 0f;

        float duration =
            Mathf.Max(
                flashDuration,
                popDuration
            );

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );


            if (backgroundImage != null)
            {
                backgroundImage.color =
                    Color.Lerp(
                        flashColor,
                        defaultBackgroundColor,
                        t
                    );
            }


            if (itemImage != null)
            {
                itemImage.color =
                    Color.Lerp(
                        flashColor,
                        defaultImageColor,
                        t
                    );
            }


            if (amountText != null)
            {
                amountText.color =
                    Color.Lerp(
                        flashColor,
                        defaultTextColor,
                        t
                    );

                amountText.transform.localScale =
                    Vector3.Lerp(
                        defaultTextScale * popScale,
                        defaultTextScale,
                        t
                    );
            }

            yield return null;
        }

        ResetEffect();

        effectCoroutine = null;
    }


    // =====================================================
    // 演出を元に戻す
    // =====================================================

    void ResetEffect()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color =
                defaultBackgroundColor;
        }

        if (itemImage != null)
        {
            itemImage.color =
                defaultImageColor;
        }

        if (amountText != null)
        {
            amountText.color =
                defaultTextColor;

            amountText.transform.localScale =
                defaultTextScale;
        }
    }


    // =====================================================
    // 2秒後に消す
    // =====================================================

    IEnumerator HideRoutine()
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

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / fadeDuration
                );

            if (canvasGroup != null)
            {
                canvasGroup.alpha =
                    Mathf.Lerp(
                        1f,
                        0f,
                        t
                    );
            }

            yield return null;
        }


        // 完全に透明
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }


        // 非表示
        logObject.SetActive(false);

        currentAmount = 0;

        hideCoroutine = null;
    }
}