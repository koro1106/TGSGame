using UnityEngine;
using UnityEngine.Playables;

public class SceneStartTimeline1 : MonoBehaviour
{
    [Header("Scene開始時に再生するTimeline")]
    [SerializeField]
    private PlayableDirector director;

    [Header("停止するPlayer")]
    [SerializeField]
    private PlayerMovement playerMovement;

    [Header("Timeline中に射撃禁止にするGunController")]
    [SerializeField]
    private GunController gunController;

    [Header("Timeline再生中に非表示にするクロスヘア")]
    [SerializeField]
    private GameObject crosshairObject;

    [Header("スライド終了後に表示するImageUI")]
    [SerializeField]
    private MainSceneImageUI mainSceneImageUI;

    private void Start()
    {
        if (director == null)
        {
            Debug.LogWarning(
                "Playable Directorが設定されていません。"
            );

            return;
        }

        // =====================================================
        // Playerを停止
        // =====================================================

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // =====================================================
        // Timeline中は射撃禁止
        // ※GunController自体は止めない
        // ※クロスヘアを動かすため
        // =====================================================

        if (gunController != null)
        {
            gunController.SetTimelinePlaying(true);
        }

        // =====================================================
        // クロスヘアを非表示
        // =====================================================

        if (crosshairObject != null)
        {
            crosshairObject.SetActive(false);
        }

        // =====================================================
        // Timelineを最初に戻す
        // =====================================================

        director.time = 0;

        // =====================================================
        // Timelineの状態を最初に更新
        // =====================================================

        director.Evaluate();

        // =====================================================
        // Timeline終了イベントを登録
        // =====================================================

        director.stopped += OnTimelineStopped;

        // =====================================================
        // Timelineを最初から再生
        // =====================================================

        director.Play();
    }

    // =========================================================
    // Timeline終了
    // =========================================================

    private void OnTimelineStopped(PlayableDirector stoppedDirector)
    {
        if (stoppedDirector != director)
            return;

        // =====================================================
        // タイトルから来た場合
        // 説明Imageを表示するためPlayerとGunを停止したまま
        // =====================================================

        if (SceneTransition.ShowMainSceneImage)
        {
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }

            if (gunController != null)
            {
                gunController.SetTimelinePlaying(true);
            }
        }
        // =====================================================
        // スキルツリーなどから戻ってきた場合
        // 説明Imageを表示しないので、そのままゲーム開始
        // =====================================================
        else
        {
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }

            if (gunController != null)
            {
                gunController.enabled = true;
                gunController.SetTimelinePlaying(false);
            }
        }

        // =====================================================
        // クロスヘアを表示
        // =====================================================

        if (crosshairObject != null)
        {
            crosshairObject.SetActive(true);
        }

        // =====================================================
        // SceneTransitionから来たか確認
        // =====================================================

        Debug.Log(
            "【SceneStartTimeline1】ShowMainSceneImage = "
            + SceneTransition.ShowMainSceneImage
        );

        // =====================================================
        // タイトルからStartGame()で来た場合のみ
        // スライド終了後にImageUIを表示
        // =====================================================

        if (mainSceneImageUI != null &&
            SceneTransition.ShowMainSceneImage)
        {
            mainSceneImageUI.ShowAfterSlide();

            // 一度表示したらフラグをOFF
            SceneTransition.ShowMainSceneImage = false;

            Debug.Log("【SceneStartTimeline1】ImageUIを表示しました");
        }

        // =====================================================
        // イベント解除
        // =====================================================

        director.stopped -= OnTimelineStopped;
    }

    private void OnDestroy()
    {
        if (director != null)
        {
            director.stopped -= OnTimelineStopped;
        }

        // シーン破棄時に射撃禁止が残らないようにする
        if (gunController != null)
        {
            gunController.SetTimelinePlaying(false);
        }
    }
}