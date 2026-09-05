using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneEnterAnimation : MonoBehaviour
{
    [Header("右から左に登場するオブジェクト")]
    [SerializeField] private RectTransform[] slideObjects;

    [Header("その中でポンポン表示する5個")]
    [SerializeField] private RectTransform[] popObjects;

    [Header("スライド設定")]
    [SerializeField] private float slideDistance = 300f;
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private float slideInterval = 0.08f;

    [Header("ポップ設定")]
    [SerializeField] private float popDuration = 0.2f;
    [SerializeField] private float popInterval = 0.12f;

    private void Start()
    {
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        // =========================
        // 初期化
        // =========================

        Vector2[] originalPositions =
            new Vector2[slideObjects.Length];

        // それぞれの本来のサイズを保存
        Vector3[] originalScales =
            new Vector3[popObjects.Length];

        // -------------------------
        // スライド対象の初期化
        // -------------------------

        for (int i = 0; i < slideObjects.Length; i++)
        {
            if (slideObjects[i] == null)
                continue;

            originalPositions[i] =
                slideObjects[i].anchoredPosition;

            // 右側へ移動
            slideObjects[i].anchoredPosition =
                originalPositions[i] +
                Vector2.right * slideDistance;

            // 透明にする
            SetAlpha(slideObjects[i], 0f);
        }

        // -------------------------
        // ポップ対象の初期化
        // -------------------------

        for (int i = 0; i < popObjects.Length; i++)
        {
            if (popObjects[i] == null)
                continue;

            // ★ 元々のサイズを保存
            originalScales[i] =
                popObjects[i].localScale;

            // 最初は見えない
            popObjects[i].localScale =
                Vector3.zero;
        }

        // =========================
        // ① 右 → 左 ＋ フェードイン
        // =========================

        for (int i = 0; i < slideObjects.Length; i++)
        {
            if (slideObjects[i] == null)
                continue;

            StartCoroutine(
                SlideIn(
                    slideObjects[i],
                    originalPositions[i]
                )
            );

            yield return new WaitForSecondsRealtime(
                slideInterval
            );
        }

        // =========================
        // ② スライド中からポンポン開始
        // =========================

        yield return new WaitForSecondsRealtime(0.1f);

        for (int i = 0; i < popObjects.Length; i++)
        {
            if (popObjects[i] == null)
                continue;

            // ★ 元のサイズを渡す
            StartCoroutine(
                PopIn(
                    popObjects[i],
                    originalScales[i]
                )
            );

            yield return new WaitForSecondsRealtime(
                popInterval
            );
        }
    }

    // ========================================
    // 右から左へ移動＋フェードイン
    // ========================================

    private IEnumerator SlideIn(
        RectTransform target,
        Vector2 targetPosition)
    {
        Vector2 startPosition =
            target.anchoredPosition;

        float time = 0f;

        while (time < slideDuration)
        {
            time += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(time / slideDuration);

            // なめらかに減速
            float eased =
                1f - Mathf.Pow(1f - t, 3f);

            target.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    targetPosition,
                    eased
                );

            SetAlpha(target, eased);

            yield return null;
        }

        target.anchoredPosition =
            targetPosition;

        SetAlpha(target, 1f);
    }

    // ========================================
    // ポンッと表示
    // ========================================

    private IEnumerator PopIn(
        RectTransform target,
        Vector3 originalScale)
    {
        float time = 0f;

        // 最初は0
        Vector3 startScale =
            Vector3.zero;

        // ★ 元のサイズの115%
        Vector3 bigScale =
            originalScale * 1.15f;

        // ★ 元のサイズに戻す
        Vector3 normalScale =
            originalScale;

        while (time < popDuration)
        {
            time += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(time / popDuration);

            if (t < 0.7f)
            {
                // 0 → 115%
                float p =
                    t / 0.7f;

                p =
                    1f - Mathf.Pow(
                        1f - p,
                        3f
                    );

                target.localScale =
                    Vector3.Lerp(
                        startScale,
                        bigScale,
                        p
                    );
            }
            else
            {
                // 115% → 元のサイズ
                float p =
                    (t - 0.7f) / 0.3f;

                p =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        p
                    );

                target.localScale =
                    Vector3.Lerp(
                        bigScale,
                        normalScale,
                        p
                    );
            }

            yield return null;
        }

        // ★ 最後は必ず本来のサイズ
        target.localScale =
            originalScale;
    }

    // ========================================
    // UIの透明度変更
    // ========================================

    private void SetAlpha(
        RectTransform target,
        float alpha)
    {
        Graphic[] graphics =
            target.GetComponentsInChildren<Graphic>(true);

        foreach (Graphic graphic in graphics)
        {
            Color color =
                graphic.color;

            color.a = alpha;

            graphic.color = color;
        }
    }
}