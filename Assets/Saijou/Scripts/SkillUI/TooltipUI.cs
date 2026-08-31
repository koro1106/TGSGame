using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// ツールチップ（説明ウィンドウ）管理
/// ・テキスト表示
/// ・UIのON/OFF
/// ・必要経験値の表示
/// を担当
/// </summary>
public class TooltipUI : MonoBehaviour
{
    public static TooltipUI instance;

    [Header("パネル")]
    public GameObject panel;

    [Header("テキスト")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI maxLevelText;

    [Header("必要経験値表示")]
    public Image[] expIcons;
    public TextMeshProUGUI[] expTexts;

    [Header("経験値アイコン")]
    [SerializeField] Sprite exp1Sprite;
    [SerializeField] Sprite exp2Sprite;
    [SerializeField] Sprite exp3Sprite;
    [SerializeField] Sprite preExpSprite;

    [Header("レベル表示")]
    [SerializeField] Image[] levelImages;

    // 未取得状態
    [SerializeField] Sprite levelOffSprite;

    // 取得済み状態
    [SerializeField] Sprite levelOnSprite;

    [Header("演出")]
    public UIAnimation UIanim;

    [Header("データ")]
    public PlayerData playerData;

    void Awake()
    {
        instance = this;

        panel.SetActive(false);
    }

    /// <summary>
    /// ポインターから表示
    /// </summary>
    public void Show(
        SkillData data,
        PointerEventData eventData,
        bool playPanelAnim = true)
    {
        ShowCommon(data, playPanelAnim);

        // 位置決定
        SetPosition(eventData);
    }

    /// <summary>
    /// 通常表示
    /// </summary>
    public void ShowText(
        SkillData data,
        bool playPanelAnim = true)
    {
        ShowCommon(data, playPanelAnim);
    }

    /// <summary>
    /// 共通表示処理
    /// </summary>
    void ShowCommon(SkillData data, bool playPanelAnim)
    {
        panel.SetActive(true);

        // スキル名
        nameText.text = data.skillName;

        // レベル
        levelText.text =
            "レベル " + data.level + "/" + data.maxLevel;

        // レベル画像更新
        UpdateLevelImages(data);

        // 説明
        descText.text = data.description;

        // 必要経験値表示
        UpdateExpText(data);

        // パネルアニメーション
        if (playPanelAnim)
        {
            UIanim.PlayBounce(
                panel.GetComponent<RectTransform>());
        }

        // レベルアップした時だけ再生
        if (data.isLevelUp)
        {
            UIanim.PlayBounce(levelText.rectTransform);

            // 新しく取得したレベル画像を演出
            int newLevelIndex;

            // 最大レベル1の場合はElement 2
            if (data.maxLevel == 1)
            {
                newLevelIndex = 2;
            }
            else
            {
                // 通常はレベルに対応した画像
                newLevelIndex = data.level - 1;
            }

            if (newLevelIndex >= 0 && newLevelIndex < levelImages.Length &&levelImages[newLevelIndex] != null)
            {
                StartCoroutine(LevelUpScaleAnimation(levelImages[newLevelIndex].rectTransform));
            }

            // 必要経験値テキストだけアニメーション
            foreach (TextMeshProUGUI expText in expTexts)
            {
                if (expText != null && expText.gameObject.activeSelf)
                {
                    UIanim.PlayBounce(expText.rectTransform);
                }
            }

            data.isLevelUp = false;
        }
    }

    /// <summary>
    /// レベルアップしたアイコンを
    /// 100% → 120% → 100% に拡大する
    /// </summary>
    IEnumerator LevelUpScaleAnimation(RectTransform target)
    {
        Vector3 originalScale = target.localScale;
        Quaternion originalRotation = target.localRotation;

        Vector3 bigScale = originalScale * 1.2f;

        float duration = 0.3f;

        // =========================
        // 100% → 120%
        // Y 0° → 360°
        // =========================

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;

            // 拡大は今まで通り滑らか
            float scaleT = Mathf.SmoothStep(0f, 1f, t);

            target.localScale =
                Vector3.Lerp(originalScale, bigScale,scaleT);

            // 回転は徐々に減速
            float rotationT = 1f - Mathf.Pow(1f - t, 3f);

            target.localRotation =originalRotation * Quaternion.Euler(0f,360f * rotationT,0f);

            yield return null;
        }

        target.localScale = bigScale;

        // =========================
        // 120% → 100%
        // Y 360° → 720°
        // =========================

        time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;

            // 縮小
            float scaleT = Mathf.SmoothStep(0f, 1f, t);

            target.localScale = Vector3.Lerp(bigScale, originalScale, scaleT);

            // 回転は徐々に減速
            float rotationT = 1f - Mathf.Pow(1f - t, 3f);

            target.localRotation = originalRotation * Quaternion.Euler(0f,360f + 360f * rotationT,0f);

            yield return null;
        }

        target.localScale = originalScale;
        target.localRotation = originalRotation;
    }

    /// <summary>
    /// 非表示
    /// </summary>
    public void Hide()
    {
        panel.SetActive(false);
    }

    /// <summary>
    /// 必要経験値を表示
    /// </summary>
    void UpdateExpText(SkillData data)
    {
        // まず全部非表示
        for (int i = 0; i < expIcons.Length; i++)
        {
            if (expIcons[i] != null)
            {
                expIcons[i].gameObject.SetActive(false);
            }

            if (i < expTexts.Length && expTexts[i] != null)
            {
                expTexts[i].gameObject.SetActive(false);
            }
        }


        // 最大レベル
        if (data.IsMaxLevel())
        {
            maxLevelText.gameObject.SetActive(true);
            maxLevelText.text = "最大レベル";
            return;
        }

        maxLevelText.gameObject.SetActive(false);

        // =========================
        // RequiredExpを表示
        // =========================

        for (int i = 0; i < data.requiredExps.Length; i++)
        {
            // UI側の数を超えたら終了
            if (i >= expIcons.Length ||
                i >= expTexts.Length)
            {
                break;
            }

            RequiredExp requiredExp =
                data.requiredExps[i];

            // 未設定なら表示しない
            if (requiredExp == null)
                continue;

            // 必要経験値が0以下なら表示しない
            if (requiredExp.needExp <= 0)
                continue;


            // =========================
            // 現在経験値
            // =========================

            int currentExp =
                data.GetCurrentExp(requiredExp.expType);


            // =========================
            // アイコン表示
            // =========================

            expIcons[i].gameObject.SetActive(true);

            expIcons[i].sprite =
                GetExpSprite(requiredExp.expType);


            // =========================
            // テキスト表示
            // =========================

            expTexts[i].gameObject.SetActive(true);

            expTexts[i].text =
                currentExp + "/" + requiredExp.needExp;


            // =========================
            // 経験値不足なら赤
            // =========================

            if (currentExp < requiredExp.needExp)
            {
                expTexts[i].color = Color.red;
            }
            else
            {
                expTexts[i].color = Color.white;
            }
        }
    }

    /// <summary>
    /// 経験値アイコン取得
    /// </summary>
    Sprite GetExpSprite(ExpType type)
    {
        switch (type)
        {
            case ExpType.Exp1:
                return exp1Sprite;

            case ExpType.Exp2:
                return exp2Sprite;

            case ExpType.Exp3:
                return exp3Sprite;

            case ExpType.PreExp:
                return preExpSprite;
        }

        return null;
    }

    /// <summary>
    /// 位置設定
    /// </summary>
    void SetPosition(PointerEventData eventData)
    {
        RectTransform tooltipRect =
            panel.GetComponent<RectTransform>();

        RectTransform target =
            eventData.pointerEnter?.GetComponent<RectTransform>();

        if (target == null)
            return;

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Vector3 worldCenter =
            (corners[0] + corners[2]) / 2f;

        Vector2 screenPos =
            RectTransformUtility.WorldToScreenPoint(
                null,
                worldCenter);

        bool isUpper =
            screenPos.y > Screen.height / 2f;

        Vector2 offset = isUpper
            ? new Vector2(-40, -360f)
            : new Vector2(-40, 300f);

        tooltipRect.position =
            screenPos + offset;
    }

    void UpdateLevelImages(SkillData data)
    {
        // =========================
        // まず全部表示状態に戻す
        // =========================
        for (int i = 0; i < levelImages.Length; i++)
        {
            if (levelImages[i] == null)
                continue;

            levelImages[i].gameObject.SetActive(true);
        }

        // =========================
        // 最大レベルが1の場合
        // Element 2だけ使用
        // =========================
        if (data.maxLevel == 1)
        {
            for (int i = 0; i < levelImages.Length; i++)
            {
                if (levelImages[i] == null)
                    continue;

                // Element 2以外を非表示
                if (i != 2)
                {
                    levelImages[i].gameObject.SetActive(false);
                    continue;
                }

                // Element 2
                levelImages[i].sprite =
                    data.level >= 1 ? levelOnSprite : levelOffSprite;
            }
            return;
        }

        // =========================
        // 通常のスキル
        // =========================
        for (int i = 0; i < levelImages.Length; i++)
        {
            if (levelImages[i] == null)
                continue;

            // 現在のレベル以下ならON
            if (i < data.level)
            {
                levelImages[i].sprite = levelOnSprite;
            }
            else
            {
                levelImages[i].sprite = levelOffSprite;
            }
        }
    }
}
