using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public GameObject arrow;
    public Image buttonImage;
    public TMP_Text buttonText;

    private Vector3 defaultScale;
    private Quaternion defaultRotation;

    void Start()
    {
        defaultScale = transform.localScale;
        defaultRotation = transform.rotation;

        // èâä˙èÛë‘
        buttonImage.color = Color.black;
        buttonText.color = Color.white;
        arrow.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        arrow.SetActive(true);

        // êFïœçX
        buttonImage.color = Color.white;
        buttonText.color = Color.black;

        // è≠ÇµëÂÇ´Ç≠
        transform.localScale = defaultScale * 1.1f;

        // è≠ÇµåXÇØÇÈ
        transform.rotation = Quaternion.Euler(0, 0, -5);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        arrow.SetActive(false);

        // å≥Ç…ñﬂÇ∑
        buttonImage.color = Color.black;
        buttonText.color = Color.white;

        transform.localScale = defaultScale;
        transform.rotation = defaultRotation;
    }
}