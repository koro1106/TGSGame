using UnityEngine;
using TMPro;
using System.Collections;

public class ComboPopup : MonoBehaviour
{
    [Header("背景画像")]
    public RectTransform backgroundRect;

    [Header("コンボテキスト")]
    public TextMeshProUGUI comboNumber;
    public TextMeshProUGUI comboLabel;

    [Header("背景の位置設定")]
    public Vector2 bgPosLeft = new Vector2(0f, 85f);
    public Vector2 bgPosRight = new Vector2(0f, 85f);
    public Vector2 bgPosUp = new Vector2(0f, 0f);

    [Header("数字テキストの位置設定")]
    public Vector2 numberPosLeft = new Vector2(-98f, 82f);
    public Vector2 numberPosRight = new Vector2(98f, 82f);
    public Vector2 numberPosUp = new Vector2(0f, 142f);

    [Header("COMBOラベルの位置設定")]
    public Vector2 labelPosLeft = new Vector2(-98f, 42f);
    public Vector2 labelPosRight = new Vector2(98f, 42f);
    public Vector2 labelPosUp = new Vector2(0f, 102f);

    [Header("アニメーション設定")]
    // 合計表示時間（秒）
    public float displayDuration = 1.0f;
    // 登場・消滅のスケールアニメーション時間（秒）
    public float scaleDuration = 0.12f;
    // 浮き上がる距離（ワールド単位）
    public float floatDistance = 0.5f;
    // 登場時のオーバーシュート倍率（1.0 = なし、1.25 = 25%膨らむ）
    public float overshootScale = 1.25f;

    Transform cachedTransform;
    Canvas childCanvas;

    TextMeshProUGUI[] allTexts;
    SpriteRenderer[] allSprites;

    void Awake()
    {
        cachedTransform = transform;
        childCanvas = GetComponentInChildren<Canvas>();
        allTexts = GetComponentsInChildren<TextMeshProUGUI>();
        allSprites = GetComponentsInChildren<SpriteRenderer>();
    }

    public void SetCombo(int comboCount, float bgRotation, int direction)
    {
        if (comboNumber != null) comboNumber.text = comboCount.ToString();
        if (comboLabel != null) comboLabel.text = "COMBO";

        if (backgroundRect != null)
            backgroundRect.localRotation = Quaternion.Euler(0f, 0f, bgRotation);

        Vector2 numPos, lblPos, bgPos;
        switch (direction)
        {
            case 0:
                numPos = numberPosLeft; lblPos = labelPosLeft; bgPos = bgPosLeft; break;
            case 1:
                numPos = numberPosRight; lblPos = labelPosRight; bgPos = bgPosRight; break;
            default:
                numPos = numberPosUp; lblPos = labelPosUp; bgPos = bgPosUp; break;
        }

        if (comboNumber != null) comboNumber.GetComponent<RectTransform>().anchoredPosition = numPos;
        if (comboLabel != null) comboLabel.GetComponent<RectTransform>().anchoredPosition = lblPos;
        if (backgroundRect != null) backgroundRect.anchoredPosition = bgPos;

        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        // Canvas の Render Mode を World Space に切り替えて
        // 親 Transform に追従させる
        if (childCanvas != null)
        {
            childCanvas.renderMode = RenderMode.WorldSpace;
            // localPosition を原点に固定（親=COMBOの座標系で動く）
            childCanvas.transform.localPosition = Vector3.zero;
            childCanvas.transform.localRotation = Quaternion.identity;
        }

        float elapsed = 0f;
        Vector3 originPos = cachedTransform.localPosition;

        // Canvas の初期 localScale を記憶
        Vector3 canvasOriginScale = childCanvas != null
            ? childCanvas.transform.localScale
            : Vector3.one;

        SetAlpha(1f);
        cachedTransform.localScale = Vector3.zero;
        if (childCanvas != null) childCanvas.transform.localScale = Vector3.zero;

        // ── 1. 登場（スケールポップ）
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float s = EaseOutBack(Mathf.Clamp01(elapsed / scaleDuration), overshootScale);
            cachedTransform.localScale = Vector3.one * s;
            if (childCanvas != null) childCanvas.transform.localScale = canvasOriginScale * s;
            yield return null;
        }
        cachedTransform.localScale = Vector3.one;
        if (childCanvas != null) childCanvas.transform.localScale = canvasOriginScale;

        // ── 2. 滞在（浮き上がる）
        // ルートの localPosition だけ動かせば Canvas も一緒に追従する
        float holdTime = Mathf.Max(0f, displayDuration - scaleDuration * 2f);
        elapsed = 0f;
        while (elapsed < holdTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / holdTime);
            cachedTransform.localPosition = originPos + Vector3.up * (floatDistance * t);
            yield return null;
        }

        // ── 3. 消滅（縮小＋フェード）
        elapsed = 0f;
        Vector3 holdPos = cachedTransform.localPosition;
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleDuration);
            float s = 1f - t;
            cachedTransform.localScale = Vector3.one * s;
            if (childCanvas != null) childCanvas.transform.localScale = canvasOriginScale * s;
            SetAlpha(s);
            cachedTransform.localPosition = holdPos + Vector3.up * (floatDistance * 0.2f * t);
            yield return null;
        }

        Destroy(gameObject);
    }

    void SetAlpha(float a)
    {
        foreach (var tmp in allTexts)
        {
            if (tmp == null) continue;
            Color c = tmp.color; c.a = a; tmp.color = c;
        }
        foreach (var sr in allSprites)
        {
            if (sr == null) continue;
            Color c = sr.color; c.a = a; sr.color = c;
        }
    }

    float EaseOutBack(float t, float overshoot)
    {
        float c1 = overshoot - 1f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}