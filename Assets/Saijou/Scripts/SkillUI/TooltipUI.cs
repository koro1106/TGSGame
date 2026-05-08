using UnityEngine;
using TMPro;
using UnityEngine.UI;
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

    public Image expIcon;　// 経験値アイコン画像
    [SerializeField] Sprite exp1Sprite;
    [SerializeField] Sprite exp2Sprite;
    [SerializeField] Sprite exp3Sprite;
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

        // 経験値取得
        int exp = GetCurrentExp(data);
        
        // 経験値アイコン切替
        expIcon.sprite = GetExpSprite(data.expType); 

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

    /// <summary>
    /// 経験値取得
    /// </summary>
    int GetCurrentExp(SkillData data)
    {
        if (data.playerData == null)
            return 0;

        switch (data.expType)
        {
            case ExpType.Exp1:
                return data.playerData.currentExp_1;

            case ExpType.Exp2:
                return data.playerData.currentExp_2;

            case ExpType.Exp3:
                return data.playerData.currentExp_3;
        }

        return 0;
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
        }

        return null;
    }
}
