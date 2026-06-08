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
        Debug.Log("SAVE");
        foreach (var skill in allSkills)
        {
            Debug.Log($"{skill.skillName} Lv:{skill.level} Unlock:{skill.isUnlocked}");
        }
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
