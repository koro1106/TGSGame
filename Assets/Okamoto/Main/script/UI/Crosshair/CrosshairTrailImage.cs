using UnityEngine;
using UnityEngine.UI;

public class CrosshairTrailImage : MonoBehaviour
{
    [Header("落下速度")]
    public float fallSpeed = 30f;

    [Header("消えるまでの時間")]
    public float lifeTime = 1f;

    [Header("最初の大きさ")]
    public float startScale = 0.5f;

    private Image image;
    private RectTransform rect;

    private Color startColor;
    private float timer;

    void Start()
    {
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();

        if (image == null)
        {
            Destroy(gameObject);
            return;
        }

        startColor = image.color;

        rect.localScale =
            Vector3.one * startScale;
    }

    void Update()
    {
        timer += Time.deltaTime;

        float t =
            Mathf.Clamp01(timer / lifeTime);

        // ==================================
        // ★ 画面の真下に落とす
        // ==================================
        Vector2 pos = rect.anchoredPosition;

        pos.y -= fallSpeed * Time.deltaTime;

        rect.anchoredPosition = pos;

        // ==================================
        // 徐々に透明
        // ==================================
        Color color = startColor;

        color.a = Mathf.Lerp(
            startColor.a,
            0f,
            t
        );

        image.color = color;

        // ==================================
        // 徐々に小さくする
        // ==================================
        float scale =
            Mathf.Lerp(
                startScale,
                0f,
                t
            );

        rect.localScale =
            Vector3.one * scale;

        // ==================================
        // 消滅
        // ==================================
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}