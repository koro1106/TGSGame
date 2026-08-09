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

    [Header("全スキル")]
    [SerializeField] private SkillData[] allSkills;

    [Header("購入済み表示")]
    [SerializeField] private GameObject unlockedObject;

    void Start()
    {
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

        // 経験値が足りないなら何もしない
        if (GetCurrentExp() < data.needExp)
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
        }

        return 0;
    }
}
