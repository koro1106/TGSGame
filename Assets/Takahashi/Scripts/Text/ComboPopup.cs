using UnityEngine;
using TMPro;

public class ComboPopup : MonoBehaviour
{
    [Header("背景画像")]
    // InspectorでBackgroundオブジェクトをアサイン
    public RectTransform backgroundRect;

    [Header("コンボテキスト")]
    // 数字部分（例：15）
    public TextMeshProUGUI comboNumber;

    // "COMBO"固定テキスト
    public TextMeshProUGUI comboLabel;

    [Header("背景の位置設定")]
    // 左・右表示のときの背景位置
    public Vector2 bgPosLeft = new Vector2(0f, 85f);
    public Vector2 bgPosRight = new Vector2(0f, 85f);
    // 上表示のときの背景位置
    public Vector2 bgPosUp = new Vector2(0f, 0f);

    [Header("数字テキストの位置設定")]
    // 左・右・上それぞれの数字位置
    public Vector2 numberPosLeft = new Vector2(-98f, 82f);
    public Vector2 numberPosRight = new Vector2(98f, 82f);
    public Vector2 numberPosUp = new Vector2(0f, 142f);

    [Header("COMBOラベルの位置設定")]
    // 左・右・上それぞれのCOMBO文字位置
    public Vector2 labelPosLeft = new Vector2(-98f, 42f);
    public Vector2 labelPosRight = new Vector2(98f, 42f);
    public Vector2 labelPosUp = new Vector2(0f, 102f);

    // ComboManagerから呼び出される
    // comboCount : 表示するコンボ数
    // bgRotation : 背景にだけ適用する回転角度
    // direction  : 表示方向（0=左、1=右、2=上）
    public void SetCombo(int comboCount, float bgRotation, int direction)
    {
        // 数字を表示
        if (comboNumber != null)
        {
            comboNumber.text = comboCount.ToString();
        }

        // "COMBO"は固定テキスト
        if (comboLabel != null)
        {
            comboLabel.text = "COMBO";
        }

        // 背景のみZ軸回転を適用（テキストには影響しない）
        if (backgroundRect != null)
        {
            backgroundRect.localRotation = Quaternion.Euler(0f, 0f, bgRotation);
        }

        // 表示方向ごとに位置を切り替え
        Vector2 numPos;
        Vector2 lblPos;
        Vector2 bgPos;

        switch (direction)
        {
            case 0: // 左
                numPos = numberPosLeft;
                lblPos = labelPosLeft;
                bgPos = bgPosLeft;
                break;

            case 1: // 右
                numPos = numberPosRight;
                lblPos = labelPosRight;
                bgPos = bgPosRight;
                break;

            case 2: // 上
            default:
                numPos = numberPosUp;
                lblPos = labelPosUp;
                bgPos = bgPosUp;
                break;
        }

        // 数字の位置を適用
        if (comboNumber != null)
        {
            comboNumber.GetComponent<RectTransform>().anchoredPosition = numPos;
        }

        // COMBOラベルの位置を適用
        if (comboLabel != null)
        {
            comboLabel.GetComponent<RectTransform>().anchoredPosition = lblPos;
        }

        // 背景の位置を適用
        if (backgroundRect != null)
        {
            backgroundRect.anchoredPosition = bgPos;
        }
    }
}