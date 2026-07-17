using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ImageHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Imageに追加したOutlineコンポーネント
    [SerializeField] private Outline outline;

    // ImageのRectTransform
    private RectTransform rectTransform;

    // 元の大きさを保存しておく
    private Vector3 originalScale;

    // ホバー時の拡大率（1.1 = 110%）
    [SerializeField] private float hoverScale = 1.1f;

    private void Awake()
    {
        // RectTransformを取得
        rectTransform = GetComponent<RectTransform>();

        // 元のサイズを保存
        originalScale = rectTransform.localScale;

        // 最初は枠線を非表示にする
        outline.enabled = false;
    }

    // マウスカーソルがImageに乗ったとき
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 黄色い枠線を表示
        outline.enabled = true;

        // Imageを少し大きくする
        rectTransform.localScale = originalScale * hoverScale;
    }

    // マウスカーソルがImageから離れたとき
    public void OnPointerExit(PointerEventData eventData)
    {
        // 枠線を非表示にする
        outline.enabled = false;

        // 元のサイズに戻す
        rectTransform.localScale = originalScale;
    }
}
