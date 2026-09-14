using System.Collections;
using TMPro;
using UnityEngine;

public class OwnedMaterialUI : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;

    [SerializeField] private TMP_Text exp1Text;
    [SerializeField] private TMP_Text exp2Text;
    [SerializeField] private TMP_Text exp3Text;
    [SerializeField] private TMP_Text preExpText;

    // ââèoÇ∑ÇÈéûä‘
    [SerializeField] private float scaleDuration = 0.15f;

    // ç≈ëÂÉTÉCÉY
    [SerializeField] private float scaleUp = 1.3f;

    private Vector3 exp1OriginalScale;
    private Vector3 exp2OriginalScale;
    private Vector3 exp3OriginalScale;
    private Vector3 preExpOriginalScale;

    private Coroutine exp1Coroutine;
    private Coroutine exp2Coroutine;
    private Coroutine exp3Coroutine;
    private Coroutine preExpCoroutine;

    private int lastExp1;
    private int lastExp2;
    private int lastExp3;
    private int lastPreExp;

    private void Start()
    {
        if (exp1Text != null)
            exp1OriginalScale = exp1Text.transform.localScale;

        if (exp2Text != null)
            exp2OriginalScale = exp2Text.transform.localScale;

        if (exp3Text != null)
            exp3OriginalScale = exp3Text.transform.localScale;

        if (preExpText != null)
            preExpOriginalScale = preExpText.transform.localScale;

        if (playerData != null)
        {
            lastExp1 = playerData.currentExp_1;
            lastExp2 = playerData.currentExp_2;
            lastExp3 = playerData.currentExp_3;
            lastPreExp = playerData.currentPreExp;
        }

        RefreshText();
    }

    private void Update()
    {
        if (playerData == null)
            return;

        // êîÇ™ëùÇ¶ÇΩÇÁââèo
        if (playerData.currentExp_1 != lastExp1)
        {
            if (playerData.currentExp_1 > lastExp1)
                PlayScaleAnimation(exp1Text, ref exp1Coroutine, exp1OriginalScale);

            lastExp1 = playerData.currentExp_1;
        }

        if (playerData.currentExp_2 != lastExp2)
        {
            if (playerData.currentExp_2 > lastExp2)
                PlayScaleAnimation(exp2Text, ref exp2Coroutine, exp2OriginalScale);

            lastExp2 = playerData.currentExp_2;
        }

        if (playerData.currentExp_3 != lastExp3)
        {
            if (playerData.currentExp_3 > lastExp3)
                PlayScaleAnimation(exp3Text, ref exp3Coroutine, exp3OriginalScale);

            lastExp3 = playerData.currentExp_3;
        }

        if (playerData.currentPreExp != lastPreExp)
        {
            if (playerData.currentPreExp > lastPreExp)
                PlayScaleAnimation(preExpText, ref preExpCoroutine, preExpOriginalScale);

            lastPreExp = playerData.currentPreExp;
        }

        RefreshText();
    }

    private void RefreshText()
    {
        if (exp1Text != null)
            exp1Text.text = "Å~" + playerData.currentExp_1;

        if (exp2Text != null)
            exp2Text.text = "Å~" + playerData.currentExp_2;

        if (exp3Text != null)
            exp3Text.text = "Å~" + playerData.currentExp_3;

        if (preExpText != null)
            preExpText.text = "Å~" + playerData.currentPreExp;
    }

    private void PlayScaleAnimation(
        TMP_Text text,
        ref Coroutine coroutine,
        Vector3 originalScale)
    {
        if (text == null)
            return;

        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = StartCoroutine(ScaleAnimation(
            text.transform,
            originalScale));
    }

    private IEnumerator ScaleAnimation(
        Transform target,
        Vector3 originalScale)
    {
        float halfDuration = scaleDuration * 0.5f;

        // å≥ÉTÉCÉY Å® 1.3î{
        float time = 0f;

        while (time < halfDuration)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / halfDuration);

            target.localScale = Vector3.Lerp(
                originalScale,
                originalScale * scaleUp,
                t);

            yield return null;
        }

        // 1.3î{ Å® å≥ÉTÉCÉY
        time = 0f;

        while (time < halfDuration)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / halfDuration);

            target.localScale = Vector3.Lerp(
                originalScale * scaleUp,
                originalScale,
                t);

            yield return null;
        }

        target.localScale = originalScale;
    }
}