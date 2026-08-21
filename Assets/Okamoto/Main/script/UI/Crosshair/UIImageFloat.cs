using UnityEngine;

public class UIImageFloat : MonoBehaviour
{
    [Header("è„â∫Ç…ìÆÇ≠ïù")]
    public float moveAmount = 20f;

    [Header("ìÆÇ≠ë¨Ç≥")]
    public float moveSpeed = 1f;

    private RectTransform rectTransform;
    private Vector2 startPosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * moveSpeed) * moveAmount;

        rectTransform.anchoredPosition =
            startPosition + new Vector2(0f, y);
    }
}