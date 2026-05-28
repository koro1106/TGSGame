using TMPro;
using UnityEngine;
/// <summary>
/// プレステージExpテキスト用
/// </summary>

public class PrestigeExpText : MonoBehaviour
{
    public PreStagePlayerData prestagePlayerData;

    public TextMeshProUGUI expText_Pre; // プレステージExp
    void Start()
    {
        UpdatePrestigeExpText();
    }

    public void UpdatePrestigeExpText()
    {
       expText_Pre.text = prestagePlayerData.prestageExp.ToString();
    }
}
