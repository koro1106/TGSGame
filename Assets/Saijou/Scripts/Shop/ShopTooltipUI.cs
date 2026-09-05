using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ショップツールチップUI
/// </summary>
public class ShopTooltipUI : MonoBehaviour
{
    public static ShopTooltipUI Instance;

    [Header("基本テキスト")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("必要素材")]
    [SerializeField] private TMP_Text costText1;
    [SerializeField] private TMP_Text costText2;
    [SerializeField] private GameObject needImages;

    [Header("最大レベル表示")]
    [SerializeField] private GameObject maxLevelText;

    [Header("人形画像")]
    [SerializeField] private Image dollImage;

    // 現在表示しているスキル
    private SkillData currentSkill;

    // 素材表示するか
    private bool showCost;
    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }
    private void Update()
    {
        // ツールチップが表示されていないなら何もしない
        if (!gameObject.activeSelf)
            return;

        // スキルが設定されていないなら何もしない
        if (currentSkill == null)
            return;

        // 最大レベルなら基本テキスト・素材テキストを非表示
        if (currentSkill.level >= currentSkill.maxLevel)
        {
            SetMaxLevelTextVisible(false);
            return;
        }

        // 最大レベルではない場合は表示
        SetMaxLevelTextVisible(true);

        // 素材数を毎フレーム更新
        UpdateCostText(currentSkill, showCost);
    }

    /// <summary>
    /// 最大レベル時のテキスト表示を切り替える
    /// </summary>
    private void SetMaxLevelTextVisible(bool visible)
    {
        if (titleText != null)
            titleText.gameObject.SetActive(visible);

        if (descriptionText != null)
            descriptionText.gameObject.SetActive(visible);

        if (costText1 != null)
            costText1.gameObject.SetActive(visible);

        if (costText2 != null)
            costText2.gameObject.SetActive(visible);

        if (needImages != null)
            needImages.gameObject.SetActive(visible);

        // 最大レベル表示
        if (maxLevelText != null)
            maxLevelText.SetActive(!visible);
    }

    public void ShowText(SkillData skill, bool showCost)
    {
        // 現在表示しているスキルを保存
        currentSkill = skill;

        // 素材表示設定を保存
        this.showCost = showCost;

        titleText.text = skill.skillName;
        descriptionText.text = skill.description;

        // 必要素材を表示
        UpdateCostText(skill, showCost);

        // 人形画像を変更
        if (dollImage != null)
        {
            dollImage.sprite = skill.dollImage;
            dollImage.gameObject.SetActive(skill.dollImage != null);
        }

        gameObject.SetActive(true);
    }

    /// <summary>
    /// スキルごとに人形画像を切り替える
    /// </summary>
    private void UpdateDollImage(SkillData skill)
    {
        if (dollImage == null)
            return;

        // 現在表示している人形を削除
        foreach (Transform child in dollImage.transform)
        {
            Destroy(child.gameObject);
        }

        // スキルに設定された人形を生成
        if (skill.dollImage != null)
        {
            Instantiate(
                skill.dollImage,
                dollImage.transform
            );
        }
    }

    /// <summary>
    /// 必要素材テキストを更新
    /// </summary>
    private void UpdateCostText(SkillData skill, bool showCost)
    {
        // まず非表示
        if (costText1 != null)
            costText1.gameObject.SetActive(false);

        if (costText2 != null)
            costText2.gameObject.SetActive(false);

        if (!showCost)
            return;

        if (skill.requiredExps == null)
            return;

        int costIndex = 0;

        foreach (RequiredExp requiredExp in skill.requiredExps)
        {
            if (requiredExp == null)
                continue;

            // 必要数が0以下なら表示しない
            if (requiredExp.needExp <= 0)
                continue;

            // 最大2種類まで
            if (costIndex >= 2)
                break;

            // 現在の素材数を取得
            int currentExp =
                skill.GetCurrentExp(requiredExp.expType);

            // 必要数だけ表示
            string text =
                requiredExp.needExp.ToString();

            TMP_Text targetText = null;

            if (costIndex == 0)
            {
                targetText = costText1;
            }
            else
            {
                targetText = costText2;
            }

            if (targetText != null)
            {
                targetText.text = text;

                // 素材が足りなければ赤
                if (currentExp < requiredExp.needExp)
                {
                    targetText.color = Color.red;
                }
                else
                {
                    targetText.color = Color.white;
                }

                targetText.gameObject.SetActive(true);
            }

            costIndex++;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
