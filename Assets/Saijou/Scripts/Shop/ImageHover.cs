using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ImageHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Imageに追加したOutlineコンポーネント
    [SerializeField] private Outline outline;

    [SerializeField] private SkillData skillData;

    // ImageのRectTransform
    private RectTransform rectTransform;

    // 元の大きさを保存しておく
    private Vector3 originalScale;

    // ホバー時の拡大率（1.1 = 110%）
    [SerializeField] private float hoverScale = 1.1f;

    [SerializeField] private float outlineSpeed = 2f; // アウトライン点滅スピード

    private Coroutine outlineCoroutine;

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

        if (outlineCoroutine != null)
            StopCoroutine(outlineCoroutine);

        outlineCoroutine = StartCoroutine(OutlinePulse());

        ShopTooltipUI.Instance.ShowText(skillData, false);
    }

    // マウスカーソルがImageから離れたとき
    public void OnPointerExit(PointerEventData eventData)
    {
        if (outlineCoroutine != null)
        {
            StopCoroutine(outlineCoroutine);
            outlineCoroutine = null;
        }

        // 枠線を非表示にする
        outline.enabled = false;

        // 元のサイズに戻す
        rectTransform.localScale = originalScale;

        ShopTooltipUI.Instance.Hide();
    }

    // アウトライン点滅
    private IEnumerator OutlinePulse()
    {
        Color color = outline.effectColor;

        while (true)
        {
            float alpha =
                Mathf.Lerp(0.1f,1f, Mathf.PingPong(Time.time * outlineSpeed, 1f));

            outline.effectColor = new Color(color.r,color.g,color.b,alpha);

            yield return null;
        }
    }
}
