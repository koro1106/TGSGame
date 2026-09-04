using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 属性弾ダメージUP＆属性弾発生確率UP
/// </summary>
public class ShopUPButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("スキルデータ")]
    [SerializeField] private SkillData data;

    [Header("スキル効果")]
    [SerializeField] private SkillEffectManager effectManager;

    [Header("プレイヤーデータ")]
    [SerializeField] private PlayerData playerData;

    [Header("プレイヤーステータス")]
    [SerializeField] private PlayerStats playerStats;

    [Header("全スキル")]
    [SerializeField] private SkillData[] allSkills;

    [Header("最大レベル表示")]
    //[SerializeField] private GameObject maxLevelText;

    [Header("レベル表示")]
    [SerializeField] private Image[] levelImages;

    // 未取得状態
    [SerializeField] private Sprite levelOffSprite;

    // 取得済み状態
    [SerializeField] private Sprite levelOnSprite;

    [SerializeField] ExpUIAnimation expUIAnimation;
    [SerializeField] UIAnimation uiAnimation;
    [SerializeField] NormalExpText normalExpText;

    private void Awake()
    {
        //if (maxLevelText != null)
        //{
        //    maxLevelText.SetActive(false);
        //}
    }

    private void Start()
    {
        UpdateLevelImages();
    }

    /// <summary>
    /// マウスカーソルがボタンに乗ったとき
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("ボタンに入った");

        if (ShopTooltipUI.Instance != null)
        {
            ShopTooltipUI.Instance.ShowText(data, true);
        }
    }


    /// <summary>
    /// マウスカーソルがボタンから離れたとき
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (ShopTooltipUI.Instance != null)
        {
            ShopTooltipUI.Instance.Hide();
        }
    }

    /// <summary>
    /// ボタンを押したとき
    /// </summary>
    public void OnClick()
    {
        // 必要経験値が足りなければ何もしない
        if (!data.CanLevelUp())
            return;

        // スキルをレベルアップ
        data.TryLevelUp();

        // レベル画像を更新
        UpdateLevelImages();

        // レベルアップしたダイヤを演出
        PlayLevelUpAnimation();

        // スキル効果を適用
        effectManager.ApplySkill(data);

        // 購入済みにする
        data.isShopUnlocked = true;

        // セーブ
        SaveManager.Save(playerData, allSkills);


        // 経験値UIアニメーション
        PlayExpAnimation();

        // 経験値UIアップデート
        if (normalExpText != null)
        {
            normalExpText.UpdateNormalExpText();
        }
    }

    // 経験値UIアニメーション
    void PlayExpAnimation()
    {
        if (data.requiredExps == null)
            return;

        foreach (RequiredExp requiredExp in data.requiredExps)
        {
            if (requiredExp == null)
                continue;

            // 必要経験値が設定されていないものは無視
            if (requiredExp.needExp <= 0)
                continue;

            switch (requiredExp.expType)
            {
                case ExpType.Exp1:

                    if (expUIAnimation.exp_1 != null)
                    {
                        uiAnimation.PlayBounce(expUIAnimation.exp_1.rectTransform);
                    }
                    break;

                case ExpType.Exp2:

                    if (expUIAnimation.exp_2 != null)
                    {
                        uiAnimation.PlayBounce(expUIAnimation.exp_2.rectTransform);
                    }
                    break;

                case ExpType.Exp3:

                    if (expUIAnimation.exp_3 != null)
                    {
                        uiAnimation.PlayBounce(expUIAnimation.exp_3.rectTransform);
                    }
                    break;

                case ExpType.PreExp:

                    if (expUIAnimation.preExp != null)
                    {
                        uiAnimation.PlayBounce(expUIAnimation.preExp.rectTransform);
                    }
                    break;
            }
        }
    }
    private void UpdateMaxLevelText()
    {
        //if (maxLevelText == null)
        //    return;

        //maxLevelText.SetActive(data != null && data.IsMaxLevel());
    }

    /// <summary>
    /// レベル画像を更新
    /// </summary>
    void UpdateLevelImages()
    {
        if (levelImages == null)
            return;

        UpdateMaxLevelText();

        // 最大レベル表示を現在のスキルだけに合わせる
        //if (maxLevelText != null)
        //{
        //    maxLevelText.SetActive(data.IsMaxLevel());
        //}

        // =========================
        // まず全部表示状態に戻す
        // =========================
        for (int i = 0; i < levelImages.Length; i++)
        {
            if (levelImages[i] == null)
                continue;

            levelImages[i].gameObject.SetActive(true);
        }

        // =========================
        // 最大レベルが1の場合
        // Element 2だけ使用
        // =========================
        if (data.maxLevel == 1)
        {
            for (int i = 0; i < levelImages.Length; i++)
            {
                if (levelImages[i] == null)
                    continue;

                // Element 2以外を非表示
                if (i != 2)
                {
                    levelImages[i].gameObject.SetActive(false);
                    continue;
                }

                // Element 2
                levelImages[i].sprite =
                    data.level >= 1
                    ? levelOnSprite
                    : levelOffSprite;
            }

            return;
        }

        // =========================
        // 通常のスキル
        // =========================
        for (int i = 0; i < levelImages.Length; i++)
        {
            if (levelImages[i] == null)
                continue;

            // 現在のレベル以下ならON
            if (i < data.level)
            {
                levelImages[i].sprite = levelOnSprite;
            }
            else
            {
                levelImages[i].sprite = levelOffSprite;
            }
        }
    }


    /// <summary>
    /// レベルアップしたダイヤをアニメーション
    /// </summary>
    void PlayLevelUpAnimation()
    {
        if (levelImages == null)
            return;

        int newLevelIndex;

        // 最大レベル1の場合はElement 2
        if (data.maxLevel == 1)
        {
            newLevelIndex = 2;
        }
        else
        {
            // レベル1 → Element 0
            // レベル2 → Element 1
            // レベル3 → Element 2
            // ...
            newLevelIndex = data.level - 1;
        }

        if (newLevelIndex < 0 ||
            newLevelIndex >= levelImages.Length)
        {
            return;
        }

        if (levelImages[newLevelIndex] == null)
            return;

        StartCoroutine(
            LevelUpScaleAnimation(
                levelImages[newLevelIndex].rectTransform
            )
        );
    }


    /// <summary>
    /// レベルアップしたダイヤを
    /// 100% → 120% → 100% に拡大しながら回転
    /// </summary>
    IEnumerator LevelUpScaleAnimation(RectTransform target)
    {
        Vector3 originalScale = target.localScale;
        Quaternion originalRotation = target.localRotation;

        Vector3 bigScale = originalScale * 1.2f;

        float duration = 0.3f;

        // =========================
        // 100% → 120%
        // Y 0° → 360°
        // =========================

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;

            // 拡大
            float scaleT = Mathf.SmoothStep(0f, 1f, t);

            target.localScale =
                Vector3.Lerp(originalScale, bigScale, scaleT);

            // 回転
            float rotationT =
                1f - Mathf.Pow(1f - t, 3f);

            target.localRotation = originalRotation * Quaternion.Euler( 0f,360f * rotationT, 0f );

            yield return null;
        }

        target.localScale = bigScale;

        // =========================
        // 120% → 100%
        // Y 360° → 720°
        // =========================

        time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;

            // 縮小
            float scaleT =
                Mathf.SmoothStep(0f, 1f, t);

            target.localScale =
                Vector3.Lerp(
                    bigScale,
                    originalScale,
                    scaleT
                );

            // 回転
            float rotationT =
                1f - Mathf.Pow(1f - t, 3f);

            target.localRotation =
                originalRotation *
                Quaternion.Euler(
                    0f,
                    360f + 360f * rotationT,
                    0f
                );

            yield return null;
        }

        target.localScale = originalScale;
        target.localRotation = originalRotation;
    }

}
