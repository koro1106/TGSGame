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

    [Header("Impact Stretch")]
    public float stretchSpeed = 10f;
    public float maxStretchLength = 1.5f;

    [Header("Random Direction")]
    public bool useRandomDirectionIfSingle = true;

    private GameObject effectInstance;

    private LineRenderer lr;

    private Vector3 startPos;
    private Vector3 endPos;

    private float timer;
    private float nextFlicker;

    // 着弾エフェクト用
    private Vector3 impactDir;
    private float currentStretch;

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

    // hasNextEnemy を追加
    public void Setup(
        Vector3 start,
        Vector3 end,
        bool hasNextEnemy = true
    )
    {
        startPos = start;
        endPos = end;

        timer = 0f;
        nextFlicker = 0f;

        currentStretch = 0f;

        lr.positionCount = segments;

        // =========================
        // 次の敵がいる場合
        // =========================
        if (hasNextEnemy)
        {
            impactDir =
                (endPos - startPos).normalized;
        }
        else
        {
            // =========================
            // 最後の敵ならランダム方向
            // =========================

            if (useRandomDirectionIfSingle)
            {
                float randomAngle =
                    Random.Range(0f, 360f);

                impactDir =
                    new Vector3(
                        Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                        Mathf.Sin(randomAngle * Mathf.Deg2Rad),
                        0f
                    ).normalized;
            }
            else
            {
                impactDir =
                    (endPos - startPos).normalized;
            }
        }

        // 着弾エフェクト生成
        if (effectPrefab != null)
        {
            effectInstance = Instantiate(
                effectPrefab,
                endPos,
                Quaternion.identity
            );

            // 敵方向へ向ける
            effectInstance.transform.right =
                impactDir;

            // 最初は長さ0
            effectInstance.transform.localScale =
                new Vector3(0f, 1f, 1f);

            // 根元固定
            effectInstance.transform.position =
                endPos;
        }

        UpdateLine();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 消滅
        if (timer >= duration)
        {
            if (effectInstance != null)
            {
                Destroy(effectInstance);
            }

            Destroy(gameObject);
            return;
        }

        // 雷更新
        if (Time.time >= nextFlicker)
        {
            UpdateLine();

            nextFlicker =
                Time.time + flickerSpeed;
        }

        // 着弾エフェクト
        if (effectInstance != null)
        {
            currentStretch +=
                stretchSpeed * Time.deltaTime;

            currentStretch =
                Mathf.Min(
                    currentStretch,
                    maxStretchLength
                );

            // 長さ
            effectInstance.transform.localScale =
                new Vector3(
                    currentStretch,
                    1f,
                    1f
                );

            // 半分前へ
            effectInstance.transform.position =
                endPos +
                impactDir *
                (currentStretch * 0.5f);

            // 向き固定
            effectInstance.transform.right =
                impactDir;
        }

        // 細くする
        float t = timer / duration;

        float width = Mathf.Lerp(
            startWidth,
            endWidth,
            t
        );

        lr.startWidth = width;
        lr.endWidth = width;

        // フェードアウト
        Color c = lr.startColor;

        c.a = 1f - t;

        lr.startColor = c;
        lr.endColor = c;
    }

    void UpdateLine()
    {
        if (lr == null) return;

        Vector3 dir =
            (endPos - startPos).normalized;

        Vector3 perpendicular =
            new Vector3(-dir.y, dir.x, 0);

        for (int i = 0; i < segments; i++)
        {
            float t =
                i / (float)(segments - 1);

            Vector3 basePos =
                Vector3.Lerp(
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