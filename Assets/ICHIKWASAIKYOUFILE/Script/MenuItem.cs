using UnityEngine;
using UnityEngine.EventSystems;

public class MenuItem : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private MenuSelector menuSelector;
    [SerializeField] private int index;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("マウスが入りました Index=" + index);

        // マウスを乗せた項目へ移動
        menuSelector.SetIndex(index);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // クリックした項目を実行
        menuSelector.Select();
    }
}