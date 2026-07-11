using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// UIボタンにマウスカーソルが乗ったときに少し拡大
/// </summary>
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 元の大きさを保存
    private Vector3 originalScale;

    // カーソルが乗ったときの拡大倍率
    public float hoverScale = 1.1f;

    // 拡大・縮小する速さ
    public float speed = 10f;

    // 現在目標としている大きさ
    private Vector3 targetScale;

    void Start()
    {
        // 初期サイズを保存
        originalScale = transform.localScale;

        // 最初の目標サイズは元のサイズ
        targetScale = originalScale;
    }

    void Update()
    {
        // 現在のサイズから目標サイズへ滑らかに変化させる
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * speed
        );
    }

    // マウスカーソルがボタンに乗ったとき
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("a");

        // 少し大きくする
        targetScale = originalScale * hoverScale;
    }

    // マウスカーソルがボタンから離れたとき
    public void OnPointerExit(PointerEventData eventData)
    {
        // 元の大きさに戻す
        targetScale = originalScale;
    }
}
