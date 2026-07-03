using UnityEngine;
using UnityEngine.EventSystems;

public class MenuItem : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private MenuSelector menuSelector;
    [SerializeField] private int index;

    public void OnPointerEnter(PointerEventData eventData)
    {
        menuSelector.SetIndex(index);
    }
}