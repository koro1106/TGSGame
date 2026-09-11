
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScale : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("カーソルを合わせた時の倍率")]
    public float hoverScale = 1.1f;

    [Header("押している間の倍率")]
    public float pressedScale = 1.2f;

    [Header("拡大・縮小速度")]
    public float scaleSpeed = 10f;

    // 元のサイズ
    private Vector3 normalScale;

    // 現在の目標サイズ
    private Vector3 targetScale;

    // カーソルがボタンの上にあるか
    private bool isHover = false;

    // ボタンを押しているか
    private bool isPressed = false;

    private void Start()
    {
        // 元のサイズを保存
        normalScale = transform.localScale;

        // 最初は通常サイズ
        targetScale = normalScale;
    }

    private void Update()
    {
        // 徐々に目標サイズへ変更
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * scaleSpeed
        );
    }

    // カーソルがボタンに入った時
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;

        // 押していなければホバーサイズ
        if (!isPressed)
        {
            targetScale = normalScale * hoverScale;
        }
    }

    // カーソルがボタンから出た時
    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;

        // 押していなければ通常サイズ
        if (!isPressed)
        {
            targetScale = normalScale;
        }
    }

    // ボタンを押した瞬間
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;

        // 押している間はさらに大きくする
        targetScale = normalScale * pressedScale;
    }

    // ボタンを離した瞬間
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;

        // カーソルが上に残っていればホバーサイズ
        if (isHover)
        {
            targetScale = normalScale * hoverScale;
        }
        else
        {
            // カーソルが外に出ていれば通常サイズ
            targetScale = normalScale;
        }
    }
}