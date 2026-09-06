using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{

    [SerializeField] PlayerStats playerStats;
    [SerializeField] GameObject shopButton;

    [Header("点滅設定")]
    [SerializeField] private float fadeSpeed = 2f;

    [Header("表示時のポンッ演出")]
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float popDuration = 0.2f;

    [SerializeField] private Image shopImage;

    private bool shopOpened = false;

    // ショップボタンを一度押したか
    public bool isClicked = false;

    private Coroutine blinkCoroutine;

    void Start()
    {
        // PlayerStatsからクリック状態を取得
        isClicked = playerStats.shopButtonClicked;

        // すでに一度クリック済み
        if (isClicked)
        {
            // ボタンを表示
            shopButton.SetActive(true);

            // ポンッ演出はしない
            // 点滅もしない

            // 元のサイズに戻す
            RectTransform rect = shopButton.GetComponent<RectTransform>();
            rect.localScale = Vector3.one * 1.2f;
            rect.localRotation = Quaternion.identity;

            // 完全表示
            Color color = shopImage.color;
            color.a = 1f;
            shopImage.color = color;

            // ここで終了
            return;
        }

        // まだクリックしていない

        // 最初は透明
        Color startColor = shopImage.color;
        startColor.a = 0f;
        shopImage.color = startColor;

        // ボタン自体は非表示
        shopButton.SetActive(false);
    }

    void Update()
    {
        // すでに一度クリック済みなら何もしない
        if (isClicked)
            return;

        if (playerStats.shopOpen && !shopOpened)
        {
            shopOpened = true;

            // ボタン表示
            shopButton.SetActive(true);

            // 表示した瞬間にポンッ
            StartCoroutine(PopIn());

            // 点滅開始
            blinkCoroutine = StartCoroutine(BlinkShopButton());
        }
    }

    // ========================================
    // ポンッと大きくして少し傾ける
    // ========================================
    private IEnumerator PopIn()
    {
        RectTransform rect = shopButton.GetComponent<RectTransform>();

        // 元のサイズ・角度を保存
        Vector3 originalScale = rect.localScale;
        Quaternion originalRotation = rect.localRotation;

        // 少し大きく
        Vector3 bigScale = originalScale * popScale;

        // 少しだけ傾ける
        Quaternion bigRotation = Quaternion.Euler(
            0f,
            0f,
            -5f
        );

        // アニメーション時間
        float duration = 0.12f;

        float time = 0f;

        // ========================================
        // ① ポンッと大きく＋傾く
        // ========================================
        while (time < duration)
        {
            // 途中でクリックされたら終了
            if (isClicked)
                yield break;

            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / duration);

            t = Mathf.SmoothStep(0f, 1f, t);

            rect.localScale = Vector3.Lerp(
                originalScale,
                bigScale,
                t
            );

            rect.localRotation = Quaternion.Lerp(
                originalRotation,
                bigRotation,
                t
            );

            yield return null;
        }

        // ========================================
        // ② 元のサイズ＋角度に戻る
        // ========================================
        time = 0f;

        while (time < duration)
        {
            if (isClicked)
                yield break;

            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / duration);

            t = Mathf.SmoothStep(0f, 1f, t);

            rect.localScale = Vector3.Lerp(
                bigScale,
                originalScale,
                t
            );

            rect.localRotation = Quaternion.Lerp(
                bigRotation,
                originalRotation,
                t
            );

            yield return null;
        }

        // 完全に戻す
        rect.localScale = originalScale;
        rect.localRotation = originalRotation;
    }

    // ========================================
    // 透明度で点滅
    // ========================================
    private IEnumerator BlinkShopButton()
    {
        while (!isClicked)
        {
            // ========================================
            // 透明 → 不透明
            // ========================================
            float time = 0f;

            while (time < 1f && !isClicked)
            {
                time += Time.unscaledDeltaTime * fadeSpeed;

                Color color = shopImage.color;
                color.a = Mathf.Lerp(0f, 1f, time);
                shopImage.color = color;

                yield return null;
            }

            // ========================================
            // 不透明 → 透明
            // ========================================
            time = 0f;

            while (time < 1f && !isClicked)
            {
                time += Time.unscaledDeltaTime * fadeSpeed;

                Color color = shopImage.color;
                color.a = Mathf.Lerp(1f, 0f, time);
                shopImage.color = color;

                yield return null;
            }
        }

        // クリック後は完全表示
        Color finalColor = shopImage.color;
        finalColor.a = 1f;
        shopImage.color = finalColor;
    }

    // ========================================
    // ショップボタンをクリック
    // ========================================
    public void OnShopButtonClick()
    {
        // 一度クリックしたらずっとtrue
        isClicked = true;

        // PlayerStatsにも保存
        playerStats.shopButtonClicked = true;

        Debug.Log("ショップボタン押した → isClicked = true");

        // 点滅停止
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        // ポンッ演出などで大きくなっていた場合に戻す
        RectTransform rect = shopButton.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        // 完全表示
        Color color = shopImage.color;
        color.a = 1f;
        shopImage.color = color;
    }
}
