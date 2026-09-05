using System.Collections;
using UnityEngine;

/// <summary>
/// UIの演出専用スクリプト
/// ・スケール（拡大縮小）
/// ・回転（傾き）
/// を使ったバウンド演出
/// </summary>
public class UIAnimation : MonoBehaviour
{
    [Header("アニメーション速度")]
    public float speed = 1.5f;

    // バウンドアニメーション再生
    public void PlayBounce(RectTransform target)
    {
        StartCoroutine(Bounce(target));
    }

    IEnumerator Bounce(RectTransform rt)
    {
        // 元のサイズ
        Vector3 original = Vector3.one * 0.8f;

        // 押した瞬間
        Vector3 press = new Vector3(1.08f, 0.72f, 1f) * 0.8f;

        // ポンッと大きく
        Vector3 big = new Vector3(0.92f, 1.18f, 1f) * 0.8f;

        // 少しだけオーバー
        Vector3 overshoot = new Vector3(1.04f, 0.96f, 1f) * 0.8f;

        // 回転
        Quaternion rot0 = Quaternion.identity;
        Quaternion rot1 = Quaternion.Euler(0, 0, 4f);
        Quaternion rot2 = Quaternion.Euler(0, 0, -3f);
        Quaternion rot3 = Quaternion.Euler(0, 0, 1f);

        float t;

        // ========================================
        // ① 押した瞬間
        // ギュッと潰れる
        // ========================================

        t = 0f;

        while (t < 0.06f)
        {
            t += Time.unscaledDeltaTime * speed;

            float p = Mathf.Clamp01(t / 0.06f);
            p = EaseOut(p);

            rt.localScale = Vector3.Lerp(
                original,
                press,
                p
            );

            rt.localRotation = Quaternion.Lerp(
                rot0,
                rot1,
                p
            );

            yield return null;
        }

        // ========================================
        // ② ポンッ！
        // 一気に跳ねる
        // ========================================

        t = 0f;

        while (t < 0.11f)
        {
            t += Time.unscaledDeltaTime * speed;

            float p = Mathf.Clamp01(t / 0.11f);

            // 最初速く、最後ゆっくり
            p = EaseOut(p);

            rt.localScale = Vector3.Lerp(
                press,
                big,
                p
            );

            rt.localRotation = Quaternion.Lerp(
                rot1,
                rot2,
                p
            );

            yield return null;
        }

        // ========================================
        // ③ 少しだけオーバー
        // ========================================

        t = 0f;

        while (t < 0.09f)
        {
            t += Time.unscaledDeltaTime * speed;

            float p = Mathf.Clamp01(t / 0.09f);
            p = EaseOut(p);

            rt.localScale = Vector3.Lerp(
                big,
                overshoot,
                p
            );

            rt.localRotation = Quaternion.Lerp(
                rot2,
                rot3,
                p
            );

            yield return null;
        }

        // ========================================
        // ④ 元のサイズへ
        // ========================================

        t = 0f;

        while (t < 0.12f)
        {
            t += Time.unscaledDeltaTime * speed;

            float p = Mathf.Clamp01(t / 0.12f);
            p = EaseOut(p);

            rt.localScale = Vector3.Lerp(
                overshoot,
                original,
                p
            );

            rt.localRotation = Quaternion.Lerp(
                rot3,
                rot0,
                p
            );

            yield return null;
        }

        // 念のため完全に戻す
        rt.localScale = original;
        rt.localRotation = rot0;
    }

    /// <summary>
    /// 最初速く、最後ゆっくり
    /// </summary>
    float EaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}