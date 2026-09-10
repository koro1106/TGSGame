using UnityEngine;

public class UIImageUpDown : MonoBehaviour
{
    [Header("上下に動かす子Image")]
    public RectTransform targetImage;

    [Header("上下の移動幅")]
    public float moveAmount = 20f;

    [Header("移動速度")]
    public float moveSpeed = 2f;

    private Vector2 startPosition;

    private void Start()
    {
        // 子Imageの開始位置を取得
        if (targetImage != null)
        {
            startPosition = targetImage.anchoredPosition;
        }
    }

    private void Update()
    {
        // 子Imageが設定されていなければ処理しない
        if (targetImage == null)
            return;

        // Time.timeScale = 0でも動くようにする
        float y = Mathf.Sin(Time.unscaledTime * moveSpeed) * moveAmount;

        // 元の位置を基準に上下移動
        targetImage.anchoredPosition =
            startPosition + new Vector2(0f, y);
    }
}