using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PrestigeManager : MonoBehaviour
{
    [SerializeField] PlayerStats playerStats;
    [SerializeField] GameObject prestigeButton;
    [Header("点滅設定")]
    [SerializeField] private float fadeSpeed = 2f;

    [Header("表示時のポンッ演出")]
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float popDuration = 0.12f;

    [SerializeField] private Image prestigeImage;

    // プレステージボタンが解放されたか
    private bool prestigeOpened = false;

    // 点滅Coroutine
    private Coroutine blinkCoroutine;

    void Start()
    {
        // PlayerStatsからクリック状態を取得
        bool isClicked = playerStats.prestigeButtonClicked;

        // ========================================
        // すでに一度クリック済み
        // ========================================
        if (isClicked)
        {
            // ボタン表示
            prestigeButton.SetActive(true);

            // ポンッ演出なし
            // 点滅なし

            // 通常サイズ
            RectTransform rect =
                prestigeButton.GetComponent<RectTransform>();

            rect.localScale = Vector3.one * 1.2f;
            rect.localRotation = Quaternion.identity;

            // 完全表示
            if (prestigeImage != null)
            {
                Color color = prestigeImage.color;
                color.a = 1f;
                prestigeImage.color = color;
            }

            prestigeOpened = true;

            return;
        }

        // ========================================
        // まだクリックしていない
        // ========================================

        // 最初は透明
        if (prestigeImage != null)
        {
            Color color = prestigeImage.color;
            color.a = 0f;
            prestigeImage.color = color;
        }

        // ボタン自体は非表示
        prestigeButton.SetActive(false);
    }

    void Update()
    {
        // すでにクリック済みなら何もしない
        if (playerStats.prestigeButtonClicked)
            return;

        // プレステージ解放
        if (playerStats.preExpDeviceUnlocked && !prestigeOpened)
        {
            prestigeOpened = true;

            // ボタン表示
            prestigeButton.SetActive(true);

            // ポンッ演出
            StartCoroutine(PopIn());

            // 点滅開始
            blinkCoroutine = StartCoroutine(BlinkPrestigeButton());
        }
    }

    // ========================================
    // ポンッと大きくして少し傾ける
    // ========================================
    private IEnumerator PopIn()
    {
        RectTransform rect =
            prestigeButton.GetComponent<RectTransform>();

        // 元のサイズ・角度
        Vector3 originalScale = Vector3.one * 1.2f;
        Quaternion originalRotation = Quaternion.identity;

        // 少し大きく
        Vector3 bigScale = originalScale * popScale;

        // 少し傾ける
        Quaternion bigRotation =
            Quaternion.Euler(0f, 0f, -5f);

        float duration = popDuration;

        float time = 0f;

        // ========================================
        // ① ポンッと大きく＋傾く
        // ========================================
        while (time < duration)
        {
            if (playerStats.prestigeButtonClicked)
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
            if (playerStats.prestigeButtonClicked)
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
    private IEnumerator BlinkPrestigeButton()
    {
        while (!playerStats.prestigeButtonClicked)
        {
            // ========================================
            // 透明 → 不透明
            // ========================================
            float time = 0f;

            while (time < 1f &&
                   !playerStats.prestigeButtonClicked)
            {
                time += Time.unscaledDeltaTime * fadeSpeed;

                if (prestigeImage != null)
                {
                    Color color = prestigeImage.color;
                    color.a = Mathf.Lerp(0f, 1f, time);
                    prestigeImage.color = color;
                }

                yield return null;
            }

            // ========================================
            // 不透明 → 透明
            // ========================================
            time = 0f;

            while (time < 1f &&
                   !playerStats.prestigeButtonClicked)
            {
                time += Time.unscaledDeltaTime * fadeSpeed;

                if (prestigeImage != null)
                {
                    Color color = prestigeImage.color;
                    color.a = Mathf.Lerp(1f, 0f, time);
                    prestigeImage.color = color;
                }

                yield return null;
            }
        }

        // クリック後は完全表示
        if (prestigeImage != null)
        {
            Color finalColor = prestigeImage.color;
            finalColor.a = 1f;
            prestigeImage.color = finalColor;
        }
    }

    // ========================================
    // プレステージボタンをクリック
    // ========================================
    public void OnPrestigeButtonClick()
    {
        // 一度クリックしたらずっとtrue
        playerStats.prestigeButtonClicked = true;

        Debug.Log("プレステージボタン押した → prestigeButtonClicked = true");

        // すべてのCoroutineを停止
        StopAllCoroutines();

        blinkCoroutine = null;

        // ========================================
        // サイズ・角度を通常状態に戻す
        // ========================================
        RectTransform rect =
            prestigeButton.GetComponent<RectTransform>();

        rect.localScale = Vector3.one * 1.2f;
        rect.localRotation = Quaternion.identity;

        // ========================================
        // 完全表示
        // ========================================
        if (prestigeImage != null)
        {
            Color color = prestigeImage.color;
            color.a = 1f;
            prestigeImage.color = color;
        }
    }
}
