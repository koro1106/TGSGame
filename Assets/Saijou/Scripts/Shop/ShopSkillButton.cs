using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// ショップでぬいぐるみ購入UI
/// </summary>
public class ShopSkillButton : MonoBehaviour
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

    [Header("購入済み表示")]
    [SerializeField] private GameObject unlockedObject;
    [SerializeField] private GameObject dollNameText;

    [Header("ロック演出")]
    [SerializeField] private RectTransform lockImage;

    [Header("仕立てるボタン")]
    [SerializeField] private GameObject makeButton;

    [Header("属性弾確率上昇ボタン")]
    [SerializeField] private GameObject elementalBulletChanceButton;

    [Header("属性ダメージ上昇ボタン")]
    [SerializeField] private GameObject effectBulletDamageButton;

    [SerializeField] ExpUIAnimation expUIAnimation;
    [SerializeField] UIAnimation uiAnimation;
    [SerializeField] NormalExpText normalExpText;

    void Start()
    {
        UpdateVisual();
    }

    void Update()
    {
        // 仕立て済みで属性弾確率上昇解放済みだったら表示
        if (data.isShopUnlocked && playerStats.shopElementalBulletChance)
        {
            elementalBulletChanceButton.SetActive(true);
        }
        // 仕立て済みで属性ダメージ上昇解放済みだったら表示
        if (data.isShopUnlocked && playerStats.shopEffectBulletDamage)
        {
            effectBulletDamageButton.SetActive(true);
        }

    }
    /// <summary>
    /// ショップボタンを押したとき
    /// </summary>
    public void OnClick()
    {
        // すでに購入済みなら何もしない
        if (data.isShopUnlocked)
            return;
        // 必要経験値が足りなければ何もしない
        if (!data.CanLevelUp())
            return;

        // スキルをレベルアップ
        data.TryLevelUp();

        // スキル効果を適用
        effectManager.ApplySkill(data);

        // 購入済みにする
        data.isShopUnlocked = true;

        // セーブ
        SaveManager.Save(playerData, allSkills);

        // 見た目更新
        StartCoroutine(UnlockAnimation());

        // 経験値UIアニメーション
        PlayExpAnimation();

        // 経験値UIアップデート
        if (normalExpText != null)
        {
            normalExpText.UpdateNormalExpText();
        }

    }

    /// <summary>
    /// ロック解除演出
    /// </summary>
    IEnumerator UnlockAnimation()
    {
        // =========================
        // 鍵穴「カチャッ」
        // =========================

        if (lockImage != null)
        {
            Vector2 originalPosition = lockImage.anchoredPosition;
            Quaternion originalRotation = lockImage.localRotation;

            float shakeTime = 0f;
            float shakeDuration = 0.18f;

            while (shakeTime < shakeDuration)
            {
                shakeTime += Time.deltaTime;

                float t = shakeTime / shakeDuration;

                // 最初は大きく、最後はすぐ止まる
                float strength = Mathf.Lerp(3f, 0f, t);

                // 左右に「カチャッ」
                float x =
                    Mathf.Sin(shakeTime * 70f) * strength;

                // 少しだけ回転
                float angle =
                    Mathf.Sin(shakeTime * 70f) * strength * 1.5f;

                lockImage.anchoredPosition =
                    originalPosition + new Vector2(x, 0f);

                lockImage.localRotation =
                    originalRotation *
                    Quaternion.Euler(0f, 0f, angle);

                yield return null;
            }

            // 元の位置・角度に戻す
            lockImage.anchoredPosition = originalPosition;
            lockImage.localRotation = originalRotation;
        }

        // 少し待つ
        yield return new WaitForSeconds(0.2f);

        // ここで一瞬だけ止める
        yield return new WaitForSeconds(0.2f);

        // =========================
        // unlockedObject フェード
        // =========================

        Graphic[] graphics =
            unlockedObject.GetComponentsInChildren<Graphic>(true);

        float fadeDuration = 0.4f;
        float fadeTime = 0f;

        // 最初のアルファを保存
        float[] alpha =
            new float[graphics.Length];

        for (int i = 0; i < graphics.Length; i++)
        {
            alpha[i] = graphics[i].color.a;
        }

        // フェード
        while (fadeTime < fadeDuration)
        {
            fadeTime += Time.deltaTime;

            float t =
                Mathf.Clamp01(fadeTime / fadeDuration);

            for (int i = 0; i < graphics.Length; i++)
            {
                Color color = graphics[i].color;

                color.a =
                    Mathf.Lerp(alpha[i], 0f, t);

                graphics[i].color = color;
            }

            yield return null;
        }

        // 完全に消す
        for (int i = 0; i < graphics.Length; i++)
        {
            Color color = graphics[i].color;
            color.a = 0f;
            graphics[i].color = color;
        }

        unlockedObject.SetActive(false);

        // =========================
        // 購入済み表示
        // =========================

        dollNameText.SetActive(true);
        makeButton.SetActive(false);
    }

    /// <summary>
    /// ショップボタンの見た目更新
    /// </summary>
    private void UpdateVisual()
    {
        if (data.isShopUnlocked)
        {
            // 購入済み
            unlockedObject.SetActive(false);
            dollNameText.SetActive(true);

            makeButton.SetActive(false); // 仕立てるボタン非表示
        }
        else
        {
            // 未購入
            unlockedObject.SetActive(true);
            dollNameText.SetActive(false);

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
                        uiAnimation.PlayBounce(
                            expUIAnimation.exp_1.rectTransform
                        );
                    }

                    break;


                case ExpType.Exp2:

                    if (expUIAnimation.exp_2 != null)
                    {
                        uiAnimation.PlayBounce(
                            expUIAnimation.exp_2.rectTransform
                        );
                    }

                    break;


                case ExpType.Exp3:

                    if (expUIAnimation.exp_3 != null)
                    {
                        uiAnimation.PlayBounce(
                            expUIAnimation.exp_3.rectTransform
                        );
                    }

                    break;


                case ExpType.PreExp:

                    if (expUIAnimation.preExp != null)
                    {
                        uiAnimation.PlayBounce(
                            expUIAnimation.preExp.rectTransform
                        );
                    }

                    break;
            }
        }

    }
}
