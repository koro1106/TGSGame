using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningLineEffect : MonoBehaviour
{
    [Header("Lightning")]
    public int segments = 12;
    public float amplitude = 0.15f;
    public float duration = 0.08f;
    public float flickerSpeed = 0.015f;

    [Header("Width")]
    public float startWidth = 0.12f;
    public float endWidth = 0.03f;

    [Header("Hit Effect")]
    public GameObject effectPrefab;

    private GameObject effectInstance;

    private LineRenderer lr;
    private Vector3 startPos;
    private Vector3 endPos;

    private float timer;
    private float nextFlicker;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();

        if (lr == null)
        {
            lr = GetComponentInChildren<LineRenderer>();
        }

        if (lr == null)
        {
            Debug.LogError("LineRendererが見つかりません");
        }
    }

    public void Setup(Vector3 start, Vector3 end)
    {
        startPos = start;
        endPos = end;

        timer = 0f;
        nextFlicker = 0f;

        lr.positionCount = segments;

        // 接触地点にエフェクト生成
        if (effectPrefab != null)
        {
            Vector3 dir = endPos - startPos;

            float angle =
                Mathf.Atan2(dir.y, dir.x) *
                Mathf.Rad2Deg;

            effectInstance = Instantiate(
                effectPrefab,
                endPos,
                Quaternion.Euler(0f, 0f, angle)
            );
        }

        UpdateLine();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= duration)
        {
            if (effectInstance != null)
            {
                Destroy(effectInstance);
            }

            Destroy(gameObject);
            return;
        }

        if (Time.time >= nextFlicker)
        {
            UpdateLine();
            nextFlicker = Time.time + flickerSpeed;
        }

        float t = timer / duration;

        float width = Mathf.Lerp(
            startWidth,
            endWidth,
            t
        );

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

        Vector3 dir = (endPos - startPos).normalized;
        Vector3 perpendicular = new Vector3(-dir.y, dir.x, 0);

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);

            Vector3 basePos = Vector3.Lerp(
                startPos,
                endPos,
                t
            );

            float strength =
                Mathf.Sin(t * Mathf.PI);

            float offset =
                Random.Range(
                    -amplitude,
                    amplitude
                ) * strength;

            Vector3 pos =
                basePos +
                perpendicular * offset;

            lr.SetPosition(i, pos);
        }
    }

    void OnDestroy()
    {
        if (effectInstance != null)
        {
            Destroy(effectInstance);
        }
    }
}