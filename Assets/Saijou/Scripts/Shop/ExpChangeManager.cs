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

    public bool expChangeOpening = false;

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
}
