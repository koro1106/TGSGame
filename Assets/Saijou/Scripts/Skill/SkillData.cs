using UnityEngine;
/// <summary>
/// スキルデータ（名前や説明所持）
/// </summary>
[CreateAssetMenu(menuName = "SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName;  // UI表示用スキル名
    public int level;         // 現在レベル
    public int maxLevel;      // 最大レベル
    public int needExp;      // 必要経験値
    public bool isUnlocked = false; //　解放状態
    public bool isLevelUp = false; // レベルアップした
    public PlayerData playerData;
    public ExpType expType;

    public SkillEffectType effectType; // スキル効果タイプ
    public float effectValue = 1;

    public int addNeedExp = 0; // Lv.UPごとに増加する必要経験値量
    // 属性弾解放用
    public GameObject elementalBulletPrefab; // UnlockElementalBullet用

    [TextArea(3, 5)]
    public string description; // ショップ用
    public bool isShopUnlocked = false; // ショップスキル解放用
    public bool isShopButton = false; // ショップボタンかどうか
    /// <summary>
    /// 現在の経験値取得
    /// </summary>
    public int GetCurrentExp()
    {
        switch (expType)
        {
            case ExpType.Exp1:
                return playerData.currentExp_1;

            case ExpType.Exp2:
                return playerData.currentExp_2;

            case ExpType.Exp3:
                return playerData.currentExp_3;

            case ExpType.PreExp:
                return playerData.currentPreExp;
        }

        return 0;
    }
    public bool CanLevelUp()
    {
        if (expType == ExpType.ShopExp)
        {
            return playerData.currentExp_2 >= 100 &&
                   playerData.currentExp_3 >= 30;
        }

        return GetCurrentExp() >= needExp;
    }

    /// <summary>
    /// 経験値消費
    /// </summary>
    void ConsumeExp(int value)
    {
        switch (expType)
        {
            case ExpType.Exp1:
                playerData.currentExp_1 -= value;
                break;

            case ExpType.Exp2:
                playerData.currentExp_2 -= value;
                break;

            case ExpType.Exp3:
                playerData.currentExp_3 -= value;
                break;
            case ExpType.PreExp:
                playerData.currentPreExp -= value;
                break;

            case ExpType.ShopExp:
                playerData.currentExp_2 -= 100;
                playerData.currentExp_3 -= 30;
                break;
        }
    }

    /// <summary>
    /// 経験値を消費してレベルアップ
    /// </summary>
    public void TryLevelUp()
    {
        // レベル上限＆経験値チェック
        if (level < maxLevel && CanLevelUp())
        {
            // 経験値消費
            ConsumeExp(needExp);

            // レベルアップ
            LevelUp();

            // 解放状態
            isUnlocked = true;

            if (!isShopButton)
            {
                // ツールチップ更新
                TooltipUI.instance?.ShowText(this, false);
            }
        }
    }

    /// <summary>
    /// レベルアップ
    /// </summary>
    void LevelUp()
    {
        level++;
        isLevelUp = true;

        // 必要経験値増加
        needExp = Mathf.RoundToInt(needExp + addNeedExp);

        // 最大レベル制限
        if (level >= maxLevel)
        {
            level = maxLevel;
        }
    }

    /// <summary>
    /// 最大レベル判定
    /// </summary>
    public bool IsMaxLevel()
    {
        return level >= maxLevel;
    }
}