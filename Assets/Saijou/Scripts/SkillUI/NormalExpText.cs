using TMPro;
using UnityEngine;
/// <summary>
/// 通常ステージExpテキスト用
/// </summary>
public class NormalExpText : MonoBehaviour
{
    public PlayerData playerData;

    public TextMeshProUGUI expText_1;   // 通常ステージExp
    public TextMeshProUGUI expText_2;
    public TextMeshProUGUI expText_3;

    void Start()
    {
        UpdateExpText();
    }

    void UpdateExpText()
    {
        expText_1.text = playerData.currentExp_1.ToString();
        expText_2.text = playerData.currentExp_2.ToString();
        expText_3.text = playerData.currentExp_3.ToString();
    }
}
