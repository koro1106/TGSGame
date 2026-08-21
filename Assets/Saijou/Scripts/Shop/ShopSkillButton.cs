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

    [Header("仕立てるボタン")]
    [SerializeField] private GameObject makeButton;

    [Header("属性弾確率上昇ボタン")]
    [SerializeField] private GameObject elementalBulletChanceButton;
   
    [Header("属性ダメージ上昇ボタン")]
    [SerializeField] private GameObject effectBulletDamageButton;

    [SerializeField] ExpUIAnimation expUIAnimation;
    [SerializeField] UIAnimation uiAnimation;

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
        // 経験値が足りないなら何もしない
        if (GetCurrentExp() < data.needExp)
            return;

        // ShopExpだけ特別な判定
        if (data.expType == ExpType.ShopExp)
        {
            if (playerData.currentExp_2 < 100 || playerData.currentExp_3 < 30)
                return;
        }
        else
        {
            // 通常の経験値判定
            if (GetCurrentExp() < data.needExp)
                return;
        }

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
        PlayExpAnimation(); // 経験値UIアニメーション

    }

    /// <summary>
    /// ショップボタンの見た目更新
    /// </summary>
    private void UpdateVisual()
    {
        if (data.isShopUnlocked)
        {
            // 購入済み
            if (unlockedObject != null)
                unlockedObject.SetActive(true);

            makeButton.SetActive(false); // 仕立てるボタン非表示
        }
        else
        {
            // 未購入
            if (unlockedObject != null)
                unlockedObject.SetActive(false);
        }
    }

    /// <summary>
    /// 現在の経験値を取得
    /// </summary>
    private int GetCurrentExp()
    {
        switch (data.expType)
        {
            case ExpType.Exp1:
                return data.playerData.currentExp_1;

            case ExpType.Exp2:
                return data.playerData.currentExp_2;

            case ExpType.Exp3:
                return data.playerData.currentExp_3;

            case ExpType.PreExp:
                return data.playerData.currentPreExp;

            case ExpType.ShopExp:
                return (data.playerData.currentExp_2 >= 100 &&
                        data.playerData.currentExp_3 >= 30)
                    ? data.needExp
                    : 0;
        }

        return 0;
    }

    // 経験値UIアニメーション
    void PlayExpAnimation()
    {
        switch (data.expType)
        {
            case ExpType.Exp1:
                uiAnimation.PlayBounce(expUIAnimation.exp_1.rectTransform);
                break;

            case ExpType.Exp2:
                uiAnimation.PlayBounce(expUIAnimation.exp_2.rectTransform);
                break;

            case ExpType.Exp3:
                uiAnimation.PlayBounce(expUIAnimation.exp_3.rectTransform);
                break;

            case ExpType.PreExp:
                uiAnimation.PlayBounce(expUIAnimation.preExp.rectTransform);
                break;
        }
    }
}
