using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainSceneImageController : MonoBehaviour
{
    [Header("MainScene開始時に表示するImage")]
    public Image startImage;

    [Header("表示する時間")]
    public float displayDuration = 2f;

    private void Start()
    {
        // =====================================================
        // Imageが設定されていなければ終了
        // =====================================================

        if (startImage == null)
            return;

        // =====================================================
        // 最初に表示
        // =====================================================

        startImage.gameObject.SetActive(true);

        // =====================================================
        // 一定時間後に非表示
        // =====================================================

        StartCoroutine(HideImage());
    }

    private IEnumerator HideImage()
    {
        yield return new WaitForSecondsRealtime(displayDuration);

        // =====================================================
        // Imageを非表示
        // =====================================================

        if (startImage != null)
        {
            startImage.gameObject.SetActive(false);
        }
    }
}