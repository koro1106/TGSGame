using TMPro;
using UnityEngine;
/// <summary>
/// プレステージExpテキスト用
/// </summary>

public class PrestageExpText : MonoBehaviour
{
    public PreStagePlayerData prestagePlayerData;

    public TextMeshProUGUI expText_Pre; // プレステージExp
    void Start()
    {
        UpdateExpText();
    }

    void UpdateExpText()
    {
       expText_Pre.text = prestagePlayerData.prestageExp.ToString();
    }
}
