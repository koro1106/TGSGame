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

    [Header("ロック解除エフェクト")]
    [SerializeField] private GameObject unlockEffectPrefab;
    [SerializeField] private Transform unlockEffectPosition;

    [SerializeField]  private bool fadeFinished = false;

    void Start()
    {
        // すでに購入済みなら、
        // シーンをまたいでもフェード演出は完了済みとして扱う
        if (data.isShopUnlocked)
        {
            fadeFinished = true;
        }


        UpdateVisual();
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
        // 鍵穴演出
        // =========================

        if (lockImage != null)
        {
            Vector2 originalPosition = lockImage.anchoredPosition;
            Quaternion originalRotation = lockImage.localRotation;

            float shakeTime = 0f;

            // 揺れる時間
            float shakeDuration = 0.35f;

            // 揺れる回数
            int shakeCount = 3;

            while (shakeTime < shakeDuration)
            {
                shakeTime += Time.deltaTime;

                float t = shakeTime / shakeDuration;

                // 最初は大きく、最後は小さく
                float strength = Mathf.Lerp(4f, 0f, t);

                // 2～3回ブルブルする
                float x =
                    Mathf.Sin(t * Mathf.PI * 2f * shakeCount) * strength;

                // 少しだけ回転
                float angle =
                    Mathf.Sin(t * Mathf.PI * 2f * shakeCount)
                    * strength
                    * 1.5f;

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

        // エフェクト生成

        if (unlockEffectPrefab != null && unlockEffectPosition != null)
        {
            GameObject effect = Instantiate(
                unlockEffectPrefab,
                unlockEffectPosition.position,
                unlockEffectPosition.rotation
            );

            ParticleSystem particle = effect.GetComponentInChildren<ParticleSystem>();

            if (particle != null)
            {
                StartCoroutine(DestroyEffectAfterPlay(effect, particle));
            }
        }

        // 完全に消す
        for (int i = 0; i < graphics.Length; i++)
        {
            Color color = graphics[i].color;
            color.a = 0f;
            graphics[i].color = color;
        }

        fadeFinished = true;
        UpdateVisual();
    }

    /// <summary>
    /// ショップボタンの見た目更新
    /// </summary>
    private void UpdateVisual()
    {
        // まだフェードが完了していない
        if (!fadeFinished)
        {
            unlockedObject.SetActive(true);
            dollNameText.SetActive(false);

            makeButton.SetActive(true);

            elementalBulletChanceButton.SetActive(false);
            effectBulletDamageButton.SetActive(false);

            return;
        }

        // =========================
        // フェード完了後
        // =========================

        unlockedObject.SetActive(false);

        // ぬいぐるみ名を表示
        dollNameText.SetActive(data.isShopUnlocked);

        // 仕立てるボタンを非表示
        makeButton.SetActive(false);

        // 属性弾確率上昇
        elementalBulletChanceButton.SetActive(
            data.isShopUnlocked &&
            playerStats.shopElementalBulletChance
        );

        // 属性ダメージ上昇
        effectBulletDamageButton.SetActive(
            data.isShopUnlocked &&
            playerStats.shopEffectBulletDamage
        );
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

    private IEnumerator DestroyEffectAfterPlay(
    GameObject effect,
    ParticleSystem particle)
    {
        // パーティクルの再生が終わるまで待つ
        yield return new WaitUntil(() => !particle.IsAlive(true));

        // エフェクト削除
        Destroy(effect);
    }
}
