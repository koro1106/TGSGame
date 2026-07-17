using TMPro;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// ショップツールチップUI
/// </summary>
public class ShopTooltipUI : MonoBehaviour
{
    public static ShopTooltipUI Instance;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void ShowText(SkillData skill, bool showCost)
    {
        titleText.text = skill.skillName;
        descriptionText.text = skill.description;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
