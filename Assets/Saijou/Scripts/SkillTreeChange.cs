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

    public void OnSkilTreeChangeButton()
    {
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
