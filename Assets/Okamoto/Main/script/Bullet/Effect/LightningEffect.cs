using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningLineEffect : MonoBehaviour
{
    public int segments = 12;
    public float amplitude = 0.5f;
    public float duration = 0.08f;
    public float flickerSpeed = 0.015f;

    private LineRenderer lr;
    private Vector3 startPos;
    private Vector3 endPos;

    [Header("太さ")]
    public float startWidth = 0.6f;
    public float endWidth = 0.1f;

    private float timer;
    private float nextFlicker;


    void Awake()
    {
        lr = GetComponentInChildren<LineRenderer>();

        if (lr == null)
        {
            Debug.LogError("LineRendererが見つからない（子も含めて）");
        }
    }
    public void Setup(Vector3 start, Vector3 end)
    {
        lr = GetComponent<LineRenderer>();

        startPos = start;
        endPos = end;

        lr.positionCount = segments;
        nextFlicker = 0f;

        UpdateLine();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= duration)
        {
            Destroy(gameObject);
            return;
        }

        // フリッカー
        if (Time.time >= nextFlicker)
        {
            UpdateLine();
            nextFlicker = Time.time + flickerSpeed;
        }

        // 太さ＆フェード
        float t = timer / duration;
        float width = Mathf.Lerp(startWidth, endWidth, t);
        lr.startWidth = width;
        lr.endWidth = width;

        Color c = lr.startColor;
        c.a = 1f - t;
        lr.startColor = c;
        lr.endColor = c;
    }

    void UpdateLine()
    {
        if (lr == null) return;

        if (lr.positionCount != segments)
        {
            lr.positionCount = segments;
        }

        Vector3 dir = (endPos - startPos).normalized;
        Vector3 perpendicular = new Vector3(-dir.y, dir.x, 0);

        for (int i = 0; i < lr.positionCount; i++)
        {
            float t = i / (float)(lr.positionCount - 1);

            Vector3 basePos = Vector3.Lerp(startPos, endPos, t);

            float strength = Mathf.Sin(t * Mathf.PI);
            float offset = Random.Range(-amplitude, amplitude) * strength;

            Vector3 pos = basePos + perpendicular * offset;

            lr.SetPosition(i, pos);
        }
    }
}