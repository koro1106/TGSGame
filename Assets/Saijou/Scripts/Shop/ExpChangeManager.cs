using System.Collections;
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

    [Header("ウィンドウアニメーション")]
    [SerializeField] private RectTransform expChangeWindowRect;

    [SerializeField] private float openDuration = 0.2f;
    [SerializeField] private float closeDuration = 0.15f;
    void Update()
    {
        if(playerStats.shopExpChange)
            expChangeButton.SetActive(true);
        else
            expChangeButton.SetActive(false);

        // expChangeOpeningに合わせてRaycastを切り替える
        BlackImage.raycastTarget = expChangeOpening;
    }

    // 素材変換閉じる
    public void OnClose()
    {
        if (windowAnimationCoroutine != null)
            StopCoroutine(windowAnimationCoroutine);

        expChangeOpening = false;

        Color color = BlackImage.color;
        color.a = 0f;
        BlackImage.color = color;

        windowAnimationCoroutine = StartCoroutine(CloseWindowAnimation());
    }

    // 素材変換開ける
    public void OnExpChangeOpen()
    {
        if (windowAnimationCoroutine != null)
            StopCoroutine(windowAnimationCoroutine);

        expChangeWindow.SetActive(true);
        expChangeOpening = true;

        Color color = BlackImage.color;
        color.a = 0.7f;
        BlackImage.color = color;

        windowAnimationCoroutine = StartCoroutine(OpenWindowAnimation());
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

            // Exp02は9999を上限にする
            playerData.currentExp_2 = Mathf.Min(
                playerData.currentExp_2 + getExp,
                PlayerData.MaxExp);
        }
        else if (useExp02)
        {
            playerData.currentExp_2 -= getExp * 10;

            // Exp03は9999を上限にする
            playerData.currentExp_3 = Mathf.Min(
                playerData.currentExp_3 + getExp,
                PlayerData.MaxExp);
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

    private Coroutine windowAnimationCoroutine;

    private IEnumerator OpenWindowAnimation()
    {
        float time = 0f;

        // 最初は少し小さく
        expChangeWindowRect.localScale = Vector3.one * 0.8f;

        while (time < openDuration)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / openDuration);

            // 最初速く、最後ゆっくり
            t = 1f - Mathf.Pow(1f - t, 3f);

            expChangeWindowRect.localScale = Vector3.Lerp(
                Vector3.one * 0.8f,
                Vector3.one,
                t
            );

            yield return null;
        }

        expChangeWindowRect.localScale = Vector3.one;
    }

    private IEnumerator CloseWindowAnimation()
    {
        float time = 0f;

        Vector3 startScale = expChangeWindowRect.localScale;

        while (time < closeDuration)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / closeDuration);

            // 閉じるときは少し速め
            t = t * t;

            expChangeWindowRect.localScale = Vector3.Lerp(
                startScale,
                Vector3.one * 0.8f,
                t
            );

            yield return null;
        }

        expChangeWindowRect.localScale = Vector3.one * 0.8f;

        expChangeWindow.SetActive(false);
    }
}
