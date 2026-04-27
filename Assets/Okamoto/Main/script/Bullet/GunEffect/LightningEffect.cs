using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningEffect : MonoBehaviour
{
    private LineRenderer line;

    public float duration = 0.1f;

    [Header("形状")]
    public int segments = 12;          // 分割数（多いほど細かい）
    public float amplitude = 0.5f;    // ぶれ幅

    [Header("アニメーション")]
    public float flickerSpeed = 0.02f; // 更新間隔

    private float timer;

    private Vector3 startPos;
    private Vector3 endPos;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void Setup(Vector3 start, Vector3 end)
    {
        startPos = start;
        endPos = end;

        line.positionCount = segments;

        InvokeRepeating(nameof(UpdateLightning), 0f, flickerSpeed);
        Destroy(gameObject, duration);
    }

    void UpdateLightning()
    {
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);

            // 真ん中ほど大きくぶれる
            float offsetStrength = Mathf.Sin(t * Mathf.PI);

            Vector2 randomOffset = Random.insideUnitCircle * amplitude * offsetStrength;

            line.SetPosition(i, pos + (Vector3)randomOffset);
        }
    }
}