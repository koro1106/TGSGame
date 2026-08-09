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

    // Ç’Ç©Ç’Ç©ÇÃëÂÇ´Ç≥
    [SerializeField] private float floatAmount = 5f;

    // Ç’Ç©Ç’Ç©ÇÃë¨Ç≥
    [SerializeField] private float floatSpeed = 3f;

    private Vector3 defaultScale;
    private Quaternion defaultRotation;
    private Vector3 defaultPosition;

    private bool isHover;

    void Start()
    {
        defaultPosition = transform.localPosition;
        defaultScale = transform.localScale;
        defaultRotation = transform.rotation;

        // èâä˙èÛë‘
        buttonImage.color = Color.black;
        buttonText.color = Color.white;
        arrow.SetActive(false);
    }
    private void Update()
    {
        if (isHover)
        {
            // è„â∫Ç…Ç’Ç©Ç’Ç©
            float y = Mathf.Sin(Time.time * floatSpeed) * floatAmount;

            transform.localPosition =
                defaultPosition + new Vector3(0, y, 0);
        }
        else
        {
            transform.localPosition = defaultPosition;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;
        
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
        isHover = false;

        arrow.SetActive(false);

        // å≥Ç…ñﬂÇ∑
        buttonImage.color = Color.black;
        buttonText.color = Color.white;

        transform.localScale = defaultScale;
        transform.rotation = defaultRotation;
    }
}