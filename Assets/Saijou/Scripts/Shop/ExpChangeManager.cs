using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 素材変換用マネージャー
/// </summary>
public class ExpChangeManager : MonoBehaviour
{
    [SerializeField] PlayerStats playerStats;

    [SerializeField] GameObject expChangeButton;
    [SerializeField] GameObject expChangeWindow;
    [SerializeField] Image BlackImage;
    [SerializeField] TextMeshProUGUI getExpText;
    [SerializeField] PlayerData playerData;
    [SerializeField] TextMeshProUGUI needExpText;
    [SerializeField] NormalExpText normalExpText;
    [SerializeField] ShopExpText shopExpText;

    // 獲得する経験値
    [SerializeField] GameObject getExp02Icon;
    [SerializeField] GameObject getExp03Icon;

    // 黒画像
    [SerializeField] GameObject blackImage01;
    [SerializeField] GameObject blackImage02;

    public bool expChangeOpening = false;

    // 現在獲得する素材量
    private int getExp = 1;

    // 変換に必要な素材数
    private int needExp01 = 100;
    private int needExp02 = 10;

    // 変換に使用するExp
    private bool useExp01 = false;
    private bool useExp02 = false;
    void Update()
    {
        if(playerStats.shopExpChange)
            expChangeButton.SetActive(true);

        // expChangeOpeningに合わせてRaycastを切り替える
        BlackImage.raycastTarget = expChangeOpening;
    }

    // 素材変換閉じる
    public void OnClose()
    {
        expChangeWindow.SetActive(false);
        expChangeOpening = false;

        Color color = BlackImage.color;
        color.a = 0f;
        BlackImage.color = color;
    }

    // 素材変換開ける
    public void OnExpChangeOpen()
    {
        expChangeWindow.SetActive(true);
        expChangeOpening = true;

        Color color = BlackImage.color;
        color.a = 0.7f;
        BlackImage.color = color;
    }

    // =========================
    // 使用するExpを選択
    // =========================

    // Exp01を使用
    public void OnExp01Button()
    {
        useExp01 = true;
        useExp02 = false;

        getExp02Icon.SetActive(true); // 獲得素材Image
        getExp03Icon.SetActive(false);
        blackImage02.SetActive(true); // 黒画像
        blackImage01.SetActive(false);

        ResetGetExpText();
        needExpText.text = getExp.ToString();
        Debug.Log("Exp01を使用");
    }
    // Exp02を使用
    public void OnExp02Button()
    {
        useExp01 = false;
        useExp02 = true;

        getExp03Icon.SetActive(true); // 獲得素材Image
        getExp02Icon.SetActive(false);
        blackImage02.SetActive(false); // 黒画像
        blackImage01.SetActive(true);

        ResetGetExpText();
        needExpText.text = getExp.ToString();
        Debug.Log("Exp02を使用");
    }

    // =========================
    // 獲得素材量
    // =========================

    // 獲得素材量増やす
    public void OnExpUpButton()
    {
        if (useExp01)
        {
            int nextGetExp = getExp + 1;

            if (playerData.currentExp_1 < needExp01 * nextGetExp)
            {
                Debug.Log("次のExp01が足りません");
                return;
            }
        }
        else if (useExp02)
        {
            int nextGetExp = getExp + 1;

            if (playerData.currentExp_2 < needExp02* nextGetExp)
            {
                Debug.Log("Exp02が足りません");
                return;
            }
        }
        else
        {
            Debug.Log("使用するExpが選択されていません");
            return;
        }

        getExp++;

        if (useExp01)
        {
           int needExp = getExp * -100;
           needExpText.text = needExp.ToString();
        }
        else if (useExp02)
        {
            int needExp = getExp * -10;
            needExpText.text = needExp.ToString();
        }
            UpdateGetExpText();
    }
    // 獲得素材量減らす
    public void OnExpDownButton()
    {
        // 0未満にならないようにする
        if (getExp > 0)
        {
            getExp--;
        }

        if (useExp01)
        {
            int needExp = getExp * -100;
            needExpText.text = needExp.ToString();
        }
        else if (useExp02)
        {
            int needExp = getExp * -10;
            needExpText.text = needExp.ToString();
        }

        UpdateGetExpText();
    }

    // テキスト更新
    private void UpdateGetExpText()
    {
        getExpText.text = getExp.ToString();
    }
    private void ResetGetExpText()
    {
        getExp = 0;
        getExpText.text = getExp.ToString();
    }

    // 変換
    public void OnChangeButton()
    {
        if (useExp01)
        {
            playerData.currentExp_1 -= getExp * 100;
            playerData.currentExp_2 += getExp;
        }
        else if (useExp02)
        {
            playerData.currentExp_2 -= getExp * 10;
            playerData.currentExp_3 += getExp;
        }

        normalExpText.UpdateNormalExpText();
        shopExpText.UpdateNormalExpText();
    }

    // 最大獲得数にする
    public void OnMaxButton()
    {
        if (useExp01)
        {
            // Exp01を100個使ってExp02を1個作る
            getExp = playerData.currentExp_1 / needExp01;

            // 必要Expを表示
            needExpText.text = (getExp * -needExp01).ToString();
        }
        else if (useExp02)
        {
            // Exp02を10個使ってExp03を1個作る
            getExp = playerData.currentExp_2 / needExp02;

            // 必要Expを表示
            needExpText.text = (getExp * -needExp02).ToString();
        }
        else
        {
            Debug.Log("使用するExpが選択されていません");
            return;
        }

        // 獲得数を表示
        UpdateGetExpText();
    }
}
