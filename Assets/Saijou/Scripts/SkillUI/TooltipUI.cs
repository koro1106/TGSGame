using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    public TextMeshProUGUI maxLevelText;

    [Header("演出")] public UIAnimation UIanim;
    [Header("データ")]public PlayerData playerData;

    public Image expIcon;　// 経験値アイコン画像
    [SerializeField] Sprite exp1Sprite;
    [SerializeField] Sprite exp2Sprite;
    [SerializeField] Sprite exp3Sprite;
    [SerializeField] Sprite preExpSprite;
    void Awake()
    {
        instance = this;
        panel.SetActive(false); // 最初非表示
    }

    // 表示する
    public void Show(SkillData data, PointerEventData eventData, bool playPanelAnim = true)
    {
        panel.SetActive(true);

        // 経験値取得
        int exp = GetCurrentExp(data);

        // テキスト更新
        nameText.text = data.skillName;
        levelText.text = "レベル " +data.level + "/" + data.maxLevel;
        UpdateExpText(data, exp);

        // 位置決定
        SetPosition(eventData);

        // パネルアニメーション
        if (playPanelAnim)
        {
            UIanim.PlayBounce(panel.GetComponent<RectTransform>());
        }

        // 経験値アイコン切替
        expIcon.sprite = GetExpSprite(data.expType);


        // レベルアップした時だけ再生
        if (data.isLevelUp)
        {
            UIanim.PlayBounce(levelText.rectTransform);
            UIanim.PlayBounce(expText.rectTransform);

            data.isLevelUp = false;
        }
    }
    public void ShowText(SkillData data, bool playPanelAnim = true)
    {
        panel.SetActive(true);

        // 経験値取得
        int exp = GetCurrentExp(data);

        // テキスト更新
        nameText.text = data.skillName;
        levelText.text = "レベル " + data.level + "/" + data.maxLevel;
        UpdateExpText(data, exp);

        // パネルアニメーション
        if (playPanelAnim)
        {
            UIanim.PlayBounce(panel.GetComponent<RectTransform>());
        }

        // 経験値アイコン切替
        expIcon.sprite = GetExpSprite(data.expType);

        
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
        switch (data.expType)
        {
            case ExpType.Exp1:
                return data.playerData.currentExp_1;

            case ExpType.Exp2:
                return data.playerData.currentExp_2;

            case ExpType.Exp3:
                return data.playerData.currentExp_3;

            case ExpType.PreExp:
                return data.playerData.currentPreExp;
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

            case ExpType.PreExp:
                return preExpSprite;
        }

        return null;
    }

    // 位置設定
    void SetPosition(PointerEventData eventData)
    {
        // ツールチップ本体
        RectTransform tooltipRect = panel.GetComponent<RectTransform>();

        // ホバーしているUI（ボタン）
        RectTransform target = eventData.pointerEnter?.GetComponent<RectTransform>();
        if (target == null) return;

        // ボタンの四隅取得
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        // 中心位置を計算（ワールド座標）
        Vector3 worldCenter = (corners[0] + corners[2]) / 2f;

        // スクリーン座標へ変換（Overlayなのでカメラnull）
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, worldCenter);

        // 画面の上下判定
        bool isUpper = screenPos.y > Screen.height / 2f;

        // 固定オフセット（上下だけ切替）
        Vector2 offset = isUpper
            ? new Vector2(-40, -360f)
            : new Vector2(-40, 300f);

        // 最終位置
        tooltipRect.position = screenPos + offset;
    }

    void UpdateExpText(SkillData data, int exp)
    {
        // MAXレベル
        if (data.IsMaxLevel())
        {
            // Exp表示削除
            expText.gameObject.SetActive(false);

            // 経験値アイコンを非表示
            expIcon.gameObject.SetActive(false);

            // 最大レベルを表示
            maxLevelText.gameObject.SetActive(true);
            return;
        }

        // MAXじゃないなら表示
        expText.gameObject.SetActive(true);

        // 最大レベル表示を消す
        maxLevelText.gameObject.SetActive(false);

        expText.text = exp + "/" + data.needExp;

        // 経験値が足りなければ赤
        if (exp < data.needExp)
        {
            expText.color = Color.red;
        }
        else
        {
            expText.color = Color.white;
        }
    }
}
