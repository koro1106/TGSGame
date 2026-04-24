using UnityEngine;
using TMPro;
/// <summary>
/// ツールチップ（説明ウィンドウ）管理
/// </summary>
public class TooltipUI : MonoBehaviour
{
    public static TooltipUI instance;

    public GameObject panel;     // 表示するパネル
    public TextMeshProUGUI text; // テキスト

    void Awake()
    {
        instance = this;
        panel.SetActive(false); // 最初非表示
        
    }

    // 表示する
    public void Show(SkillData data)
    {
        panel.SetActive(true);
        text.text = data.skillName + "\n" + data.description + "\n" + data.level + "/" + data.maxLevel 
                    + "\n" + data.currentExp + "/" + data.needExp;
    }

    // 非表示にする
    public void Hide()
    {
        panel.SetActive(false);
    }
}
