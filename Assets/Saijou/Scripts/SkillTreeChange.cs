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

    public void OnSkilTreeChangeButton()
    {
        // セーブ
        SaveManager.Save(playerData, allSkills);
        StartCoroutine(PlayAnimationAndLoad());
    }

    // ショップに移動
    public void MoveToShop()
    {
        // セーブ
        SaveManager.Save(playerData, allSkills);
        StartCoroutine(PlayAnimationAndLoad());
    }
    IEnumerator PlayAnimationAndLoad()
    {
        // バウンド演出再生
        uiAnimation.PlayBounce(targetButton);

        // アニメーション終了待ち
        yield return new WaitForSecondsRealtime(animationTime);

        // シーン移動
        SceneManager.LoadScene(sceneName);
    }
}
