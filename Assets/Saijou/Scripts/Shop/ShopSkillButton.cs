using UnityEngine;
using UnityEngine.UI;
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
        UpdateVisual();

        // 経験値UIアニメーション
        PlayExpAnimation();

        // 経験値UIアップデート
        if (normalExpText != null)
        {
            normalExpText.UpdateNormalExpText();
        }

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
