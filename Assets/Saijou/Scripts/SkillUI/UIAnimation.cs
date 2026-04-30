using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// UIの演出専用スクリプト
/// ・スケール（拡大縮小）
/// ・回転（傾き）
/// を使ったバウンド演出
/// </summary>
public class UIAnimation : MonoBehaviour
{
    public float speed = 1.5f; // 大きいほど速い
    // バウンドアニメーション再生
    public void PlayBounce(RectTransform target)
    {
        StartCoroutine(Bounce(target)); // コルーチン開始
    }

    IEnumerator Bounce(RectTransform rt)
    {
        // 元のサイズ
        Vector3 original = Vector3.one;

        // 少し縮むサイズ
        Vector3 small = original * 0.9f;

        // 大きくなるサイズ
        Vector3 big = original * 1.2f;

        // 回転（角度）
        Quaternion rot0 = Quaternion.identity;         // 元
        Quaternion rot1 = Quaternion.Euler(0, 0, 10f); // 右に傾く
        Quaternion rot2 = Quaternion.Euler(0, 0, -8f); // 左に傾く

        float t = 0;

        // ①一瞬小さく＋右に傾く
        while(t < 0.08f)
        {
            t += Time.unscaledDeltaTime * speed; // timeScale無視(停止中でも動く)
            float p = t / 0.08f;

            rt.localScale = Vector3.Lerp(original, small, p);
            rt.rotation = Quaternion.Lerp(rot0, rot1, p);

            yield return null;
        }
        
        t = 0;

        // ②一気に大きく＋反対に傾く
        while(t < 0.12f)
        {
            t += Time.unscaledDeltaTime * speed;
            float p = t / 0.12f;

            rt.localScale = Vector3.Lerp(small, big, p);
            rt.rotation = Quaternion.Lerp(rot1, rot2, p);

            yield return null;
        }

        t = 0;

        // ③元に戻る
        while (t < 0.15f)
        {
            t += Time.unscaledDeltaTime * speed;
            float p = t / 0.15f;

            rt.localScale = Vector3.Lerp(big, original, p);
            rt.rotation = Quaternion.Lerp(rot2, rot0, p);

            yield return null;
        }

        // 念のため完全に元に戻す
        rt.localScale = original;
        rt.rotation = rot0;
    }
}
