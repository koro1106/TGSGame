using UnityEngine;
using System.Collections.Generic;

public class LightningDotEffect : MonoBehaviour
{
    public GameObject dotPrefab;

    public int segments = 12;
    public float amplitude = 0.5f;
    public float duration = 0.1f;
    public float flickerSpeed = 0.02f;

    private Vector3 startPos;
    private Vector3 endPos;

    private List<GameObject> dots = new List<GameObject>();

    public void Setup(Vector3 start, Vector3 end)
    {
        if (dotPrefab == null)
        {
            Debug.LogError("dotPrefabが未設定");
            return;
        }

        startPos = start;
        endPos = end;

        // ★向きを着弾方向に合わせる
        Vector3 dir = (endPos - startPos).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // ドット生成
        for (int i = 0; i < segments; i++)
        {
            GameObject dot = Instantiate(dotPrefab, transform);
            dots.Add(dot);
        }

        InvokeRepeating(nameof(UpdateDots), 0f, flickerSpeed);
        Destroy(gameObject, duration);
    }

    void UpdateDots()
    {
        if (dots.Count == 0) return;

        // ★進行方向
        Vector3 dir = (endPos - startPos).normalized;

        // ★横方向（これが超重要）
        Vector3 perpendicular = new Vector3(-dir.y, dir.x, 0);

        for (int i = 0; i < dots.Count; i++)
        {
            float t = i / (float)(segments - 1);

            // 直線上の位置
            Vector3 basePos = Vector3.Lerp(startPos, endPos, t);

            // 真ん中ほど強くブレる
            float strength = Mathf.Sin(t * Mathf.PI);

            // ★横方向だけにブレる
            float offset = Random.Range(-amplitude, amplitude) * strength;

            Vector3 finalPos = basePos + perpendicular * offset;

            dots[i].transform.position = finalPos;

            // ★ドットの向きも揃える（任意だけどおすすめ）
            dots[i].transform.rotation = transform.rotation;

            // ★太さ変化（雷っぽさUP）
            float scale = Mathf.Lerp(0.5f, 1.5f, strength);
            dots[i].transform.localScale = Vector3.one * scale;
        }
    }
}