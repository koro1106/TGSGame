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

    //　大きくなる速さ
    [SerializeField] private float scaleSpeed = 8f;

    // ぷかぷかの大きさ
    [SerializeField] private float floatAmount = 5f;

    // ぷかぷかの速さ
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

        // 初期状態
        buttonImage.color = Color.black;
        buttonText.color = Color.white;
        arrow.SetActive(false);
    }
    private void Update()
    {
        // 徐々に大きくする / 元に戻す
        Vector3 targetScale = isHover
            ? defaultScale * 1.1f
            : defaultScale;

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * scaleSpeed
        );
        if (isHover)
        {
            // 上下にぷかぷか
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

        // 色変更
        buttonImage.color = Color.white;
        buttonText.color = Color.black;

        // 少し大きく
        //transform.localScale = defaultScale * 1.1f;

        // 少し傾ける
        transform.rotation = Quaternion.Euler(0, 0, -5);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;

        arrow.SetActive(false);

        // 元に戻す
        buttonImage.color = Color.black;
        buttonText.color = Color.white;

        //transform.localScale = defaultScale;
        transform.rotation = defaultRotation;
    }
}