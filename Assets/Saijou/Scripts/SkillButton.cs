using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// スキルボタンの処理
/// マウスが乗ったら説明表示、離れたら消す
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

    // レベルアップボタンの処理
    public void OnLevelUpButtonClick()
    {
        // 経験値を消費してレベルアップを試みる
        data.TryLevelUp();
    }
}
