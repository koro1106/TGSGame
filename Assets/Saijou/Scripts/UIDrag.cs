using UnityEngine;
/// <summary>
/// スキルツリードラッグ用
/// </summary>
public class UIDrag : MonoBehaviour
{
    public RectTransform target; // スキルツリーの親オブジェクト
    private Vector2 lastMousePos; // マウスがどのぐらい動いたかを出すための
    private bool isDragging = false; // ドラッグ中か

    void Update()
    {
        // 押した瞬間
        if(Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePos = Input.mousePosition;
        }

        // 離したら終了
        if(Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // ドラッグ中
        if(isDragging)
        {
            Vector2 currentMousePos = Input.mousePosition;
            Vector2 delta = currentMousePos - lastMousePos; // 差分

            // UI移動
            target.anchoredPosition += delta;

            lastMousePos = currentMousePos;
        }
    }
}
