using TMPro;
using UnityEngine;
/// <summary>
/// ショップExpテキスト用
/// </summary>
public class ShopExpText : MonoBehaviour
{
    public PlayerData playerData;

    public TextMeshProUGUI expText_1;   // 通常Exp
    public TextMeshProUGUI expText_2;
    void Start()
    {
        UpdateNormalExpText();
    }

    public void UpdateNormalExpText()
    {
        expText_1.text = playerData.currentExp_1.ToString();
        expText_2.text = playerData.currentExp_2.ToString();
    }
}
