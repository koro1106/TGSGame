using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 属性弾ダメージUP＆属性弾発生確率UP
/// </summary>
public class ShopUPButton : MonoBehaviour
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

    [SerializeField] ExpUIAnimation expUIAnimation;
    [SerializeField] UIAnimation uiAnimation;
    [SerializeField] NormalExpText normalExpText;

    /// <summary>
    /// マウスカーソルがボタンに乗ったとき
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 解放済みならツールチップを表示しない
        if (data.isUnlocked)
            return;

        if (ShopTooltipUI.Instance != null)
        {
            ShopTooltipUI.Instance.ShowText(data, false);
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
