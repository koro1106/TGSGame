using UnityEngine;
using TMPro;
/// <summary>
/// ツールチップ（説明ウィンドウ）管理
/// ・テキスト表示
/// ・UIのON/OFF
/// を担当（演出は別スクリプト）
/// </summary>
public class TooltipUI : MonoBehaviour
{
    public static TooltipUI instance;

    [Header("パネル")] public GameObject panel;     // 表示するパネル
    
    [Header("テキスト分割")]
    public TextMeshProUGUI nameText;     // スキル名
    public TextMeshProUGUI descText;     // 説明
    public TextMeshProUGUI levelText;    // レベル
    public TextMeshProUGUI expText;      // Exp

    [Header("演出")] public UIAnimation UIanim;
    [Header("データ")]public PlayerData playerData;
    void Awake()
    {
        instance = this;
        panel.SetActive(false); // 最初非表示
    }

    // 表示する
    public void Show(SkillData data, bool playPanelAnim = true)
    {
        panel.SetActive(true);

        // パネルアニメーション
        if (playPanelAnim)
        {
            UIanim.PlayBounce(panel.GetComponent<RectTransform>());
        }

        // PlayerExp取得
        int exp = 0;
        if (data.playerData != null)
        {
            exp = data.playerData.currentExp;
        }

        // テキスト更新
        nameText.text = data.skillName;
        descText.text = data.description;
        levelText.text = data.level + " / " + data.maxLevel;
        expText.text =  exp + " / " + data.needExp;

        // レベルアップした時だけ再生
        if(data.isLevelUp)
        {
            UIanim.PlayBounce(levelText.rectTransform);
            UIanim.PlayBounce(expText.rectTransform);

            data.isLevelUp = false;
        }
    }

    // 非表示にする
    public void Hide()
    {
        panel.SetActive(false);
    }
}
