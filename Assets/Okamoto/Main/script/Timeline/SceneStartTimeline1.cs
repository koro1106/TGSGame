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

    [Header("Timeline再生中に非表示にするクロスヘア")]
    [SerializeField]
    private GameObject crosshairObject;

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
        // Player操作を復帰
        // =====================================================

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }


        // =====================================================
        // クロスヘアを表示
        // =====================================================

        if (crosshairObject != null)
        {
            crosshairObject.SetActive(true);
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
    }
}