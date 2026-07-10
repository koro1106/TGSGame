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
    public float speed = 1.5f; // 大きいほど速い
    // バウンドアニメーション再生
    public void PlayBounce(RectTransform target)
    {
        StartCoroutine(Bounce(target)); // コルーチン開始
    }

    IEnumerator Bounce(RectTransform rt)
    {
        Vector3 original = Vector3.one;    // 元のサイズ

        // 押した瞬間（横に少し広がって縦が潰れる）
        Vector3 small = new Vector3(1.08f, 0.2f, 1f);

        // 膨らむ（縦に少し伸びる）
        Vector3 big = new Vector3(0.95f, 1.5f, 1f);

        // 落ち着く
        Vector3 settle = new Vector3(1.02f, 0.98f, 1f);

        // 回転（角度）
        Quaternion rot0 = Quaternion.identity;         // 元
        Quaternion rot1 = Quaternion.Euler(0, 0, 5f); // 右に傾く
        Quaternion rot2 = Quaternion.Euler(0, 0, -5f); // 左に傾く

        float t = 0;

        // ①一瞬小さく＋右に傾く
        while(t < 0.05f)
        {
            t += Time.unscaledDeltaTime * speed; // timeScale無視(停止中でも動く)
            //float p = t / 0.05f;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.05f);

            rt.localScale = Vector3.Lerp(original, small, p);
            rt.rotation = Quaternion.Lerp(rot0, rot1, p);

            yield return null;
        }
        
        t = 0;

        // ②一気に大きく＋反対に傾く
        while(t < 0.1f)
        {
            t += Time.unscaledDeltaTime * speed;
            float p = t / 0.1f;

            rt.localScale = Vector3.Lerp(small, big, p);
            rt.rotation = Quaternion.Lerp(rot1, rot2, p);

            yield return null;
        }

        // ③少し縮む
        t = 0;
        while (t < 0.08f)
        {
            t += Time.unscaledDeltaTime * speed;
            float p = Mathf.SmoothStep(0, 1, t / 0.08f);

            rt.localScale = Vector3.Lerp(big, settle, p);
            yield return null;
        }

        // ④元に戻る
        t = 0;
        while (t < 0.12f)
        {
            //t += Time.unscaledDeltaTime * speed;
            //float p = t / 0.12f;

            //rt.localScale = Vector3.Lerp(big, original, p);
            //rt.rotation = Quaternion.Lerp(rot2, rot0, p);

            //yield return null;

            t += Time.unscaledDeltaTime * speed;
            float p = Mathf.SmoothStep(0, 1, t / 0.08f);

            rt.localScale = Vector3.Lerp(settle, original, p);
            yield return null;
        }

        // 念のため完全に元に戻す
        rt.localScale = original;
        rt.rotation = rot0;
    }
}
