using UnityEngine;

public class CrosshairTrail : MonoBehaviour
{
    [Header("クロスヘア")]
    public RectTransform crosshair;

    [Header("残像ImageのPrefab")]
    public GameObject trailPrefab;

    [Header("残像を入れるCanvas")]
    public Canvas canvas;

    [Header("生成間隔")]
    public float spawnInterval = 0.08f;

    private Vector2 lastPosition;
    private float timer;

    void Start()
    {
        if (crosshair == null)
            crosshair = GetComponent<RectTransform>();

        lastPosition = crosshair.position;
    }

    void Update()
    {
        if (crosshair == null || trailPrefab == null || canvas == null)
            return;

        timer += Time.deltaTime;

        Vector2 currentPosition = crosshair.position;

        if (Vector2.Distance(currentPosition, lastPosition) > 1f)
        {
            if (timer >= spawnInterval)
            {
                CreateTrail(lastPosition);

                timer = 0f;
            }
        }

        lastPosition = currentPosition;
    }

    void CreateTrail(Vector2 screenPosition)
    {
        // Canvasの子として生成
        GameObject trail = Instantiate(
            trailPrefab,
            canvas.transform
        );

        RectTransform trailRect =
            trail.GetComponent<RectTransform>();

        // Canvas上のローカル座標に変換
        Vector2 localPosition;

        RectTransform canvasRect =
            canvas.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera,
            out localPosition
        );

        // クロスヘアが通った場所
        trailRect.anchoredPosition = localPosition;

        // 回転を完全にリセット
        trailRect.localRotation = Quaternion.identity;

        // Prefabの初期状態はそのまま
        trailRect.localScale = Vector3.one;
    }
}