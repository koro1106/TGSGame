using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OptionMenu : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private MenuSelector menuSelector;
    [Header("Volume Profile")]
    [SerializeField] private VolumeProfile volumeProfile;

    [Header("Vignette Animation")]
    [SerializeField] private float vignetteMoveTime = 0.3f;

    private Vignette vignette;
    private Coroutine vignetteCoroutine;

    private void Awake()
    {
        if (volumeProfile != null)
        {
            if (volumeProfile.TryGet<Vignette>(out vignette))
            {
                SetVignetteCenterX(0.3f);
            }
        }
    }
    public void OpenOption()
    {
        animator.SetTrigger("Open");
        MoveVignetteCenterX(0.7f);
    }

    public void CloseOption()
    {
        animator.SetTrigger("Close");
        MoveVignetteCenterX(0.3f);
    }
    private void MoveVignetteCenterX(float targetX)
    {
        // 前のアニメーションを止める
        if (vignetteCoroutine != null)
        {
            StopCoroutine(vignetteCoroutine);
        }

        vignetteCoroutine = StartCoroutine(MoveVignette(targetX));
    }

    private IEnumerator MoveVignette(float targetX)
    {
        if (vignette == null)
            yield break;

        float startX = vignette.center.value.x;
        float elapsed = 0f;

        while (elapsed < vignetteMoveTime)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / vignetteMoveTime;

            // 徐々に移動
            float currentX = Mathf.Lerp(startX, targetX, t);

            Vector2 center = vignette.center.value;
            center.x = currentX;
            vignette.center.value = center;

            yield return null;
        }

        // 最後は確実に目標値へ
        Vector2 finalCenter = vignette.center.value;
        finalCenter.x = targetX;
        vignette.center.value = finalCenter;

        vignetteCoroutine = null;
    }

    private void SetVignetteCenterX(float x)
    {
        if (vignette == null)
            return;

        Vector2 center = vignette.center.value;
        center.x = x;
        vignette.center.value = center;
    }
}