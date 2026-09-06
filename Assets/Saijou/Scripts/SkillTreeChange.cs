using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// プレステージ遷移ボタン
/// </summary>
public class SkillTreeChange : MonoBehaviour
{
    public UIAnimation uiAnimation;
    public RectTransform targetButton; // 対象ボタン
    public string sceneName;           // 遷移先シーン名
    public float animationTime = 0.3f; // アニメーション時間
    public SkillData[] allSkills;
    public PlayerData playerData;

    public bool isShop = false; // ショップが開かれているか

    [SerializeField] private ShopManager shopManager;
    public void OnSkilTreeChangeButton()
    {
        // セーブ
        SaveManager.Save(playerData, allSkills);
        // シーン移動
        SceneManager.LoadScene(sceneName);
    }

    // ショップに移動
    public void MoveToShop()
    {
        //// ショップボタンの点滅を止める
        if (shopManager != null)
        {
            shopManager.OnShopButtonClick();
        }
        // セーブ
        SaveManager.Save(playerData, allSkills);
        // シーン移動
        SceneManager.LoadScene(sceneName);
              
        isShop = true;
    }
}
