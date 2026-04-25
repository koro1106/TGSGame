using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// ホバー専用(ツールチップだけ)
/// </summary>
public class SkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SkillData data; // このボタンに対応するスキルデータ

    // マウスが乗った時
    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipUI.instance.Show(data);
    }

    // マウスが離れたとき
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.instance.Hide();
    }
}
